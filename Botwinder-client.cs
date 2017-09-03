using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Botwinder.entities;
using Discord;
using Discord.WebSocket;

using guid = System.Int64;

namespace Botwinder.core
{
	public partial class BotwinderClient<TUser> : IBotwinderClient<TUser>,IDisposable where TUser : UserData, new()
	{
		private readonly DbConfig DbConfig;
		private GlobalContext GlobalDb;
		public GlobalConfig GlobalConfig;
		private Shard CurrentShard;

		private DiscordSocketClient DiscordClient;
		public Events Events;

		public readonly DateTime TimeStarted = DateTime.Now;
		private DateTime TimeConnected = DateTime.MaxValue;

		private bool IsInitialized = false;
		public bool IsConnected{
			get{
				return this.DiscordClient.LoginState == LoginState.LoggedIn &&
				       this.DiscordClient.ConnectionState == ConnectionState.Connected &&
				       _Connected;
			}
			private set{ _Connected = value; }
		}
		private bool _Connected = false;

		private CancellationTokenSource MainUpdateCancel;
		private Task MainUpdateTask;

		public readonly List<IModule> Modules = new List<IModule>();

		private const string GameStatusConnecting = "Connecting...";
		private const string GameStatusUrl = "at http://botwinder.info";

		private readonly Dictionary<string, Command> Commands = new Dictionary<string, Command>();


		public BotwinderClient()
		{
			this.DbConfig = DbConfig.Load();
			this.GlobalDb = GlobalContext.Create(this.DbConfig.GetDbConnectionString());
		}

		public void Dispose()
		{
			this.DiscordClient.Dispose();
			this.DiscordClient = null;

			this.GlobalDb.Dispose();
			this.GlobalDb = null;

			//todo
		}

		public async Task Connect()
		{
			LoadConfig();

			//Find a shard for grabs.
			while( (this.CurrentShard = this.GlobalDb.Shards.FirstOrDefault(s =>s.IsTaken == false)) == null )
			{
				await Task.Delay(10000);
			}

			this.CurrentShard.IsTaken = true;
			this.GlobalDb.SaveChanges();

			DiscordSocketConfig config = new DiscordSocketConfig();
			config.ShardId = (int) this.CurrentShard.Id;
			config.TotalShards = (int) this.GlobalConfig.TotalShards;
			config.LogLevel = this.GlobalConfig.LogDebug ? LogSeverity.Debug : LogSeverity.Warning;
			config.DefaultRetryMode = RetryMode.Retry502 & RetryMode.RetryRatelimit & RetryMode.RetryTimeouts;
			config.AlwaysDownloadUsers = true;
			config.LargeThreshold = 100;
			config.HandlerTimeout = null;
			config.MessageCacheSize = 100;
			config.ConnectionTimeout = int.MaxValue; //todo - figure out something reasonable?

			this.DiscordClient = new DiscordSocketClient(config);

			this.DiscordClient.Connecting += OnConnecting;
			this.DiscordClient.Connected += OnConnected;
			this.DiscordClient.Ready += OnReady;
			this.DiscordClient.Disconnected += OnDisconnected;
			this.Events = new Events(this.DiscordClient);
			this.Events.MessageReceived += OnMessageReceived;
			this.Events.MessageUpdated += OnMessageUpdated;
			this.Events.LogEntryAdded += Log;
			this.Events.Exception += Log;
			this.Events.Connected += async () => await this.DiscordClient.SetGameAsync(GameStatusUrl);
			this.Events.Initialize += InitCommands;
			this.Events.Initialize += InitModules;

			await this.DiscordClient.LoginAsync(TokenType.Bot, this.GlobalConfig.DiscordToken);
			await this.DiscordClient.StartAsync();
		}

		private void LoadConfig()
		{
			Console.WriteLine("BotwinderClient: Loading configuration.");

			bool save = false;
			if( !this.GlobalDb.GlobalConfigs.Any() )
			{
				this.GlobalDb.GlobalConfigs.Add(new GlobalConfig());
				save = true;
			}

			if( save )
				this.GlobalDb.SaveChanges();

			this.GlobalConfig = this.GlobalDb.GlobalConfigs.First(c => c.ConfigName == this.DbConfig.ConfigName);
		}

//Events
		private async Task OnConnecting()
		{
			//Some other node is already connecting, wait.
			bool awaited = false;
			while( this.GlobalDb.Shards.Any(s => s.IsConnecting) )
			{
				if( !awaited )
					Console.WriteLine("BotwinderClient: Waiting for other shards...");

				awaited = true;
				await Task.Delay(10000);
			}

			this.CurrentShard.IsConnecting = true;
			this.GlobalDb.SaveChanges();
			if( awaited )
				await Task.Delay(5000); //Ensure sufficient delay between connecting shards.

			Console.WriteLine("BotwinderClient: Connecting...");
		}

		private async Task OnConnected()
		{
			Console.WriteLine("BotwinderClient: Connected.");

			this.CurrentShard.IsConnecting = false;
			this.GlobalDb.SaveChanges();

			this.TimeConnected = DateTime.Now;
			await this.DiscordClient.SetGameAsync(GameStatusConnecting);
		}

		private Task OnReady()
		{
			Console.WriteLine("BotwinderClient: Ready.");

			if( this.MainUpdateTask == null )
			{
				this.MainUpdateCancel = new CancellationTokenSource();
				this.MainUpdateTask = Task.Factory.StartNew(MainUpdate, this.MainUpdateCancel.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
			}

			return Task.CompletedTask;
		}

		private async Task OnDisconnected(Exception exception)
		{
			Console.WriteLine("BotwinderClient: Disconnected.");
			this.IsConnected = false;

			await LogException(exception, "--D.NET Client Disconnected");

			try
			{
				if( this.Events.Disconnected != null )
					await this.Events.Disconnected(exception);
			}
			catch(Exception e)
			{
				await LogException(e, "--Events.Disconnected");
			}
		}

		private async Task OnMessageReceived(SocketMessage message)
		{
		}

		private async Task OnMessageUpdated(SocketMessage originalMessage, SocketMessage updatedMessage, ISocketMessageChannel channel)
		{
		}

		private Task Log(ExceptionEntry exceptionEntry)
		{
			this.GlobalDb.Exceptions.Add(exceptionEntry);
			this.GlobalDb.SaveChanges();
			return Task.CompletedTask;
		}

		private Task Log(LogEntry logEntry)
		{
			this.GlobalDb.Log.Add(logEntry);
			this.GlobalDb.SaveChanges();
			return Task.CompletedTask;
		}

//Update
		private async Task MainUpdate()
		{
			while( !this.MainUpdateCancel.IsCancellationRequested )
			{
				DateTime frameTime = DateTime.Now;

				if( this.DiscordClient.ConnectionState != ConnectionState.Connected ||
				    this.DiscordClient.LoginState != LoginState.LoggedIn ||
				    this.TimeConnected - DateTime.Now < TimeSpan.FromSeconds(this.GlobalConfig.InitialUpdateDelay) )
					continue;

				if( !this.IsInitialized )
				{
					try
					{
						this.IsInitialized = true;
						await this.Events.Initialize();
					}
					catch(Exception exception)
					{
						await LogException(exception, "--Events.Initialize");
					}
				}
				if( !this.IsConnected )
				{
					try
					{
						this.IsConnected = true;
						await this.Events.Connected();
					}
					catch(Exception exception)
					{
						await LogException(exception, "--Events.Connected");
					}

					continue; //Don't run update in the same loop as init.
				}

				try
				{
					await Update();
				}
				catch(Exception exception)
				{
					await LogException(exception, "--Update");
				}

				await UpdateModules();

				TimeSpan deltaTime = DateTime.Now - frameTime;
				await Task.Delay(TimeSpan.FromSeconds(1f / this.GlobalConfig.TargetFps) - deltaTime);
			}
		}

		private async Task InitModules()
		{
			List<Command> newCommands;
			foreach( IModule module in this.Modules )
			{
				try
				{
					module.HandleException += async (e, d, id) => await LogException(e, "--ModuleInit." + module.ToString() + " | " + d, id);
					newCommands = await module.Init(this);

					foreach( Command cmd in newCommands )
					{
						if( this.Commands.ContainsKey(cmd.Id) )
						{
							this.Commands[cmd.Id] = cmd;
							continue;
						}

						this.Commands.Add(cmd.Id, cmd);
					}
				}
				catch(Exception exception)
				{
					await LogException(exception, "--ModuleInit." + module.ToString());
				}
			}
		}

		private async Task UpdateModules()
		{
			foreach( IModule module in this.Modules )
			{
				try
				{
					await module.Update(this);
				}
				catch(Exception exception)
				{
					await LogException(exception, "--ModuleUpdate." + module.ToString());
				}
			}
		}
	}
}

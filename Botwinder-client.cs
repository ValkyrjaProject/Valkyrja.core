using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Botwinder.entities;
using Discord;
using Discord.WebSocket;

using guid = System.Int64;

namespace Botwinder.core
{
	public partial class BotwinderClient<TUser> : IDisposable where TUser : UserData, new()
	{
		private readonly DbConfig DbConfig;
		private GlobalContext GlobalDb;
		private GlobalConfig GlobalConfig;
		private Shard CurrentShard;

		private DiscordSocketClient DiscordClient;
		public Events Events;

		private const string GameStatusConnecting = "Connecting...";
		private const string GameStatusUrl = "at http://botwinder.info";
		private string CurrentGameStatus = GameStatusConnecting;

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

			await this.DiscordClient.LoginAsync(TokenType.Bot, this.GlobalConfig.DiscordToken);
			await this.DiscordClient.StartAsync();
			await this.DiscordClient.SetGameAsync(this.CurrentGameStatus);
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
			Console.WriteLine("BotwinderClient: Waiting for other shards...");

			//Some other node is already connecting, wait.
			while( this.GlobalDb.Shards.Any(s => s.IsConnecting) )
			{
				await Task.Delay(10000);
			}

			this.CurrentShard.IsConnecting = true;
			this.GlobalDb.SaveChanges();

			Console.WriteLine("BotwinderClient: Connecting...");
		}

		private Task OnConnected()
		{
			Console.WriteLine("BotwinderClient: Connected.");

			this.CurrentShard.IsConnecting = false;
			this.GlobalDb.SaveChanges();

			return Task.CompletedTask;
		}

		private Task OnReady()
		{
			Console.WriteLine("BotwinderClient: Ready.");

			return Task.CompletedTask;
		}

		private async Task OnDisconnected(Exception exception)
		{
			Console.WriteLine("BotwinderClient: Disconnected.");
			this.CurrentGameStatus = GameStatusConnecting;

			await LogException(exception, "--D.NET Client Disconnected");
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
	}
}

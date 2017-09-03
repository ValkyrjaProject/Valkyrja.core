using System;
using System.Linq;
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

		public async Task<string> TestDb()
		{
			await Connect();

			return "Loaded global config: " + this.GlobalConfig.ConfigName;
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
			ReloadConfig();
			Shard shard = null;

			//Find a shard for grabs.
			while( (shard = this.GlobalDb.Shards.FirstOrDefault(s =>s.IsTaken == false)) == null )
			{
				await Task.Delay(10000);
			}

			shard.IsTaken = true;
			this.GlobalDb.SaveChanges();

			//Some other node is already connecting, wait.
			while( this.GlobalDb.Shards.Any(s => s.IsConnecting) )
			{
				await Task.Delay(10000);
			}

			shard.IsConnecting = true; //todo - don't forget to set it back to false somewhere later heh...
			this.GlobalDb.SaveChanges();

			DiscordSocketConfig config = new DiscordSocketConfig();
			config.ShardId = (int) shard.Id;
			config.TotalShards = (int) this.GlobalConfig.TotalShards;
			config.LogLevel = this.GlobalConfig.LogDebug ? LogSeverity.Debug : LogSeverity.Warning;
			config.DefaultRetryMode = RetryMode.Retry502 & RetryMode.RetryRatelimit & RetryMode.RetryTimeouts;
			config.AlwaysDownloadUsers = true;
			config.LargeThreshold = 100;
			config.HandlerTimeout = null;
			config.MessageCacheSize = 100;
			//config.ConnectionTimeout

			this.DiscordClient = new DiscordSocketClient(config);

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

		private void ReloadConfig()
		{
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
		private Task OnConnected()
		{
			return Task.CompletedTask;
		}

		private Task OnReady()
		{
			return Task.CompletedTask;
		}

		private Task OnDisconnected(Exception exception)
		{
			this.CurrentGameStatus = GameStatusConnecting;

			ExceptionEntry e = new ExceptionEntry();
			e.Message += exception.Message;
			e.Stack += exception.StackTrace;
			e.Data += "--D.NET Client Disconnected";
			this.Events.Exception(e);
			return Task.CompletedTask;
		}

		private Task OnMessageReceived(SocketMessage message)
		{
			return Task.CompletedTask;
		}

		private Task OnMessageUpdated(SocketMessage originalMessage, SocketMessage updatedMessage, ISocketMessageChannel channel)
		{
			return Task.CompletedTask;
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

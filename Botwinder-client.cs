using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Botwinder.entities;
using Discord;
using Discord.WebSocket;

using guid = System.UInt64;

namespace Botwinder.core
{
	public partial class BotwinderClient<TUser> : IBotwinderClient<TUser>, IDisposable where TUser : UserData, new()
	{
		private readonly DbConfig DbConfig;
		private GlobalContext GlobalDb;
		private ServerContext ServerDb;
		public GlobalConfig GlobalConfig{ get; set; }
		public Shard CurrentShard{ get; set; }

		private DiscordSocketClient DiscordClient;
		public Events Events;

		public readonly DateTime TimeStarted = DateTime.Now;
		private DateTime TimeConnected = DateTime.MaxValue;

		private bool IsInitialized = false;

		public bool IsConnected{
			get => this.DiscordClient.LoginState == LoginState.LoggedIn &&
			       this.DiscordClient.ConnectionState == ConnectionState.Connected &&
			       this._Connected;
			set => this._Connected = value;
		}

		private bool _Connected = false;

		private CancellationTokenSource MainUpdateCancel;
		private Task MainUpdateTask;

		public readonly List<IModule> Modules = new List<IModule>();

		private const string GameStatusConnecting = "Connecting...";
		private const string GameStatusUrl = "at http://botwinder.info";
		private readonly Regex RegexCommandParams = new Regex("\"[^\"]+\"|\\S+", RegexOptions.Compiled);

		private readonly ConcurrentDictionary<guid, Server<TUser>> Servers = new ConcurrentDictionary<guid, Server<TUser>>();
		private readonly Dictionary<string, Command<TUser>> Commands = new Dictionary<string, Command<TUser>>();
		public List<Operation<TUser>> CurrentOperations{ get; set; } = new List<Operation<TUser>>();
		public Object OperationsLock{ get; set; } = new Object();

		private readonly List<guid> LeaveNotifiedOwners = new List<guid>();


		public BotwinderClient()
		{
			this.DbConfig = DbConfig.Load();
			this.GlobalDb = GlobalContext.Create(this.DbConfig.GetDbConnectionString());
			this.ServerDb = ServerContext.Create(this.DbConfig.GetDbConnectionString());
		}

		public void Dispose()
		{
			this.CurrentShard.IsTaken = false;
			this.CurrentShard.IsConnecting = false;
			this.GlobalDb.SaveChanges();
			this.ServerDb.SaveChanges();
			Task.Delay(500).Wait();

			this.DiscordClient.Dispose();
			this.DiscordClient = null;

			this.GlobalDb.Dispose();
			this.GlobalDb = null;

			this.ServerDb.Dispose();
			this.ServerDb = null;

			//todo
		}

		public async Task Connect()
		{
			LoadConfig();

			//Find a shard for grabs.
			while( (this.CurrentShard = this.GlobalDb.Shards.FirstOrDefault(s => s.IsTaken == false)) == null )
			{
				await Task.Delay(Utils.Random.Next(5000, 10000));
			}

			this.CurrentShard.IsTaken = true;
			this.CurrentShard.ResetStats(this.TimeStarted);
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
			this.Events.GuildAvailable += OnGuildAvailable;
			this.Events.JoinedGuild += OnGuildJoined;
			this.Events.LeftGuild += OnGuildLeft;

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
				await Task.Delay(Utils.Random.Next(5000, 10000));
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
			SocketTextChannel channel = message.Channel as SocketTextChannel;
			if( channel == null || !this.Servers.ContainsKey(channel.Guild.Id) )
				return;

			Server<TUser> server = this.Servers[channel.Guild.Id];
			if( server.Config.IgnoreBots && message.Author.IsBot ||
			    server.Config.IgnoreEveryone && message.MentionedRoles.Any(r => r.IsEveryone) )
				return;

			bool commandExecuted = false;
			string prefix;
			if( (!string.IsNullOrWhiteSpace(server.Config.CommandPrefix) &&
			     message.Content.StartsWith(prefix = server.Config.CommandPrefix)) ||
			    (!string.IsNullOrWhiteSpace(server.Config.CommandPrefixAlt) &&
			     message.Content.StartsWith(prefix = server.Config.CommandPrefixAlt)) )
				commandExecuted = await HandleCommand(server, channel, message, prefix);
		}

		private async Task OnMessageUpdated(SocketMessage originalMessage, SocketMessage updatedMessage, ISocketMessageChannel iChannel)
		{
			SocketTextChannel channel = iChannel as SocketTextChannel;
			if( channel == null || !this.Servers.ContainsKey(channel.Guild.Id) )
				return;

			Server<TUser> server = this.Servers[channel.Guild.Id];
			if( server.Config.IgnoreBots && updatedMessage.Author.IsBot ||
			    server.Config.IgnoreEveryone && updatedMessage.MentionedRoles.Any(r => r.IsEveryone) )
				return;

			bool commandExecuted = false;
			string prefix;
			if( (!string.IsNullOrWhiteSpace(server.Config.CommandPrefix) &&
			     updatedMessage.Content.StartsWith(prefix = server.Config.CommandPrefix)) ||
			    (!string.IsNullOrWhiteSpace(server.Config.CommandPrefixAlt) &&
			     updatedMessage.Content.StartsWith(prefix = server.Config.CommandPrefixAlt)) )
				commandExecuted = await HandleCommand(server, channel, updatedMessage, prefix);
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

		private async Task Update()
		{
			if( this.GlobalConfig.EnforceRequirements )
			{
				await BailBadServers();
			}

			this.GlobalDb.SaveChanges(); //Note that this method checks for changes first.
			this.ServerDb.SaveChanges();

			//todo - maintenance
		}

//Modules
		private async Task InitModules()
		{
			List<Command<TUser>> newCommands;
			foreach( IModule module in this.Modules )
			{
				try
				{
					module.HandleException += async (e, d, id) =>
						await LogException(e, "--ModuleInit." + module.ToString() + " | " + d, id);
					newCommands = await module.Init(this);

					foreach( Command<TUser> cmd in newCommands )
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

//Commands
		private void GetCommandAndParams(string message, out string commandString, out string trimmedMessage, out string[] parameters)
		{
			trimmedMessage = "";
			parameters = null;

			MatchCollection regexMatches = this.RegexCommandParams.Matches(message);
			if( regexMatches.Count == 0 )
			{
				commandString = message.Trim();
				return;
			}

			commandString = regexMatches[0].Value;

			if( regexMatches.Count > 1 )
			{
				trimmedMessage = message.Substring(regexMatches[1].Index).Trim('\"', ' ', '\n');
				Match[] matches = new Match[regexMatches.Count];
				regexMatches.CopyTo(matches, 0);
				parameters = matches.Skip(1).Select(p => p.Value).ToArray();
				for( int i = 0; i < parameters.Length; i++ )
					parameters[i] = parameters[i].Trim('"');
			}
		}

		private async Task<bool> HandleCommand(Server<TUser> server, SocketTextChannel channel, SocketMessage message, string prefix)
		{
			GetCommandAndParams(message.Content.Substring(prefix.Length), out string commandString, out string trimmedMessage, out string[] parameters);

			if( server.Commands.ContainsKey(commandString) ||
			    (server.CustomAliases.ContainsKey(commandString) &&
			     server.Commands.ContainsKey(commandString = server.CustomAliases[commandString].CommandId)) )
			{
				Command<TUser> command = server.Commands[commandString];
				if( !string.IsNullOrEmpty(command.ParentId) ) //Internal, not-custom alias.
					command = server.Commands[command.ParentId];

				CommandOptions commandOptions = server.GetCommandOptions(commandString);
				CommandArguments<TUser> args = new CommandArguments<TUser>(this, command, server, channel, message, trimmedMessage, parameters, commandOptions);

				if( command.CanExecute(this, server, channel, message.Author as SocketGuildUser) )
					return await command.Execute(args);
			}
			else if( server.CustomCommands.ContainsKey(commandString) ||
			         (server.CustomAliases.ContainsKey(commandString) &&
			          server.CustomCommands.ContainsKey(commandString = server.CustomAliases[commandString].CommandId)) )
			{
				if( server.CustomCommands[commandString].CanExecute(this, server, channel, message.Author as SocketGuildUser) )
					return await HandleCustomCommand(server.CustomCommands[commandString], channel, message);
			}

			return false;
		}

		private async Task<bool> HandleCustomCommand(CustomCommand cmd, SocketTextChannel channel, SocketMessage message)
		{
			//todo - rewrite using string builder...
			string msg = cmd.Response;

			if( msg.Contains("{sender}") || msg.Contains("{{sender}}") )
			{
				msg = msg.Replace("{{sender}}", "<@{0}>").Replace("{sender}", "<@{0}>");
				msg = string.Format(msg, message.Author.Id);
			}

			if( (msg.Contains("{mentioned}") || msg.Contains("{{mentioned}}")) && message.MentionedUsers != null )
			{
				string mentions = "";
				SocketUser[] mentionedUsers = message.MentionedUsers.ToArray();
				for( int i = 0; i < mentionedUsers.Length; i++ )
				{
					if( i != 0 )
						mentions += (i == mentionedUsers.Length - 1) ? " and " : ", ";

					mentions += "<@" + mentionedUsers[i].Id + ">";
				}

				if( string.IsNullOrEmpty(mentions) )
				{
					msg = msg.Replace("{{mentioned}}", "Nobody").Replace("{mentioned}", "Nobody");
				}
				else
				{
					msg = msg.Replace("{{mentioned}}", "{0}").Replace("{mentioned}", "{0}");
					msg = string.Format(msg, mentions);
				}
			}

			await SendMessageToChannel(channel, msg);
			return true;
		}


// Guild events
		private async Task OnGuildJoined(SocketGuild guild)
		{
			try
			{
				await OnGuildAvailable(guild);

				string msg = Localisation.SystemStrings.GuildJoined;
				if( !IsPartner(guild.Id) && !IsSubscriber(guild.OwnerId) )
					msg += Localisation.SystemStrings.GuildJoinedTrial;

				try
				{
					await guild.Owner.SendMessageSafe(msg);
				}
				catch(Exception) { }
			}
			catch(Exception exception)
			{
				await LogException(exception, "--OnGuildJoined", guild.Id);
			}
		}

		private async Task OnGuildLeft(SocketGuild guild)
		{
			try
			{
				if( !this.Servers.ContainsKey(guild.Id) )
					return;

				//todo - cancel operations
				this.Servers.Remove(guild.Id);
			}
			catch(Exception exception)
			{
				await LogException(exception, "--OnGuildLeft", guild.Id);
			}
		}

		private async Task OnGuildAvailable(SocketGuild guild)
		{
			try
			{
				Server<TUser> server;
				if( this.Servers.ContainsKey(guild.Id) )
				{
					server = this.Servers[guild.Id];
					server.ReloadConfig(this.ServerDb);
				}
				else
				{
					server = new Server<TUser>(guild, this.Commands, this.ServerDb);
					server.LoadConfig(this.ServerDb);
				}
			}
			catch(Exception exception)
			{
				await LogException(exception, "--OnGuildAvailable", guild.Id);
			}
		}

		private async Task BailBadServers()
		{
			try
			{
				List<Server<TUser>> serversToLeave = new List<Server<TUser>>();

				foreach( KeyValuePair<guid, Server<TUser>> pair in this.Servers )
				{
					try
					{
						//Partnered servers
						if( !IsPartner(pair.Value.Id) && !IsSubscriber(pair.Value.Id) && !serversToLeave.Contains(pair.Value) &&
						    (!pair.Value.Guild.CurrentUser.JoinedAt.HasValue ||
						     DateTime.UtcNow - pair.Value.Guild.CurrentUser.JoinedAt.Value.ToUniversalTime() > TimeSpan.FromHours(this.GlobalConfig.VipTrialHours)) )
						{
							serversToLeave.Add(pair.Value);
							if( !this.LeaveNotifiedOwners.Contains(pair.Value.Guild.OwnerId) )
							{
								this.LeaveNotifiedOwners.Add(pair.Value.Guild.OwnerId);
								try
								{
									await pair.Value.Guild.Owner.SendMessageSafe(Localisation.SystemStrings.VipPmLeaving);
								}
								catch(Exception) { }
							}
							continue;
						}

						//Blacklisted servers
						if( this.GlobalDb.Blacklist.Any(b => b.Id == pair.Value.Id || b.Id == pair.Value.Guild.OwnerId) &&
						    !serversToLeave.Contains(pair.Value) )
						{
							serversToLeave.Add(pair.Value);
							continue;
						}
					}
					catch(Exception exception)
					{
						await LogException(exception, "--BailBadServers", pair.Value.Id);
					}
				}

				foreach( Server<TUser> server in serversToLeave )
				{
					await server.Guild.LeaveAsync();
				}
			}
			catch(Exception exception)
			{
				await LogException(exception, "--BailBadServers");
			}
		}
	}
}

﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Valkyrja.core;
using Discord.Net;
using guid = System.UInt64;

namespace Valkyrja.entities
{
	public class Events
	{
		private ValkyrjaClient Client;


		/// <summary> Triggers only once, as soon as the client connects for the first time. Consider using IModule.Init instead. </summary>
		public Func<Task> Initialize = null;
		/// <summary> Triggers after every re-connect (including the first connect) </summary>
		public Func<Task> Connected = null;
		/// <summary> Triggers every disconnect </summary>
		public Func<Exception, Task> Disconnected = null;
		// This is probably useless and doesn't have to be public, we have the above...
		internal Func<Task> Ready = null;
		/// <summary> Log entry was added. </summary>
		public Func<LogEntry, Task> LogEntryAdded = null;
		/// <summary> Exception was added. Don't call this event directly, call ValkyrjaClient.LogException </summary>
		public Func<ExceptionEntry, Task> Exception = null;

		public Func<SocketGuild, Task> JoinedGuild = null;
		public Func<SocketGuild, Task> LeftGuild = null;
		public Func<SocketGuild, Task> GuildAvailable = null;
		public Func<SocketGuild, Task> GuildUnavailable = null;
		public Func<SocketGuild, SocketGuild, Task> GuildUpdated = null;
		public Func<SocketGuild, Task> GuildMembersDownloaded = null;
		public Func<SocketGuildUser, SocketGuildUser, Task> GuildMemberUpdated = null;

		public Func<SocketRole, Task> RoleCreated = null;
		public Func<SocketRole, SocketRole, Task> RoleUpdated = null;
		public Func<SocketRole, Task> RoleDeleted = null;

		public Func<SocketChannel, Task> ChannelCreated = null;
		public Func<SocketChannel, SocketChannel, Task> ChannelUpdated = null;
		public Func<SocketChannel, Task> ChannelDestroyed = null;

		public Func<SocketMessage, Task> MessageReceived = null;
		/// <summary> Expects true to cancel the execution of other message events. </summary>
		public Func<SocketMessage, Task<bool>> PriorityMessageReceived = null;
		public Func<IMessage, SocketMessage, ISocketMessageChannel, Task> MessageUpdated = null;
		public Func<IMessage, IMessageChannel, Task> MessageDeleted = null;

		public Func<IUserMessage, IMessageChannel, SocketReaction, Task> ReactionAdded = null;
		public Func<IUserMessage, IMessageChannel, SocketReaction, Task> ReactionRemoved = null;
		public Func<IUserMessage, IMessageChannel, Task> ReactionsCleared = null;

		public Func<SocketGuildUser, Task> UserJoined = null;
		public Func<SocketGuild, SocketUser, Task> UserLeft = null;
		public Func<IUser, IMessageChannel, Task> UserTyping = null;
		public Func<SocketUser, SocketUser, Task> UserUpdated = null;
		public Func<SocketUser, SocketVoiceState, SocketVoiceState, Task> UserVoiceStateUpdated = null;
		public Func<SocketUser, SocketGuild, Task> UserBanned = null;
		public Func<SocketUser, SocketGuild, Task> UserUnbanned = null;

		/// <summary> An event used to pass a ban instruction to the responsible module.
		/// This will merely record the ban into the database, without actually banning them. <br />
		/// <see cref="T:guid" />: Server on which to ban. <br />
		/// <see cref="T:guid" />: User to be banned. <br />
		/// <see cref="T:TimeSpan" />: Duration of the ban. <br />
		/// <see cref="T:string" />: Reason for the ban. </summary>
		public Func<guid, guid, TimeSpan, string, Task> AddBan = null;

		/// <summary> An event used to pass a ban instruction to the responsible module. <br />
		/// <see cref="T:Server" />: Server on which to ban. <br />
		/// <see cref="T:UserData" />: Users to be banned. <br />
		/// <see cref="T:TimeSpan" />: Duration of the ban. <br />
		/// <see cref="T:string" />: Reason for the ban. <br />
		/// <see cref="T:SocketGuildUser" />: Who issued the ban. <br />
		/// <see cref="T:bool" />: Silent? True to not PM the users. <br />
		/// <see cref="T:bool" />: True to prune recent messages. </summary>
		public Func<Server, UserData, TimeSpan, string, SocketGuildUser, bool, bool, Task> BanUser = null;

		/// <summary> An event used to pass a ban instruction to the responsible module. <br />
		/// <see cref="T:Server" />: Server on which to ban. <br />
		/// <see cref="T:List{UserData}" />: Users to be banned. <br />
		/// <see cref="T:TimeSpan" />: Duration of the ban. <br />
		/// <see cref="T:string" />: Reason for the ban. <br />
		/// <see cref="T:SocketGuildUser" />: Who issued the ban. <br />
		/// <see cref="T:bool" />: Silent? True to not PM the users. <br />
		/// <see cref="T:bool" />: True to prune recent messages. <br />
		/// <see cref="T:bool" />: True for redacted user ID and discrim. </summary>
		public Func<Server, List<UserData>, TimeSpan, string, SocketGuildUser, bool, bool, bool, Task> BanUsers = null;

		/// <summary> An event used to pass an unban instruction to the responsible module. <br />
		/// <see cref="T:Server" />: Server on which to unban. <br />
		/// <see cref="T:List{UserData}" />: Users to be unbanned. <br />
		/// <see cref="T:SocketGuildUser" />: Who issued the unban. </summary>
		public Func<Server, List<UserData>, SocketGuildUser, Task> UnBanUsers = null;

		/// <summary> An event used to pass a mute instruction to the responsible module. <br />
		/// <see cref="T:Server" />: Server on which to mute. <br />
		/// <see cref="T:List{UserData}" />: User to be muted. <br />
		/// <see cref="T:TimeSpan" />: Duration of the mute. <br />
		/// <see cref="T:IRole" />: MutedRole. <br />
		/// <see cref="T:SocketGuildUser" />: Who issued the mute. <br />
		/// <see cref="T:string" />: Reason for the mute. </summary>
		public Func<Server, UserData, TimeSpan, IRole, SocketGuildUser, string, Task> MuteUser = null;

		/// <summary> An event used to pass a mute instruction to the responsible module. <br />
		/// <see cref="T:Server" />: Server on which to mute. <br />
		/// <see cref="T:List{UserData}" />: Users to be muted. <br />
		/// <see cref="T:TimeSpan" />: Duration of the mute. <br />
		/// <see cref="T:IRole" />: MutedRole. <br />
		/// <see cref="T:SocketGuildUser" />: Who issued the mute. <br />
		/// <see cref="T:string" />: Reason for the mute. </summary>
		public Func<Server, List<UserData>, TimeSpan, IRole, SocketGuildUser, string, Task> MuteUsers = null;

		/// <summary> An event used to pass a mute instruction to the responsible module. <br />
		/// <see cref="T:Server" />: Server on which to mute. <br />
		/// <see cref="T:List{UserData}" />: Users to be muted. <br />
		/// <see cref="T:IRole" />: MutedRole. <br />
		/// <see cref="T:SocketGuildUser" />: Who issued the unmute. </summary>
		public Func<Server, List<UserData>, IRole, SocketGuildUser, Task> UnMuteUsers = null;


		/// <summary> An event used to pass a logMessage instruction to the responsible module. <br />
		/// <see cref="T:Server" />: Server on which to log. <br />
		/// <see cref="T:string" />: User name(s) <br />
		/// <see cref="T:guid" />: User id(s) <br />
		/// <see cref="T:string" />: Warning message. <br />
		/// <see cref="T:SocketGuildUser" />: User who issued the kick. </summary>
		public Func<Server, List<string>, List<guid>, string, SocketGuildUser, Task> LogWarning = null;

		/// <summary> An event used to pass a logMessage instruction to the responsible module. <br />
		/// <see cref="T:Server" />: Server on which to log. <br />
		/// <see cref="T:string" />: Banned user name. <br />
		/// <see cref="T:guid" />: Banned user id. <br />
		/// <see cref="T:string" />: Ban reason. <br />
		/// <see cref="T:string" />: Ban duration. <br />
		/// <see cref="T:SocketGuildUser" />: User who issued the ban. </summary>
		public Func<Server, string, guid, string, string, SocketGuildUser, Task> LogBan = null;

		/// <summary> An event used to pass a logMessage instruction to the responsible module. <br />
		/// <see cref="T:Server" />: Server on which to log. <br />
		/// <see cref="T:string" />: Banned user name. <br />
		/// <see cref="T:guid" />: Banned user id. <br />
		/// <see cref="T:SocketGuildUser" />: User who issued the unban. </summary>
		public Func<Server, string, guid, SocketGuildUser, Task> LogUnban = null;

		/// <summary> An event used to pass a logMessage instruction to the responsible module. <br />
		/// <see cref="T:Server" />: Server on which to log. <br />
		/// <see cref="T:string" />: Kicked user name. <br />
		/// <see cref="T:guid" />: Kicked user id. <br />
		/// <see cref="T:string" />: Kick reason. <br />
		/// <see cref="T:SocketGuildUser" />: User who issued the kick. </summary>
		public Func<Server, string, guid, string, SocketGuildUser, Task> LogKick = null;

		/// <summary> An event used to pass a logMessage instruction to the responsible module. <br />
		/// <see cref="T:Server" />: Server on which to log. <br />
		/// <see cref="T:IGuildUser" />: Muted user. <br />
		/// <see cref="T:string" />: Mute duration. <br />
		/// <see cref="T:SocketGuildUser" />: Who issued the mute. <br />
		/// <see cref="T:string" />: Reason for the mute. </summary>
		public Func<Server, IGuildUser, string, SocketGuildUser, string, Task> LogMute = null;

		/// <summary> An event used to pass a logMessage instruction to the responsible module. <br />
		/// <see cref="T:Server" />: Server on which to log. <br />
		/// <see cref="T:IGuildUser" />: Muted user. <br />
		/// <see cref="T:SocketGuildUser" />: User who issued the unmute. </summary>
		public Func<Server, IGuildUser, SocketGuildUser, Task> LogUnmute = null;

		/// <summary> An event used to pass a logMessage instruction to the responsible module. <br />
		/// <see cref="T:Server" />: Server on which to log. <br />
		/// <see cref="T:SocketGuildChannel" />: Muted channel. <br />
		/// <see cref="T:string" />: Mute duration. <br />
		/// <see cref="T:SocketGuildUser" />: User who issued the mute. </summary>
		public Func<Server, SocketGuildChannel, string, SocketGuildUser, Task> LogMutedChannel = null;

		/// <summary> An event used to pass a logMessage instruction to the responsible module. <br />
		/// <see cref="T:Server" />: Server on which to log. <br />
		/// <see cref="T:SocketGuildChannel" />: Muted channel. <br />
		/// <see cref="T:SocketGuildUser" />: User who issued the unmute. </summary>
		public Func<Server, SocketGuildChannel, SocketUser, Task> LogUnmutedChannel = null;

		/// <summary> An event used to pass a logMessage instruction to the responsible module. <br />
		/// <see cref="T:Server" />: Server on which to log. <br />
		/// <see cref="T:IGuildUser" />: User who was promoted. <br />
		/// <see cref="T:string" />: Name of the role. <br />
		/// <see cref="T:SocketGuildUser" />: User who issued the promote. </summary>
		public Func<Server, IGuildUser, string, SocketGuildUser, Task> LogPromote = null;

		/// <summary> An event used to pass a logMessage instruction to the responsible module. <br />
		/// <see cref="T:Server" />: Server on which to log. <br />
		/// <see cref="T:IGuildUser" />: User who was demoted. <br />
		/// <see cref="T:string" />: Name of the role. <br />
		/// <see cref="T:SocketGuildUser" />: User who issued the demote. </summary>
		public Func<Server, IGuildUser, string, SocketGuildUser, Task> LogDemote = null;

		/// <summary> An event used to pass a logMessage instruction to the responsible module. <br />
		/// <see cref="T:Server" />: Server on which to log. <br />
		/// <see cref="T:SocketGuildUser" />: User who joined a role. <br />
		/// <see cref="T:string" />: Name of the role. </summary>
		public Func<Server, SocketGuildUser, string, Task> LogPublicRoleJoin = null;

		/// <summary> An event used to pass a logMessage instruction to the responsible module. <br />
		/// <see cref="T:Server" />: Server on which to log. <br />
		/// <see cref="T:SocketGuildUser" />: User who left a role. <br />
		/// <see cref="T:string" />: Name of the role. </summary>
		public Func<Server, SocketGuildUser, string, Task> LogPublicRoleLeave = null;

		public Events(DiscordSocketClient discordClient, ValkyrjaClient valkyrjaClient)
		{
			this.Client = valkyrjaClient;

			discordClient.Log += OnLogEntryAdded;

			discordClient.JoinedGuild += OnGuildJoined;
			discordClient.LeftGuild += OnGuildLeft;
			discordClient.GuildAvailable += OnGuildAvailable;
			discordClient.GuildUnavailable += OnGuildUnavailable;
			discordClient.GuildUpdated += OnGuildUpdated;
			discordClient.GuildMembersDownloaded += OnGuildMembersDownloaded;
			discordClient.GuildMemberUpdated += OnGuildMemberUpdated;

			discordClient.RoleCreated += OnRoleCreated;
			discordClient.RoleUpdated += OnRoleUpdated;
			discordClient.RoleDeleted += OnRoleDeleted;

			discordClient.ChannelCreated += OnChannelCreated;
			discordClient.ChannelUpdated += OnChannelUpdated;
			discordClient.ChannelDestroyed += OnChannelDestroyed;

			discordClient.MessageReceived += OnMessageReceived;
			discordClient.MessageUpdated += OnMessageUpdated;
			discordClient.MessageDeleted += OnMessageDeleted;

			discordClient.ReactionAdded += OnReactionAdded;
			discordClient.ReactionRemoved += OnReactionRemoved;
			discordClient.ReactionsCleared += OnReactionsCleared;

			discordClient.UserJoined += OnUserJoined;
			discordClient.UserLeft += OnUserLeft;
			discordClient.UserIsTyping += OnUserTyping;
			discordClient.UserUpdated += OnUserUpdated;
			discordClient.UserVoiceStateUpdated += OnUserVoiceStateUpdated;
			discordClient.UserBanned += OnUserBanned;
			discordClient.UserUnbanned += OnUserUnbanned;
		}

		private Task OnLogEntryAdded(LogMessage logMessage)
		{
			if( logMessage.Exception != null )
			{
				if( this.Exception != null && logMessage.Exception.Message != "Server requested a reconnect" &&
				    logMessage.Exception.Message != "Server missed last heartbeat" &&
				    logMessage.Exception.Message != "WebSocket connection was closed" )
				{
					ExceptionEntry exceptionEntry = new ExceptionEntry();
					exceptionEntry.Message = logMessage.Exception.Message;
					exceptionEntry.Stack = logMessage.Exception.StackTrace;
					exceptionEntry.Data = "D.NET Message: " + logMessage.Message + "\n--Source: " + logMessage.Source;

					Task.Run(async () => await this.Exception(exceptionEntry));
				}

				return Task.CompletedTask;
			}

			if( this.LogEntryAdded == null )
				return Task.CompletedTask;

			LogEntry logEntry = new LogEntry();
			logEntry.Type = LogType.Debug;
			logEntry.Message = "D.NET Message: " + logMessage.Message + "\n--Source: " + logMessage.Source;
			Task.Run(async () => await this.LogEntryAdded(logEntry));
			return Task.CompletedTask;
		}

//Guild events
		private Task OnGuildJoined(SocketGuild guild)
		{
			if( this.JoinedGuild == null )
				return Task.CompletedTask;

			Task.Run(async () => await this.JoinedGuild(guild));
			return Task.CompletedTask;
		}

		private Task OnGuildLeft(SocketGuild guild)
		{
			if( this.LeftGuild == null )
				return Task.CompletedTask;

			Task.Run(async () => await this.LeftGuild(guild));
			return Task.CompletedTask;
		}

		private Task OnGuildAvailable(SocketGuild guild)
		{
			if( this.GuildAvailable == null )
				return Task.CompletedTask;

			Task.Run(async () => await this.GuildAvailable(guild));
			return Task.CompletedTask;
		}

		private Task OnGuildUnavailable(SocketGuild guild)
		{
			if( this.GuildUnavailable == null )
				return Task.CompletedTask;

			Task.Run(async () => await this.GuildUnavailable(guild));
			return Task.CompletedTask;
		}

		private Task OnGuildUpdated(SocketGuild originalGuild, SocketGuild updatedGuild)
		{
			if( this.GuildUpdated == null )
				return Task.CompletedTask;

			Task.Run(async () => await this.GuildUpdated(originalGuild, updatedGuild));
			return Task.CompletedTask;
		}

		private Task OnGuildMembersDownloaded(SocketGuild guild)
		{
			if( this.GuildMembersDownloaded == null )
				return Task.CompletedTask;

			Task.Run(async () => await this.GuildMembersDownloaded(guild));
			return Task.CompletedTask;
		}

//Role events
		private Task OnRoleCreated(SocketRole role)
		{
			if( this.RoleCreated == null )
				return Task.CompletedTask;

			Task.Run(async () => await this.RoleCreated(role));
			return Task.CompletedTask;
		}

		private Task OnRoleUpdated(SocketRole originalRole, SocketRole updatedRole)
		{
			if( this.RoleUpdated == null )
				return Task.CompletedTask;

			Task.Run(async () => await this.RoleUpdated(originalRole, updatedRole));
			return Task.CompletedTask;
		}

		private Task OnRoleDeleted(SocketRole role)
		{
			if( this.RoleDeleted == null )
				return Task.CompletedTask;

			Task.Run(async () => await this.RoleDeleted(role));
			return Task.CompletedTask;
		}

//Channel events
		private Task OnChannelCreated(SocketChannel channel)
		{
			if( this.ChannelCreated == null )
				return Task.CompletedTask;

			Task.Run(async () => await this.ChannelCreated(channel));
			return Task.CompletedTask;
		}

		private Task OnChannelUpdated(SocketChannel originalChannel, SocketChannel updatedChannel)
		{
			if( this.ChannelUpdated == null )
				return Task.CompletedTask;

			Task.Run(async () => await this.ChannelUpdated(originalChannel, updatedChannel));
			return Task.CompletedTask;
		}

		private Task OnChannelDestroyed(SocketChannel channel)
		{
			if( this.ChannelDestroyed == null )
				return Task.CompletedTask;

			Task.Run(async () => await this.ChannelDestroyed(channel));
			return Task.CompletedTask;
		}

//Message events
		private Task OnMessageReceived(SocketMessage message)
		{
			DateTime startTime = DateTime.UtcNow;
			if( this.Client.GlobalConfig.LogDebug )
				Console.WriteLine($"ValkyrjaClient: [EVENT] MessageReceived: {message.Id}");

			this.Client.Monitoring.Messages.Inc();
			if( this.Client.GlobalConfig.LogDebug )
				Console.WriteLine($"ValkyrjaClient: [EVENT] MessageReceived: {message.Id} | Monitoring took {(DateTime.UtcNow - startTime).Milliseconds}ms");

			if( this.PriorityMessageReceived != null && this.PriorityMessageReceived(message).GetAwaiter().GetResult() )
				return Task.CompletedTask;
			if( this.Client.GlobalConfig.LogDebug )
				Console.WriteLine($"ValkyrjaClient: [EVENT] MessageReceived: {message.Id} | Priority took {(DateTime.UtcNow - startTime).Milliseconds}ms");

			if( this.MessageReceived != null )
				Task.Run(async () => await TriggerMessageReceivedEvent(message));
			return Task.CompletedTask;
		}

		private async Task TriggerMessageReceivedEvent(SocketMessage message)
		{
			try
			{
				await this.MessageReceived(message);
			}
			catch(Exception exception)
			{
				await this.Client.LogException(exception, $"--MessageId: {message.Id}\n--ChannelId: {message.Channel.Id}\n--Content: {message.Content}");
			}
		}

		private Task OnMessageUpdated(Cacheable<IMessage, ulong> originalMessage, SocketMessage updatedMessage, ISocketMessageChannel channel)
		{
			if( this.MessageUpdated == null )
				return Task.CompletedTask;

			this.Client.Monitoring.Messages.Inc();

			IMessage msg = null;
			if( channel is SocketGuildChannel guildChannel && guildChannel.Guild.CurrentUser.GuildPermissions.ReadMessageHistory )
				msg = originalMessage.GetOrDownloadAsync().GetAwaiter().GetResult();

			Task.Run(async () => await this.MessageUpdated(msg, updatedMessage, channel));
			return Task.CompletedTask;
		}


		private Task OnMessageDeleted(Cacheable<IMessage, ulong> originalMessage, Cacheable<IMessageChannel, ulong> channel)
		{
			if( this.MessageDeleted == null )
				return Task.CompletedTask;

			IMessage msg = null;
			SocketGuildChannel c = channel.Value as SocketGuildChannel;
			if( c == null || !this.Client.Servers.ContainsKey(c.Guild.Id) )
				return Task.CompletedTask;

			try
			{
				msg = originalMessage.HasValue ? originalMessage.Value : null;
			}
			catch( HttpException e )
			{
				this.Client.Servers[c.Guild.Id].HandleHttpException(e, $"I couldn't read messages in <#{c.Id}>, please ensure that I have `ReadMessageHistory`!").GetAwaiter().GetResult();
			}
			catch( Exception e )
			{
				this.Client.LogException(e, "Event Exception", c.Guild.Id).GetAwaiter().GetResult();
			}

			if( msg != null )
				Task.Run(async () => await this.MessageDeleted(msg, channel.Value));
			return Task.CompletedTask;
		}

//Reaction events
		private Task OnReactionAdded(Cacheable<IUserMessage, ulong> originalMessage, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction)
		{
			if( this.ReactionAdded == null )
				return Task.CompletedTask;

			IUserMessage msg = null;
			SocketGuildChannel c = channel.Value as SocketGuildChannel;
			if( c == null || !this.Client.Servers.ContainsKey(c.Guild.Id) )
				return Task.CompletedTask;

			try
			{
				msg = originalMessage.GetOrDownloadAsync().GetAwaiter().GetResult();
			}
			catch( HttpException e )
			{
				this.Client.Servers[c.Guild.Id].HandleHttpException(e, $"I couldn't read messages in <#{c.Id}>, please ensure that I have `ReadMessageHistory`!").GetAwaiter().GetResult();
			}
			catch( Exception e )
			{
				this.Client.LogException(e, "Event Exception", c.Guild.Id).GetAwaiter().GetResult();
			}

			if( msg != null )
				Task.Run(async () => await this.ReactionAdded(msg, channel.Value, reaction));
			return Task.CompletedTask;
		}

		private Task OnReactionRemoved(Cacheable<IUserMessage, ulong> originalMessage, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction)
		{
			if( this.ReactionRemoved == null )
				return Task.CompletedTask;

			IUserMessage msg = null;
			SocketGuildChannel c = channel.Value as SocketGuildChannel;
			if( c == null || !this.Client.Servers.ContainsKey(c.Guild.Id) )
				return Task.CompletedTask;

			try
			{
				msg = originalMessage.GetOrDownloadAsync().GetAwaiter().GetResult();
			}
			catch( HttpException e )
			{
				this.Client.Servers[c.Guild.Id].HandleHttpException(e, $"I couldn't read messages in <#{c.Id}>, please ensure that I have `ReadMessageHistory`!").GetAwaiter().GetResult();
			}
			catch( Exception e )
			{
				this.Client.LogException(e, "Event Exception", c.Guild.Id).GetAwaiter().GetResult();
			}

			if( msg != null )
				Task.Run(async () => await this.ReactionRemoved(msg, channel.Value, reaction));
			return Task.CompletedTask;
		}

		private Task OnReactionsCleared(Cacheable<IUserMessage, ulong> originalMessage, Cacheable<IMessageChannel, ulong> channel)
		{
			if( this.ReactionsCleared == null )
				return Task.CompletedTask;

			IUserMessage msg = null;
			SocketGuildChannel c = channel.Value as SocketGuildChannel;
			if( c == null || !this.Client.Servers.ContainsKey(c.Guild.Id) )
				return Task.CompletedTask;

			try
			{
				msg = originalMessage.GetOrDownloadAsync().GetAwaiter().GetResult();
			}
			catch( HttpException e )
			{
				this.Client.Servers[c.Guild.Id].HandleHttpException(e, $"I couldn't read messages in <#{c.Id}>, please ensure that I have `ReadMessageHistory`!").GetAwaiter().GetResult();
			}
			catch( Exception e )
			{
				this.Client.LogException(e, "Event Exception", c.Guild.Id).GetAwaiter().GetResult();
			}

			if( msg != null )
				Task.Run(async () => await this.ReactionsCleared(msg, channel.Value));
			return Task.CompletedTask;
		}

//User events
		private Task OnUserJoined(SocketGuildUser user)
		{
			if( this.UserJoined == null )
				return Task.CompletedTask;

			Task.Run(async () => await this.UserJoined(user));
			return Task.CompletedTask;
		}

		private Task OnUserLeft(SocketGuild guild, SocketUser user)
		{
			if( this.UserLeft == null )
				return Task.CompletedTask;

			Task.Run(async () => await this.UserLeft(guild, user));
			return Task.CompletedTask;
		}

		private Task OnUserTyping(Cacheable<IUser, ulong> user, Cacheable<IMessageChannel, ulong> channel)
		{
			if( this.UserTyping == null || !user.HasValue || !channel.HasValue )
				return Task.CompletedTask;

			Task.Run(async () => await this.UserTyping(user.Value, channel.Value));
			return Task.CompletedTask;
		}

		private Task OnUserUpdated(SocketUser originalUser, SocketUser updatedUser)
		{
			if( this.UserUpdated == null )
				return Task.CompletedTask;

			Task.Run(async () => await this.UserUpdated(originalUser, updatedUser));
			return Task.CompletedTask;
		}

		private Task OnGuildMemberUpdated(Cacheable<SocketGuildUser, ulong> originalUser, SocketGuildUser updatedUser)
		{
			if( this.GuildMemberUpdated == null || !originalUser.HasValue )
				return Task.CompletedTask;

			Task.Run(async () => await this.GuildMemberUpdated(originalUser.Value, updatedUser));
			return Task.CompletedTask;
		}

		private Task OnUserVoiceStateUpdated(SocketUser user, SocketVoiceState originalState, SocketVoiceState updatedState)
		{
			if( this.UserVoiceStateUpdated == null )
				return Task.CompletedTask;

			Task.Run(async () => await this.UserVoiceStateUpdated(user, originalState, updatedState));
			return Task.CompletedTask;
		}

		private Task OnUserBanned(SocketUser user, SocketGuild guild)
		{
			if( this.UserBanned == null )
				return Task.CompletedTask;

			Task.Run(async () => await this.UserBanned(user, guild));
			return Task.CompletedTask;
		}

		private Task OnUserUnbanned(SocketUser user, SocketGuild guild)
		{
			if( this.UserUnbanned == null )
				return Task.CompletedTask;

			Task.Run(async () => await this.UserUnbanned(user, guild));
			return Task.CompletedTask;
		}

	}
}

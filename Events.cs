using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace Botwinder.entities
{
	public class Events
	{
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
		/// <summary> Exception was added. Don't call this event directly, call BotwinderClient.LogException </summary>
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
		public Func<SocketMessage, SocketMessage, ISocketMessageChannel, Task> MessageUpdated = null;
		public Func<SocketMessage, ISocketMessageChannel, Task> MessageDeleted = null;

		public Func<IUserMessage, ISocketMessageChannel, SocketReaction, Task> ReactionAdded = null;
		public Func<IUserMessage, ISocketMessageChannel, SocketReaction, Task> ReactionRemoved = null;
		public Func<IUserMessage, ISocketMessageChannel, Task> ReactionsCleared = null;

		public Func<SocketGuildUser, Task> UserJoined = null;
		public Func<SocketGuildUser, Task> UserLeft = null;
		public Func<SocketUser, ISocketMessageChannel, Task> UserTyping = null;
		public Func<SocketUser, SocketUser, Task> UserUpdated = null;
		public Func<SocketUser, SocketVoiceState, SocketVoiceState, Task> UserVoiceStateUpdated = null;
		public Func<SocketUser, SocketGuild, Task> UserBanned = null;
		public Func<SocketUser, SocketGuild, Task> UserUnbanned = null;


		public Events(DiscordSocketClient discordClient)
		{
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
				ExceptionEntry exceptionEntry = new ExceptionEntry();
				exceptionEntry.Message = logMessage.Exception.Message;
				exceptionEntry.Stack = logMessage.Exception.StackTrace;
				exceptionEntry.Data = "D.NET Message: " + logMessage.Message + "\n--Source: " + logMessage.Source;

				if( this.Exception != null )
				{
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
		private Task OnRoleCreated(SocketRole arg)
		{
			throw new NotImplementedException();
		}

		private Task OnRoleUpdated(SocketRole arg1, SocketRole arg2)
		{
			throw new NotImplementedException();
		}

		private Task OnRoleDeleted(SocketRole arg)
		{
			throw new NotImplementedException();
		}

//Channel events
		private Task OnChannelCreated(SocketChannel arg)
		{
			throw new NotImplementedException();
		}

		private Task OnChannelUpdated(SocketChannel arg1, SocketChannel arg2)
		{
			throw new NotImplementedException();
		}

		private Task OnChannelDestroyed(SocketChannel arg)
		{
			throw new NotImplementedException();
		}

//Message events
		private Task OnMessageReceived(SocketMessage message)
		{
			if( this.MessageReceived == null )
				return Task.CompletedTask;

			Task.Run(async () => await this.MessageReceived(message));
			return Task.CompletedTask;
		}

		private Task OnMessageUpdated(Cacheable<IMessage, ulong> originalMessage, SocketMessage updatedMessage, ISocketMessageChannel channel)
		{
			if( this.MessageUpdated == null )
				return Task.CompletedTask;

			IMessage msg = originalMessage.GetOrDownloadAsync().GetAwaiter().GetResult();
			Task.Run(async () => await this.MessageUpdated(msg as SocketMessage, updatedMessage, channel));
			return Task.CompletedTask;
		}

		private Task OnMessageDeleted(Cacheable<IMessage, ulong> originalMessage, ISocketMessageChannel channel)
		{
			if( this.MessageDeleted == null )
				return Task.CompletedTask;

			IMessage msg = originalMessage.GetOrDownloadAsync().GetAwaiter().GetResult();
			Task.Run(async () => await this.MessageDeleted(msg as SocketMessage, channel));
			return Task.CompletedTask;
		}

//Reaction events
		private Task OnReactionAdded(Cacheable<IUserMessage, ulong> originalMessage, ISocketMessageChannel channel, SocketReaction reaction)
		{
			if( this.ReactionAdded == null )
				return Task.CompletedTask;

			IUserMessage msg = originalMessage.GetOrDownloadAsync().GetAwaiter().GetResult();
			Task.Run(async () => await this.ReactionAdded(msg, channel, reaction));
			return Task.CompletedTask;
		}

		private Task OnReactionRemoved(Cacheable<IUserMessage, ulong> originalMessage, ISocketMessageChannel channel, SocketReaction reaction)
		{
			if( this.ReactionRemoved == null )
				return Task.CompletedTask;

			IUserMessage msg = originalMessage.GetOrDownloadAsync().GetAwaiter().GetResult();
			Task.Run(async () => await this.ReactionRemoved(msg, channel, reaction));
			return Task.CompletedTask;
		}

		private Task OnReactionsCleared(Cacheable<IUserMessage, ulong> originalMessage, ISocketMessageChannel channel)
		{
			if( this.ReactionsCleared == null )
				return Task.CompletedTask;

			IUserMessage msg = originalMessage.GetOrDownloadAsync().GetAwaiter().GetResult();
			Task.Run(async () => await this.ReactionsCleared(msg, channel));
			return Task.CompletedTask;
		}

//User events
		private Task OnUserJoined(SocketGuildUser arg)
		{
			throw new NotImplementedException();
		}

		private Task OnUserLeft(SocketGuildUser arg)
		{
			throw new NotImplementedException();
		}

		private Task OnUserTyping(SocketUser arg1, ISocketMessageChannel arg2)
		{
			throw new NotImplementedException();
		}

		private Task OnUserUpdated(SocketUser arg1, SocketUser arg2)
		{
			throw new NotImplementedException();
		}

		private Task OnGuildMemberUpdated(SocketGuildUser originalUser, SocketGuildUser updatedUser)
		{
			if( this.GuildMemberUpdated == null )
				return Task.CompletedTask;

			Task.Run(async () => await this.GuildMemberUpdated(originalUser, updatedUser));
			return Task.CompletedTask;
		}

		private Task OnUserVoiceStateUpdated(SocketUser arg1, SocketVoiceState arg2, SocketVoiceState arg3)
		{
			throw new NotImplementedException();
		}

		private Task OnUserBanned(SocketUser arg1, SocketGuild arg2)
		{
			throw new NotImplementedException();
		}

		private Task OnUserUnbanned(SocketUser arg1, SocketGuild arg2)
		{
			throw new NotImplementedException();
		}

	}
}

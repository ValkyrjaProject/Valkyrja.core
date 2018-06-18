using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Diagnostics;
using Botwinder.entities;
using Discord.Net;
using Discord.WebSocket;
using guid = System.UInt64;

namespace Botwinder.core
{
	public partial class BotwinderClient : IBotwinderClient, IDisposable
	{
		public async Task SendRawMessageToChannel(SocketTextChannel channel, string message)
		{
			await LogMessage(LogType.Response, channel, this.GlobalConfig.UserId, message);
			await channel.SendMessageSafe(message);
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsGlobalAdmin(guid id)
		{
			return this.GlobalConfig.AdminUserId == id;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsSupportTeam(guid id)
		{
			return this.SupportTeam.Contains(id);
		}


		public bool IsSubscriber(guid id)
		{
			GlobalContext dbContext = GlobalContext.Create(this.DbConnectionString);
			bool val = IsGlobalAdmin(id) || dbContext.Subscribers.Any(u => u.UserId == id);
			dbContext.Dispose();
			return val;
		}

		public bool IsPartner(guid id)
		{
			GlobalContext dbContext = GlobalContext.Create(this.DbConnectionString);
			bool val = dbContext.PartneredServers.Any(s => s.ServerId == id);
			dbContext.Dispose();
			return val;
		}

		public bool IsPremiumSubscriber(guid id)
		{
			GlobalContext dbContext = GlobalContext.Create(this.DbConnectionString);
			bool val = IsGlobalAdmin(id) || dbContext.Subscribers.Any(u => u.UserId == id && u.IsPremium);
			dbContext.Dispose();
			return val;
		}

		public bool IsBonusSubscriber(guid id)
		{
			GlobalContext dbContext = GlobalContext.Create(this.DbConnectionString);
			bool val = IsGlobalAdmin(id) || dbContext.Subscribers.Any(u => u.UserId == id && u.HasBonus);
			dbContext.Dispose();
			return val;
		}

		public bool IsPremiumPartner(guid id)
		{
			GlobalContext dbContext = GlobalContext.Create(this.DbConnectionString);
			bool val = dbContext.PartneredServers.Any(s => s.ServerId == id && s.IsPremium);
			dbContext.Dispose();
			return val;
		}


		public async Task LogMessage(LogType logType, SocketTextChannel channel, guid authorId, string message)
		{
			LogEntry logEntry = new LogEntry(){
				Type = logType,
				UserId = authorId,
				ChannelId = channel.Id,
				ServerId = channel.Guild.Id,
				DateTime = DateTime.UtcNow,
				Message = message
			};
			await this.Events.LogEntryAdded(logEntry);
		}

		public async Task LogMessage(LogType logType, SocketTextChannel channel, SocketMessage message)
		{
			LogEntry logEntry = new LogEntry(){
				Type = logType,
				UserId = message.Author.Id,
				MessageId = message.Id,
				ChannelId = channel?.Id ?? 0,
				ServerId = channel?.Guild.Id ?? 0,
				DateTime = DateTime.UtcNow,
				Message = message.Content
			};
			await this.Events.LogEntryAdded(logEntry);
		}

		public async Task LogException(Exception exception, CommandArguments args) =>
			await LogException(exception, "--Command: "+ args.Command.Id + " | Parameters: " + args.TrimmedMessage, args.Server.Id);

		public async Task LogException(Exception exception, string data, guid serverId = 0)
		{
			if( exception is RateLimitedException )
				return;

			Console.WriteLine(exception.Message);
			Console.WriteLine(exception.StackTrace);
			Console.WriteLine($"{data} | ServerId:{serverId}");

			ExceptionEntry exceptionEntry = new ExceptionEntry(){
				Type = exception.GetType().ToString(),
				Message = exception.Message,
				Stack = exception.StackTrace,
				Data = data,
				DateTime = DateTime.UtcNow,
				ServerId = serverId,
				ShardId = this.CurrentShard?.Id ?? 0
			};
			await this.Events.Exception(exceptionEntry);

			if( exception.InnerException != null && exception.Message != exception.InnerException.Message )
				await LogException(exception.InnerException, "InnerException | " + data, serverId);
		}

		public async Task LogMaintenanceAndExit()
		{
			if( this.CurrentOperations.Any() )
				return;

			try
			{
				SocketTextChannel channel = null;
				SocketGuild server = null;
				if( (server = this.DiscordClient.GetGuild(this.GlobalConfig.MainServerId)) != null && (channel = server.GetTextChannel(this.GlobalConfig.MainChannelId)) != null )
				{
					TimeSpan uptime = DateTimeOffset.UtcNow - this.TimeStarted;
					int days = uptime.Days;
					int hours = uptime.Hours;
					int minutes = uptime.Minutes;
					int seconds = uptime.Seconds;

					string response = string.Format("__**Performing automated maintenance**__\n\n" +
					                                "Time: `{0}`\n" +
					                                "Uptime: `{1}`\n" +
					                                "Disconnects: `{2:0}`\n" +
					                                "Active Threads: `{3}`\n" +
					                                "Memory usage: `{4:#0.00} MB`\n" +
					                                "Operations ran: `{5}`\n" +
					                                "Messages total: `{6}`\n" +
					                                "Messages per minute: `{7}`\n",
						Utils.GetTimestamp(),
						(days == 0 ? "" : (days.ToString() + (days == 1 ? " day, " : " days, "))) +
						(hours == 0 ? "" : (hours.ToString() + (hours == 1 ? " hour, " : " hours, "))) +
						(minutes == 0 ? "" : (minutes.ToString() + (minutes == 1 ? " minute, " : " minutes "))) +
						((days == 0 && hours == 0 && minutes == 0 ? "" : "and ") + seconds.ToString() + (seconds == 1 ? " second." : " seconds.")),
						this.CurrentShard.Disconnects,
						Process.GetCurrentProcess().Threads.Count,
						(GC.GetTotalMemory(false) / 1000000f),
						this.CurrentShard.OperationsRan,
						this.CurrentShard.MessagesTotal,
						this.CurrentShard.MessagesPerMinute
					);

					await channel.SendMessageSafe(response);
				}
			} catch(Exception exception)
			{
				await LogException(exception, "--LogMaintenance");
			}

			await Task.Delay(500);
			Dispose();

			await Task.Delay(500);
			Environment.Exit(0);
		}

		public List<UserData> GetMentionedUsersData(ServerContext dbContext, CommandArguments e) //todo - Move this elsewhere...
		{
			List<guid> mentionedUserIds = GetMentionedUserIds(e);

			if( !mentionedUserIds.Any() )
				return new List<UserData>();

			List<UserData> found = dbContext.UserDatabase.Where(u => u.ServerId == e.Server.Id && mentionedUserIds.Contains(u.UserId)).ToList();
			if( found.Count < mentionedUserIds.Count )
			{
				for( int i = 0; i < mentionedUserIds.Count; i++ )
				{
					if(found.Any(u => u.UserId == mentionedUserIds[i]))
						continue;

					UserData newUserData = new UserData(){
						ServerId = e.Server.Id,
						UserId = mentionedUserIds[i]
					};

					dbContext.UserDatabase.Add(newUserData); //No need to save this here.
					found.Add(newUserData);
				}
			}
			return found;
		}

		public List<SocketGuildUser> GetMentionedGuildUsers(CommandArguments e) //todo - Move this elsewhere...
		{
			List<SocketGuildUser> mentionedUsers = new List<SocketGuildUser>();
			foreach( SocketUser user in GetMentionedUsers(e) )
			{
				if(user is SocketGuildUser guildUser)
					mentionedUsers.Add(guildUser);
			}

			return mentionedUsers;
		}
		public List<SocketUser> GetMentionedUsers(CommandArguments e) //todo - Move this elsewhere...
		{
			List<SocketUser> mentionedUsers = new List<SocketUser>();

			if( e.Message.MentionedUsers != null && e.Message.MentionedUsers.Any() )
			{
				mentionedUsers.AddRange(e.Message.MentionedUsers);
			}
			else if( e.MessageArgs != null && e.MessageArgs.Length > 0 )
			{
				for( int i = 0; i < e.MessageArgs.Length; i++)
				{
					guid id;
					SocketUser user;
					if( !guid.TryParse(e.MessageArgs[i], out id) || id == 0 || (user = e.Server.Guild.GetUser(id)) == null )
						break;
					if( mentionedUsers.Contains(user) )
					{
						List<string> newArgs = new List<string>(e.MessageArgs);
						newArgs.RemoveAt(i);
						e.MessageArgs = newArgs.ToArray();
						continue;
					}

					mentionedUsers.Add(user);
				}
			}

			return mentionedUsers;
		}

		public List<guid> GetMentionedUserIds(CommandArguments e) //todo - Move this elsewhere...
		{
			List<guid> mentionedIds = new List<guid>();

			if( e.Message.MentionedUsers != null && e.Message.MentionedUsers.Any() )
			{
				mentionedIds.AddRange(e.Message.MentionedUsers.Select(u => u.Id));
			}
			else if( e.MessageArgs != null && e.MessageArgs.Length > 0 )
			{
				for( int i = 0; i < e.MessageArgs.Length; i++)
				{
					guid id;
					if( !guid.TryParse(e.MessageArgs[i], out id) || id < int.MaxValue )
						break;
					if( mentionedIds.Contains(id) )
					{
						List<string> newArgs = new List<string>(e.MessageArgs);
						newArgs.RemoveAt(i--);
						e.MessageArgs = newArgs.ToArray();
						continue;
					}

					mentionedIds.Add(id);
				}
			}

			return mentionedIds;
		}
	}
}

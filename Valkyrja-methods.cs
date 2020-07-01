using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using Valkyrja.entities;
using Discord.Net;
using Discord.WebSocket;

using guid = System.UInt64;

namespace Valkyrja.core
{
	public partial class ValkyrjaClient : IValkyrjaClient, IDisposable
	{
		public async Task SendRawMessageToChannel(SocketTextChannel channel, string message)
		{
			//await LogMessage(LogType.Response, channel, this.GlobalConfig.UserId, message);
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
			return IsGlobalAdmin(id) || this.Subscribers.ContainsKey(id);
		}

		public bool IsPartner(guid id)
		{
			return this.PartneredServers.ContainsKey(id);
		}

		public bool IsPremiumSubscriber(guid id)
		{
			return IsGlobalAdmin(id) || (this.Subscribers.ContainsKey(id) && this.Subscribers[id].IsPremium);
		}

		public bool IsBonusSubscriber(guid id)
		{
			return IsGlobalAdmin(id) || (this.Subscribers.ContainsKey(id) && this.Subscribers[id].HasBonus);
		}

		public bool IsPremiumPartner(guid id)
		{
			return this.PartneredServers.ContainsKey(id) && this.PartneredServers[id].IsPremium;
		}

		public bool IsPremium(Server server)
		{
			return IsPremiumSubscriber(server.Guild.OwnerId) || IsPremiumPartner(server.Id);
		}

		public bool IsTrialServer(guid id)
		{
			ServerContext dbContext = ServerContext.Create(this.DbConnectionString);
			bool isTrial = dbContext.ServerStats.AsQueryable().Where(s => s.ServerId == id || s.OwnerId == id).AsEnumerable().Any(s => s.JoinedCount < this.GlobalConfig.VipTrialJoins && (!this.Servers.ContainsKey(id) || this.Servers[s.ServerId] == null || this.Servers[s.ServerId].Guild.CurrentUser == null || this.Servers[s.ServerId].Guild.CurrentUser.JoinedAt == null || DateTime.UtcNow - this.Servers[s.ServerId].Guild.CurrentUser.JoinedAt.Value.ToUniversalTime() < TimeSpan.FromHours(this.GlobalConfig.VipTrialHours)));
			dbContext.Dispose();
			return isTrial;
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
			if( (exception is HttpException httpException && (int)httpException.HttpCode >= 500) || data.Contains("Error handling Dispatch") )
			{
				this.Monitoring.Error500s.Inc();
			}

			if( (exception is WebSocketClosedException websocketException) )
			{
				data += $"\nCloseCode:{websocketException.CloseCode}\nReason:{websocketException.Reason}\nTarget:{websocketException.TargetSite}";
			}

			if( exception.Message == "Server requested a reconnect" ||
			    exception.Message == "Server missed last heartbeat" ||
			    exception.Message.Contains("Discord.PermissionTarget") ) //it's a spam
				return;

			Console.WriteLine(exception.Message);
			Console.WriteLine(exception.StackTrace);
			Console.WriteLine($"{data} | ServerId:{serverId}");

			if( exception is RateLimitedException || exception.Message.Contains("WebSocket connection was closed") ) //hack to not spam my logs
				return;

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

		public List<UserData> GetMentionedUsersData(ServerContext dbContext, CommandArguments e) //todo - Move this elsewhere...
		{
			List<guid> mentionedUserIds = GetMentionedUserIds(e);

			if( !mentionedUserIds.Any() )
				return new List<UserData>();

			List<UserData> found = dbContext.UserDatabase.AsQueryable().Where(u => u.ServerId == e.Server.Id).AsEnumerable().Where(u => mentionedUserIds.Contains(u.UserId)).ToList();
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
					dbContext.SaveChanges();
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

		public List<guid> GetMentionedUserIds(CommandArguments e, bool endOnFailure = true) //todo - Move this elsewhere...
		{
			List<guid> mentionedIds = new List<guid>();

			/*if( e.Message.MentionedUsers != null && e.Message.MentionedUsers.Any() )
			{
				mentionedIds.AddRange(e.Message.MentionedUsers.Select(u => u.Id));
			}
			else*/ if( e.MessageArgs != null && e.MessageArgs.Length > 0 )
			{
				for( int i = 0; i < e.MessageArgs.Length; i++)
				{
					guid id;
					if( !guid.TryParse(e.MessageArgs[i].Trim('<','@','!','>'), out id) || id < int.MaxValue )
						if( endOnFailure ) break;
						else continue;
					if( mentionedIds.Contains(id) )
					{
						//This code is necessary to be able to further parse arguments by some commands (e.g. ban reason)
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

		private string GetPatchnotes()
		{
			if( !Directory.Exists("updates") || !File.Exists(Path.Combine("updates", "changelog")) )
				return "This is not the original <https://valkyrja.app>, therefor I can not tell you, what's new here :<";

			string changelog = File.ReadAllText(Path.Combine("updates", "changelog"));
			int start = changelog.IndexOf("**Valkyrja");
			int valkEnd = changelog.Substring(start+1).IndexOf("**Valkyrja") + 1;
			int bwEnd = changelog.Substring(start+1).IndexOf("**Valkyrja") + 1;
			int end = valkEnd > start ? valkEnd : bwEnd;
			int hLength = valkEnd > start ? "**Valkyrja".Length : "**Valkyrja".Length;

			if( start >= 0 && end <= changelog.Length && end > start && (changelog = changelog.Substring(start, end-start+hLength)).Length > 0 )
				return changelog + "\n\nSee the full changelog and upcoming features at <https://valkyrja.app/updates>!";

			return "There is an error in the data so I have failed to retrieve the patchnotes. Sorry mastah!";
		}

		/// <summary>
		/// Returns:
		///  1 = success;
		///  0 = first 3 attempts failed;
		/// -1 = more than 3 attempts failed;
		/// -2 = failed due to Discord server issues;
		/// -3 = user not found;
		/// </summary>
		public async Task<int> SendPmSafe(SocketUser user, string message)
		{
			if( user == null )
				return -3;
			if( this.FailedPmCount.ContainsKey(user.Id) && this.FailedPmCount[user.Id] >= 3 )
				return -1;
			try
			{
				await user.SendMessageSafe(message);
				return 1;
			}
			catch( HttpException e ) when( (int)e.HttpCode == 403 || (e.DiscordCode.HasValue && e.DiscordCode == 50007) || e.Message.Contains("50007") )
			{
				if( !this.FailedPmCount.ContainsKey(user.Id) )
					this.FailedPmCount.Add(user.Id, 0);
				this.FailedPmCount[user.Id]++;
				return 0;
			}
			catch( HttpException e ) when( (int)e.HttpCode >= 500 )
			{
				this.Monitoring.Error500s.Inc();
				return -2;
			}
			catch( Exception e )
			{
				await LogException(e, "Unknown PM error.", 0);
				return -2;
			}
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using Discord;
using Valkyrja.entities;
using Discord.Net;
using Discord.Rest;
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

		public async Task<List<IGuildUser>> GetMentionedGuildUsers(CommandArguments e) //todo - Move this elsewhere...
		{

			if( e.MessageArgs == null || e.MessageArgs.Length == 0 )
				return new List<IGuildUser>();

			List<IGuildUser> mentionedUsers = new List<IGuildUser>();
			for( int i = 0; i < e.MessageArgs.Length; i++ )
			{
				guid id;
				if( !guid.TryParse(e.MessageArgs[i].Trim('<','@','!','>'), out id) || id == 0 )
					break;

				IGuildUser user = e.Server.Guild.GetUser(id);
				if( user == null )
					user = await this.DiscordClient.Rest.GetGuildUserAsync(e.Server.Id, id);
				if( user == null )
					continue;

				if( mentionedUsers.Contains(user) )
				{
					List<string> newArgs = new List<string>(e.MessageArgs);
					newArgs.RemoveAt(i);
					e.MessageArgs = newArgs.ToArray();
					continue;
				}

				mentionedUsers.Add(user);
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

		public async Task<PmErrorCode> SendPmSafe(IUser user, string message, Embed embed = null)
		{
			if( user == null )
				return PmErrorCode.UserNull;
			if( this.FailedPmCount.ContainsKey(user.Id) && this.FailedPmCount[user.Id] >= 3 )
				return PmErrorCode.ThresholdExceeded;
			try
			{
				await user.SendMessageSafe(message, embed);
				return PmErrorCode.Success;
			}
			catch( HttpException e ) when( (int)e.HttpCode == 403 || (e.DiscordCode.HasValue && e.DiscordCode == 50007) || e.Message.Contains("50007") )
			{
				if( !this.FailedPmCount.ContainsKey(user.Id) )
					this.FailedPmCount.Add(user.Id, 0);
				this.FailedPmCount[user.Id]++;
				return PmErrorCode.Failed;
			}
			catch( HttpException e ) when( (int)e.HttpCode >= 500 )
			{
				this.Monitoring.Error500s.Inc();
				return PmErrorCode.Thrown500;
			}
			catch( Exception e )
			{
				await LogException(e, "Unknown PM error.", 0);
				return PmErrorCode.Unknown;
			}
		}

		public async Task SendEmbedFromCli(CommandArguments cmdArgs, SocketUser pmInstead = null)
		{
			if( string.IsNullOrEmpty(cmdArgs.TrimmedMessage) || cmdArgs.TrimmedMessage == "-h" || cmdArgs.TrimmedMessage == "--help" )
			{
				await cmdArgs.SendReplySafe("```md\nCreate an embed using the following parameters:\n" +
				                      "[ --channel     ] Channel where to send the embed.\n" +
				                      "[ --edit <msgId>] Replace a MessageId with a new embed (use after --channel)\n" +
				                      "[ --title       ] Title\n" +
				                      "[ --description ] Description\n" +
				                      "[ --footer      ] Footer\n" +
				                      "[ --color       ] #rrggbb hex color used for the embed stripe.\n" +
				                      "[ --image       ] URL of a Hjuge image in the bottom.\n" +
				                      "[ --thumbnail   ] URL of a smol image on the side.\n" +
				                      "[ --fieldName   ] Create a new field with specified name.\n" +
				                      "[ --fieldValue  ] Text value of a field - has to follow a name.\n" +
				                      "[ --fieldInline ] Use to set the field as inline.\n" +
				                      "Where you can repeat the field* options multiple times.\n```"
				);
				return;
			}

			SocketTextChannel channel = cmdArgs.Channel;

			bool debug = false;
			IMessage msg = null;
			EmbedFieldBuilder currentField = null;
			EmbedBuilder embedBuilder = new EmbedBuilder();

			foreach( Match match in this.RegexCliParam.Matches(cmdArgs.TrimmedMessage) )
			{
				string optionString = this.RegexCliOption.Match(match.Value).Value;

				if( optionString == "--debug" )
				{
					if( IsGlobalAdmin(cmdArgs.Message.Author.Id) || IsSupportTeam(cmdArgs.Message.Author.Id) )
						debug = true;
					continue;
				}

				if( optionString == "--fieldInline" )
				{
					if( currentField == null )
					{
						await cmdArgs.SendReplySafe($"`fieldInline` can not precede `fieldName`.");
						return;
					}

					currentField.WithIsInline(true);
					if( debug )
						await channel.SendMessageSafe($"Setting inline for field `{currentField.Name}`");
					continue;
				}

				string value;
				if( match.Value.Length <= optionString.Length || string.IsNullOrWhiteSpace(value = match.Value.Substring(optionString.Length + 1).Trim()) )
				{
					await cmdArgs.SendReplySafe($"Invalid value for `{optionString}`");
					return;
				}

				if( value.Length >= UserProfileOption.ValueCharacterLimit )
				{
					await cmdArgs.SendReplySafe($"`{optionString}` is too long! (It's {value.Length} characters while the limit is {UserProfileOption.ValueCharacterLimit})");
					return;
				}

				switch( optionString )
				{
					case "--channel":
						if( !guid.TryParse(value.Trim('<', '>', '#'), out guid id) || (channel = cmdArgs.Server.Guild.GetTextChannel(id)) == null )
						{
							await cmdArgs.SendReplySafe($"Channel {value} not found.");
							return;
						}

						if( debug )
							await channel.SendMessageSafe($"Channel set: `{channel.Name}`");

						break;
					case "--title":
						if( value.Length > 256 )
						{
							await cmdArgs.SendReplySafe($"`--title` is too long (`{value.Length} > 256`)");
							return;
						}

						embedBuilder.WithTitle(value);
						if( debug )
							await channel.SendMessageSafe($"Title set: `{value}`");

						break;
					case "--description":
						if( value.Length > 2048 )
						{
							await cmdArgs.SendReplySafe($"`--description` is too long (`{value.Length} > 2048`)");
							return;
						}

						embedBuilder.WithDescription(value);
						if( debug )
							await channel.SendMessageSafe($"Description set: `{value}`");

						break;
					case "--footer":
						if( value.Length > 2048 )
						{
							await cmdArgs.SendReplySafe($"`--footer` is too long (`{value.Length} > 2048`)");
							return;
						}

						embedBuilder.WithFooter(value);
						if( debug )
							await channel.SendMessageSafe($"Description set: `{value}`");

						break;
					case "--image":
						try
						{
							embedBuilder.WithImageUrl(value.Trim('<', '>'));
						}
						catch( Exception )
						{
							await cmdArgs.SendReplySafe($"`--image` is invalid url");
							return;
						}

						if( debug )
							await channel.SendMessageSafe($"Image URL set: `{value}`");

						break;
					case "--thumbnail":
						try
						{
							embedBuilder.WithThumbnailUrl(value.Trim('<', '>'));
						}
						catch( Exception )
						{
							await cmdArgs.SendReplySafe($"`--thumbnail` is invalid url");
							return;
						}

						if( debug )
							await channel.SendMessageSafe($"Thumbnail URL set: `{value}`");

						break;
					case "--color":
						try
						{
							uint color = uint.Parse(value.TrimStart('#'), System.Globalization.NumberStyles.AllowHexSpecifier);
							if( color > uint.Parse("FFFFFF", System.Globalization.NumberStyles.AllowHexSpecifier) )
							{
								await cmdArgs.SendReplySafe("Color out of range.");
								return;
							}

							embedBuilder.WithColor(color);
							if( debug )
								await channel.SendMessageSafe($"Color `{value}` set.");
						}
						catch( Exception )
						{
							await cmdArgs.SendReplySafe("Invalid color format.");
							return;
						}
						break;
					case "--fieldName":
						if( value.Length > 256 )
						{
							await cmdArgs.SendReplySafe($"`--fieldName` is too long (`{value.Length} > 256`)\n```\n{value}\n```");
							return;
						}

						if( currentField != null && currentField.Value == null )
						{
							await cmdArgs.SendReplySafe($"Field `{currentField.Name}` is missing a value!");
							return;
						}

						if( embedBuilder.Fields.Count >= 25 )
						{
							await cmdArgs.SendReplySafe("Too many fields! (Limit is 25)");
							return;
						}

						embedBuilder.AddField(currentField = new EmbedFieldBuilder().WithName(value));
						if( debug )
							await channel.SendMessageSafe($"Creating new field `{currentField.Name}`");

						break;
					case "--fieldValue":
						if( value.Length > 1024 )
						{
							await cmdArgs.SendReplySafe($"`--fieldValue` is too long (`{value.Length} > 1024`)\n```\n{value}\n```");
							return;
						}

						if( currentField == null )
						{
							await cmdArgs.SendReplySafe($"`fieldValue` can not precede `fieldName`.");
							return;
						}

						currentField.WithValue(value);
						if( debug )
							await channel.SendMessageSafe($"Setting value:\n```\n{value}\n```\n...for field:`{currentField.Name}`");

						break;
					case "--edit":
						if( !guid.TryParse(value, out guid msgId) || (msg = await channel.GetMessageAsync(msgId)) == null )
						{
							await cmdArgs.SendReplySafe($"`--edit` did not find a message with ID `{value}` in the <#{channel.Id}> channel.");
							return;
						}

						break;
					default:
						await cmdArgs.SendReplySafe($"Unknown option: `{optionString}`");
						return;
				}
			}

			if( currentField != null && currentField.Value == null )
			{
				await cmdArgs.SendReplySafe($"Field `{currentField.Name}` is missing a value!");
				return;
			}

			switch( msg )
			{
				case null:
					if( pmInstead != null )
						await pmInstead.SendMessageAsync(embed: embedBuilder.Build());
					else
						await cmdArgs.SendReplySafe(embed: embedBuilder.Build());
					break;
				case RestUserMessage message:
					await message?.ModifyAsync(m => m.Embed = embedBuilder.Build());
					break;
				case SocketUserMessage message:
					await message?.ModifyAsync(m => m.Embed = embedBuilder.Build());
					break;
				default:
					await cmdArgs.SendReplySafe("GetMessage went bork.");
					break;
			}
		}
	}
}

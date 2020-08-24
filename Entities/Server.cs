using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Valkyrja.core;
using Discord;
using Discord.Net;
using Discord.WebSocket;

using guid = System.UInt64;

namespace Valkyrja.entities
{
	public class Server
	{
		private ValkyrjaClient Client;

		public readonly guid Id;

		public SocketGuild Guild;

		internal string DbConnectionString;
		public ServerConfig Config;
		public Localisation Localisation;
		public Dictionary<string, Command> Commands;
		public Dictionary<string, CustomCommand> CustomCommands;
		public Dictionary<string, CustomAlias> CustomAliases;
		private CommandOptions CachedCommandOptions;
		private List<CommandChannelOptions> CachedCommandChannelOptions;

		public DateTime ClearAntispamMuteTime = DateTime.UtcNow;
		public Dictionary<guid, int> AntispamMuteCount = new Dictionary<guid, int>();
		public Dictionary<guid, int> AntispamMessageCount = new Dictionary<guid, int>();
		public Dictionary<guid, SocketMessage[]> AntispamRecentUserMessages = new Dictionary<guid, SocketMessage[]>();
		public SocketMessage[] AntispamRecentMessages = null;
		public Dictionary<guid, int> UserMentionCount = new Dictionary<guid, int>();
		public List<SocketGuildUser> AntispamRecentUsernames = new List<SocketGuildUser>();
		public ConcurrentDictionary<guid, guid> CommandReplyMsgIds = new ConcurrentDictionary<guid, guid>();

		public List<guid> IgnoredChannels;
		public List<guid> AutoAnnounceChannels;

		public Regex AlertRegex = null;
		public Regex DeleteAlertRegex = null;
		public Dictionary<guid, RoleConfig> Roles;
		/*public Dictionary<guid, CategoryMuteRole> CategoryMuteRoles;*/
		public Dictionary<guid, CategoryMemberRole> CategoryMemberRoles;
		public List<ReactionAssignedRole> ReactionAssignedRoles;
		public SemaphoreSlim ReactionRolesLock{ get; set; } = new SemaphoreSlim(1, 1);

		private int HttpExceptionCount = 0;
		private bool IgnoreMuteSetup = false;


		public Server(SocketGuild guild)
		{
			this.Id = guild.Id;
			this.Guild = guild;
		}

		public async Task ReloadConfig(ValkyrjaClient client, ServerContext dbContext, Dictionary<string, Command> allCommands)
		{
			this.Client = client;
			this.DbConnectionString = client.DbConnectionString;

			if( this.Commands?.Count != allCommands.Count )
			{
				this.Commands = new Dictionary<string, Command>(allCommands);
			}

			DateTime lastTouched = this.Config?.LastTouched ?? DateTime.MinValue;
			this.Config = dbContext.ServerConfigurations.AsQueryable().FirstOrDefault(c => c.ServerId == this.Id);
			if( this.Config == null )
			{
				this.Config = new ServerConfig(){ServerId = this.Id, Name = this.Guild.Name};
				dbContext.ServerConfigurations.Add(this.Config);
				dbContext.SaveChanges();
			}

			if( this.Config.NotificationChannelId > 0 && lastTouched != DateTime.MinValue && Math.Abs((this.Config.LastTouched - lastTouched).TotalSeconds) > 1f )
			{
				try
				{
					SocketTextChannel channel = this.Guild.GetTextChannel(this.Config.NotificationChannelId);
					if( channel != null )
						await channel.SendMessageSafe($"`{Utils.GetTimestamp()}` Configuration reloaded.");
				}
				catch( Exception exception )
				{
					await this.Client.LogException(exception, "Server.ReloadConfig - failed to send notification", this.Id);
				}
			}

			this.CustomCommands?.Clear();
			this.CustomCommands = dbContext.CustomCommands.AsQueryable().Where(c => c.ServerId == this.Id).ToDictionary(c => c.CommandId.ToLower());

			this.CustomAliases?.Clear();
			this.CustomAliases = dbContext.CustomAliases.AsQueryable().Where(c => c.ServerId == this.Id).ToDictionary(c => c.Alias.ToLower());

			this.Roles?.Clear();
			this.Roles = dbContext.Roles.AsQueryable().Where(c => c.ServerId == this.Id).ToDictionary(c => c.RoleId);

			/*this.CategoryMuteRoles?.Clear();
			this.CategoryMuteRoles = dbContext.CategoryMuteRoles.AsQueryable().Where(c => c.ServerId == this.Id).ToDictionary(c => c.ModRoleId);*/

			this.CategoryMemberRoles?.Clear();
			this.CategoryMemberRoles = dbContext.CategoryMemberRoles.AsQueryable().Where(c => c.ServerId == this.Id).ToDictionary(c => c.ModRoleId);

			this.ReactionRolesLock.Wait();
			{
				this.ReactionAssignedRoles?.Clear();
				this.ReactionAssignedRoles = dbContext.ReactionAssignedRoles.AsQueryable().Where(c => c.ServerId == this.Id).ToList();
			}
			this.ReactionRolesLock.Release();

			List<ChannelConfig> channels = dbContext.Channels.AsQueryable().Where(c => c.ServerId == this.Id).ToList();
			this.IgnoredChannels = channels.Where(c => c.Ignored).Select(c => c.ChannelId).ToList();
			this.AutoAnnounceChannels = channels.Where(c => c.AutoAnnounce).Select(c => c.ChannelId).ToList();

			if( !string.IsNullOrWhiteSpace(this.Config.LogAlertRegex) && this.Config.AlertChannelId != 0 )
			{
				try
				{
					this.AlertRegex = new Regex($"({this.Config.LogAlertRegex})", RegexOptions.Compiled | RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(150));
				}
				catch(Exception e)
				{
					this.AlertRegex = null;
					await client.LogException(e, $"ReloadConfig failed AlertRegex: {this.Config.LogAlertRegex}", this.Id);
				}
			}
			else
			{
				this.AlertRegex = null;
			}

			if( !string.IsNullOrWhiteSpace(this.Config.DeleteAlertRegex) && this.Config.AlertChannelId != 0 )
			{
				try
				{
					this.DeleteAlertRegex = new Regex($"({this.Config.DeleteAlertRegex})", RegexOptions.Compiled | RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(150));
				}
				catch(Exception e)
				{
					this.DeleteAlertRegex = null;
					await client.LogException(e, $"ReloadConfig failed AlertRegex: {this.Config.DeleteAlertRegex}", this.Id);
				}
			}
			else
			{
				this.DeleteAlertRegex = null;
			}

			SocketRole role;
			if( !this.IgnoreMuteSetup && this.Config.MuteRoleId != 0 && (role = this.Guild?.GetRole(this.Config.MuteRoleId)) != null && (this.Guild?.CurrentUser?.GuildPermissions.ManageChannels ?? false) )
			{
				foreach( SocketGuildChannel channel in this.Guild.Channels.Where(c => (c is SocketTextChannel || c is SocketCategoryChannel) && !(c is SocketNewsChannel)) )
				{
					if( this.Config.MuteIgnoreChannelId == channel.Id ||
					    channel.PermissionOverwrites.Any(p => p.TargetId == role.Id) )
						continue;

					try
					{
						channel.AddPermissionOverwriteAsync(role, new OverwritePermissions(sendMessages: PermValue.Deny, addReactions: PermValue.Deny)).GetAwaiter().GetResult();
					}
					catch( HttpException e )
					{
						await HandleHttpException(e, $"Couldn't configure channel permissions for the muted role for channel <#{channel.Id}> (if you truly do not want it configured in some channels, just add that role to these channels manually with no permissions.)");
						this.IgnoreMuteSetup = true;
						break;
					}
					catch( Exception e )
					{
						await this.Client.LogException(e, "ReloadConfig - muted role channel permissions", this.Id);
					}
				}
			}

			if( this.Config.LocalisationId == 0 )
			{
				this.Localisation = new Localisation();
			}
			else
			{
				this.Localisation = dbContext.Localisations.FirstOrDefault(l => l.Id == this.Id) ?? new Localisation();
			}
		}

		public async Task LoadConfig(ValkyrjaClient client, ServerContext dbContext, Dictionary<string, Command> allCommands)
		{
			await ReloadConfig(client, dbContext, allCommands);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public CommandOptions GetCommandOptions(string commandString)
		{
			string lowerCommandString = commandString.ToLower();
			if( this.CustomAliases.ContainsKey(lowerCommandString) )
				commandString = this.CustomAliases[lowerCommandString].CommandId;

			if( this.CachedCommandOptions != null && this.CachedCommandOptions.CommandId == commandString )
				return this.CachedCommandOptions;

			ServerContext dbContext = ServerContext.Create(this.DbConnectionString);
			this.CachedCommandOptions = dbContext.CommandOptions.AsQueryable().FirstOrDefault(c => c.ServerId == this.Id && c.CommandId == commandString);
			dbContext.Dispose();
			return this.CachedCommandOptions ?? new CommandOptions{ServerId = this.Id, CommandId = commandString};
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public List<CommandChannelOptions> GetCommandChannelOptions(string commandString)
		{
			CommandChannelOptions tmp;
			if( this.CachedCommandChannelOptions != null &&
			   (tmp = this.CachedCommandChannelOptions.FirstOrDefault()) != null && tmp.CommandId == commandString )
				return this.CachedCommandChannelOptions;

			ServerContext dbContext = ServerContext.Create(this.DbConnectionString);
			this.CachedCommandChannelOptions = dbContext.CommandChannelOptions.AsQueryable().Where(c => c.ServerId == this.Id && c.CommandId == commandString)?.ToList();
			dbContext.Dispose();
			return this.CachedCommandChannelOptions ?? new List<CommandChannelOptions>();
		}

		///<summary> Returns the correct commandId if it exists, empty otherwise. Returns null if it is restricted command. </summary>
		public string GetCommandOptionsId(string commandString)
		{
			string commandId = "";
			commandString = commandString.ToLower();

			if( this.CustomAliases.ContainsKey(commandString) )
				commandString = this.CustomAliases[commandString].CommandId.ToLower();

			if( this.Commands.ContainsKey(commandString) )
			{
				Command command;
				if( (command = this.Commands[commandString]).IsCoreCommand ||
				    command.RequiredPermissions == PermissionType.OwnerOnly )
				{
					return null;
				}

				commandId = command.Id;
				if( command.IsAlias && !string.IsNullOrEmpty(command.ParentId) )
					commandId = command.ParentId;
			}
			else if( this.CustomCommands.ContainsKey(commandString) )
				commandId = this.CustomCommands[commandString].CommandId;

			return commandId;
		}

		public bool CanExecuteCommand(string commandId, int commandPermissions, SocketGuildChannel channel, SocketGuildUser user)
		{
			CommandOptions commandOptions = GetCommandOptions(commandId);
			List<CommandChannelOptions> commandChannelOptions = GetCommandChannelOptions(commandId);

			//Custom Command Channel Permissions
			CommandChannelOptions currentChannelOptions = null;
			if( commandPermissions != PermissionType.OwnerOnly &&
			    channel != null && commandChannelOptions != null &&
				(currentChannelOptions = commandChannelOptions.FirstOrDefault(c => c.ChannelId == channel.Id)) != null &&
			    currentChannelOptions.Blocked )
				return false;

			if( commandPermissions != PermissionType.OwnerOnly &&
			    channel != null && commandChannelOptions != null &&
			    commandChannelOptions.Any(c => c.Allowed) &&
			    ((currentChannelOptions = commandChannelOptions.FirstOrDefault(c => c.ChannelId == channel.Id)) == null ||
			    !currentChannelOptions.Allowed) )
				return false; //False only if there are *some* whitelisted channels, but it's not the current one.

			//Custom Command Permission Overrides
			if( commandOptions != null && commandOptions.PermissionOverrides != PermissionOverrides.Default )
			{
				switch(commandOptions.PermissionOverrides)
				{
					case PermissionOverrides.Nobody:
						return false;
					case PermissionOverrides.ServerOwner:
						commandPermissions = PermissionType.ServerOwner;
						break;
					case PermissionOverrides.Admins:
						commandPermissions = PermissionType.ServerOwner | PermissionType.Admin;
						break;
					case PermissionOverrides.Moderators:
						commandPermissions = PermissionType.ServerOwner | PermissionType.Admin | PermissionType.Moderator;
						break;
					case PermissionOverrides.SubModerators:
						commandPermissions = PermissionType.ServerOwner | PermissionType.Admin | PermissionType.Moderator | PermissionType.SubModerator;
						break;
					case PermissionOverrides.Members:
						commandPermissions = PermissionType.ServerOwner | PermissionType.Admin | PermissionType.Moderator | PermissionType.SubModerator | PermissionType.Member;
						break;
					case PermissionOverrides.Everyone:
						commandPermissions = PermissionType.Everyone;
						break;
					default:
						throw new ArgumentOutOfRangeException("permissionOverrides");
				}
			}

			//Actually check them permissions!
			return ((commandPermissions & PermissionType.Everyone) > 0) ||
			       ((commandPermissions & PermissionType.ServerOwner) > 0 && IsOwner(user)) ||
			       ((commandPermissions & PermissionType.Admin) > 0 && IsAdmin(user)) ||
			       ((commandPermissions & PermissionType.Moderator) > 0 && IsModerator(user)) ||
			       ((commandPermissions & PermissionType.SubModerator) > 0 && IsSubModerator(user)) ||
			       ((commandPermissions & PermissionType.Member) > 0 && IsMember(user));

		}

		public Embed GetManPage(Command command)
		{
			if( command.ManPage == null || command.RequiredPermissions == PermissionType.OwnerOnly )
				return null;

			return command.ManPage.ToEmbed(this, command);
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsOwner(SocketGuildUser user)
		{
			return this.Guild.OwnerId == user.Id || (user.GuildPermissions.ManageGuild && user.GuildPermissions.Administrator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsAdmin(SocketGuildUser user)
		{
			return IsOwner(user) || user.Roles.Any(r => this.Roles.Any(p => p.Value.PermissionLevel >= RolePermissionLevel.Admin && p.Value.RoleId == r.Id));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsModerator(SocketGuildUser user)
		{
			return IsOwner(user) || user.Roles.Any(r => this.Roles.Any(p => p.Value.PermissionLevel >= RolePermissionLevel.Moderator && p.Value.RoleId == r.Id));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsSubModerator(SocketGuildUser user)
		{
			return IsOwner(user) || user.Roles.Any(r => this.Roles.Any(p => p.Value.PermissionLevel >= RolePermissionLevel.SubModerator && p.Value.RoleId == r.Id));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsMember(SocketGuildUser user)
		{
			return IsOwner(user) || user.Roles.Any(r => this.Roles.Any(p => p.Value.PermissionLevel >= RolePermissionLevel.Member && p.Value.RoleId == r.Id));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool HasIgnoredRole(SocketGuildUser user)
		{
			return this.Client.IsGlobalAdmin(user.Id) || IsOwner(user) ||
			       user.Roles.Any(r => this.Roles.Any(p => p.Value.RoleId == r.Id && (p.Value.PermissionLevel >= RolePermissionLevel.SubModerator || p.Value.AntispamIgnored)));
		}


		public string GetPropertyValue(string propertyName)
		{
			string propertyValue = this.Config.GetPropertyValue(propertyName);
			if( string.IsNullOrEmpty(propertyValue) )
				return null;

			guid id;
			if( guid.TryParse(propertyValue, out id) && id > int.MaxValue )
			{
				string propertyValueDereferenced = (this.Guild.GetChannel(id)?.Name ?? this.Guild.GetRole(id)?.Name);
				if( propertyValueDereferenced != null )
					propertyValue = propertyValueDereferenced + "` | `" + propertyValue;
			}

			return propertyValue.Replace("@everyone", "@-everyone");
		}

		public SocketRole GetRole(string expression, out string response)
		{
			guid id = 0;
			IEnumerable<SocketRole> roles = this.Guild.Roles;
			IEnumerable<SocketRole> foundRoles = null;
			SocketRole role = null;

			if( !(guid.TryParse(expression, out id) && (role = this.Guild.GetRole(id)) != null) &&
			    !(foundRoles = roles.Where(r => r.Name == expression)).Any() &&
			    !(foundRoles = roles.Where(r => r.Name.ToLower() == expression.ToLower())).Any() &&
			    !(foundRoles = roles.Where(r => r.Name.ToLower().Contains(expression.ToLower()))).Any() )
			{
				response = "I did not find a role based on that expression.";
				return null;
			}

			if( foundRoles != null && foundRoles.Count() > 1 )
			{
				response = "I found more than one role with that expression, please be more specific.";
				return null;
			}

			if( role == null )
			{
				role = foundRoles.First();
			}

			response = "Done.";
			return role;
		}

		public async Task<bool> HandleHttpException(HttpException exception, string helptext = "")
		{
			string logMsg = "HttpException - further logging disabled";
			if( (int)exception.HttpCode >= 500 )
				logMsg = "DiscordPoop";
			else if( (int)exception.HttpCode == 404 )
				logMsg = "";
			else if( exception.Message.Contains("50007") )
				logMsg = "Failed to PM";
			else if( this.HttpExceptionCount < 5 )
			{
				try
				{
					logMsg = $"Received error code `{(int)exception.HttpCode}`\n{helptext}";
					string msg = $"{logMsg}\n\nPlease fix my permissions and channel access on your Discord Server `{this.Guild.Name}`.\n\nYou can also set these messages to be sent into a notification channel on the config page. If you are unsure what's going on, consult our support team at {GlobalConfig.DiscordInvite}";

					SocketTextChannel channel = null;
					if( this.Config.NotificationChannelId > 0 && (channel = this.Guild.GetTextChannel(this.Config.NotificationChannelId)) != null )
					{
						await channel.SendMessageSafe(msg);
					}
					else
						await this.Client.SendPmSafe(this.Guild.Owner, msg);
				}
				catch( Exception e )
				{
					if( !(e is HttpException) )
						await this.Client.LogException(e, "Server.HandleHttpException", this.Id);
				}
			}

			if( ++this.HttpExceptionCount > 1 )
			{
				logMsg = null;
			}

			if( !string.IsNullOrEmpty(logMsg) )
				await this.Client.LogException(exception, logMsg, this.Id);

			return this.HttpExceptionCount > 3;
		}
	}
}

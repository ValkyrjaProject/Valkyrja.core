using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

using guid = System.UInt64;

namespace Botwinder.entities
{
	public class Server
	{
		public readonly guid Id;

		public SocketGuild Guild;

		private string DbConnectionString;
		public ServerConfig Config;
		public Localisation Localisation;
		public readonly Dictionary<string, Command> Commands;
		public Dictionary<string, CustomCommand> CustomCommands;
		public Dictionary<string, CustomAlias> CustomAliases;
		private CommandOptions CachedCommandOptions;
		private List<CommandChannelOptions> CachedCommandChannelOptions;

		public DateTime ClearAntispamMuteTime = DateTime.UtcNow;
		public Dictionary<guid, int> AntispamMuteCount = new Dictionary<guid, int>();
		public Dictionary<guid, int> AntispamMessageCount = new Dictionary<guid, int>();
		public Dictionary<guid, SocketMessage[]> AntispamRecentMessages = new Dictionary<guid, SocketMessage[]>();

		public List<guid> IgnoredChannels;

		public Dictionary<guid, RoleConfig> Roles;


		public Server(SocketGuild guild, Dictionary<string, Command> allCommands)
		{
			this.Id = guild.Id;
			this.Guild = guild;
			this.Commands = new Dictionary<string, Command>(allCommands);
		}

		public void ReloadConfig(string dbConnectionString, ServerContext dbContext)
		{
			this.DbConnectionString = dbConnectionString;

			this.Config = dbContext.ServerConfigurations.FirstOrDefault(c => c.ServerId == this.Id);
			if( this.Config == null )
			{
				this.Config = new ServerConfig(){ServerId = this.Id, Name = this.Guild.Name};
				dbContext.ServerConfigurations.Add(this.Config);
				dbContext.SaveChanges();
			}

			this.CustomCommands?.Clear();
			this.CustomAliases?.Clear();
			this.Roles?.Clear();

			this.CustomCommands = dbContext.CustomCommands.Where(c => c.ServerId == this.Id).ToDictionary(c => c.CommandId);
			this.CustomAliases = dbContext.CustomAliases.Where(c => c.ServerId == this.Id).ToDictionary(c => c.Alias);
			this.Roles = dbContext.Roles.Where(c => c.ServerId == this.Id).ToDictionary(c => c.RoleId);

			List<ChannelConfig> channels = dbContext.Channels.Where(c => c.ServerId == this.Id).ToList();
			this.IgnoredChannels = channels.Where(c => c.Ignored).Select(c => c.ChannelId).ToList();

			dbContext.Dispose();

			SocketRole role;
			if( this.Config.MuteRoleId != 0 && (role = this.Guild.GetRole(this.Config.MuteRoleId)) != null )
			{
				foreach( SocketTextChannel channel in this.Guild.TextChannels )
				{
					if( this.Config.MuteIgnoreChannelId == channel.Id ||
					    channel.PermissionOverwrites.Any(p => p.TargetId == role.Id))
						continue;

					try{
						channel.AddPermissionOverwriteAsync(role, new OverwritePermissions(sendMessages: PermValue.Deny)).GetAwaiter().GetResult();
					} catch(Exception) { }
				}
			}
		}

		public void LoadConfig(string dbConnectionString, ServerContext dbContext)
		{
			ReloadConfig(dbConnectionString, dbContext);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public CommandOptions GetCommandOptions(string commandString)
		{
			if( this.CustomAliases.ContainsKey(commandString) )
				commandString = this.CustomAliases[commandString].CommandId;

			if( this.CachedCommandOptions != null && this.CachedCommandOptions.CommandId == commandString )
				return this.CachedCommandOptions;

			ServerContext dbContext = ServerContext.Create(this.DbConnectionString);
			this.CachedCommandOptions = dbContext.CommandOptions.FirstOrDefault(c => c.ServerId == this.Id && c.CommandId == commandString);
			dbContext.Dispose();
			return this.CachedCommandOptions;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public List<CommandChannelOptions> GetCommandChannelOptions(string commandString)
		{
			CommandChannelOptions tmp;
			if( this.CachedCommandChannelOptions != null &&
			   (tmp = this.CachedCommandChannelOptions.FirstOrDefault()) != null && tmp.CommandId == commandString )
				return this.CachedCommandChannelOptions;

			ServerContext dbContext = ServerContext.Create(this.DbConnectionString);
			this.CachedCommandChannelOptions = dbContext.CommandChannelOptions.Where(c => c.ServerId == this.Id && c.CommandId == commandString)?.ToList();
			dbContext.Dispose();
			return this.CachedCommandChannelOptions;
		}

		///<summary> Returns the correct commandId if it exists, empty otherwise. Returns null if it is restricted command. </summary>
		public string GetCommandOptionsId(string commandId)
		{
			if( (!this.CustomAliases.ContainsKey(commandId) &&
			     !this.Commands.ContainsKey(commandId) &&
			     !this.CustomCommands.ContainsKey(commandId)) )
			{
				return "";
			}

			if( this.CustomAliases.ContainsKey(commandId) )
				commandId = this.CustomAliases[commandId].CommandId;

			if( this.Commands.ContainsKey(commandId) )
			{
				Command command;
				if( (command = this.Commands[commandId]).IsCoreCommand ||
				    command.RequiredPermissions == PermissionType.OwnerOnly )
				{
					return null;
				}

				if( command.IsAlias && !string.IsNullOrEmpty(command.ParentId) )
					commandId = command.ParentId;
			}

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
			    currentChannelOptions.Blacklisted )
				return false;

			if( commandPermissions != PermissionType.OwnerOnly &&
			    channel != null && commandChannelOptions != null &&
			    commandChannelOptions.Any(c => c.Whitelisted) &&
			    ((currentChannelOptions = commandChannelOptions.FirstOrDefault(c => c.ChannelId == channel.Id)) == null ||
			    !currentChannelOptions.Whitelisted) )
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
						commandPermissions = PermissionType.ServerOwner | PermissionType.Admin | PermissionType.Moderator |
						                      PermissionType.SubModerator;
						break;
					case PermissionOverrides.Members:
						commandPermissions = PermissionType.ServerOwner | PermissionType.Admin | PermissionType.Moderator |
						                      PermissionType.SubModerator | PermissionType.Member;
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
	}
}

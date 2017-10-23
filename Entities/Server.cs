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
		public List<guid> MutedChannels = new List<guid>();
		public List<guid> TemporaryChannels = new List<guid>();

		public Dictionary<guid, RoleConfig> Roles;


		public Server(SocketGuild guild, Dictionary<string, Command> allCommands)
		{
			this.Id = guild.Id;
			this.Guild = guild;
			this.Commands = new Dictionary<string, Command>(allCommands);
		}

		public void ReloadConfig(string dbConnectionString)
		{
			this.DbConnectionString = dbConnectionString;
			ServerContext dbContext = ServerContext.Create(dbConnectionString);

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

			dbContext.Dispose();

			SocketRole role;
			if( this.Config.MuteRoleId != 0 && (role = this.Guild.GetRole(this.Config.MuteRoleId)) != null )
			{
				foreach( SocketTextChannel channel in this.Guild.TextChannels )
				{
					if( this.Config.MuteIgnoreChannelId == channel.Id )
						continue;

					try{
						channel.AddPermissionOverwriteAsync(role, new OverwritePermissions(sendMessages: PermValue.Deny)).GetAwaiter().GetResult();
					} catch(Exception) { }
				}
			}
		}

		public void LoadConfig(string dbConnectionString)
		{
			ServerContext dbContext = ServerContext.Create(dbConnectionString);
			this.IgnoredChannels = dbContext.Channels.Where(c => c.ServerId == this.Id && c.Ignored).Select(c => c.ChannelId).ToList();
			this.TemporaryChannels = dbContext.Channels.Where(c => c.ServerId == this.Id && c.Temporary).Select(c => c.ChannelId).ToList();
			this.MutedChannels = dbContext.Channels.Where(c => c.ServerId == this.Id && c.MutedUntil > DateTime.MinValue).Select(c => c.ChannelId).ToList();

			dbContext.Dispose();
			ReloadConfig(dbConnectionString);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public CommandOptions GetCommandOptions(string commandString)
		{
			if( this.CachedCommandOptions != null && this.CachedCommandOptions.CommandId == commandString )
				return this.CachedCommandOptions;

			return this.CachedCommandOptions = ServerContext.Create(this.DbConnectionString).CommandOptions.FirstOrDefault(c => c.ServerId == this.Id && c.CommandId == commandString);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public List<CommandChannelOptions> GetCommandChannelOptions(string commandString)
		{
			CommandChannelOptions tmp;
			if( this.CachedCommandChannelOptions != null &&
			   (tmp = this.CachedCommandChannelOptions.FirstOrDefault()) != null && tmp.CommandId == commandString )
				return this.CachedCommandChannelOptions;

			return this.CachedCommandChannelOptions = ServerContext.Create(this.DbConnectionString).CommandChannelOptions.Where(c => c.ServerId == this.Id && c.CommandId == commandString)?.ToList();
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
	}
}

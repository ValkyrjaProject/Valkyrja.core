using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using Discord.WebSocket;
using guid = System.UInt64;

namespace Botwinder.entities
{
	public class Server<TUser> where TUser: UserData, new()
	{
		public readonly guid Id;

		public readonly SocketGuild Guild;

		private ServerContext DbContext;
		public readonly Dictionary<string, Command<TUser>> Commands;
		public Dictionary<string, CustomCommand> CustomCommands;
		public Dictionary<string, CustomAlias> CustomAliases;
		public ServerConfig Config;

		public List<guid> IgnoredChannels;
		public List<guid> MutedChannels = new List<guid>();
		public List<guid> TemporaryChannels = new List<guid>();

		public Dictionary<guid, RoleConfig> Roles;


		public Server(SocketGuild guild, Dictionary<string, Command<TUser>> allCommands, ServerContext db)
		{
			this.Id = guild.Id;
			this.Guild = guild;
			this.Commands = new Dictionary<string, Command<TUser>>(allCommands);
			ReloadConfig(db);
		}

		public void ReloadConfig(ServerContext db)
		{
			this.DbContext = db;

			this.Config = db.ServerConfigurations.FirstOrDefault(c => c.ServerId == this.Id);
			if( this.Config == null )
			{
				this.Config = new ServerConfig(); //todo actually create that config properly...
				db.ServerConfigurations.Add(this.Config);
				db.SaveChanges();
			}

			this.CustomCommands.Clear();
			this.CustomAliases.Clear();
			this.Roles.Clear();

			this.CustomCommands = db.CustomCommands.Where(c => c.ServerId == this.Id).ToDictionary(c => c.CommandId);
			this.CustomAliases = db.CustomAliases.Where(c => c.ServerId == this.Id).ToDictionary(c => c.Alias);
			this.Roles = db.Roles.Where(c => c.ServerId == this.Id).ToDictionary(c => c.RoleId);
		}

		public void LoadConfig(ServerContext db)
		{
			this.IgnoredChannels = db.Channels.Where(c => c.ServerId == this.Id && c.Ignored).Select(c => c.ChannelId).ToList();
			this.TemporaryChannels = db.Channels.Where(c => c.ServerId == this.Id && c.Temporary).Select(c => c.ChannelId).ToList();
			this.MutedChannels = db.Channels.Where(c => c.ServerId == this.Id && c.MutedUntil > DateTime.MinValue).Select(c => c.ChannelId).ToList();

			ReloadConfig(db);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public CommandOptions GetCommandOptions(string commandString)
		{
			return this.DbContext.CommandOptions.FirstOrDefault(c => c.ServerId == this.Id && c.CommandId == commandString);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public List<CommandChannelOptions> GetCommandChannelOptions(string commandString)
		{
			return this.DbContext.CommandChannelOptions.Where(c => c.ServerId == this.Id && c.CommandId == commandString)?.ToList();
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsOwner(SocketGuildUser user)
		{
			return this.Guild.OwnerId == user.Id || (user.GuildPermissions.ManageGuild && user.GuildPermissions.Administrator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsAdmin(SocketGuildUser user)
		{
			return user.Roles.Any(r => this.Roles.Any(p => p.Value.PermissionLevel == RolePermissionLevel.Admin && p.Value.RoleId == r.Id));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsModerator(SocketGuildUser user)
		{
			return user.Roles.Any(r => this.Roles.Any(p => p.Value.PermissionLevel == RolePermissionLevel.Moderator && p.Value.RoleId == r.Id));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsSubModerator(SocketGuildUser user)
		{
			return user.Roles.Any(r => this.Roles.Any(p => p.Value.PermissionLevel == RolePermissionLevel.SubModerator && p.Value.RoleId == r.Id));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsMember(SocketGuildUser user)
		{
			return user.Roles.Any(r => this.Roles.Any(p => p.Value.PermissionLevel == RolePermissionLevel.Member && p.Value.RoleId == r.Id));
		}
	}
}

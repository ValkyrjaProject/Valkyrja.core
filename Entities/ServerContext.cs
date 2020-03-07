using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Pomelo.EntityFrameworkCore.MySql;

using guid = System.UInt64;

namespace Valkyrja.entities
{
	public class ServerContext: DbContext
	{
		public DbSet<ServerConfig> ServerConfigurations;
		public DbSet<ServerStats> ServerStats;
		public DbSet<ChannelConfig> Channels;
		public DbSet<RoleConfig> Roles;
		public DbSet<ReactionAssignedRole> ReactionAssignedRoles;
		public DbSet<RoleGroupConfig> PublicRoleGroups;

		public DbSet<CommandOptions> CommandOptions;
		public DbSet<CommandChannelOptions> CommandChannelOptions;
		public DbSet<CustomCommand> CustomCommands;
		public DbSet<CustomAlias> CustomAliases;

		public DbSet<UserData> UserDatabase;
		public DbSet<Username> Usernames;
		public DbSet<Nickname> Nicknames;

		public DbSet<Quote> Quotes;
		public DbSet<ProfileOption> ProfileOptions;
		public DbSet<UserProfileOption> UserProfileOptions;

		public DbSet<StatsDaily> StatsDaily;
		public DbSet<StatsTotal> StatsTotal;

		public ServerContext(DbContextOptions<ServerContext> options) : base(options)
		{
			this.ServerConfigurations = new InternalDbSet<ServerConfig>(this);
			this.ServerStats = new InternalDbSet<ServerStats>(this);
			this.Channels = new InternalDbSet<ChannelConfig>(this);
			this.Roles = new InternalDbSet<RoleConfig>(this);
			this.ReactionAssignedRoles = new InternalDbSet<ReactionAssignedRole>(this);
			this.PublicRoleGroups = new InternalDbSet<RoleGroupConfig>(this);
			this.CommandOptions = new InternalDbSet<CommandOptions>(this);
			this.CommandChannelOptions = new InternalDbSet<CommandChannelOptions>(this);
			this.CustomCommands = new InternalDbSet<CustomCommand>(this);
			this.CustomAliases = new InternalDbSet<CustomAlias>(this);
			this.UserDatabase = new InternalDbSet<UserData>(this);
			this.Usernames = new InternalDbSet<Username>(this);
			this.Nicknames = new InternalDbSet<Nickname>(this);
			this.Quotes = new InternalDbSet<Quote>(this);
			this.ProfileOptions = new InternalDbSet<ProfileOption>(this);
			this.UserProfileOptions = new InternalDbSet<UserProfileOption>(this);
			this.StatsDaily = new InternalDbSet<StatsDaily>(this);
			this.StatsTotal = new InternalDbSet<StatsTotal>(this);
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<ServerConfig>()
				.HasKey(p => p.ServerId);

			modelBuilder.Entity<ServerStats>()
				.HasKey(p => p.ServerId);

			modelBuilder.Entity<ChannelConfig>()
				.HasKey(p => p.ChannelId);

			modelBuilder.Entity<RoleConfig>()
				.HasKey(p => p.RoleId);

			modelBuilder.Entity<ReactionAssignedRole>()
				.HasKey(p => p.RoleId);

			modelBuilder.Entity<RoleGroupConfig>()
				.HasKey(p => new{p.ServerId, p.GroupId});

			modelBuilder.Entity<CommandOptions>()
				.HasKey(p => new{p.ServerId, p.CommandId});

			modelBuilder.Entity<CommandChannelOptions>()
				.HasKey(p => new{p.ServerId, p.CommandId, p.ChannelId});

			modelBuilder.Entity<CustomCommand>()
				.HasKey(p => new{p.ServerId, p.CommandId});

			modelBuilder.Entity<CustomAlias>()
				.HasKey(p => new{p.ServerId, p.Alias});

			modelBuilder.Entity<UserData>()
				.HasKey(p => new{p.ServerId, p.UserId});

			modelBuilder.Entity<Username>()
				.HasKey(p => new{p.ServerId, p.UserId, p.Name});

			modelBuilder.Entity<Nickname>()
				.HasKey(p => new{p.ServerId, p.UserId, p.Name});

			modelBuilder.Entity<Quote>()
				.HasKey(p => new{p.ServerId, p.Id});

			modelBuilder.Entity<ProfileOption>()
				.HasKey(p => new{p.ServerId, p.Option});

			modelBuilder.Entity<UserProfileOption>()
				.HasKey(p => new{p.ServerId, p.UserId, p.Option});

			modelBuilder.Entity<StatsDaily>()
				.HasKey(p => p.ServerId);

			modelBuilder.Entity<StatsTotal>()
				.HasKey(p => new{p.ServerId, p.DateTime});
		}

		public static ServerContext Create(string connectionString)
		{
			DbContextOptionsBuilder<ServerContext> optionsBuilder = new DbContextOptionsBuilder<ServerContext>();
			optionsBuilder.UseMySql(connectionString).EnableSensitiveDataLogging();

			ServerContext newContext = new ServerContext(optionsBuilder.Options);
			newContext.Database.EnsureCreated();
			return newContext;
		}

		public UserData GetOrAddUser(guid serverId, guid userId)
		{
			UserData userData = this.UserDatabase.FirstOrDefault(u => u.ServerId == serverId && u.UserId == userId);
			if( userData == null )
			{
				userData = new UserData(){
					ServerId = serverId,
					UserId = userId
				};
				this.UserDatabase.Add(userData);
				SaveChanges();
			}

			return userData;
		}

		public RoleConfig GetOrAddRole(guid serverId, guid roleId)
		{
			RoleConfig roleConfig = this.Roles.FirstOrDefault(u => u.ServerId == serverId && u.RoleId == roleId);
			if( roleConfig == null )
			{
				roleConfig = new RoleConfig(){
					ServerId = serverId,
					RoleId = roleId
				};
				this.Roles.Add(roleConfig);
			}

			return roleConfig;
		}

		public CommandOptions GetOrAddCommandOptions(Server server, string commandId)
		{
			CommandOptions options = this.CommandOptions.FirstOrDefault(c => c.ServerId == server.Id && c.CommandId == commandId);
			if( options == null )
			{
				options = new CommandOptions(){
					ServerId = server.Id,
					CommandId = commandId
				};
				this.CommandOptions.Add(options);
			}

			return options;
		}

		public CommandChannelOptions GetOrAddCommandChannelOptions(guid serverId, guid channelId, string commandId)
		{
			CommandChannelOptions options = this.CommandChannelOptions.FirstOrDefault(c => c.ServerId == serverId && c.CommandId == commandId && c.ChannelId == channelId);
			if( options == null )
			{
				options = new CommandChannelOptions{
					ServerId = serverId,
					ChannelId = channelId,
					CommandId = commandId
				};

				this.CommandChannelOptions.Add(options);
			}
			return options;
		}
	}
}

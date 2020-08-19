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
		public DbSet<ServerConfig> ServerConfigurations{ get; set; }
		public DbSet<ServerStats> ServerStats{ get; set; }
		public DbSet<Localisation> Localisations{ get; set; }
		public DbSet<ChannelConfig> Channels{ get; set; }
		public DbSet<RoleConfig> Roles{ get; set; }
		public DbSet<CategoryMuteRole> CategoryMuteRoles{ get; set; }
		public DbSet<CategoryMemberRole> CategoryMemberRoles{ get; set; }
		public DbSet<ReactionAssignedRole> ReactionAssignedRoles{ get; set; }
		public DbSet<RoleGroupConfig> PublicRoleGroups{ get; set; }

		public DbSet<CommandOptions> CommandOptions{ get; set; }
		public DbSet<CommandChannelOptions> CommandChannelOptions{ get; set; }
		public DbSet<CustomCommand> CustomCommands{ get; set; }
		public DbSet<CustomAlias> CustomAliases{ get; set; }

		public DbSet<UserData> UserDatabase{ get; set; }
		public DbSet<Username> Usernames{ get; set; }
		public DbSet<Nickname> Nicknames{ get; set; }

		public DbSet<Quote> Quotes{ get; set; }
		public DbSet<ProfileOption> ProfileOptions{ get; set; }
		public DbSet<UserProfileOption> UserProfileOptions{ get; set; }

		public DbSet<StatsDaily> StatsDaily{ get; set; }
		public DbSet<StatsTotal> StatsTotal{ get; set; }

		public DbSet<VerificationData> Verification{ get; set; }

		private ServerContext(DbContextOptions<ServerContext> options) : base(options)
		{
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<ServerConfig>()
				.HasKey(p => p.ServerId);

			modelBuilder.Entity<ServerStats>()
				.HasKey(p => p.ServerId);

			modelBuilder.Entity<Localisation>()
				.HasKey(p => p.Id);

			modelBuilder.Entity<ChannelConfig>()
				.HasKey(p => p.ChannelId);

			modelBuilder.Entity<RoleConfig>()
				.HasKey(p => p.RoleId);

			modelBuilder.Entity<CategoryMuteRole>()
				.HasKey(p => new{p.ServerId, p.ModRoleId, p.MuteRoleId});

			modelBuilder.Entity<CategoryMemberRole>()
				.HasKey(p => new{p.ServerId, p.ModRoleId, p.MemberRoleId});

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
				.HasKey(p => new{p.ServerId, p.DateTime});

			modelBuilder.Entity<StatsTotal>()
				.HasKey(p => new{p.ServerId, p.DateTime});

			modelBuilder.Entity<VerificationData>()
				.HasKey(p => new{p.ServerId, p.UserId});
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
			UserData userData = this.UserDatabase.AsQueryable().FirstOrDefault(u => u.ServerId == serverId && u.UserId == userId);
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

		public CategoryMemberRole GetOrAddMemberRole(guid serverId, guid modRoleId, guid memberRoleId)
		{
			CategoryMemberRole roleConfig = this.CategoryMemberRoles.AsQueryable().FirstOrDefault(u => u.ServerId == serverId && u.ModRoleId == modRoleId && u.MemberRoleId == memberRoleId);
			if( roleConfig == null )
			{
				roleConfig = new CategoryMemberRole(){
					ServerId = serverId,
					ModRoleId = modRoleId,
					MemberRoleId = memberRoleId,
				};
				this.CategoryMemberRoles.Add(roleConfig);
			}

			return roleConfig;
		}

		public CategoryMuteRole GetOrAddMuteRole(guid serverId, guid modRoleId, guid muteRoleId)
		{
			CategoryMuteRole roleConfig = this.CategoryMuteRoles.AsQueryable().FirstOrDefault(u => u.ServerId == serverId && u.ModRoleId == modRoleId && u.MuteRoleId == muteRoleId);
			if( roleConfig == null )
			{
				roleConfig = new CategoryMuteRole(){
					ServerId = serverId,
					ModRoleId = modRoleId,
					MuteRoleId = muteRoleId,
				};
				this.CategoryMuteRoles.Add(roleConfig);
			}

			return roleConfig;
		}

		public CommandOptions GetOrAddCommandOptions(Server server, string commandId)
		{
			CommandOptions options = this.CommandOptions.AsQueryable().FirstOrDefault(c => c.ServerId == server.Id && c.CommandId == commandId);
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
			CommandChannelOptions options = this.CommandChannelOptions.AsQueryable().FirstOrDefault(c => c.ServerId == serverId && c.CommandId == commandId && c.ChannelId == channelId);
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

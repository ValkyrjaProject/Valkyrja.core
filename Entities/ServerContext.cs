using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Pomelo.EntityFrameworkCore.MySql;

using guid = System.UInt64;

namespace Botwinder.entities
{
	public class ServerContext: DbContext
	{
		public DbSet<ServerConfig> ServerConfigurations;
		public DbSet<ServerStats> ServerStats;
		public DbSet<ChannelConfig> Channels;
		public DbSet<RoleConfig> Roles;

		public DbSet<CommandOptions> CommandOptions;
		public DbSet<CommandChannelOptions> CommandChannelOptions;
		public DbSet<CustomCommand> CustomCommands;
		public DbSet<CustomAlias> CustomAliases;

		public DbSet<UserData> UserDatabase;
		public DbSet<Username> Usernames;
		public DbSet<Nickname> Nicknames;

		public ServerContext(DbContextOptions<ServerContext> options) : base(options)
		{
			this.ServerConfigurations = new InternalDbSet<ServerConfig>(this);
			this.ServerStats = new InternalDbSet<ServerStats>(this);
			this.Channels = new InternalDbSet<ChannelConfig>(this);
			this.Roles = new InternalDbSet<RoleConfig>(this);
			this.CommandOptions = new InternalDbSet<CommandOptions>(this);
			this.CommandChannelOptions = new InternalDbSet<CommandChannelOptions>(this);
			this.CustomCommands = new InternalDbSet<CustomCommand>(this);
			this.CustomAliases = new InternalDbSet<CustomAlias>(this);
			this.UserDatabase = new InternalDbSet<UserData>(this);
			this.Usernames = new InternalDbSet<Username>(this);
			this.Nicknames = new InternalDbSet<Nickname>(this);
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
				.HasKey(p => p.Id);

			modelBuilder.Entity<Nickname>()
				.HasKey(p => p.Id);
		}

		public static ServerContext Create(string connectionString)
		{
			DbContextOptionsBuilder<ServerContext> optionsBuilder = new DbContextOptionsBuilder<ServerContext>();
			optionsBuilder.UseMySql(connectionString);

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
			}

			return userData;
		}
	}
}

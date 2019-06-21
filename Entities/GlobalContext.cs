using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Pomelo.EntityFrameworkCore.MySql;

using guid = System.UInt64;

namespace Botwinder.entities
{
	public class GlobalContext: DbContext
	{
		public DbSet<GlobalConfig> GlobalConfigs;
		public DbSet<SupportTeamMember> SupportTeam;
		public DbSet<Subscriber> Subscribers;
		public DbSet<PartneredServer> PartneredServers;
		public DbSet<BlacklistEntry> Blacklist;
		public DbSet<LogEntry> Log;
		public DbSet<ExceptionEntry> Exceptions;
		public DbSet<Shard> Shards;
		public DbSet<Localisation> Localisations;
		public DbSet<AntispamUrl> AntispamUrls;

		public GlobalContext(DbContextOptions<GlobalContext> options) : base(options)
		{
			this.GlobalConfigs = new InternalDbSet<GlobalConfig>(this);
			this.SupportTeam = new InternalDbSet<SupportTeamMember>(this);
			this.Subscribers = new InternalDbSet<Subscriber>(this);
			this.PartneredServers = new InternalDbSet<PartneredServer>(this);
			this.Blacklist = new InternalDbSet<BlacklistEntry>(this);
			this.Log = new InternalDbSet<LogEntry>(this);
			this.Exceptions = new InternalDbSet<ExceptionEntry>(this);
			this.Shards = new InternalDbSet<Shard>(this);
			this.Localisations = new InternalDbSet<Localisation>(this);
			this.AntispamUrls = new InternalDbSet<AntispamUrl>(this);
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<GlobalConfig>()
				.HasKey(p => p.ConfigName);

			modelBuilder.Entity<SupportTeamMember>()
				.HasKey(p => p.UserId);

			modelBuilder.Entity<Subscriber>()
				.HasKey(p => p.UserId);

			modelBuilder.Entity<PartneredServer>()
				.HasKey(p => p.ServerId);

			modelBuilder.Entity<BlacklistEntry>()
				.HasKey(p => p.Id);

			modelBuilder.Entity<LogEntry>()
				.HasKey(p => p.Id);

			modelBuilder.Entity<ExceptionEntry>()
				.HasKey(p => p.Id);

			modelBuilder.Entity<Shard>()
				.HasKey(p => p.Id);

			modelBuilder.Entity<Localisation>()
				.HasKey(p => p.Id);

			modelBuilder.Entity<AntispamUrl>()
				.HasKey(p => p.Id);
		}

		public static GlobalContext Create(string connectionString)
		{
			DbContextOptionsBuilder<GlobalContext> optionsBuilder = new DbContextOptionsBuilder<GlobalContext>();
			optionsBuilder.UseMySql(connectionString);

			GlobalContext newContext = new GlobalContext(optionsBuilder.Options);
			newContext.Database.EnsureCreated();
			return newContext;
		}
	}
}

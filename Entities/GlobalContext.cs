using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Pomelo.EntityFrameworkCore.MySql;

using guid = System.UInt64;

namespace Valkyrja.entities
{
	public class GlobalContext: DbContext
	{
		public DbSet<GlobalConfig> GlobalConfigs{ get; set; }
		public DbSet<SupportTeamMember> SupportTeam{ get; set; }
		public DbSet<Subscriber> Subscribers{ get; set; }
		public DbSet<PartneredServer> PartneredServers{ get; set; }
		public DbSet<BlacklistEntry> Blacklist{ get; set; }
		public DbSet<LogEntry> Log{ get; set; }
		public DbSet<ExceptionEntry> Exceptions{ get; set; }
		public DbSet<Shard> Shards{ get; set; }
		public DbSet<Localisation> Localisations{ get; set; }
		public DbSet<AntispamUrl> AntispamUrls{ get; set; }

		private GlobalContext(DbContextOptions<GlobalContext> options) : base(options)
		{
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

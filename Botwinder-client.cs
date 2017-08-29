using System;
using System.Linq;
using System.Threading.Tasks;
using Botwinder.entities;
using Microsoft.EntityFrameworkCore;
using guid = System.Int64;

namespace Botwinder.core
{
	public partial class BotwinderClient<TUser> : IDisposable where TUser : UserData, new()
	{
		private readonly DbConfig DbConfig;
		private readonly GlobalContext GlobalDb;
		private GlobalConfig GlobalConfig;

		public BotwinderClient()
		{
			this.DbConfig = DbConfig.Load();
			this.GlobalDb = GlobalContext.Create(this.DbConfig.GetDbConnectionString());
		}

		public async Task<string> TestDb()
		{
			await Connect();

			return "Loaded global config: " + this.GlobalConfig.ConfigName;
		}

		public void Dispose()
		{
			//todo
		}

		public async Task Connect()
		{
			ReloadConfig();

		}

		public void ReloadConfig()
		{
			if( !this.GlobalDb.GlobalConfigs.Any() )
			{
				this.GlobalDb.GlobalConfigs.Add(new GlobalConfig());
				this.GlobalDb.SaveChanges();
			}

			this.GlobalConfig = this.GlobalDb.GlobalConfigs.First();
		}
	}
}

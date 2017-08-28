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

#pragma warning disable 1998
		public async Task<string> TestDb()
		{
			return "Loaded global config: " + this.GlobalConfig.ConfigName;
		}
#pragma warning restore 1998

		public void Dispose()
		{
			//todo
		}

		public void Connect()
		{
			ReloadConfig();

		}

		public void ReloadConfig()
		{
			this.GlobalConfig = this.GlobalDb.GlobalConfigs.First();
		}
	}
}

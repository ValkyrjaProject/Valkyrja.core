using System;
using System.Threading.Tasks;
using Botwinder.entities;

using guid = System.Int64;

namespace Botwinder.core
{
	public partial class BotwinderClient<TUser> : IDisposable where TUser : UserData, new()
	{
		private async Task InitCommands()
		{
			//todo
		}
	}
}

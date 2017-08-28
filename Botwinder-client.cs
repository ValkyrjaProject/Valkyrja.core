using System;
using Botwinder.entities;

using guid = System.Int64;

namespace Botwinder.core
{
	public partial class BotwinderClient<TUser> : IDisposable where TUser : UserData, new()
	{

		public BotwinderClient()
		{
			//load main json config

		}



		public void Dispose()
		{
			//todo
		}
	}
}

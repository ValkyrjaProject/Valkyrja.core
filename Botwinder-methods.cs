using System;
using System.Threading.Tasks;
using Botwinder.entities;

using guid = System.Int64;

namespace Botwinder.core
{
	public partial class BotwinderClient<TUser> : IDisposable where TUser : UserData, new()
	{
		public async Task LogException(Exception exception, string data)
		{
			ExceptionEntry exceptionEntry = new ExceptionEntry(){
				Message = exception.Message,
				Stack = exception.StackTrace,
				Data = data
			};
			await this.Events.Exception(exceptionEntry);
		}

		private async Task Update()
		{
			//todo
		}
	}
}

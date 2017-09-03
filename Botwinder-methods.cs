using System;
using System.Threading.Tasks;
using Botwinder.entities;

using guid = System.Int64;

namespace Botwinder.core
{
	public partial class BotwinderClient<TUser> : IDisposable where TUser : UserData, new()
	{
		public async Task SendMessage(string message) //todo
		{
			LogEntry logEntry = new LogEntry(){
				Type = LogType.Response,
				//ChannelId = 0,//todo
				//ServerId = 0, //todo
				Message = message
			};
			this.GlobalDb.Log.Add(logEntry);
			this.GlobalDb.SaveChanges();

			//todo
		}

		private async Task Update()
		{

			//todo
		}

		public async Task LogException(Exception exception, string data, guid serverId = 0)
		{
			ExceptionEntry exceptionEntry = new ExceptionEntry(){
				Message = exception.Message,
				Stack = exception.StackTrace,
				Data = data,
				ServerId = serverId
			};
			await this.Events.Exception(exceptionEntry);
		}
	}
}

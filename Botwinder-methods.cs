using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Botwinder.entities;

using guid = System.UInt64;

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


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsGlobalAdmin(guid id)
		{
			return this.GlobalConfig.AdminUserId == id;
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsSubscriber(guid id)
		{
			return this.GlobalDb.PartneredServers.Any(u => u.ServerId == id);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsPartner(guid id)
		{
			return this.GlobalDb.PartneredServers.Any(s => s.ServerId == id);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsPremiumSubscriber(guid id)
		{
			return this.GlobalDb.Subscribers.Any(u => u.UserId == id && u.IsPremium);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsBonusSubscriber(guid id)
		{
			return this.GlobalDb.Subscribers.Any(u => u.UserId == id && u.HasBonus);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsPremiumPartner(guid id)
		{
			return this.GlobalDb.PartneredServers.Any(s => s.ServerId == id && s.IsPremium);
		}


		public async Task LogException(Exception exception, CommandArguments<TUser> args) =>
			await LogException(exception, "--Command: "+ args.Command.Id + " | Parameters: " + args.TrimmedMessage, args.Server.Id);

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

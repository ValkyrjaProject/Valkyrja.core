using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.WebSocket;

using guid = System.UInt64;

namespace Botwinder.entities
{
	public interface IBotwinderClient<TUser> where TUser : UserData, new()
	{
		bool IsConnected{ get; set; }
		GlobalConfig GlobalConfig{ get; set; }
		Shard CurrentShard{ get; set; }
		List<Operation<TUser>> CurrentOperations{ get; set; }
		Object OperationsLock{ get; set; }

		Task SendMessageToChannel(SocketTextChannel channel, string message);
		bool IsGlobalAdmin(guid id);
		bool IsSubscriber(guid id);
		bool IsPartner(guid id);
		bool IsPremiumSubscriber(guid id);
		bool IsBonusSubscriber(guid id);
		bool IsPremiumPartner(guid id);
		Task LogMessage(LogType logType, SocketTextChannel channel, guid authorId, string message);
		Task LogMessage(LogType logType, SocketTextChannel channel, SocketMessage message);
		Task LogException(Exception exception, CommandArguments<TUser> args);
		Task LogException(Exception exception, string data, guid serverId = 0);
	}
}

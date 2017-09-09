using System;
using System.Threading.Tasks;

using guid = System.UInt64;

namespace Botwinder.entities
{
	public interface IBotwinderClient<TUser> where TUser : UserData, new()
	{
		GlobalConfig GlobalConfig{ get; set; }

		bool IsGlobalAdmin(guid id);
		bool IsSubscriber(guid id);
		bool IsPartner(guid id);
		bool IsPremiumSubscriber(guid id);
		bool IsBonusSubscriber(guid id);
		bool IsPremiumPartner(guid id);
		Task LogException(Exception exception, CommandArguments<TUser> args);
		Task LogException(Exception exception, string data, guid serverId = 0);
	}
}

using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Botwinder.entities;

using guid = System.UInt64;

namespace Botwinder.core
{
	public partial class BotwinderClient<TUser> : IDisposable where TUser : UserData, new()
	{
		private Task InitCommands()
		{
			Command<TUser> newCommand = null;


// !global
			newCommand = new Command<TUser>("global");
			newCommand.Type = CommandType.Standard;
			newCommand.Description = "Display all teh numbers.";
			newCommand.RequiredPermissions = PermissionType.OwnerOnly;
			newCommand.OnExecute += async e => {
				StringBuilder shards = new StringBuilder();
				Shard globalCount = new Shard();
				foreach( Shard shard in this.GlobalDb.Shards )
				{
					globalCount.ServerCount += shard.ServerCount;
					globalCount.UserCount += shard.UserCount;
					globalCount.MemoryUsed += shard.MemoryUsed;
					globalCount.ThreadsActive += shard.ThreadsActive;
					globalCount.MessagesTotal += shard.MessagesTotal;
					globalCount.MessagesPerMinute += shard.MessagesPerMinute;
					globalCount.OperationsRan += shard.OperationsRan;
					globalCount.OperationsActive += shard.OperationsActive;
					globalCount.Disconnects += shard.Disconnects;

					shards.AppendLine(shard.GetStatsString());
				}

				string message = "Server Status: <http://status.botwinder.info>\n\n" +
				                 $"Global Servers: `{globalCount.ServerCount}`\n" +
				                 $"Global Members `{globalCount.UserCount}`" +
				                 $"Global Allocated data Memory: `{globalCount.MemoryUsed} MB`\n" +
				                 $"Global Threads: `{globalCount.ThreadsActive}`\n" +
				                 $"Global Messages received: `{globalCount.MessagesTotal}`\n" +
				                 $"Global Messages per minute: `{globalCount.MessagesPerMinute}`\n" +
				                 $"Global Operations ran: `{globalCount.OperationsRan}`\n" +
				                 $"Global Operations active: `{globalCount.OperationsActive}`\n" +
				                 $"Global Disconnects: `{globalCount.Disconnects}`\n" +
				                 $"\n**Shards: `{shards.Length}`\n\n" +
				                 $"{shards.ToString()}";

				await SendMessageToChannel(e.Channel, message);
			};
			this.Commands.Add(newCommand.Id, newCommand);

// !maintenance
			newCommand = new Command<TUser>("maintenance");
			newCommand.Type = CommandType.Standard;
			newCommand.Description = "Performe maintenance";
			newCommand.RequiredPermissions = PermissionType.OwnerOnly;
			newCommand.OnExecute += async e =>{
				await SendMessageToChannel(e.Channel, "Okay, this may take a while...");
				await LogMaintenanceAndExit();
			};
			this.Commands.Add(newCommand.Id, newCommand);

			return Task.CompletedTask;
		}
	}
}

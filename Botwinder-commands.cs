using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
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

// !getServer
			newCommand = new Command<TUser>("getServer");
			newCommand.Type = CommandType.Standard;
			newCommand.Description = "Display some info about specific server with id/name, or owners id/username.";
			newCommand.RequiredPermissions = PermissionType.OwnerOnly;
			newCommand.OnExecute += async e => {
				guid id;
				StringBuilder response = new StringBuilder();
				IEnumerable<ServerStats> foundServers = null;
				if( string.IsNullOrEmpty(e.TrimmedMessage) ||
				    (!guid.TryParse(e.TrimmedMessage, out id) ||
				     !(foundServers = this.ServerDb.ServerStats.Where(s =>
					     s.ServerId == id || s.OwnerId == id || s.ServerName.Contains(e.TrimmedMessage) || s.OwnerName.Contains(e.TrimmedMessage)
				     )).Any()) )
				{
					await SendMessageToChannel(e.Channel, "Server not found.");
					return;
				}

				if( foundServers.Count() > 5 )
				{
					response.AppendLine("__**Found more than 5 servers!**__\n");
				}

				foreach(ServerStats server in foundServers.Take(5))
				{
					response.AppendLine(server.ToString());
					response.AppendLine();
				}

				await SendMessageToChannel(e.Channel, response.ToString());
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

// !getExceptions
			newCommand = new Command<TUser>("getExceptions");
			newCommand.Type = CommandType.Standard;
			newCommand.Description = "Get a list of exceptions.";
			newCommand.RequiredPermissions = PermissionType.OwnerOnly;
			newCommand.OnExecute += async e => {
				StringBuilder response = new StringBuilder();
				if( string.IsNullOrEmpty(e.TrimmedMessage) || !int.TryParse(e.TrimmedMessage, out int n) || n <= 0 )
					n = 10;

				foreach( ExceptionEntry exception in this.GlobalDb.Exceptions.Skip(Math.Max(0, this.GlobalDb.Exceptions.Count() - n)) )
				{
					response.AppendLine(exception.GetMessage());
				}

				string responseString = response.ToString();
				if( string.IsNullOrWhiteSpace(responseString) )
					responseString = "I did not record any errors :stuck_out_tongue:";
				await SendMessageToChannel(e.Channel, responseString);
			};
			this.Commands.Add(newCommand.Id, newCommand);

// !getException
			newCommand = new Command<TUser>("getException");
			newCommand.Type = CommandType.Standard;
			newCommand.Description = "Get an exception stack for specific ID.";
			newCommand.RequiredPermissions = PermissionType.OwnerOnly;
			newCommand.OnExecute += async e => {
				string responseString = "I couldn't find that exception.";
				ExceptionEntry exception = null;
				if( !string.IsNullOrEmpty(e.TrimmedMessage) && int.TryParse(e.TrimmedMessage, out int id) && (exception = this.GlobalDb.Exceptions.FirstOrDefault(ex => ex.Id == id)) != null )
					responseString = exception.GetStack();

				await SendMessageToChannel(e.Channel, responseString);
			};
			this.Commands.Add(newCommand.Id, newCommand);

// !operations
			newCommand = new Command<TUser>("operations");
			newCommand.Type = CommandType.Standard;
			newCommand.Description = "Display info about all queued or running operations on your server.";
			newCommand.RequiredPermissions = PermissionType.ServerOwner | PermissionType.Admin;
			newCommand.OnExecute += async e => {
				StringBuilder response = new StringBuilder();
				bool allOperations = IsGlobalAdmin(e.Message.Author.Id);

				response.AppendLine($"Total operations in the queue: `{this.CurrentOperations.Count}`");
				if( allOperations )
					response.AppendLine($"Currently allocated data Memory: `{(GC.GetTotalMemory(false) / 1000000f):#0.00} MB`");

				response.AppendLine();
				lock( this.OperationsLock )
				{
					foreach( Operation<TUser> op in this.CurrentOperations )
					{
						if( !allOperations && op.CommandArgs.Server.Id != e.Server.Id )
							continue;

						if( allOperations )
							response.AppendLine($"Command: `{op.CommandArgs.Command.Id}`\n" +
							                    $"Status: `{op.CurrentState}`\n" +
							                    $"Author: `{op.CommandArgs.Message.Author.GetUsername()}`\n" +
							                    $"Server: `{op.CommandArgs.Server.Guild.Name}`\n" +
							                    $"ServerID: `{op.CommandArgs.Server.Id}`\n" +
							                    $"Channel: `#{op.CommandArgs.Channel.Name}`\n" +
							                    $"Allocated DataMemory: `{op.AllocatedMemoryStarted:#0.00} MB`\n" +
							                    $"TimeCreated: `{Utils.GetTimestamp(op.TimeCreated)}`\n" +
							                    $"TimeStarted: `{(op.TimeStarted == DateTime.MinValue ? "0" : Utils.GetTimestamp(op.TimeStarted))}`");
						else
							response.AppendLine($"Command: `{op.CommandArgs.Command.Id}`\n" +
							                    $"Status: `{op.CurrentState}`\n" +
							                    $"Author: `{op.CommandArgs.Message.Author.GetUsername()}`\n" +
							                    $"Channel: `#{op.CommandArgs.Channel.Name}`\n" +
							                    $"TimeCreated: `{Utils.GetTimestamp(op.TimeCreated)}`\n" +
							                    $"TimeStarted: `{(op.TimeStarted == DateTime.MinValue ? "0" : Utils.GetTimestamp(op.TimeStarted))}`");
					}
				}

				string responseString = response.ToString();
				if( string.IsNullOrEmpty(responseString) )
					responseString = "There are no operations running.";

				await SendMessageToChannel(e.Channel, responseString);
			};
			this.Commands.Add(newCommand.Id, newCommand);

/*
// !command
			newCommand = new Command<TUser>("command");
			newCommand.Type = CommandType.Standard;
			newCommand.Description = "";
			newCommand.RequiredPermissions = PermissionType.OwnerOnly;
			newCommand.OnExecute += async e => {
				string responseString = "";
				await SendMessageToChannel(e.Channel, responseString);
			};
			this.Commands.Add(newCommand.Id, newCommand);

*/
			return Task.CompletedTask;
		}
	}
}

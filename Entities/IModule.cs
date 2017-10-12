using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using guid = System.UInt64;

namespace Botwinder.entities
{
	public interface IModule
	{
		/// <summary> Correctly log exceptions. </summary>
		Func<Exception, string, guid, Task> HandleException{ get; set; }

		/// <summary> Initialise the module, startup call only. </summary>
		/// <returns> Return a list of Commands for this module. </returns>
		Task<List<Command>> Init(IBotwinderClient client);

		/// <summary> Main Update loop for this module. Do whatever you want. </summary>
		Task Update(IBotwinderClient client);
	}

	/*
	Notes:

	* To send a message to a channel, use client.SendMessageToChannel(channel, string message, embed = null)
	* Do not use channel.SendMessageSafe or channel.SafeMessageAsync directly !!!
	* Use user.SendMessageSafe instead of SendMessageAsync !!!


	public class ExampleModule: IModule
	{
		public Func<Exception, string, guid, Task> HandleException{ get; set; }

		public async Task<List<Command>> Init(IBotwinderClient iClient)
		{
			//This way you can actually use all the sweets that the client offers...
			BotwinderClient client = iClient as BotwinderClient;
			throw new NotImplementedException();
		}

		public Task Update(IBotwinderClient iClient)
		{
			BotwinderClient client = iClient as BotwinderClient;
			throw new NotImplementedException();
		}
	}
	*/
}

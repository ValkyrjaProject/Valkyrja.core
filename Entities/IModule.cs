using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using guid = System.UInt64;

namespace Valkyrja.entities
{
	public interface IModule
	{
		/// <summary> Correctly log exceptions. </summary>
		Func<Exception, string, guid, Task> HandleException{ get; set; }

		/// <summary> Initialise the module, startup call only. </summary>
		/// <returns> Return a list of Commands for this module. </returns>
		List<Command> Init(IValkyrjaClient client);

		/// <summary> True to trigger the Update </summary>
		bool DoUpdate{ get; set; }

		/// <summary> Main Update loop for this module. Do whatever you want. </summary>
		Task Update(IValkyrjaClient client);
	}

	/*
	Notes:

	* To send a message to a channel, use client.SendRawMessageToChannel(channel, string message, embed = null)
	* Within commands, use cmdArgs.SendMessage...
	* Do not use channel.SendMessageSafe or channel.SafeMessageAsync directly !!!
	* Use user.SendMessageSafe instead of SendMessageAsync !!!


	public class ExampleModule: IModule
	{
		public Func<Exception, string, guid, Task> HandleException{ get; set; }

		public List<Command> Init(IValkyrjaClient iClient)
		{
			//This way you can actually use all the sweets that the client offers...
			ValkyrjaClient client = iClient as ValkyrjaClient;
			throw new NotImplementedException();
		}

		public Task Update(IValkyrjaClient iClient)
		{
			ValkyrjaClient client = iClient as ValkyrjaClient;
			throw new NotImplementedException();
		}
	}
	*/
}

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
		Task<List<Command<TUser>>> Init<TUser>(IBotwinderClient<TUser> client) where TUser: UserData, new();

		/// <summary> Main Update loop for this module. Do whatever you want. </summary>
		Task Update<TUser>(IBotwinderClient<TUser> client) where TUser: UserData, new();
	}

	/*
	Notes:

	* To send a message to a channel, use client.SendMessageToChannel(channel, string message, embed = null)
	* Do not use channel.SendMessageSafe or channel.SafeMessageAsync directly !!!
	* Use user.SendMessageSafe instead of SendMessageAsync !!!


	public class ExampleModule: IModule
	{
		public Func<Exception, string, guid, Task> HandleException{ get; set; }

		public async Task<List<Command>> Init<TUser>(IBotwinderClient<TUser> iClient) where TUser : UserData, new()
		{
			//This way you can actually use all the sweets that the client offers...
			BotwinderClient<TUser> client = iClient as BotwinderClient<TUser>;
			throw new NotImplementedException();
		}

		public Task Update<TUser>(IBotwinderClient<TUser> iClient) where TUser : UserData, new()
		{
			BotwinderClient<TUser> client = iClient as BotwinderClient<TUser>;
			throw new NotImplementedException();
		}
	}
	*/
}

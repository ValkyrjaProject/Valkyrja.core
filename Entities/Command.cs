using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Discord.WebSocket;

using guid = System.UInt64;

namespace Botwinder.entities
{
	public class Command<TUser> where TUser: UserData, new()
	{
		/// <summary> ID of a command is what you use in combination with the command character to execute it. </summary>
		public string Id{ get; set; } = "";

		/// <summary> List of aliases to this command. </summary>
		public List<string> Aliases{ get; set; }

		/// <summary> Parent of this command, if it is an alias. </summary>
		public string ParentId{ get; set; }

		/// <summary> Set to true to handle as cancelable and tracked long running operation. </summary>
		public CommandType Type{ get; set; } = CommandType.Standard;

		/// <summary> Send the "typing" event before executing the command? </summary>
		public bool SendTyping{ get; set; } = true;

		/// <summary> Delete the message that issued the command? </summary>
		public bool DeleteRequest{ get; set; }

		/// <summary> True if this command is only alias, and not the original. </summary>
		public bool IsAlias{ get; internal set; }

		/// <summary> True if this command is hidden from the list of commands. </summary>
		public bool IsHidden{ get; set; }

		/// <summary> True if this command can not be changed by PermissionOverrides </summary>
		public bool IsCoreCommand{ get; set; }

		/// <summary> True if this is a custom command. </summary>
		public bool IsCustomCommand{ get; set; }

		/// <summary> Subscriber bonus only </summary>
		public bool IsBonusCommand{ get; set; }

		/// <summary> Subscriber bonus only </summary>
		public bool IsPremiumCommand{ get; set; }

		/// <summary> Subscriber bonus only </summary>
		public bool IsPremiumServerwideCommand{ get; set; }

		/// <summary> Use <c>Command.PermissionType</c> to determine who can use this command. Defaults to ServerOwnder + Whitelisted or Everyone </summary>
		public int RequiredPermissions = PermissionType.Everyone | PermissionType.ServerOwner;

		/// <summary> Description of this command will be used when the user invokes `help` command. </summary>
		public string Description{ get; set; } = "";

		/// <summary> The Code Stuff! </summary>
		public Func<CommandArguments<TUser>, Task> OnExecute = null;

		/// <summary> Initializes a new instance of the <see cref="Botwinder.entities.Command"/> class. </summary>
		/// <param name="id"> You will execute the command by using CommandCharacter+Command.ID </param>
		public Command(string id)
		{
			this.Id = id;
		}

		/// <summary> Creates an alias to the Command and returns it as new command. This is runtime alias, which means that it will not affect all the servers. </summary>
		public Command<TUser> CreateRuntimeAlias(string alias)
		{
			Command<TUser> newCommand = CreateCopy(alias);
			newCommand.IsAlias = true;
			newCommand.ParentId = this.Id;

			return newCommand;
		}

		/// <summary> Creates an alias to the Command and returns it as new command. </summary>
		public Command<TUser> CreateAlias(string alias)
		{
			Command<TUser> newCommand = CreateRuntimeAlias(alias);

			if( this.Aliases == null )
				this.Aliases = new List<string>();

			this.Aliases.Add(alias);

			return newCommand;
		}

		/// <summary> Creates a copy of the Command and returns it as new command. This does not copy Aliases. </summary>
		public Command<TUser> CreateCopy(string newID)
		{
			if( this.IsAlias )
				throw new Exception("Command.CreateCopy: Trying to create a copy of an alias.");

			Command<TUser> newCommand = new Command<TUser>(newID);
			newCommand.Id = newID;
			newCommand.SendTyping = this.SendTyping;
			newCommand.DeleteRequest = this.DeleteRequest;
			newCommand.IsHidden = this.IsHidden;
			newCommand.IsCoreCommand = this.IsCoreCommand;
			newCommand.IsCustomCommand = this.IsCustomCommand;
			newCommand.Type = this.Type;
			newCommand.IsBonusCommand = this.IsBonusCommand;
			newCommand.IsPremiumCommand = this.IsPremiumCommand;
			newCommand.IsPremiumServerwideCommand = this.IsPremiumServerwideCommand;
			newCommand.RequiredPermissions = this.RequiredPermissions;
			newCommand.Description = this.Description;
			newCommand.OnExecute = this.OnExecute;
			return newCommand;
		}

		/// <summary> Returns true if the User has permission to execute this command. </summary>
		/// <param name="commandChannelOptions"> List of all the channel options for specific command. </param>
		public bool CanExecute(IBotwinderClient<TUser> client, Server<TUser> server, SocketGuildChannel channel, SocketGuildUser user)
		{
			if( client.IsGlobalAdmin(user.Id) )
				return true;

			//Premium-only commands
			if( this.IsPremiumCommand && !client.IsPremiumSubscriber(user.Id) )
				return false;
			if( this.IsBonusCommand && !client.IsBonusSubscriber(user.Id) )
				return false;
			if( this.IsPremiumServerwideCommand && !client.IsPremiumSubscriber(server.Guild.OwnerId) && !client.IsPremiumPartner(server.Id) )
				return false;

			return server.CanExecuteCommand(this.Id, this.RequiredPermissions, channel, user);
		}

		public async Task<bool> Execute(CommandArguments<TUser> e)
		{
			if( this.OnExecute == null )
				return false;

			if( (this.DeleteRequest || (e.CommandOptions != null && e.CommandOptions.DeleteRequest)) && e.Server.Guild.CurrentUser.GuildPermissions.ManageMessages )
			{
				try
				{
					await e.Message.DeleteAsync();
				} catch(Exception)
				{}
			}

			try
			{
				e.Client.LogMessage(LogType.Command, e.Channel, e.Message);

				if( this.SendTyping )
					await e.Channel.TriggerTypingAsync();

				if( !string.IsNullOrWhiteSpace(e.TrimmedMessage) && e.TrimmedMessage == "help" )
				{
					await e.Client.SendMessageToChannel(e.Channel, e.Command.Description);
					return true;
				}

				if( this.Type == CommandType.Standard )
				{
					Task task = this.OnExecute(e);
					if( await Task.WhenAny(task, Task.Delay(GlobalConfig.CommandExecutionTimeout)) == task )
					{
						await task;
					}
					else
					{
						await e.Client.SendMessageToChannel(e.Channel, "Command execution timed out. _(Please wait a moment before trying again.)_");
					}
				}
				else
				{
					Operation<TUser> operation = Operation<TUser>.Create(e);
					await operation.Execute();
				}
			} catch(Exception exception)
			{
				await e.Client.LogException(exception, e);
			}
			return true;
		}
	}

	public class CommandArguments<TUser> where TUser: UserData, new() //todo ...
	{
		/// <summary> Reference to the client. </summary>
		public IBotwinderClient<TUser> Client{ get; private set; }

		/// <summary> The parrent command. </summary>
		public Command<TUser> Command{ get; private set; }

		/// <summary> Custom server-side options, can be null! </summary>
		public CommandOptions CommandOptions{ get; private set; }

		/// <summary> Server, where this command was executed. Null for PM. </summary>
		public Server<TUser> Server{ get; private set; }

		/// <summary> Message, where the command was invoked. </summary>
		public SocketMessage Message{ get; private set; }

		/// <summary> Channel, where the command was invoked. </summary>
		public SocketTextChannel Channel{ get; private set; }

		/// <summary> Text of the Message, where the command was invoked. The command itself is excluded. </summary>
		public string TrimmedMessage{ get; private set; }

		/// <summary> Command parameters (individual words) from the original message. MessageArgs[0] == Command.ID; </summary>
		public string[] MessageArgs{ get; private set; }

		/// <summary> Null if this is standard command. </summary>
		public Operation<TUser> Operation{ get; set; } //Necessary evul.


		public CommandArguments(IBotwinderClient<TUser> client, Command<TUser> command, Server<TUser> server, SocketTextChannel channel, SocketMessage message, string trimmedMessage, string[] messageArgs, CommandOptions options = null)
		{
			this.Client = client;
			this.Command = command;
			this.CommandOptions = options;
			this.Server = server;
			this.Channel = channel;
			this.Message = message;
			this.TrimmedMessage = trimmedMessage;
			this.MessageArgs = messageArgs;
		}
	}

	public enum CommandType
	{
		Standard,
		Operation,
		LargeOperation
	}
}

using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Discord;
using Discord.Net;
using Discord.Rest;
using Discord.WebSocket;

using guid = System.UInt64;

namespace Valkyrja.entities
{
	public class Command
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

		/// <summary> True if this command can be executed by SupportTeamMembers </summary>
		public bool IsSupportCommand{ get; set; }

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
		public Func<CommandArguments, Task> OnExecute = null;

		/// <summary> Initializes a new instance of the <see cref="Valkyrja.entities.Command"/> class. </summary>
		/// <param name="id"> You will execute the command by using CommandCharacter+Command.ID </param>
		public Command(string id)
		{
			this.Id = id;
		}

		/// <summary> Creates an alias to the Command and returns it as new command. This is runtime alias, which means that it will not affect all the servers. </summary>
		public Command CreateRuntimeAlias(string alias)
		{
			Command newCommand = CreateCopy(alias);
			newCommand.IsAlias = true;
			newCommand.ParentId = this.Id;

			return newCommand;
		}

		/// <summary> Creates an alias to the Command and returns it as new command. </summary>
		public Command CreateAlias(string alias)
		{
			Command newCommand = CreateRuntimeAlias(alias);

			if( this.Aliases == null )
				this.Aliases = new List<string>();

			this.Aliases.Add(alias);

			return newCommand;
		}

		/// <summary> Creates a copy of the Command and returns it as new command. This does not copy Aliases. </summary>
		public Command CreateCopy(string newID)
		{
			if( this.IsAlias )
				throw new Exception("Command.CreateCopy: Trying to create a copy of an alias.");

			Command newCommand = new Command(newID);
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
		public bool CanExecute(IValkyrjaClient client, Server server, SocketGuildChannel channel, SocketGuildUser user)
		{
			if( client.IsGlobalAdmin(user.Id) )
				return true;

			if( this.IsSupportCommand && client.IsSupportTeam(user.Id) )
				return true;

			//Premium-only commands
			if( this.IsPremiumCommand && !client.IsPremiumSubscriber(user.Id) )
				return false;
			if( this.IsBonusCommand && !client.IsBonusSubscriber(user.Id) )
				return false;
			if( this.IsPremiumServerwideCommand && !(client.IsPremiumSubscriber(server.Guild.OwnerId) || client.IsPremiumPartner(server.Id)) && !client.IsTrialServer(server.Id) )
				return false;

			return server.CanExecuteCommand(this.Id, this.RequiredPermissions, channel, user);
		}

		public async Task<bool> Execute(CommandArguments e)
		{
			if( this.OnExecute == null )
				return false;

			if( (this.DeleteRequest || (e.CommandOptions != null && e.CommandOptions.DeleteRequest)) && e.Server.Guild.CurrentUser.GuildPermissions.ManageMessages )
			{
				try
				{
					if( !e.Message.Deleted )
						await e.Message.DeleteAsync();
				}
				catch( HttpException exception )
				{
					await e.Server.HandleHttpException(exception, $"Failed to delete the command message in <#{e.Channel.Id}>");
				}
				catch( Exception exception )
				{
					await e.Client.LogException(exception, e);
				}
			}

			try
			{
				e.Client.Monitoring.Commands.Inc();
				await e.Client.LogMessage(LogType.Command, e.Channel, e.Message);

				if( this.SendTyping )
					await e.Channel.TriggerTypingAsync();

				if( !string.IsNullOrWhiteSpace(e.TrimmedMessage) && e.TrimmedMessage == "help" )
				{
					await e.SendReplySafe(e.Command.Description);
					return true;
				}

				if( this.Type == CommandType.Standard )
				{
					Task task = this.OnExecute(e);
					/*if( await Task.WhenAny(task, Task.Delay(GlobalConfig.CommandExecutionTimeout)) == task ) //todo
					{*/
					await task;
					/*}
					else
					{
						await e.SendReplySafe("Command execution timed out. _(Please wait a moment before trying again.)_");
						throw new TimeoutException();
					}*/
				}
				else
				{
					Operation operation = Operation.Create(e);
					await operation.Execute();
				}
			}
			catch( HttpException exception )
			{
				await e.Server.HandleHttpException(exception, $"This happened in <#{e.Channel.Id}> when executing command `{e.CommandId}`");
			}
			catch( Exception exception )
			{
				await e.Client.LogException(exception, e);
			}

			return true;
		}
	}

	public class CommandArguments
	{
		/// <summary> Reference to the client. </summary>
		public IValkyrjaClient Client{ get; private set; }

		/// <summary> The parrent command. </summary>
		public Command Command{ get; private set; }

		/// <summary> Custom server-side options, can be null! </summary>
		public CommandOptions CommandOptions{ get; private set; }

		/// <summary> Server, where this command was executed. Null for PM. </summary>
		public Server Server{ get; private set; }

		/// <summary> Message, where the command was invoked. </summary>
		public SocketMessage Message{ get; private set; }

		/// <summary> Channel, where the command was invoked. </summary>
		public SocketTextChannel Channel{ get; private set; }

		/// <summary> Text of the Message, where the command was invoked. The command itself is excluded. </summary>
		public string CommandId{ get; private set; }

		/// <summary> Text of the Message, where the command was invoked. The command itself is excluded. </summary>
		public string TrimmedMessage{ get; private set; }

		/// <summary> Command parameters (individual words) from the original message. MessageArgs[0] == Command.ID; </summary>
		public string[] MessageArgs{ get; internal set; }

		/// <summary> Null if this is standard command. </summary>
		public Operation Operation{ get; set; } //Necessary evul.


		public CommandArguments(IValkyrjaClient client, Command command, Server server, SocketTextChannel channel, SocketMessage message, string commandId, string trimmedMessage, string[] messageArgs, CommandOptions options = null)
		{
			this.Client = client;
			this.Command = command;
			this.CommandOptions = options;
			this.Server = server;
			this.Channel = channel;
			this.Message = message;
			this.CommandId = commandId;
			this.TrimmedMessage = trimmedMessage;
			this.MessageArgs = messageArgs;
		}

		public async Task SendReplySafe(string message)
		{
			await this.Client.LogMessage(LogType.Response, this.Channel, this.Client.GlobalConfig.UserId, message);

			if( this.Server.Config.IgnoreEveryone )
				message = message.Replace("@everyone", "@-everyone").Replace("@here", "@-here");

			await this.Channel.SendMessageSafe(message);
		}

		public async Task SendReplyUnsafe(string message)
		{
			await this.Client.LogMessage(LogType.Response, this.Channel, this.Client.GlobalConfig.UserId, message);

			if( this.Server.Config.IgnoreEveryone )
				message = message.Replace("@everyone", "@-everyone").Replace("@here", "@-here");

			RestUserMessage msg = await this.Channel.SendMessageAsync(message, allowedMentions: new AllowedMentions(AllowedMentionTypes.Users | AllowedMentionTypes.Everyone));

			if( this.CommandOptions != null && this.CommandOptions.DeleteReply )
			{
				await Task.Delay(3000);
				await msg.DeleteAsync();
			}
		}
	}

	public enum CommandType
	{
		Standard,
		Operation,
		LargeOperation
	}
}

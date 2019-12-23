using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Discord.WebSocket;

using guid = System.UInt64;

namespace Valkyrja.entities
{
	[Table("custom_commands")]
	public class CustomCommand
	{
		[Column("serverid")]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public guid ServerId{ get; set; } = 0;

		[Column("commandid", TypeName = "varchar(127)")]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public string CommandId{ get; set; } = "";

		[Column("response", TypeName = "text")]
		public string Response{ get; set; } = "This custom command was not configured.";

		[Column("description", TypeName = "text")]
		public string Description{ get; set; } = "This is custom command on this server.";

		/// <summary> Returns true if the User has permission to execute this command. </summary>
		/// <param name="commandChannelOptions"> List of all the channel options for specific command. </param>
		public bool CanExecute(IValkyrjaClient client, Server server, SocketGuildChannel channel,
			SocketGuildUser user)
		{
			if( client.IsGlobalAdmin(user.Id) )
				return true;

			return server.CanExecuteCommand(this.CommandId, PermissionType.Everyone, channel, user);
		}
	}

	[Table("custom_aliases")]
	public class CustomAlias
	{
		[Column("serverid")]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public guid ServerId{ get; set; } = 0;

		[Column("commandid", TypeName = "varchar(255)")]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public string CommandId{ get; set; } = "";

		[Column("alias", TypeName = "varchar(255)")]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public string Alias{ get; set; } = "";
	}
}

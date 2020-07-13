using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

using guid = System.UInt64;

namespace Valkyrja.entities
{
	[Table("command_options")]
	public class CommandOptions
	{
		[Column("serverid")]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public guid ServerId{ get; set; } = 0;

		[Column("commandid", TypeName = "varchar(255)")]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public string CommandId{ get; set; } = "";

		[Column("permission_overrides", TypeName = "tinyint")]
		public PermissionOverrides PermissionOverrides{ get; set; } = PermissionOverrides.Default;

		[Column("delete_request")]
		public bool DeleteRequest{ get; set; } = false;

		[Column("delete_reply")]
		public bool DeleteReply{ get; set; } = false;
	}

	[Table("command_channel_options")]
	public class CommandChannelOptions
	{
		[Column("serverid")]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public guid ServerId{ get; set; } = 0;

		[Column("commandid", TypeName = "varchar(255)")]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public string CommandId{ get; set; } = "";

		[Column("channelid")]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public guid ChannelId{ get; set; } = 0;

		[Column("blacklisted")] //Not gonna mess with production database.
		public bool Blocked{ get; set; } = false;

		[Column("whitelisted")] //Not gonna mess with production database.
		public bool Allowed{ get; set; } = false;
	}
}

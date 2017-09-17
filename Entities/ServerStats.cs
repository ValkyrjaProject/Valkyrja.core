using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;

using guid = System.UInt64;

namespace Botwinder.entities
{
	public class ServerStats
	{
		[Key]
		[Required]
		[Column("serverid")]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public guid ServerId{ get; set; } = 0;

		[Column("name", TypeName = "varchar(255)")]
		public string ServerName{ get; set; } = "";

		[Column("shardid")]
		public Int64 ShardId{ get; set; } = 0;

		[Column("ownerid")]
		public guid OwnerId{ get; set; } = 0;

		[Column("owner_name", TypeName = "varchar(255)")]
		public string OwnerName{ get; set; } = "";

		[Column("joined_count")]
		public Int64 JoinedCount{ get; set; } = 0;

		[Column("joined_first")]
		public DateTime JoinedTimeFirst{ get; set; } = DateTime.MaxValue;

		[Column("joined_last")]
		public DateTime JoinedTime{ get; set; } = DateTime.MaxValue;

		[Column("user_count")]
		public Int64 UserCount{ get; set; } = 0;

		[Column("vip")]
		public bool IsDiscordPartner{ get; set; } = false;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override string ToString()
		{
			return $"**Server ID: `{this.ServerId}`**\n" +
			       $"Server Name: `{this.ServerName}`\n" +
			       $"Owner: <@{this.OwnerId}>\n" +
			       $"Owner ID: `{this.OwnerId}`\n" +
			       $"Owner name: `{this.OwnerName}`\n" +
			       $"Discord Partner: `{this.IsDiscordPartner}`\n" +
			       $"Members count: `{this.UserCount}`" +
			       $"Shard ID: `{this.ShardId}`\n" +
			       $"First Joined at: `{Utils.GetTimestamp(this.JoinedTimeFirst)}`\n" +
			       $"Last Joined at: `{Utils.GetTimestamp(this.JoinedTime)}`\n" +
			       $"Joined count: `{this.JoinedCount}`\n";
		}
	}
}

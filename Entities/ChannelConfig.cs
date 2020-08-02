using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

using guid = System.UInt64;

namespace Valkyrja.entities
{
	[Table("channels")]
	public class ChannelConfig
	{
		[Key]
		[Required]
		[Column("channelid")]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public guid ChannelId{ get; set; } = 0;

		[Required]
		[Column("serverid")]
		public guid ServerId{ get; set; } = 0;

		[Column("ignored")]
		public bool Ignored{ get; set; } = false;

		[Column("auto_announce")]
		public bool AutoAnnounce{ get; set; } = false;

		[Column("temporary")]
		public bool Temporary{ get; set; } = false;

		[Column("muted")]
		public bool Muted{ get; set; } = false;

		[Column("muted_until")]
		public DateTime MutedUntil{ get; set; } = DateTime.MinValue;
	}
}

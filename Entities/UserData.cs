using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

using guid = System.Int64;

namespace Botwinder.entities
{
	[Table("users")]
	public class UserData
	{
		[Required]
		[Column("serverid")]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public guid ServerId{ get; set; } = 0;

		[Required]
		[Column("userid")]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public guid UserId{ get; set; } = 0;

		[Column("verified")]
		public bool Verified{ get; set: } = false;
		
		[Column("karma_count")]
		public Int64 KarmaCount{ get; set; } = 0;
		
		[Column("warning_count")]
		public Int64 WarningCount{ get; set; } = 0;
		
		[Column("notes", TypeName = "text")]
		public string Notes{ get; set; } = "";
		
		[Column("last_thanks_time")]
		public DateTime LastThanksTime{ get; set; } = DateTime.MinValue;
		
		[Column("banned_until")]
		public DateTime BannedUntil{ get; set; } = DateTime.MinValue;
		
		[Column("muted_until")]
		public DateTime MutedUntil{ get; set; } = DateTime.MinValue;

		[Column("ignored")]
		public bool Ignored{ get; set: } = false;
		
		[Column("count_messages")]
		public Int64 CountMessages{ get; set; } = 0;
		
		[Column("count_attachments")]
		public Int64 CountAttachments{ get; set; } = 0;
		
		[Column("level_relative")]
		public Int64 LevelRelative{ get; set; } = 0;
		
		[Column("exp_relative")]
		public Int64 ExpRelative{ get; set; } = 0;

		public List<Username> Usernames;
		public List<Nickname> Nicknames;
	}

	[Table("usernames")]
	public class Username
	{
		[Key]
		[Column("id")]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public Int64 Id{ get; set; } = 0;

		[Required]
		[Column("serverid")]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public guid ServerId{ get; set; } = 0;

		[Required]
		[Column("userid")]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public guid UserId{ get; set; } = 0;

		[Column("username")]
		public string Name{ get; set; } = "";

		public UserData UserData{ get; set; }
	}

	[Table("nicknames")]
	public class Nickname
	{
		[Key]
		[Column("id")]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public Int64 Id{ get; set; } = 0;

		[Required]
		[Column("serverid")]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public guid ServerId{ get; set; } = 0;

		[Required]
		[Column("userid")]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public guid UserId{ get; set; } = 0;

		[Column("nickname")]
		public string Name{ get; set; } = "";

		public UserData UserData{ get; set; }
	}
}

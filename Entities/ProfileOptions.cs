using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using guid = System.UInt64;

namespace Valkyrja.entities
{
	[Table("profile_options")]
	public class ProfileOption
	{
		[Required]
		[Column("serverid")]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public guid ServerId{ get; set; } = 0;

		[Required]
		[Column("option", TypeName = "varchar(255)")]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public string Option{ get; set; } = "";

		[Column("option_alt", TypeName = "varchar(255)")]
		public string OptionAlt{ get; set; } = "";

		[Column("label", TypeName = "varchar(512)")]
		public string Label{ get; set; } = "";

		[Column("property_order")]
		public Int64 Order{ get; set; } = 0;

		[Column("inline")]
		public bool IsInline{ get; set; } = false;
	}
}

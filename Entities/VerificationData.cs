using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using guid = System.UInt64;

namespace Valkyrja.entities
{
	[Table("verification_data")]
	public class VerificationData
	{
		[Required]
		[Column("serverid")]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public guid ServerId{ get; set; } = 0;

		[Required]
		[Column("userid")]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public guid UserId{ get; set; } = 0;

		[Column("value", TypeName = "varchar(255)")]
		public string Value{ get; set; } = "";
	}
}

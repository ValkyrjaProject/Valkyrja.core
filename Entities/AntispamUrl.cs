using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

using guid = System.UInt64;

namespace Botwinder.entities
{
	[Table("antispam_urls")]
	public class AntispamUrl
	{
		[Key]
		[Required]
		[Column("id")]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public guid Id{ get; set; } = 0;

		[Column("url", TypeName = "varchar(255)")]
		public string Url{ get; set; } = "";
	}
}

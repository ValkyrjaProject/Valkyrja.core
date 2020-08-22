using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

using guid = System.UInt64;

namespace Valkyrja.entities
{
/*	[Table("category_mute_roles")]
	public class CategoryMuteRole
	{
		[Key]
		[Required]
		[Column("serverid")]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public guid ServerId{ get; set; } = 0;

		[Required]
		[Column("mod_roleid")]
		public guid ModRoleId{ get; set; } = 0;

		[Required]
		[Column("mute_roleid")]
		public guid MuteRoleId{ get; set; } = 0;
	}
*/
	[Table("category_member_roles")]
	public class CategoryMemberRole
	{
		[Key]
		[Required]
		[Column("serverid")]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public guid ServerId{ get; set; } = 0;

		[Required]
		[Column("mod_roleid")]
		public guid ModRoleId{ get; set; } = 0;

		[Required]
		[Column("member_roleid")]
		public guid MemberRoleId{ get; set; } = 0;
	}
}

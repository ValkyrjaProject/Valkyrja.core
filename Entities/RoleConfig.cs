﻿using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

using guid = System.UInt64;

namespace Valkyrja.entities
{
	[Table("roles")]
	public class RoleConfig
	{
		[Key]
		[Required]
		[Column("roleid")]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public guid RoleId{ get; set; } = 0;

		[Required]
		[Column("serverid")]
		public guid ServerId{ get; set; } = 0;

		[Column("permission_level", TypeName = "tinyint")]
		public RolePermissionLevel PermissionLevel{ get; set; } = RolePermissionLevel.None;

		[Column("public_id")]
		public Int64 PublicRoleGroupId{ get; set; } = 0;

		[Column("logging_ignored")]
		public bool LoggingIgnored{ get; set; } = false;

		[Column("antispam_ignored")]
		public bool AntispamIgnored{ get; set; } = false;

		[Column("inverse_persistence")]
		public bool InversePersistence{ get; set; } = false;

		[Column("persistence_user_flag")]
		public Int64 PersistenceUserFlag{ get; set; } = 0;

		[Column("level")]
		public Int64 ExpLevel{ get; set; } = 0;

		//[Column("temporary")]
		//public bool Temporary{ get; set; } = false;

		//[Column("delete_at_time")]
		//public DateTime DeleteAtTime{ get; set; } = DateTime.MinValue;
	}

	[Table("role_groups")]
	public class RoleGroupConfig
	{
		[Required]
		[Column("serverid")]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public guid ServerId{ get; set; } = 0;

		[Required]
		[Column("groupid")]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public Int64 GroupId{ get; set; } = 0;

		[Column("role_limit")]
		public Int64 RoleLimit{ get; set; } = 1;

		[Column("name", TypeName = "varchar(255)")]
		public string Name{ get; set; } = "";
	}

	[Table("reaction_roles")]
	public class ReactionAssignedRole
	{
		[Key]
		[Required]
		[Column("roleid")]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public guid RoleId{ get; set; } = 0;

		[Required]
		[Column("serverid")]
		public guid ServerId{ get; set; } = 0;

		[Required]
		[Column("messageid")]
		public guid MessageId{ get; set; } = 0;

		[Column("emoji", TypeName = "varchar(255)")]
		public string Emoji{ get; set; } = "";
	}
}

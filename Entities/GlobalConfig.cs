using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

using guid = System.UInt64;

namespace Valkyrja.entities
{
	[Table("support_team")]
	public class SupportTeamMember
	{
		public guid UserId{ get; set; } = 0;
	}

	[Table("global_config")]
	public class GlobalConfig
	{
		public const int MessageCharacterLimit = 2000;
		public const int CommandExecutionTimeout = 10000;
		public const string DataFolder = "data";
		public const string DiscordInvite = "https://discord.gg/XgVvkXx";


		[Key]
		[Required]
		[Column("configuration_name", TypeName = "varchar(255)")]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public string ConfigName{ get; set; } = "default";

		[Column("discord_token", TypeName = "varchar(255)")]
		public string DiscordToken{ get; set; } = "";

		[Column("userid")]
		public guid UserId{ get; set; } = 278834060053446666;

		[Column("admin_userid")]
		public guid AdminUserId{ get; set; } = 89805412676681728;

		[Column("enforce_requirements")]
		public bool EnforceRequirements{ get; set; } = false;

		[Column("verification_enabled")]
		public bool VerificationUpdateEnabled{ get; set; } = false;

		[Column("timers_enabled")]
		public bool ModuleUpdateEnabled{ get; set; } = false;

		[Column("polls_enabled")]
		public bool PollsEnabled{ get; set; } = false; //Not Implemented anymore (legacy from Mk.II)

		[Column("events_enabled")]
		public bool EventsEnabled{ get; set; } = false; //Not Implemented anymore (legacy from Mk.II)

		[Column("giveaways_enabled")]
		public bool GiveawaysEnabled{ get; set; } = false; //Not Implemented anymore (legacy from Mk.II)

		[Column("livestream_enabled")]
		public bool LivestreamEnabled{ get; set; } = false; //Not Implemented anymore (legacy from Mk.II)

		[Column("total_shards")]
		public Int64 TotalShards{ get; set; } = 1;

		[Column("initial_update_delay")]
		public Int64 InitialUpdateDelay{ get; set; } = 3;

		[Column("command_prefix", TypeName = "varchar(255)")]
		public string CommandPrefix{ get; set; } = "!"; //Not Implemented anymore (legacy from Mk.II)

		[Column("main_serverid")]
		public guid MainServerId{ get; set; } = 155821059960995840;

		[Column("main_channelid")]
		public guid MainChannelId{ get; set; } = 170139120318808065;

		[Column("vip_skip_queue")]
		public bool VipSkipQueue{ get; set; } = false;

		[Column("vip_members_max")]
		public Int64 MinMembers{ get; set; } = 0; //vip_members_max is no longer used, using this as threshold under which the bot leaves the server.

		[Column("vip_trial_hours")]
		public Int64 VipTrialHours{ get; set; } = 36;

		[Column("vip_trial_joins")]
		public Int64 VipTrialJoins{ get; set; } = 5;

		[Column("antispam_clear_interval")]
		public Int64 AntispamClearInterval{ get; set; } = 0;

		[Column("antispam_safety_limit")]
		public Int64 AntispamSafetyLimit{ get; set; } = 0;

		[Column("antispam_fastmessages_per_update")]
		public Int64 AntispamFastmessagesPerUpdate{ get; set; } = 0;

		[Column("antispam_update_interval")]
		public Int64 AntispamUpdateInterval{ get; set; } = 0;

		[Column("antispam_message_cache_size")]
		public Int64 AntispamMessageCacheSize{ get; set; } = 0;

		[Column("antispam_allowed_duplicates")]
		public Int64 AntispamAllowedDuplicateMessages{ get; set; } = 0;

		[Column("antispam_allowed_duplicateusers")]
		public Int64 AntispamAllowedDuplicateUsernames{ get; set; } = 0;

		[Column("antispam_duplicateusers_period")]
		public Int64 AntispamUsernamePeriod{ get; set; } = 0;

		[Column("target_fps")]
		public float TargetFps{ get; set; } = 0.05f;

		[Column("message_cache_size")]
		public int ChannelMessageCacheSize{ get; set; } = 500;

		[Column("operations_max")]
		public Int64 OperationsMax{ get; set; } = 2;

		[Column("operations_extra")]
		public Int64 OperationsExtra{ get; set; } = 1;

		[Column("maintenance_memory_threshold")]
		public Int64 MaintenanceMemoryThreshold{ get; set; } = 3000;

		[Column("maintenance_thread_threshold")]
		public Int64 MaintenanceThreadThreshold{ get; set; } = 44;

		[Column("maintenance_operations_threshold")]
		public Int64 MaintenanceOperationsThreshold{ get; set; } = 300;

		[Column("maintenance_disconnect_threshold")]
		public Int64 MaintenanceDisconnectsThreshold{ get; set; } = 20;

		[Column("log_debug")]
		public bool LogDebug{ get; set; } = true;

		[Column("log_exceptions")]
		public bool LogExceptions{ get; set; } = true;

		[Column("log_commands")]
		public bool LogCommands{ get; set; } = true;

		[Column("log_responses")]
		public bool LogResponses{ get; set; } = true;
	}

	[Table("subscribers")]
	public class Subscriber
	{
		[Key]
		[Required]
		[Column("userid")]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public guid UserId{ get; set; } = 0;

		[Column("premium")]
		public bool IsPremium{ get; set; } = false;

		[Column("has_bonus")]
		public bool HasBonus{ get; set; } = false;
	}

	[Table("partners")]
	public class PartneredServer
	{
		[Key]
		[Required]
		[Column("serverid")]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public guid ServerId{ get; set; } = 0;

		[Column("premium")]
		public bool IsPremium{ get; set; } = false;
	}

	[Table("blacklist")]
	public class BlacklistEntry
	{
		[Key]
		[Required]
		[Column("id")]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public guid Id{ get; set; } = 0;
	}
}

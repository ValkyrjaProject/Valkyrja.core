using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

using guid = System.UInt64;

namespace Botwinder.entities
{
	[Table("server_config")]
	public class ServerConfig
	{
		[Key]
		[Required]
		[Column("serverid")]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public guid ServerId{ get; set; } = 0;

		[Column("name", TypeName = "varchar(255)")]
		public string Name{ get; set; } = "";

		[Column("invite_url", TypeName = "varchar(255)")]
		public string InviteUrl{ get; set; } = "";

		[Column("localisation_id")]
		public guid LocalisationId{ get; set; } = 0;

		[Column("timezone_utc_relative")]
		public Int64 TimezoneUtcRelative{ get; set; } = 0;

		[Column("ignore_bots")]
		public bool IgnoreBots{ get; set; } = true;

		[Column("ignore_everyone")]
		public bool IgnoreEveryone{ get; set; } = true;

		[Column("command_prefix", TypeName = "varchar(255)")]
		public string CommandPrefix{ get; set; } = "!";

		[Column("command_prefix_alt", TypeName = "varchar(255)")]
		public string CommandPrefixAlt{ get; set; } = "";

		[Column("execute_on_edit")]
		public bool ExecuteOnEdit{ get; set; } = true;

		[Column("antispam_priority")]
		public bool AntispamPriority{ get; set; } = false;

		[Column("antispam_invites")]
		public bool AntispamInvites{ get; set; } = false;

		[Column("antispam_invites_ban")]
		public bool AntispamInvitesBan{ get; set; } = false;

		[Column("antispam_duplicate")]
		public bool AntispamDuplicate{ get; set; } = false;

		[Column("antispam_duplicate_crossserver")]
		public bool AntispamDuplicateCrossserver{ get; set; } = false;

		[Column("antispam_duplicate_ban")]
		public bool AntispamDuplicateBan{ get; set; } = false;

		[Column("antispam_mentions_max")]
		public Int64 AntispamMentionsMax{ get; set; } = 0;

		[Column("antispam_mentions_ban")]
		public bool AntispamMentionsBan{ get; set; } = false;

		[Column("antispam_mute")]
		public bool AntispamMute{ get; set; } = false;

		[Column("antispam_mute_duration")]
		public Int64 AntispamMuteDuration{ get; set; } = 5;

		[Column("antispam_links_extended")]
		public bool AntispamLinksExtended{ get; set; } = false;

		[Column("antispam_links_extended_ban")]
		public bool AntispamLinksExtendedBan{ get; set; } = false;

		[Column("antispam_links_standard")]
		public bool AntispamLinksStandard{ get; set; } = false;

		[Column("antispam_links_standard_ban")]
		public bool AntispamLinksStandardBan{ get; set; } = false;

		[Column("antispam_links_youtube")]
		public bool AntispamLinksYoutube{ get; set; } = false;

		[Column("antispam_links_youtube_ban")]
		public bool AntispamLinksYoutubeBan{ get; set; } = false;

		[Column("antispam_links_twitch")]
		public bool AntispamLinksTwitch{ get; set; } = false;

		[Column("antispam_links_twitch_ban")]
		public bool AntispamLinksTwitchBan{ get; set; } = false;

		[Column("antispam_links_hitbox")]
		public bool AntispamLinksHitbox{ get; set; } = false;

		[Column("antispam_links_hitbox_ban")]
		public bool AntispamLinksHitboxBan{ get; set; } = false;

		[Column("antispam_links_beam")]
		public bool AntispamLinksBeam{ get; set; } = false;

		[Column("antispam_links_beam_ban")]
		public bool AntispamLinksBeamBan{ get; set; } = false;

		[Column("antispam_links_imgur")]
		public bool AntispamLinksImgur{ get; set; } = false;

		[Column("antispam_links_imgur_ban")]
		public bool AntispamLinksImgurBan{ get; set; } = false;

		[Column("antispam_tolerance")]
		public Int64 AntispamTolerance{ get; set; } = 7;

		[Column("antispam_ignore_members")]
		public bool AntispamIgnoreMembers{ get; set; } = false;

		[Column("operator_roleid")]
		public guid OperatorRoleId{ get; set; } = 0;

		[Column("quickban_duration")]
		public Int64 QuickbanDuration{ get; set; } = 12;

		[Column("quickban_reason", TypeName = "text")]
		public string QuickbanReason{ get; set; }

		[Column("mute_roleid")]
		public guid MuteRoleId{ get; set; } = 0;

		[Column("mute_ignore_channelid")]
		public guid MuteIgnoreChannelId{ get; set; } = 0;

		[Column("karma_enabled")]
		public bool KarmaEnabled{ get; set; } = false;

		[Column("karma_limit_mentions")]
		public Int64 KarmaLimitMentions{ get; set; } = 5;

		[Column("karma_limit_minutes")]
		public Int64 KarmaLimitMinutes{ get; set; } = 60;

		[Column("karma_limit_response")]
		public bool KarmaLimitResponse{ get; set; } = true;

		[Column("karma_currency", TypeName = "varchar(255)")]
		public string KarmaCurrency{ get; set; } = "cookies";

		[Column("karma_currency_singular", TypeName = "varchar(255)")]
		public string KarmaCurrencySingular{ get; set; } = "cookie";

		[Column("karma_consume_command", TypeName = "varchar(255)")]
		public string KarmaConsumeCommand{ get; set; } = "nom";

		[Column("karma_consume_verb", TypeName = "varchar(255)")]
		public string KarmaConsumeVerb{ get; set; } = "nommed";

		[Column("log_Channelid")]
		public guid LogChannelId{ get; set; } = 0;

		[Column("mod_channelid")]
		public guid ModChannelId{ get; set; } = 0;

		[Column("log_bans")]
		public bool LogBans{ get; set; } = false;

		[Column("log_promotions")]
		public bool LogPromotions{ get; set; } = false;

		[Column("log_deletedmessages")]
		public bool LogDeletedMessages{ get; set; } = false;

		[Column("log_editedmessages")]
		public bool LogEditedMessages{ get; set; } = false;

		[Column("activity_channelid")]
		public guid ActivityChannelId{ get; set; } = 0;

		[Column("log_join")]
		public bool LogJoin{ get; set; } = false;

		[Column("log_leave")]
		public bool LogLeave{ get; set; } = false;

		[Column("log_message_join", TypeName = "text")]
		public string LogMessageJoin{ get; set; } = "{0} joined the server.";

		[Column("log_message_leave", TypeName = "text")]
		public string LogMessageLeave{ get; set; } = "{0} left.";

		[Column("log_mention_join")]
		public bool LogMentionJoin{ get; set; } = false;

		[Column("log_mention_leave")]
		public bool LogMentionLeave{ get; set; } = false;

		[Column("log_timestamp_join")]
		public bool LogTimestampJoin{ get; set; } = false;

		[Column("log_timestamp_leave")]
		public bool LogTimestampLeave{ get; set; } = false;

		[Column("welcome_pm")]
		public bool WelcomeMessageEnabled{ get; set; } = false;

		[Column("welcome_message", TypeName = "text")]
		public string WelcomeMessage{ get; set; } = "Hi {0}, welcome to our server!";

		[Column("welcome_roleid")]
		public guid WelcomeRoleId{ get; set; } = 0;

		[Column("verify")]
		public bool VerificationEnabled{ get; set; } = false;

		[Column("verify_on_welcome")]
		public bool VerifyOnWelcome{ get; set; } = false;

		[Column("verify_roleid")]
		public guid VerifyRoleId{ get; set; } = 0;

		[Column("verify_karma")]
		public Int64 VerifyKarma{ get; set; } = 3;

		[Column("verify_message", TypeName = "text")]
		public string VerifyMessage{ get; set; } = "**1.** Be respectful to others, do not start huge drama and arguments.\n" +
			                "**2.** Hate speech, \"doxxing,\" or leaking personal information will not be tolerated. Free speech isn't free of consequences.\n" +
			                "**3.** Sexual harassment, even slightly suggesting anything gender biased is inappropriate. Yes, suggesting that women should be in the kitchen is sexual harassment. And we are not a dating service either.\n" +
			                "**4.** Homophobic language or racial slurs are immature and you should not use them.\n" +
			                "**5.** Do not post explicitly sexual, gore or otherwise disturbing content. This includes jokes and meme that have somewhat racist background.\n" +
			                "**6.** Do not break the application (e.g. spamming the text that goes vertical, or having a name \"everyone\", etc...) and don't spam excessively (walls of emotes, etc...)\n" +
			                "**7.** Avoid sensitive topics, such as politics or religion.\n" +
			                "**8.** Respect authority, and do not troll moderators on duty. Do not impersonate Admins or Mods, or anyone else.\n" +
			                "**9.** Don't join just to advertise your stuff, it's rude. If you have something worthy, get in touch with us, and we can maybe give you a place in the news channel. This includes discord invite links, which will be automatically removed - get in touch with the Mods.\n" +
			                "**10.** Use common sense together with everything above.";

		[Column("exp_enabled")]
		public bool ExpEnabled{ get; set; } = false;

		[Column("base_exp_to_levelup")]
		public Int64 BaseExpToLevelup{ get; set; } = 0;

		[Column("exp_announce_levelup")]
		public bool ExpAnnounceLevelup{ get; set; } = false;

		[Column("exp_per_message")]
		public Int64 ExpPerMessage{ get; set; } = 0;

		[Column("exp_per_attachment")]
		public Int64 ExpPerAttachment{ get; set; } = 0;

		[Column("exp_cumulative_roles")]
		public bool ExpCumulativeRoles{ get; set; } = false;
	}
}

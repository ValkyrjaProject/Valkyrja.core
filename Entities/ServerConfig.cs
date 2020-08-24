using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

using guid = System.UInt64;

namespace Valkyrja.entities
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

		[Column("tos")]
		public bool ToS{ get; set; } = false;

		[Column("nicknames")]
		public bool Nicknames{ get; set; } = true;

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

		[Column("antispam_username")]
		public bool AntispamUsername{ get; set; } = false;

		[Column("antispam_norole")]
		public bool AntispamNoRole{ get; set; } = false;

		[Column("antispam_norole_recent")]
		public bool AntispamNoRoleRecent{ get; set; } = false;

		[Column("antispam_norole_minutes")]
		public Int64 AntispamNoRoleMinutes{ get; set; } = 7;

		[Column("antispam_invites")]
		public bool AntispamInvites{ get; set; } = false;

		[Column("antispam_invites_ban")]
		public bool AntispamInvitesBan{ get; set; } = false;

		[Column("antispam_porn")]
		public bool AntispamPorn{ get; set; } = false;

		[Column("antispam_duplicate")]
		public bool AntispamDuplicate{ get; set; } = false;

		[Column("antispam_duplicate_crossserver")]
		public bool AntispamDuplicateCrossserver{ get; set; } = false;

		[Column("antispam_duplicate_multiuser")]
		public bool AntispamDuplicateMultiuser{ get; set; } = false;

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

		[Column("antispam_links_chan")]
		public bool AntispamLinksChan{ get; set; } = false;

		[Column("antispam_links_chan_ban")]
		public bool AntispamLinksChanBan{ get; set; } = false;

		[Column("antispam_tolerance")]
		public Int64 AntispamTolerance{ get; set; } = 2;

		[Column("antispam_tolerance_ban")]
		public Int64 AntispamToleranceBan{ get; set; } = 6;

		[Column("antispam_voice_switching")]
		public bool AntispamVoiceChannelSwitching{ get; set; } = false;

		[Column("antispam_ignore_members")]
		public bool AntispamIgnoreMembers{ get; set; } = false;

		[Column("operator_enforce")]
		public bool OperatorEnforce{ get; set; } = true;

		[Column("operator_roleid")]
		public guid OperatorRoleId{ get; set; } = 0;

		[Column("ban_duration", TypeName = "varchar(255)")]
		public string BanDuration{ get; set; } = "";

		[Column("quickban_duration")]
		public Int64 QuickbanDuration{ get; set; } = 12;

		[Column("quickban_reason", TypeName = "text")]
		public string QuickbanReason{ get; set; }

		[Column("mute_roleid")]
		public guid MuteRoleId{ get; set; } = 0;

		[Column("mute_ignore_channelid")]
		public guid MuteIgnoreChannelId{ get; set; } = 0;

		[Column("mute_message", TypeName = "text")]
		public string MuteMessage{ get; set; }

		[Column("slowmode_default")]
		public int SlowmodeDefaultSeconds{ get; set; } = 60;

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

		[Column("tempchannel_giveadmin")]
		public bool TempChannelGiveAdmin{ get; set; } = false;

		[Column("tempchannel_categoryid")]
		public guid TempChannelCategoryId{ get; set; } = 0;

		[Column("voice_channelid")]
		public guid VoiceChannelId{ get; set; } = 0;

		[Column("activity_channelid")]
		public guid ActivityChannelId{ get; set; } = 0;

		[Column("alert_channelid")]
		public guid AlertChannelId{ get; set; } = 0;

		[Column("alert_whitelistid")]
		public guid AlertWhitelistId{ get; set; } = 0;

		[Column("log_channelid")]
		public guid LogChannelId{ get; set; } = 0;

		[Column("mod_channelid")]
		public guid ModChannelId{ get; set; } = 0;

		[Column("embed_voicechannel")]
		public bool VoiceChannelEmbeds{ get; set; } = false;

		[Column("embed_activitychannel")]
		public bool ActivityChannelEmbeds{ get; set; } = false;

		[Column("embed_logchannel")]
		public bool LogChannelEmbeds{ get; set; } = false;

		[Column("embed_modchannel")]
		public bool ModChannelEmbeds{ get; set; } = false;

		[Column("color_voicechannel")]
		public uint VoiceChannelColor{ get; set; } = 65280;

		[Column("color_activitychannel")]
		public uint ActivityChannelColor{ get; set; } = 65535;

		[Column("color_alertchannel")]
		public uint AlertChannelColor{ get; set; } = 10421504;

		[Column("color_logchannel")]
		public uint LogChannelColor{ get; set; } = 255;

		[Column("color_modchannel")]
		public uint ModChannelColor{ get; set; } = 16711680;

		[Column("color_logwarning")]
		public uint LogWarningColor{ get; set; } = 16489984;

		[Column("color_logmessages")]
		public uint LogMessagesColor{ get; set; } = 16776960;

		[Column("log_antispam_kick")]
		public bool LogAntispamKick{ get; set; } = false;

		[Column("log_warnings")]
		public bool LogWarnings{ get; set; } = false;

		[Column("log_bans")]
		public bool LogBans{ get; set; } = false;

		[Column("log_promotions")]
		public bool LogPromotions{ get; set; } = false;

		[Column("log_deletedmessages")]
		public bool LogDeletedMessages{ get; set; } = false;

		[Column("log_editedmessages")]
		public bool LogEditedMessages{ get; set; } = false;

		[Column("log_join")]
		public bool LogJoin{ get; set; } = false;

		[Column("log_leave")]
		public bool LogLeave{ get; set; } = false;

		[Column("alert_role_mention")]
		public guid AlertRoleMention{ get; set; } = 0;

		[Column("log_alert_regex", TypeName = "text")]
		public string LogAlertRegex{ get; set; } = null;

		[Column("delete_alert_regex", TypeName = "text")]
		public string DeleteAlertRegex{ get; set; } = null;

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

		[Column("stats")]
		public bool StatsEnabled{ get; set; } = false;

		[Column("verify")]
		public bool CodeVerificationEnabled{ get; set; } = false;

		[Column("captcha")]
		public bool CaptchaVerificationEnabled{ get; set; } = false;

		[Column("verify_accountage")]
		public bool VerifyAccountAge{ get; set; } = false;

		[Column("verify_accountage_days")]
		public Int64 VerifyAccountAgeDays{ get; set; } = 14;

		[Column("verify_on_welcome")]
		public bool VerifyOnWelcome{ get; set; } = false;

		[Column("verify_roleid")]
		public guid VerifyRoleId{ get; set; } = 0;

		[Column("verify_channelid")]
		public guid VerifyChannelId{ get; set; } = 0;

		[Column("verify_karma")]
		public Int64 VerifyKarma{ get; set; } = 3;

		[Column("verify_message", TypeName = "text")]
		public string CodeVerifyMessage{ get; set; } = "**1.** Be respectful to others, do not start huge drama and arguments.\n" +
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
		public Int64 BaseExpToLevelup{ get; set; } = 10;

		[Column("exp_per_message")]
		public Int64 ExpPerMessage{ get; set; } = 1;

		[Column("exp_per_attachment")]
		public Int64 ExpPerAttachment{ get; set; } = 3;

		[Column("exp_max_level")]
		public Int64 ExpMaxLevel{ get; set; } = 0;

		[Column("karma_per_level")]
		public Int64 KarmaPerLevel{ get; set; } = 3;

		[Column("exp_announce_levelup")]
		public bool ExpAnnounceLevelup{ get; set; } = true;

		[Column("exp_cumulative_roles")]
		public bool ExpCumulativeRoles{ get; set; } = false;

		[Column("exp_advance_users")]
		public bool ExpAdvanceUsers{ get; set; } = false;

		[Column("exp_member_messages")]
		public Int64 ExpMemberMessages{ get; set; } = 0;

		[Column("exp_member_roleid")]
		public guid ExpMemberRoleId{ get; set; } = 0;

		[Column("profile_channelid")]
		public guid ProfileChannelId{ get; set; } = 0;

		[Column("profile_enabled")]
		public bool ProfileEnabled{ get; set; } = false;

		[Column("memo_enabled")]
		public bool MemoEnabled{ get; set; } = false;

		[Column("notification_channelid")]
		public guid NotificationChannelId{ get; set; } = 0;

		[Column("last_touched")]
		public DateTime LastTouched{ get; set; } = DateTime.MinValue;


		public string GetPropertyValue(string propertyName)
		{
			if( propertyName == "CustomCommands" || propertyName == "Aliases" )
				return null;

			System.Reflection.PropertyInfo info = GetType().GetProperty(propertyName);
			object value;
			if( info == null || (value = info.GetValue(this)) == null )
				return null;

			return value.ToString();
		}

		public string SetPropertyValue(string propertyName, object value)
		{
			if( propertyName == "CustomCommands" || propertyName == "Aliases" )
				return null;

			System.Reflection.PropertyInfo info = GetType().GetProperty(propertyName);
			if( info == null )
				return null;

			string oldValue = info.GetValue(this).ToString();
			info.SetValue(this, value);

			return oldValue;
		}
	}
}

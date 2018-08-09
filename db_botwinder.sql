-- MySQL dump 10.16  Distrib 10.2.13-MariaDB, for Linux (x86_64)
--
-- Host: localhost    Database: db_botwinder
-- ------------------------------------------------------
-- Server version	10.2.13-MariaDB

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `blacklist`
--

DROP TABLE IF EXISTS `blacklist`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `blacklist` (
  `id` bigint(20) unsigned NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `id` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `channels`
--

DROP TABLE IF EXISTS `channels`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `channels` (
  `serverid` bigint(20) unsigned NOT NULL,
  `channelid` bigint(20) unsigned NOT NULL,
  `ignored` tinyint(1) NOT NULL DEFAULT 0,
  `temporary` tinyint(1) NOT NULL DEFAULT 0,
  `muted_until` datetime NOT NULL,
  PRIMARY KEY (`channelid`),
  UNIQUE KEY `channelid` (`channelid`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `command_channel_options`
--

DROP TABLE IF EXISTS `command_channel_options`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `command_channel_options` (
  `serverid` bigint(20) unsigned NOT NULL,
  `commandid` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL,
  `channelid` bigint(20) unsigned NOT NULL,
  `blacklisted` tinyint(1) NOT NULL,
  `whitelisted` tinyint(1) NOT NULL,
  PRIMARY KEY (`serverid`,`commandid`,`channelid`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `command_options`
--

DROP TABLE IF EXISTS `command_options`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `command_options` (
  `serverid` bigint(20) unsigned NOT NULL,
  `commandid` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL,
  `permission_overrides` tinyint(4) NOT NULL,
  `delete_request` tinyint(1) NOT NULL,
  `delete_reply` tinyint(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`serverid`,`commandid`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `custom_aliases`
--

DROP TABLE IF EXISTS `custom_aliases`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `custom_aliases` (
  `serverid` bigint(20) unsigned NOT NULL,
  `commandid` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL,
  `alias` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL,
  PRIMARY KEY (`serverid`,`alias`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `custom_commands`
--

DROP TABLE IF EXISTS `custom_commands`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `custom_commands` (
  `serverid` bigint(20) unsigned NOT NULL,
  `commandid` varchar(127) COLLATE utf8mb4_unicode_ci NOT NULL,
  `response` text COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `description` text COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  PRIMARY KEY (`serverid`,`commandid`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `events`
--

DROP TABLE IF EXISTS `events`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `events` (
  `serverid` bigint(20) unsigned NOT NULL,
  `userid` bigint(20) unsigned NOT NULL,
  `checkedin` tinyint(1) NOT NULL,
  `score` float NOT NULL,
  PRIMARY KEY (`serverid`,`userid`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `exceptions`
--

DROP TABLE IF EXISTS `exceptions`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `exceptions` (
  `id` bigint(20) NOT NULL AUTO_INCREMENT,
  `shardid` bigint(20) NOT NULL,
  `serverid` bigint(20) unsigned NOT NULL,
  `datetime` datetime NOT NULL,
  `message` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL,
  `stack` mediumtext COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `data` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL,
  `type` varchar(255) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=1266 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `global_config`
--

DROP TABLE IF EXISTS `global_config`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `global_config` (
  `configuration_name` varchar(255) NOT NULL,
  `discord_token` varchar(255) NOT NULL,
  `userid` bigint(20) unsigned NOT NULL DEFAULT 278834060053446666,
  `admin_userid` bigint(20) unsigned NOT NULL DEFAULT 89805412676681728,
  `enforce_requirements` tinyint(1) NOT NULL DEFAULT 0,
  `verification_enabled` tinyint(1) NOT NULL DEFAULT 0,
  `timers_enabled` tinyint(1) NOT NULL DEFAULT 0,
  `polls_enabled` tinyint(1) NOT NULL DEFAULT 0,
  `events_enabled` tinyint(1) NOT NULL DEFAULT 0,
  `giveaways_enabled` tinyint(1) NOT NULL DEFAULT 0,
  `livestream_enabled` tinyint(1) NOT NULL DEFAULT 0,
  `total_shards` bigint(20) NOT NULL DEFAULT 1,
  `initial_update_delay` bigint(20) NOT NULL DEFAULT 1,
  `command_prefix` varchar(255) NOT NULL DEFAULT '!',
  `main_serverid` bigint(20) unsigned NOT NULL DEFAULT 155821059960995840,
  `main_channelid` bigint(20) unsigned NOT NULL DEFAULT 170139120318808065,
  `vip_skip_queue` tinyint(1) NOT NULL DEFAULT 0,
  `vip_members_max` bigint(20) NOT NULL DEFAULT 0,
  `vip_trial_hours` bigint(20) NOT NULL DEFAULT 36,
  `vip_trial_joins` bigint(20) NOT NULL DEFAULT 5,
  `antispam_clear_interval` bigint(20) NOT NULL DEFAULT 10,
  `antispam_safety_limit` bigint(20) NOT NULL DEFAULT 30,
  `antispam_fastmessages_per_update` bigint(20) NOT NULL DEFAULT 5,
  `antispam_update_interval` bigint(20) NOT NULL DEFAULT 6,
  `antispam_message_cache_size` bigint(20) NOT NULL DEFAULT 6,
  `antispam_allowed_duplicates` bigint(20) NOT NULL DEFAULT 2,
  `target_fps` float NOT NULL DEFAULT 0.05,
  `operations_max` bigint(20) NOT NULL DEFAULT 2,
  `operations_extra` bigint(20) NOT NULL DEFAULT 1,
  `maintenance_memory_threshold` bigint(20) NOT NULL DEFAULT 3000,
  `maintenance_thread_threshold` bigint(20) NOT NULL DEFAULT 44,
  `maintenance_operations_threshold` bigint(20) NOT NULL DEFAULT 300,
  `maintenance_disconnect_threshold` bigint(20) NOT NULL DEFAULT 20,
  `log_debug` tinyint(1) NOT NULL DEFAULT 0,
  `log_exceptions` tinyint(1) NOT NULL DEFAULT 1,
  `log_commands` tinyint(1) NOT NULL DEFAULT 1,
  `log_responses` tinyint(1) NOT NULL DEFAULT 1,
  PRIMARY KEY (`configuration_name`),
  UNIQUE KEY `configuration_name` (`configuration_name`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `livestream`
--

DROP TABLE IF EXISTS `livestream`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `livestream` (
  `serverid` bigint(20) unsigned NOT NULL,
  `channelid` bigint(20) unsigned NOT NULL,
  `type` tinyint(4) NOT NULL,
  `channel` varchar(255) NOT NULL,
  `islive` tinyint(1) NOT NULL,
  PRIMARY KEY (`channelid`,`type`,`channel`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `localisation`
--

DROP TABLE IF EXISTS `localisation`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `localisation` (
  `id` bigint(20) unsigned NOT NULL,
  `iso` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL,
  `about` mediumtext COLLATE utf8mb4_unicode_ci NOT NULL,
  `string1` mediumtext COLLATE utf8mb4_unicode_ci NOT NULL,
  `string2...` mediumtext COLLATE utf8mb4_unicode_ci NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `id` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `logs`
--

DROP TABLE IF EXISTS `logs`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `logs` (
  `id` bigint(20) NOT NULL AUTO_INCREMENT,
  `messageid` bigint(20) unsigned NOT NULL,
  `serverid` bigint(20) unsigned NOT NULL,
  `channelid` bigint(20) unsigned NOT NULL,
  `userid` bigint(20) unsigned NOT NULL,
  `type` tinyint(4) NOT NULL,
  `datetime` datetime NOT NULL,
  `message` mediumtext COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=558482 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `nicknames`
--

DROP TABLE IF EXISTS `nicknames`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `nicknames` (
  `id` bigint(20) NOT NULL AUTO_INCREMENT,
  `serverid` bigint(20) unsigned NOT NULL,
  `userid` bigint(20) unsigned NOT NULL,
  `nickname` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=90811 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `partners`
--

DROP TABLE IF EXISTS `partners`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `partners` (
  `serverid` bigint(20) unsigned NOT NULL,
  `premium` tinyint(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`serverid`),
  UNIQUE KEY `serverid` (`serverid`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `poll_options`
--

DROP TABLE IF EXISTS `poll_options`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `poll_options` (
  `serverid` bigint(20) unsigned NOT NULL,
  `option` mediumtext COLLATE utf8mb4_unicode_ci NOT NULL,
  PRIMARY KEY (`serverid`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `polls`
--

DROP TABLE IF EXISTS `polls`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `polls` (
  `serverid` bigint(20) unsigned NOT NULL,
  `title` mediumtext COLLATE utf8mb4_unicode_ci NOT NULL,
  `type` tinyint(4) NOT NULL,
  PRIMARY KEY (`serverid`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `profile_options`
--

DROP TABLE IF EXISTS `profile_options`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `profile_options` (
  `serverid` bigint(20) unsigned NOT NULL DEFAULT 0,
  `property_order` bigint(20) NOT NULL DEFAULT 0,
  `option` varchar(255) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `option_alt` varchar(255) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `label` varchar(2048) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `inline` tinyint(4) NOT NULL DEFAULT 0
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `quotes`
--

DROP TABLE IF EXISTS `quotes`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `quotes` (
  `serverid` bigint(20) unsigned NOT NULL DEFAULT 0,
  `id` bigint(20) NOT NULL DEFAULT 0,
  `created_time` datetime DEFAULT NULL,
  `username` varchar(255) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `value` longtext COLLATE utf8mb4_unicode_ci DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `roles`
--

DROP TABLE IF EXISTS `roles`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `roles` (
  `serverid` bigint(20) unsigned NOT NULL,
  `roleid` bigint(20) unsigned NOT NULL,
  `permission_level` tinyint(4) NOT NULL DEFAULT 0,
  `public_id` bigint(20) NOT NULL DEFAULT 0,
  `logging_ignored` tinyint(1) NOT NULL DEFAULT 0,
  `antispam_ignored` tinyint(1) NOT NULL DEFAULT 0,
  `level` bigint(20) NOT NULL DEFAULT 0,
  `delete_at_time` datetime NOT NULL DEFAULT from_unixtime(0),
  PRIMARY KEY (`roleid`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `server_config`
--

DROP TABLE IF EXISTS `server_config`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `server_config` (
  `serverid` bigint(20) unsigned NOT NULL,
  `name` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL,
  `invite_url` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL,
  `localisation_id` bigint(20) unsigned NOT NULL DEFAULT 0,
  `timezone_utc_relative` bigint(20) NOT NULL DEFAULT 0,
  `use_database` tinyint(1) NOT NULL DEFAULT 1,
  `ignore_bots` tinyint(1) NOT NULL DEFAULT 1,
  `ignore_everyone` tinyint(1) NOT NULL DEFAULT 1,
  `command_prefix` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT '!',
  `command_prefix_alt` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL,
  `execute_on_edit` tinyint(1) NOT NULL DEFAULT 1,
  `antispam_priority` tinyint(1) NOT NULL DEFAULT 0,
  `antispam_invites` tinyint(1) NOT NULL DEFAULT 0,
  `antispam_invites_ban` tinyint(1) NOT NULL DEFAULT 0,
  `antispam_duplicate` tinyint(1) NOT NULL DEFAULT 0,
  `antispam_duplicate_crossserver` tinyint(1) NOT NULL DEFAULT 0,
  `antispam_duplicate_ban` tinyint(1) NOT NULL DEFAULT 0,
  `antispam_mentions_max` bigint(20) NOT NULL DEFAULT 0,
  `antispam_mentions_ban` tinyint(1) NOT NULL DEFAULT 0,
  `antispam_mute` tinyint(1) NOT NULL DEFAULT 0,
  `antispam_mute_duration` bigint(20) NOT NULL DEFAULT 5,
  `antispam_links_extended` tinyint(1) NOT NULL DEFAULT 0,
  `antispam_links_extended_ban` tinyint(1) NOT NULL DEFAULT 0,
  `antispam_links_standard` tinyint(1) NOT NULL DEFAULT 0,
  `antispam_links_standard_ban` tinyint(1) NOT NULL DEFAULT 0,
  `antispam_links_youtube` tinyint(1) NOT NULL DEFAULT 0,
  `antispam_links_youtube_ban` tinyint(1) NOT NULL DEFAULT 0,
  `antispam_links_twitch` tinyint(1) NOT NULL DEFAULT 0,
  `antispam_links_twitch_ban` tinyint(1) NOT NULL DEFAULT 0,
  `antispam_links_hitbox` tinyint(1) NOT NULL DEFAULT 0,
  `antispam_links_hitbox_ban` tinyint(1) NOT NULL DEFAULT 0,
  `antispam_links_beam` tinyint(1) NOT NULL DEFAULT 0,
  `antispam_links_beam_ban` tinyint(1) NOT NULL DEFAULT 0,
  `antispam_links_imgur` tinyint(1) NOT NULL DEFAULT 0,
  `antispam_links_imgur_ban` tinyint(1) NOT NULL DEFAULT 0,
  `antispam_tolerance` bigint(20) NOT NULL DEFAULT 4,
  `antispam_ignore_members` tinyint(1) NOT NULL DEFAULT 0,
  `operator_roleid` bigint(20) unsigned NOT NULL DEFAULT 0,
  `quickban_duration` bigint(20) NOT NULL DEFAULT 0,
  `quickban_reason` mediumtext COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `mute_roleid` bigint(20) unsigned NOT NULL DEFAULT 0,
  `mute_ignore_channelid` bigint(20) unsigned NOT NULL DEFAULT 0,
  `karma_enabled` tinyint(1) NOT NULL DEFAULT 0,
  `karma_limit_mentions` bigint(20) NOT NULL DEFAULT 5,
  `karma_limit_minutes` bigint(20) NOT NULL DEFAULT 30,
  `karma_limit_response` tinyint(1) NOT NULL DEFAULT 1,
  `karma_currency` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'cookies',
  `karma_currency_singular` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'cookies',
  `karma_consume_command` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'nom',
  `karma_consume_verb` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'nommed',
  `log_channelid` bigint(20) unsigned NOT NULL DEFAULT 0,
  `mod_channelid` bigint(20) unsigned NOT NULL DEFAULT 0,
  `log_bans` tinyint(1) NOT NULL DEFAULT 0,
  `log_promotions` tinyint(1) NOT NULL DEFAULT 0,
  `log_deletedmessages` tinyint(1) NOT NULL DEFAULT 0,
  `log_editedmessages` tinyint(1) NOT NULL DEFAULT 0,
  `activity_channelid` bigint(20) unsigned NOT NULL DEFAULT 0,
  `log_join` tinyint(1) NOT NULL DEFAULT 0,
  `log_leave` tinyint(1) NOT NULL DEFAULT 0,
  `log_message_join` mediumtext COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `log_message_leave` mediumtext COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `log_mention_join` tinyint(1) NOT NULL DEFAULT 0,
  `log_mention_leave` tinyint(1) NOT NULL DEFAULT 0,
  `log_timestamp_join` tinyint(1) NOT NULL DEFAULT 0,
  `log_timestamp_leave` tinyint(1) NOT NULL DEFAULT 0,
  `welcome_pm` tinyint(1) NOT NULL DEFAULT 0,
  `welcome_message` mediumtext COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `welcome_roleid` bigint(20) unsigned NOT NULL DEFAULT 0,
  `verify` tinyint(1) NOT NULL DEFAULT 0,
  `verify_on_welcome` tinyint(1) NOT NULL DEFAULT 0,
  `verify_roleid` bigint(20) unsigned NOT NULL DEFAULT 0,
  `verify_karma` bigint(20) NOT NULL DEFAULT 3,
  `verify_message` mediumtext COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `exp_enabled` tinyint(1) NOT NULL DEFAULT 0,
  `base_exp_to_levelup` bigint(20) NOT NULL,
  `exp_announce_levelup` tinyint(1) NOT NULL,
  `exp_per_message` bigint(20) NOT NULL,
  `exp_per_attachment` bigint(20) NOT NULL,
  `exp_cumulative_roles` tinyint(1) NOT NULL DEFAULT 0,
  `voice_channelid` bigint(20) unsigned NOT NULL DEFAULT 0,
  `antispam_voice_switching` tinyint(1) NOT NULL DEFAULT 0,
  `color_logmessages` int(10) unsigned NOT NULL DEFAULT 16776960,
  `color_modchannel` int(10) unsigned NOT NULL DEFAULT 16711680,
  `color_logchannel` int(10) unsigned NOT NULL DEFAULT 255,
  `color_activitychannel` int(10) unsigned NOT NULL DEFAULT 65535,
  `color_voicechannel` int(10) unsigned NOT NULL DEFAULT 65280,
  `embed_modchannel` tinyint(1) NOT NULL DEFAULT 0,
  `embed_logchannel` tinyint(1) NOT NULL DEFAULT 0,
  `embed_activitychannel` tinyint(1) NOT NULL DEFAULT 0,
  `embed_voicechannel` tinyint(1) NOT NULL DEFAULT 0,
  `karma_per_level` bigint(20) unsigned NOT NULL DEFAULT 3,
  `exp_max_level` bigint(20) unsigned NOT NULL DEFAULT 0,
  `exp_advance_users` tinyint(1) NOT NULL DEFAULT 0,
  `profile_enabled` tinyint(1) NOT NULL DEFAULT 0,
  `memo_enabled` tinyint(1) NOT NULL DEFAULT 0,
  `log_warnings` tinyint(1) NOT NULL DEFAULT 0,
  `color_logwarning` int(10) unsigned NOT NULL DEFAULT 16489984,
  PRIMARY KEY (`serverid`),
  UNIQUE KEY `serverid` (`serverid`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `server_stats`
--

DROP TABLE IF EXISTS `server_stats`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `server_stats` (
  `shardid` bigint(20) NOT NULL,
  `serverid` bigint(20) unsigned NOT NULL,
  `name` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL,
  `ownerid` bigint(20) unsigned NOT NULL,
  `owner_name` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL,
  `joined_first` datetime NOT NULL,
  `joined_last` datetime NOT NULL,
  `joined_count` bigint(20) NOT NULL DEFAULT 0,
  `user_count` bigint(20) NOT NULL DEFAULT 0,
  `vip` tinyint(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`serverid`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `shards`
--

DROP TABLE IF EXISTS `shards`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `shards` (
  `id` bigint(20) NOT NULL AUTO_INCREMENT,
  `taken` tinyint(1) NOT NULL DEFAULT 0,
  `connecting` tinyint(1) NOT NULL DEFAULT 0,
  `time_started` datetime NOT NULL,
  `memory_used` bigint(20) NOT NULL DEFAULT 0,
  `threads_active` bigint(20) NOT NULL DEFAULT 0,
  `server_count` bigint(20) NOT NULL DEFAULT 0,
  `user_count` bigint(20) NOT NULL DEFAULT 0,
  `messages_total` bigint(20) NOT NULL DEFAULT 0,
  `messages_per_minute` bigint(20) NOT NULL DEFAULT 0,
  `operations_ran` bigint(20) NOT NULL DEFAULT 0,
  `operations_active` bigint(20) NOT NULL DEFAULT 0,
  `disconnects` bigint(20) NOT NULL DEFAULT 0,
  PRIMARY KEY (`id`),
  UNIQUE KEY `id` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `subscribers`
--

DROP TABLE IF EXISTS `subscribers`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `subscribers` (
  `userid` bigint(20) unsigned NOT NULL,
  `has_bonus` tinyint(1) NOT NULL,
  `premium` tinyint(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`userid`),
  UNIQUE KEY `userid` (`userid`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `support_team`
--

DROP TABLE IF EXISTS `support_team`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `support_team` (
  `userid` bigint(20) unsigned NOT NULL,
  PRIMARY KEY (`userid`),
  UNIQUE KEY `userid` (`userid`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `timer_responses`
--

DROP TABLE IF EXISTS `timer_responses`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `timer_responses` (
  `id` bigint(20) NOT NULL,
  `timerid` bigint(20) unsigned NOT NULL,
  `message` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `timers`
--

DROP TABLE IF EXISTS `timers`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `timers` (
  `timerid` bigint(20) unsigned NOT NULL,
  `serverid` bigint(20) unsigned NOT NULL,
  `channelid` bigint(20) unsigned NOT NULL,
  `enabled` tinyint(1) NOT NULL,
  `self_command` tinyint(1) NOT NULL,
  `last_triggered` datetime NOT NULL,
  `start_at` datetime NOT NULL,
  `expire_after` datetime NOT NULL,
  `repeat_interval` bigint(20) NOT NULL,
  PRIMARY KEY (`timerid`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `user_profile_options`
--

DROP TABLE IF EXISTS `user_profile_options`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `user_profile_options` (
  `serverid` bigint(20) unsigned NOT NULL DEFAULT 0,
  `userid` bigint(20) unsigned NOT NULL DEFAULT 0,
  `option` varchar(255) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `value` varchar(2048) COLLATE utf8mb4_unicode_ci DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `usernames`
--

DROP TABLE IF EXISTS `usernames`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `usernames` (
  `id` bigint(20) NOT NULL AUTO_INCREMENT,
  `serverid` bigint(20) unsigned NOT NULL,
  `userid` bigint(20) unsigned NOT NULL,
  `username` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=1140232 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `users`
--

DROP TABLE IF EXISTS `users`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `users` (
  `userid` bigint(20) unsigned NOT NULL,
  `serverid` bigint(20) unsigned NOT NULL,
  `verified` tinyint(1) NOT NULL DEFAULT 0,
  `karma_count` bigint(20) NOT NULL DEFAULT 1,
  `warning_count` bigint(20) NOT NULL DEFAULT 0,
  `notes` mediumtext COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `last_thanks_time` datetime NOT NULL,
  `banned_until` datetime NOT NULL,
  `muted_until` datetime NOT NULL,
  `ignored` tinyint(1) NOT NULL,
  `count_message` bigint(20) NOT NULL DEFAULT 0,
  `count_attachments` bigint(20) NOT NULL DEFAULT 0,
  `level_relative` bigint(20) NOT NULL DEFAULT 0,
  `exp_relative` bigint(20) NOT NULL,
  `memo` text COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  PRIMARY KEY (`userid`,`serverid`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `votes`
--

DROP TABLE IF EXISTS `votes`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `votes` (
  `serverid` bigint(20) unsigned NOT NULL,
  `userid` bigint(20) unsigned NOT NULL,
  `voted` mediumtext COLLATE utf8mb4_unicode_ci NOT NULL,
  PRIMARY KEY (`serverid`,`userid`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2018-08-09 12:51:26

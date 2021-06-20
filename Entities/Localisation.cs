using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;

using guid = System.UInt64;
// ReSharper disable UnusedMember.Global

namespace Valkyrja.entities
{
	[Table("localisation")]
	public class Localisation
	{
		public static class SystemStrings
		{
			public const string DiscordShitEmoji = "<:DiscordPoop:356545886454677506>";

			public const string SmallPmLeaving = "Hulloh!\n" +
			                                     "I'm sorry but I can not sit around in empty servers!";

			public const string VipPmLeaving = "Hulloh!\n" +
			                                   "I'm afraid that you are not eligible to use the Valkyrja bot!\n" +
			                                   "Take a look at <https://valkyrja.app/invite> for more details. _(Please do read the whole page!)_";

			public static readonly string GuildJoined = "Hai! I have some info for you =]\n" +
			                                            "You can find full list of features and commands in the docs, and most importantly the configuration, on our website: <https://valkyrja.app>\n" +
			                                            $"If you have any questions or experience any problems, feel free to poke our Support team for help at: {GlobalConfig.DiscordInvite}\n" +
			                                            "Please make sure to be in the support server and follow the `#news` channel to be notified of possible maintenance and new releases.\n\n";

			public const string GuildJoinedTrial = "Valkyrja is now available only to contributors. You can use it as a trial demo version for one day from now, then it will leave your server.\n"+
			                                       "Should you wish to continue using it, do take a look at conditions and instructions at <https://valkyrja.app/invite>";

			public const string OperationQueuedString = "This command was placed in a queue for large operations at position `{0}` and will be executed as soon as possible. Should you wish to cancel it at any time, use `!cancel {1}`\n_(Premium Contributors do not have to wait.)_";

			public const string MentionHelp = "Find out everything about me, my authors, all the features, commands and configuration at the https://valkyrja.app";

			public const string MentionPrefix = "Try this: `{0}`\n_(Server owner can change it at <https://valkyrja.app/config>!)_";

			public const string MentionPrefixEmpty = "Command prefix is empty on this server, you will not be able to execute any commands. Please configure it at <https://valkyrja.app/config>!";
		}


		[Key]
		[Required]
		[Column("id")]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public guid Id{ get; set; } = 0;

		[Column("moderation_ban_done", TypeName = "text")]
		public string moderation_ban_done{ get; set; } = "<|>_\\*fires them railguns at {0}*_  Ò_Ó<|>_\\*PewPewPew!!*_\nRIP {0}<|>";

		[Column("moderation_mute_done", TypeName = "text")]
		public string moderation_mute_done{ get; set; } = "*Silence!!  ò_ó\n...\nI keel u, {0}!!*  Ò_Ó";

		[Column("moderation_mute_ignorechannel", TypeName = "text")]
		public string moderation_mute_ignorechannel{ get; set; } = "{0}, you've been muted.";

		[Column("moderation_kick_done", TypeName = "text")]
		public string moderation_kick_done{ get; set; } = "<|>I've fired them railguns at {0}.<|>Bye {0}! o/<|>";

		[Column("moderation_op_missing", TypeName = "text")]
		public string moderation_op_missing{ get; set; } = "`{0}op`?";

		[Column("moderation_op_enabled", TypeName = "text")]
		public string moderation_op_enabled{ get; set; } = "Go get em tiger!";

		[Column("moderation_op_disabled", TypeName = "text")]
		public string moderation_op_disabled{ get; set; } = "All done?";

		[Column("moderation_nth_infraction", TypeName = "text")]
		public string moderation_nth_infraction{ get; set; } = "`{0}` now has `{1}` infractions.";

		[Column("role_promote_done", TypeName = "text")]
		public string role_promote_done{ get; set; } = "Done!";

		[Column("role_demote_done", TypeName = "text")]
		public string role_demote_done{ get; set; } = "Done!";

		[Column("role_join_done", TypeName = "text")]
		public string role_join_done{ get; set; } = "Done!";

		[Column("role_join_exclusiveremoved", TypeName = "text")]
		public string role_join_exclusiveremoved{ get; set; } = "\n_(I've removed the other exclusive roles from the same role group.)_";

		[Column("role_leave_done", TypeName = "text")]
		public string role_leave_done{ get; set; } = "Done!";

		[Column("role_publicroles_print", TypeName = "text")]
		public string role_publicroles_print{ get; set; } = "You can use `{0}join` and `{0}leave` commands with these Public Roles: ";

		[Column("role_publicroles_group", TypeName = "text")]
		public string role_publicroles_group{ get; set; } = "\n\n**{0}** - you can join {1} of these:\n";

		[Column("role_memberroles_print", TypeName = "text")]
		public string role_memberroles_print{ get; set; } = "You can use `{0}promote` and `{0}demote` commands with these Member Roles: {1}";


		static internal readonly Regex RngRegex = new Regex("(?<=<\\|>).*?(?=<\\|>)", RegexOptions.Singleline | RegexOptions.Compiled, TimeSpan.FromMilliseconds(50));
		public string GetString(string key, params object[] args)
		{
			if( key == "CustomCommands" || key == "Aliases" )
				return null;

			System.Reflection.PropertyInfo info = GetType().GetProperty(key);
			object value;
			if( info == null || (value = info.GetValue(this)) == null )
				return null;

			string result = value.ToString();
			MatchCollection matches = RngRegex.Matches(result);
			if( matches.Count > 1 )
				result = matches[Utils.Random.Next(0, matches.Count)].Value;

			if( args != null && args.Length > 0 )
			{
				for( int i = 0; i < args.Length; i++ )
				{
					if( !result.Contains($"{{{i}}}") )
						return $"Invalid localisation string `{key}`\nArgument `{{{i}}}` is missing, this string requires **{args.Length}** argument{(args.Length == 1 ? "" : "s")}.";
				}

				result = string.Format(result, args);
			}

			return result;
		}
	}
}

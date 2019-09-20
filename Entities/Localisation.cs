using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

using guid = System.UInt64;

namespace Botwinder.entities
{
	[Table("localisation")]
	public class Localisation
	{
		[Key]
		[Required]
		[Column("id")]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public guid Id{ get; set; } = 0;

		[Column("iso", TypeName = "varchar(255)")]
		public string Iso{ get; set; } = "";

		[Column("string1", TypeName = "text")]
		public string String1{ get; set; } = "";

		public static class SystemStrings
		{
			public const string DiscordShitEmoji = "<:DiscordShit:356545886454677506>";

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

			public const string MentionHelp = "Find out everything about me, my author, all the features, commands and configuration at the https://valkyrja.app";

			public const string MentionPrefix = "Try this: `{0}`\n_(Server owner can change it at <https://valkyrja.app/config>!)_";

			public const string MentionPrefixEmpty = "Command prefix is empty on this server, you will not be able to execute any commands. Please configure it at <https://valkyrja.app/config>!";
		}
	}
}

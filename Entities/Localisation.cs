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
			public const string VipPmLeaving = "Hulloh!\n" +
			                                   "I'm afraid that you are not eligible to use the Botwinder Mk.III !\n" +
			                                   "Take a look at <http://botwinder.info/invite> for more details. _(Please do read the whole page!)_";

			public const string GuildJoined = "Hai! I have some info for you =]\n" +
			                                  "You can find full list of features and commands in the docs, and most importantly the configuration, on our website: http://botwinder.info\n" +
			                                  "If you have any questions or experience any problems, feel free to poke our Support team for help at: <http://support.botwinder.info>\n" +
			                                  "You can also `!subscribe` to patchnotes and maintenance notifications!!\n\n";

			public const string GuildJoinedTrial = "Botwinder is now available only to contributors. You can use it as a trial demo version for one day from now, then it will leave your server.\n"+
			                                       "Should you wish to continue using it, do take a look at conditions and instructions at <http://botwinder.info/invite>";

			public const string OperationQueuedString = "This command was placed in a queue for large operations at position `{0}` and will be executed as soon as possible. Should you wish to cancel it at any time, use `!cancel {1}`\n_(Premium Contributors do not have to wait.)_";
		}
	}
}

using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;

using guid = System.UInt64;

namespace Valkyrja.entities
{
	[Table("stats_daily")]
	public class StatsDaily
	{
		[Required]
		[Column("serverid")]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public guid ServerId{ get; set; } = 0;

		[Column("datetime")]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public DateTime DateTime{ get; set; } = DateTime.MinValue;

		[Column("user_joined")]
		public Int64 UserJoined{ get; set; } = 0;

		[Column("user_left")]
		public Int64 UserLeft{ get; set; } = 0;

		[Column("user_verified")]
		public Int64 UserVerified{ get; set; } = 0;

		[Column("user_banned_valk")]
		public Int64 UserBannedByValk{ get; set; } = 0;

		[Column("user_kicked_valk")]
		public Int64 UserKickedByValk{ get; set; } = 0;

		[Column("user_kicked_discord")]
		public Int64 UserKickedByDiscord{ get; set; } = 0;

		public StatsDaily() { }

		public StatsDaily(guid serverid)
		{
			this.ServerId = serverid;
		}

		public StatsTotal CreateTotal()
		{
			StatsTotal clone = new StatsTotal{
				ServerId = this.ServerId,
				DateTime = this.DateTime,
				UserJoined = this.UserJoined,
				UserLeft = this.UserLeft,
				UserVerified = this.UserVerified,
				UserBannedByValk = this.UserBannedByValk,
				UserKickedByValk = this.UserKickedByValk,
				UserKickedByDiscord = this.UserKickedByDiscord
			};

			return clone;
		}

		public void Reset()
		{
			this.DateTime = DateTime.UtcNow;
			this.UserJoined = 0;
			this.UserLeft = 0;
			this.UserVerified = 0;
			this.UserBannedByValk = 0;
			this.UserKickedByValk = 0;
			this.UserKickedByDiscord = 0;
		}
	}

	[Table("stats_total")]
	public class StatsTotal: StatsDaily
	{
		public void Add(StatsDaily stats)
		{
			this.UserJoined += stats.UserJoined;
			this.UserLeft += stats.UserLeft;
			this.UserVerified += stats.UserVerified;
			this.UserBannedByValk += stats.UserBannedByValk;
			this.UserKickedByValk += stats.UserKickedByValk;
			this.UserKickedByDiscord += stats.UserKickedByDiscord;
		}

		public override string ToString()
		{
			return $"`{this.UserJoined.ToString().PrependSpaces(4)}` user{(this.UserJoined == 1 ? "" : "s")} joined\n" +
			       $"`{this.UserLeft.ToString().PrependSpaces(4)}` user{(this.UserLeft == 1 ? "" : "s")} left on their own\n" +
			       $"`{this.UserVerified.ToString().PrependSpaces(4)}` user{(this.UserVerified == 1 ? "" : "s")} passed verification\n" +
			       $"`{this.UserKickedByDiscord.ToString().PrependSpaces(4)}` user{(this.UserKickedByDiscord == 1 ? "" : "s")} were removed by Discord\n" +
			       $"`{this.UserKickedByValk.ToString().PrependSpaces(4)}` user{(this.UserKickedByValk == 1 ? "" : "s")} were kicked by Valkyrja\n" +
			       $"`{this.UserBannedByValk.ToString().PrependSpaces(4)}` user{(this.UserBannedByValk == 1 ? "" : "s")} were banned by Valkyrja's antispam\n";
		}
	}
}

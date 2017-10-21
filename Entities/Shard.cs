using System;
using System.Runtime.CompilerServices;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Botwinder.entities
{
	[Table("shards")]
	public class Shard
	{
		[Key]
		[Required]
		[Column("id")]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public Int64 Id{ get; set; } = 0;

		[Column("taken")]
		public bool IsTaken{ get; set; } = false;

		[Column("connecting")]
		public bool IsConnecting{ get; set; } = false;

		[Column("time_started")]
		public DateTime TimeStarted{ get; set; } = DateTime.MinValue;

		[Column("memory_used")]
		public Int64 MemoryUsed{ get; set; } = 0;

		[Column("threads_active")]
		public Int64 ThreadsActive{ get; set; } = 0;

		[Column("server_count")]
		public Int64 ServerCount{ get; set; } = 0;

		[Column("user_count")]
		public Int64 UserCount{ get; set; } = 0;

		[Column("messages_total")]
		public Int64 MessagesTotal{ get; set; } = 0;

		[Column("messages_per_minute")]
		public Int64 MessagesPerMinute{ get; set; } = 0;

		[Column("operations_ran")]
		public Int64 OperationsRan{ get; set; } = 0;

		[Column("operations_active")]
		public Int64 OperationsActive{ get; set; } = 0;

		[Column("disconnects")]
		public Int64 Disconnects{ get; set; } = 0;

		public void ResetStats(DateTime timeStarted)
		{
			this.TimeStarted = timeStarted;
			this.MemoryUsed = 0;
			this.ThreadsActive = 0;
			this.ServerCount = 0;
			this.UserCount = 0;
			this.MessagesTotal = 0;
			this.MessagesPerMinute = 0;
			this.OperationsRan = 0;
			this.OperationsActive = 0;
			this.Disconnects = 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public string GetStatsString()
		{
			TimeSpan uptime = this.TimeStarted == DateTime.MinValue ? TimeSpan.Zero : DateTime.UtcNow - this.TimeStarted;
			int days = uptime.Days;
			int hours = uptime.Hours;
			int minutes = uptime.Minutes;
			int seconds = uptime.Seconds;

			string uptimeString = (days == 0 ? "" : (days.ToString() + (days == 1 ? " day, " : " days, "))) +
				(hours == 0 ? "" : (hours.ToString() + (hours == 1 ? " hour, " : " hours, "))) +
				(minutes == 0 ? "" : (minutes.ToString() + (minutes == 1 ? " minute, " : " minutes "))) +
				((days == 0 && hours == 0 && minutes == 0 ? "" : "and ") + seconds.ToString() + (seconds == 1 ? " second." : " seconds."));

			return $"**Shard ID: `{this.Id - 1}`**\n" +
			       $"  Time Started: `{Utils.GetTimestamp(this.TimeStarted)}`\n" +
			       $"  Uptime: `{uptimeString}`\n" +
			       $"  Allocated data Memory: `{this.MemoryUsed} MB`\n" +
			       $"  Threads: `{this.ThreadsActive}`\n" +
			       $"  Messages received: `{this.MessagesTotal}`\n" +
			       $"  Messages per minute: `{this.MessagesPerMinute}`\n" +
			       $"  Operations ran: `{this.OperationsRan}`\n" +
			       $"  Operations active: `{this.OperationsActive}`\n" +
			       $"  Disconnects: `{this.Disconnects}`\n" +
			       $"  Servers: `{this.ServerCount}`\n" +
			       $"  Members `{this.UserCount}`";
		}
	}
}

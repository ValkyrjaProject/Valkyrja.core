using System;
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
	}
}

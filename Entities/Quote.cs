﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using System.Text.RegularExpressions;
using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;

using guid = System.UInt64;

namespace Valkyrja.entities
{
	[Table("quotes")]
	public class Quote
	{
		private const string QuoteString = "{0}\n        **{1}**; {2:dddd, d MMMM yyyy}";

		[Required]
		[Column("serverid")]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public guid ServerId{ get; set; } = 0;

		[Required]
		[Column("id")]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public Int64 Id{ get; set; } = 0;

		[Column("created_time")]
		public DateTime CreatedTime{ get; set; } = DateTime.MinValue;

		[Column("username", TypeName = "varchar(255)")]
		public string Username{ get; set; } = "";

		[Column("value", TypeName = "text")]
		public string Value{ get; set; } = "";

		public override string ToString()
		{
			string formattedValue = Regex.Replace(Regex.Replace(this.Value.Replace("_","\\_"), "^", "> _", RegexOptions.Multiline), "$", "_", RegexOptions.Multiline);
			return string.Format(QuoteString, formattedValue, this.Username.Replace('*',' '), this.CreatedTime);
		}

		public Quote Clone(Int64 newId)
		{
			return new Quote(){
				ServerId = this.ServerId,
				Id = newId,
				CreatedTime = this.CreatedTime,
				Username = this.Username,
				Value = this.Value
			};
		}
	}
}

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
	[Table("users")]
	public class UserData
	{
		[Required]
		[Column("serverid")]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public guid ServerId{ get; set; } = 0;

		[Required]
		[Column("userid")]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public guid UserId{ get; set; } = 0;

		[Column("username")]
		public string LastUsername{ get; set; } = "";

		[Column("verified")]
		public bool Verified{ get; set; } = false;

		[Column("karma_count")]
		public Int64 KarmaCount{ get; set; } = 0;

		[Column("warning_count")]
		public Int64 WarningCount{ get; set; } = 0;

		[Column("notes", TypeName = "text")]
		public string Notes{ get; set; } = "";

		[Column("last_thanks_time")]
		public DateTime LastThanksTime{ get; set; } = DateTime.MinValue;

		[Column("banned")]
		public bool Banned{ get; set; } = false;

		[Column("banned_until")]
		public DateTime BannedUntil{ get; set; } = DateTime.MinValue;

		[Column("muted")]
		public bool Muted{ get; set; } = false;

		[Column("muted_until")]
		public DateTime MutedUntil{ get; set; } = DateTime.MinValue;

		[Column("ignored")]
		public bool Ignored{ get; set; } = false;

		[Column("exp_locked")]
		public bool ExpLocked{ get; set; } = false;

		[Column("count_message")]
		public Int64 CountMessages{ get; set; } = 0;

		[Column("count_attachments")]
		public Int64 CountAttachments{ get; set; } = 0;

		[Column("level_relative")]
		public Int64 Level{ get; set; } = 0;

		[Column("exp_relative")]
		public Int64 Exp{ get; set; } = 0;

		[Column("persistence_flags")]
		public Int64 PersistenceFlags{ get; set; } = 0;

		[Column("memo", TypeName = "text")]
		public string Memo{ get; set; } = "";

		public void AssignPersistence(RoleConfig roleConfig)
		{
			Int64 roleFlag = this.PersistenceFlags & (1 << (int)roleConfig.PersistenceUserFlag);
			this.PersistenceFlags = !roleConfig.InversePersistence ? this.PersistenceFlags | roleFlag : this.PersistenceFlags & ~roleFlag;
		}
		public void RemovePersistence(RoleConfig roleConfig)
		{
			Int64 roleFlag = this.PersistenceFlags & (1 << (int)roleConfig.PersistenceUserFlag);
			this.PersistenceFlags = roleConfig.InversePersistence ? this.PersistenceFlags | roleFlag : this.PersistenceFlags & ~roleFlag;
		}

		public string GetNamesString(ServerContext dbContext, IGuildUser user = null)
		{
			StringBuilder whoisString = new StringBuilder();

			if( user != null )
				whoisString.AppendLine($"<@{this.UserId}>: `{this.UserId}` | `{user.GetUsername()}`\n" +
				                       $"    Account created at: `{Utils.GetTimestamp(Utils.GetTimeFromId(this.UserId))}`");
			else
				whoisString.AppendLine($"<@{this.UserId}>: `{this.UserId}`\n" +
				                       $"    Account created at: `{Utils.GetTimestamp(Utils.GetTimeFromId(this.UserId))}`");

			List<string> foundUsernames = dbContext.Usernames.AsQueryable()
				.Where(u => u.ServerId == this.ServerId && u.UserId == this.UserId).AsEnumerable()
				.Select(u => u.Name.Replace('`', '\'')).ToList();
			whoisString.Append($"    **{foundUsernames.Count}** known username{(foundUsernames.Count > 1 ? "s" : "")}: ");
			whoisString.AppendLine(foundUsernames.ToNames());

			List<string> foundNicknames = dbContext.Nicknames.AsQueryable()
				.Where(u => u.ServerId == this.ServerId && u.UserId == this.UserId).AsEnumerable()
				.Select(u => u.Name.Replace('`', '\'')).ToList();
			whoisString.Append($"    **{foundNicknames.Count}** known nickname{(foundNicknames.Count > 1 ? "s" : "")}: ");
			whoisString.AppendLine(foundNicknames.ToNames());

			return whoisString.ToString().Replace("@everyone", "@-everyone").Replace("@here", "@-here");
		}
		public string GetWhoisString(ServerContext dbContext, IGuildUser user = null)
		{
			StringBuilder whoisString = new StringBuilder();

			if( user != null )
				whoisString.AppendLine($"<@{this.UserId}>: `{this.UserId}` | `{user.GetUsername()}`\n" +
									   $"    Account created at: `{Utils.GetTimestamp(Utils.GetTimeFromId(this.UserId))}`");
			else
				whoisString.AppendLine($"<@{this.UserId}>: `{this.UserId}`\n" +
									   $"    Account created at: `{Utils.GetTimestamp(Utils.GetTimeFromId(this.UserId))}`");

			if( user?.JoinedAt != null )
				whoisString.AppendLine($"    Joined the server: `{Utils.GetTimestamp(user.JoinedAt.Value)}`");

			if( user is SocketGuildUser u )
				whoisString.AppendLine("    Roles: " + u.Roles.Select(r => r.Name.Replace('`', '\'')).ToNames());

			if( this.Verified )
				whoisString.AppendLine("    Verified: `true`");

			if( this.Ignored )
				whoisString.AppendLine("    Ignored by Logging: `true`");

			if( this.MutedUntil > DateTime.UtcNow )
				whoisString.AppendLine("    Muted until: " + Utils.GetTimestamp(this.MutedUntil));

			if( this.BannedUntil > DateTime.UtcNow )
				whoisString.AppendLine("    Banned until: " + Utils.GetTimestamp(this.BannedUntil));

			List<string> foundUsernames = dbContext.Usernames.AsQueryable()
				.Where(u => u.ServerId == this.ServerId && u.UserId == this.UserId).AsEnumerable()
				.Select(u => u.Name.Replace('`', '\'')).ToList();
			whoisString.Append($"    **{foundUsernames.Count}** known username{(foundUsernames.Count > 1 ? "s" : "")}: ");
			whoisString.Append(foundUsernames.Take(5).ToNames());
			if( foundUsernames.Count > 5 )
				whoisString.Append($" (to see them all use `names <userid or mention>`)");
			whoisString.AppendLine();

			List<string> foundNicknames = dbContext.Nicknames.AsQueryable()
				.Where(u => u.ServerId == this.ServerId && u.UserId == this.UserId).AsEnumerable()
				.Select(u => u.Name.Replace('`', '\'')).ToList();
			whoisString.Append($"    **{foundNicknames.Count}** known nickname{(foundNicknames.Count > 1 ? "s" : "")}: ");
			whoisString.Append(foundNicknames.Skip(foundNicknames.Count < 5 ? 0 : foundNicknames.Count-5).ToNames());
			if( foundNicknames.Count > 5 )
				whoisString.Append($" (to see them all use `names <userid or mention>`)");
			whoisString.AppendLine();

			if( this.WarningCount > 0 || !string.IsNullOrEmpty(this.Notes) )
			{
				whoisString.AppendLine($"They have **{this.WarningCount}** warning{(this.WarningCount > 1 ? "s" : "")}, with these notes:");
				whoisString.AppendLine(GetWarningsString(true));
			}

			return whoisString.ToString().Replace("@everyone", "@-everyone").Replace("@here", "@-here");
		}

		public string GetWarningsString(bool skipCount = false)
		{
			StringBuilder whoisString = new StringBuilder();
			if( this.WarningCount > 0 || !string.IsNullOrEmpty(this.Notes) )
			{
				if( !skipCount )
					whoisString.AppendLine($"You have **{this.WarningCount}** warning{(this.WarningCount > 1 ? "s" : "")}, with these notes:");
				int i = 0;
				foreach( string w in this.Notes.Split('|') )
				{
					whoisString.AppendLine($"**{++i})** {w.Trim()}");
				}
				return whoisString.ToString().Replace("@everyone", "@-everyone").Replace("@here", "@-here");
			}

			return "You've been a Good boi..\n_\\*pats*_";
		}

		public void AddWarning(string warning)
		{
			warning = $"{Utils.GetDatestamp()}: {warning}";
			this.WarningCount++;
			this.Notes += string.IsNullOrEmpty(this.Notes) ? warning : (" | " + warning);
		}
	}

	[Table("usernames")]
	public class Username
	{
		[Required]
		[Column("serverid")]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public guid ServerId{ get; set; } = 0;

		[Required]
		[Column("userid")]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public guid UserId{ get; set; } = 0;

		[Required]
		[Column("username")]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public string Name{ get; set; } = "";
	}

	[Table("nicknames")]
	public class Nickname
	{
		[Required]
		[Column("serverid")]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public guid ServerId{ get; set; } = 0;

		[Required]
		[Column("userid")]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public guid UserId{ get; set; } = 0;

		[Required]
		[Column("nickname")]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public string Name{ get; set; } = "";
	}
}

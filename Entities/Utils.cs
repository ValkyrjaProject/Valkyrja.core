using System;
using System.Runtime.CompilerServices;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Net;
using Discord.WebSocket;

using guid = System.UInt64;

namespace Valkyrja.entities
{
	public class Utils
	{
		public static Random Random{ get; set; } = new Random();

		public static TimeSpan? GetTimespanFromString(string input)
		{
			try
			{
				TimeSpan result = TimeSpan.Zero;
				Match dayMatch = Regex.Match(input, "\\d+d", RegexOptions.IgnoreCase);
				Match hourMatch = Regex.Match(input, "\\d+h", RegexOptions.IgnoreCase);
				Match minuteMatch = Regex.Match(input, "\\d+m", RegexOptions.IgnoreCase);
				Match secondMatch = Regex.Match(input, "\\d+s", RegexOptions.IgnoreCase);

				if( !dayMatch.Success && !hourMatch.Success && !minuteMatch.Success && !secondMatch.Success )
				{
					return null;
				}

				if( dayMatch.Success )
					result += TimeSpan.FromDays(int.Parse(dayMatch.Value.Trim('d').Trim('D')));
				if( hourMatch.Success )
					result += TimeSpan.FromHours(int.Parse(hourMatch.Value.Trim('h').Trim('H')));
				if( minuteMatch.Success )
					result += TimeSpan.FromMinutes(int.Parse(minuteMatch.Value.Trim('m').Trim('M')));
				if( secondMatch.Success )
					result += TimeSpan.FromSeconds(int.Parse(secondMatch.Value.Trim('s').Trim('S')));

				return result;
			}
			catch( Exception )
			{
				return null;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static DateTime GetTimeFromId(guid id)
		{
			return new DateTime((long)(((id / 4194304) + 1420070400000) * 10000 + 621355968000000000));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string GetTimestamp()
		{
			return GetTimestamp(DateTime.UtcNow);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string GetTimestamp(DateTime time)
		{
			return time.ToUniversalTime().ToString("yyyy-MM-dd_HH:mm:ss") + " UTC";
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string GetDatestamp()
		{
			return GetDatestamp(DateTime.UtcNow);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string GetDatestamp(DateTime time)
		{
			return time.ToUniversalTime().ToString("yyyy-MM-dd");
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string GetTimestamp(DateTimeOffset time)
		{
			return time.ToUniversalTime().ToString("yyyy-MM-dd_HH:mm:ss") + " UTC";
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string HandleHttpException(HttpException exception)
		{
			if( exception.HttpCode == System.Net.HttpStatusCode.Forbidden || (exception.DiscordCode.HasValue && exception.DiscordCode.Value == 50013) || exception.Message.Contains("Missing Access") || exception.Message.Contains("Missing Permissions") )
				return "Something went wrong, I may not have server permissions to do that.\n(Hint: <http://i.imgur.com/T8MPvME.png>)";
			if( exception.HttpCode == System.Net.HttpStatusCode.NotFound || exception.Message.Contains("NotFound") )
				return "Not found.";

			return "<:DiscordPoop:356545886454677506>";
		}
	}

	public static class Bash
	{
		public static string Run(string cmd)
		{
			string escapedArgs = cmd.Replace("\"", "\\\"");

			Process process = new Process() {
				StartInfo = new ProcessStartInfo {
					FileName = "/bin/bash",
					Arguments = $"-c \"{escapedArgs}\"",
					RedirectStandardOutput = true,
					UseShellExecute = false,
					CreateNoWindow = true,
				}
			};
			process.Start();
			string result = process.StandardOutput.ReadToEnd();
			process.WaitForExit();
			process.Dispose();
			return result;
		}
	}

	public static class Extensions
	{
		public static string ToFancyString(this TimeSpan self)
		{
			StringBuilder stringBuilder = new StringBuilder();
			bool continued = false;
			if( self.Days > 0 )
			{
				stringBuilder.Append(self.Days == 1 ? " day" : $" `{self.Days}` days");
				continued = true;
			}
			if( self.Hours > 0 )
			{
				if( continued )
					stringBuilder.Append(self.Minutes + self.Seconds > 0 ? ", " : " and");
				stringBuilder.Append(self.Hours == 1 ? " hour" : $" `{self.Hours}` hours");
				continued = true;
			}
			if( self.Minutes > 0 )
			{
				if( continued )
					stringBuilder.Append(self.Seconds > 0 ? ", " : " and");
				stringBuilder.Append(self.Minutes == 1 ? " minute" : $" `{self.Minutes}` minutes");
				continued = true;
			}
			if( self.Seconds > 0 )
			{
				if( continued )
					stringBuilder.Append(" and");
				stringBuilder.Append(self.Seconds == 1 ? " second" : $" `{self.Seconds}` seconds");
			}
			stringBuilder.Append(".");
			return stringBuilder.ToString();
		}

		public static string GetSpaces(this string self, uint totalWidth)
		{
			string spaces = "";
			for( int i = self.Length; i <= totalWidth; i++ )
				spaces += " ";
			return spaces;
		}
		public static string PrependSpaces(this string self, uint totalWidth)
		{
			return self.GetSpaces(totalWidth) + self;
		}
		public static string AppendSpaces(this string self, uint totalWidth)
		{
			return self + self.GetSpaces(totalWidth);
		}

		public static string ToMentions(this guid[] self)
		{
			if( self == null || !self.Any() )
				return "Nobody.";

			StringBuilder builder = new StringBuilder();
			for(int i = 0; i < self.Length; i++)
			{
				builder.Append((i == 0 ? "<@" : i < self.Length-1 ? ">, <@" : "> and <@") + self[i].ToString());
			}

			if( self.Length > 0 )
				builder.Append(">");

			return builder.ToString();
		}

		public static string ToMentions(this List<guid> self)
		{
			if( self == null || !self.Any() )
				return "Nobody.";

			StringBuilder builder = new StringBuilder();
			for(int i = 0; i < self.Count; i++)
			{
				builder.Append((i == 0 ? "<@" : i < self.Count-1 ? ">, <@" : "> and <@") + self[i].ToString());
			}

			if( self.Count > 0 )
				builder.Append(">");

			return builder.ToString();
		}

		public static string ToMentions(this IEnumerable<guid> self)
		{
			if( self == null || !self.Any() )
				return "Nobody.";

			StringBuilder builder = new StringBuilder();
			int count = self.Count();
			int i = -1;
			foreach(guid element in self)
			{
				builder.Append((++i == 0 ? "<@" : i < count-1 ? ">, <@" : "> and <@") + element.ToString());
			}

			if( count > 0 )
				builder.Append(">");

			return builder.ToString();
		}

		public static string ToNames(this string[] self)
		{
			if( self == null || !self.Any() )
				return "None.";

			StringBuilder builder = new StringBuilder();
			for(int i = 0; i < self.Length; i++)
			{
				builder.Append((i == 0 ? "`" : i < self.Length-1 ? "`, `" : "` and `") + self[i].Replace("`", "'"));
			}

			if( self.Length > 0 )
				builder.Append("`");

			return builder.ToString();
		}

		public static string ToNames(this List<string> self)
		{
			if( self == null || !self.Any() )
				return "None.";

			StringBuilder builder = new StringBuilder();
			for(int i = 0; i < self.Count; i++)
			{
				builder.Append((i == 0 ? "`" : i < self.Count-1 ? "`, `" : "` and `") + self[i].Replace("`", "'"));
			}

			if( self.Count > 0 )
				builder.Append("`");

			return builder.ToString();
		}

		public static string ToNames(this IEnumerable<string> self)
		{
			if( self == null || !self.Any() )
				return "None";

			StringBuilder builder = new StringBuilder();
			int count = self.Count();
			int i = -1;
			foreach(string element in self)
			{
				builder.Append((++i == 0 ? "`" : i < count-1 ? "`, `" : "` and `") + element.Replace("`", "'"));
			}

			if( count > 0 )
				builder.Append("`");

			return builder.ToString();
		}

		public static string ToNamesList(this string[] self)
		{
			if( self == null || !self.Any() )
				return "None";

			StringBuilder builder = new StringBuilder();
			for(int i = 0; i < self.Length; i++)
			{
				builder.Append((i == 0 ? "`" : "`\n`") + self[i].Replace("`", "'"));
			}

			if( self.Length > 0 )
				builder.Append("`");

			return builder.ToString();
		}

		public static string ToNamesList(this List<string> self)
		{
			if( self == null || !self.Any() )
				return "None.";

			StringBuilder builder = new StringBuilder();
			for(int i = 0; i < self.Count; i++)
			{
				builder.Append((i == 0 ? "`" : "`\n`") + self[i].Replace("`", "'"));
			}

			if( self.Count > 0 )
				builder.Append("`");

			return builder.ToString();
		}

		public static string ToNamesList(this IEnumerable<string> self)
		{
			if( self == null || !self.Any() )
				return "None.";

			StringBuilder builder = new StringBuilder();
			int count = self.Count();
			int i = -1;
			foreach(string element in self)
			{
				builder.Append((++i == 0 ? "`" : "`\n`") + element.Replace("`", "'"));
			}

			if( count > 0 )
				builder.Append("`");

			return builder.ToString();
		}
	}

	public static class ConcurrentDictionaryEx
	{
		public static bool Remove<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> self, TKey key)
		{
			return self.TryRemove(key, out _);
		}
		public static bool Add<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> self, TKey key, TValue value)
		{
			return self.TryAdd(key, value);
		}
	}

	public static class DiscordEx
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string GetNickname(this IGuildUser self)
		{
			return !string.IsNullOrWhiteSpace(self.Nickname) ? self.Nickname : self.Username;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string GetNickname(this IUser self)
		{
			return (self as IGuildUser).GetNickname();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string GetName(this IGuildUser self)
		{
			return !string.IsNullOrWhiteSpace(self.Nickname) ? self.Nickname : self.Username +"#"+ self.Discriminator;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string GetUsername(this IUser self)
		{
			return self.Username + "#" + self.DiscriminatorValue.ToString("0000");
		}

		public static async Task<IUserMessage> SendMessageSafe(this IUser self, string message, Embed embed = null) => await SendMessageSafe(async m => await self.SendMessageAsync(m, false, embed), message);
		public static async Task<IUserMessage> SendMessageSafe(this ISocketMessageChannel self, string message, Embed embed = null, AllowedMentions allowedMentions = null) => await SendMessageSafe(async m => await self.SendMessageAsync(m, false, embed, allowedMentions: allowedMentions ?? AllowedMentions.Regular), message);

		private static async Task<IUserMessage> SendMessageSafe(Func<string, Task<IUserMessage>> sendMessage, string message)
		{
			if( message == null )
			{
				return await sendMessage(null);
			}


			string safetyCopy = "";
			string newChunk = "";

			//message = Regex.Replace(message, "<@&\\d+>", "@role"); //HACK - temporary solution to ensure that we're not pinging roles til D.NET figures their shit out.

			while( message.Length > GlobalConfig.MessageCharacterLimit )
			{
				int split = message.Substring(0, GlobalConfig.MessageCharacterLimit).LastIndexOf('\n');
				string chunk = "";

				if( split == -1 )
				{
					chunk = message;
					message = "";
				}
				else
				{
					chunk = message.Substring(0, split);
					message = message.Substring(split + 1);
				}

				while( chunk.Length > GlobalConfig.MessageCharacterLimit )
				{
					safetyCopy = newChunk;
					split = chunk.Substring(0, GlobalConfig.MessageCharacterLimit).LastIndexOf(' ');
					if( split == -1 || (safetyCopy.Length == (newChunk = chunk.Substring(0, split)).Length && safetyCopy == newChunk) )
					{
						return await sendMessage("I've encountered an error trying to send a single word longer than " + GlobalConfig.MessageCharacterLimit.ToString() + " characters.");
					}

					await sendMessage(newChunk);
					chunk = chunk.Substring(split + 1);
				}

				await sendMessage(chunk);
			}

			if( !string.IsNullOrWhiteSpace(message) )
				return await sendMessage(message);

			return null;
		}
	}
}

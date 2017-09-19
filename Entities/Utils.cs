using System;
using System.Runtime.CompilerServices;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

using guid = System.UInt64;

namespace Botwinder.entities
{
	public class Utils
	{
		public static Random Random{ get; set; } = new Random();

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
		public static string GetTimestamp(DateTimeOffset time)
		{
			return time.ToUniversalTime().ToString("yyyy-MM-dd_HH:mm:ss") + " UTC";
		}
	}

	public static class ConcurrentDictionaryEx
	{
		public static bool Remove<TKey, TValue>(
			this ConcurrentDictionary<TKey, TValue> self, TKey key) {
			TValue ignored;
			return self.TryRemove(key, out ignored);
		}
		public static bool Add<TKey, TValue>(
			this ConcurrentDictionary<TKey, TValue> self, TKey key, TValue value) {
			return self.TryAdd(key, value);
		}
	}

	public static class DiscordEx
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string GetName(this IGuildUser self)
		{
			return !string.IsNullOrWhiteSpace(self.Nickname) ? self.Nickname : self.Username +"#"+ self.Discriminator;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string GetUsername(this IUser self)
		{
			return self.Username +"#"+ self.Discriminator;
		}

		public static async Task SendMessageSafe(this IUser self, string message, Embed embed = null) => await SendMessageSafe(async m => await self.SendMessageAsync(m, false, embed), message);
		public static async Task SendMessageSafe(this ISocketMessageChannel self, string message, Embed embed = null) => await SendMessageSafe(async m => await self.SendMessageAsync(m, false, embed), message);
		//public static async Task SendMessageSafe(this IDMChannel self, string message, Embed embed = null) => await SendMessageSafe(async m => await self.SendMessageAsync(m, false, embed), message); // I don't think that we will ever need this one.

		public static async Task SendMessageSafe(Func<string, Task> sendMessage, string message)
		{
			string safetyCopy = "";
			string newChunk = "";

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
						await sendMessage("I've encountered an error trying send a single word longer than " + GlobalConfig.MessageCharacterLimit.ToString() + " characters.");
						return;
					}

					await sendMessage(newChunk);
					chunk = chunk.Substring(split + 1);
				}
				await sendMessage(chunk);
			}

			if( !string.IsNullOrWhiteSpace(message) )
				await sendMessage(message);
		}
	}
}

using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Diagnostics;
using Botwinder.entities;
using Discord.WebSocket;
using guid = System.UInt64;

namespace Botwinder.core
{
	public partial class BotwinderClient<TUser> : IBotwinderClient<TUser>, IDisposable where TUser : UserData, new()
	{
		public async Task SendMessageToChannel(SocketTextChannel channel, string message)
		{
			LogMessage(LogType.Response, channel, this.GlobalConfig.UserId, message);
			await channel.SendMessageSafe(message);
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsGlobalAdmin(guid id)
		{
			return this.GlobalConfig.AdminUserId == id;
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsSubscriber(guid id)
		{
			return this.GlobalDb.PartneredServers.Any(u => u.ServerId == id);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsPartner(guid id)
		{
			return this.GlobalDb.PartneredServers.Any(s => s.ServerId == id);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsPremiumSubscriber(guid id)
		{
			return this.GlobalDb.Subscribers.Any(u => u.UserId == id && u.IsPremium);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsBonusSubscriber(guid id)
		{
			return this.GlobalDb.Subscribers.Any(u => u.UserId == id && u.HasBonus);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsPremiumPartner(guid id)
		{
			return this.GlobalDb.PartneredServers.Any(s => s.ServerId == id && s.IsPremium);
		}


		public void LogMessage(LogType logType, SocketTextChannel channel, guid authorId, string message)
		{
			LogEntry logEntry = new LogEntry(){
				Type = logType,
				UserId = authorId,
				ChannelId = channel.Id,
				ServerId = channel.Guild.Id,
				DateTime = DateTime.UtcNow,
				Message = message
			};
			this.GlobalDb.Log.Add(logEntry);

			lock(this.DbLock)
				this.GlobalDb.SaveChanges();
		}

		public void LogMessage(LogType logType, SocketTextChannel channel, SocketMessage message)
		{
			LogEntry logEntry = new LogEntry(){
				Type = logType,
				UserId = message.Author.Id,
				MessageId = message.Id,
				ChannelId = channel.Id,
				ServerId = channel.Guild.Id,
				DateTime = DateTime.UtcNow,
				Message = message.Content
			};
			this.GlobalDb.Log.Add(logEntry);

			lock(this.DbLock)
				this.GlobalDb.SaveChanges();
		}

		public async Task LogException(Exception exception, CommandArguments<TUser> args) =>
			await LogException(exception, "--Command: "+ args.Command.Id + " | Parameters: " + args.TrimmedMessage, args.Server.Id);

		public async Task LogException(Exception exception, string data, guid serverId = 0)
		{
			Console.WriteLine(exception.Message);
			Console.WriteLine(exception.StackTrace);
			Console.WriteLine(data);

			ExceptionEntry exceptionEntry = new ExceptionEntry(){
				Message = exception.Message,
				Stack = exception.StackTrace,
				Data = data,
				ServerId = serverId,
				DateTime = DateTime.UtcNow
			};
			await this.Events.Exception(exceptionEntry);
		}

		public async Task LogMaintenanceAndExit()
		{
			if( this.CurrentOperations.Any() )
				return;

			try
			{
				SocketTextChannel channel = null;
				SocketGuild server = null;
				if( (server = this.DiscordClient.GetGuild(this.GlobalConfig.MainServerId)) != null && (channel = server.GetTextChannel(this.GlobalConfig.MainChannelId)) != null )
				{
					TimeSpan uptime = DateTimeOffset.UtcNow - this.TimeStarted;
					int days = uptime.Days;
					int hours = uptime.Hours;
					int minutes = uptime.Minutes;
					int seconds = uptime.Seconds;

					string response = string.Format("__**Performing automated maintenance**__\n\n" +
					                                "Time: `{0}`\n" +
					                                "Uptime: `{1}`\n" +
					                                "Disconnects: `{2:0}`\n" +
					                                "Active Threads: `{3}`\n" +
					                                "Memory usage: `{4:#0.00} MB`\n" +
					                                "Operations ran: `{5}`\n" +
					                                "Messages total: `{6}`\n" +
					                                "Messages per minute: `{7}`\n",
						Utils.GetTimestamp(),
						(days == 0 ? "" : (days.ToString() + (days == 1 ? " day, " : " days, "))) +
						(hours == 0 ? "" : (hours.ToString() + (hours == 1 ? " hour, " : " hours, "))) +
						(minutes == 0 ? "" : (minutes.ToString() + (minutes == 1 ? " minute, " : " minutes "))) +
						((days == 0 && hours == 0 && minutes == 0 ? "" : "and ") + seconds.ToString() + (seconds == 1 ? " second." : " seconds.")),
						this.CurrentShard.Disconnects,
						Process.GetCurrentProcess().Threads.Count,
						(GC.GetTotalMemory(false) / 1000000f),
						this.CurrentShard.OperationsRan,
						this.CurrentShard.MessagesTotal,
						this.CurrentShard.MessagesPerMinute
					);

					await channel.SendMessageSafe(response);
				}
			} catch(Exception exception)
			{
				await LogException(exception, "--LogMaintenance");
			}

			Dispose();

			await Task.Delay(500);
			Environment.Exit(0);
		}
	}
}

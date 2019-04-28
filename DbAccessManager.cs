using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Botwinder.entities;
using Discord.WebSocket;

using guid = System.UInt64;

namespace Botwinder.core
{
	public class DbAccessManager
	{
		private readonly BotwinderClient Client;
		public readonly string DbConnectionString;

		private readonly SemaphoreSlim UserDatabaseLock;

		public DbAccessManager(BotwinderClient client, string dbConnectionString)
		{
			this.Client = client;
			this.DbConnectionString = dbConnectionString;
			this.UserDatabaseLock = new SemaphoreSlim(0, 1);
		}

		public async Task<List<ChannelConfig>> GetReadOnlyChannelConfig(Func<ChannelConfig, bool> query)
		{
			List<ChannelConfig> channels = null;
			ServerContext context = ServerContext.Create(this.DbConnectionString);
			try
			{
				channels = context.Channels.Where(query).ToList();
			}
			catch(Exception e)
			{
				await this.Client.LogException(e, "GetReadOnlyChannelConfig");
			}
			finally
			{
				context.Dispose();
			}

			return channels;
		}

		public async Task<List<UserData>> GetReadOnlyUserData(Func<UserData, bool> query)
		{
			await Lock();
			List<UserData> userData = null;
			ServerContext context = ServerContext.Create(this.DbConnectionString);
			try
			{
				userData = context.UserDatabase.Where(query).ToList();
			}
			catch(Exception e)
			{
				await this.Client.LogException(e, "GetReadOnlyUserData");
			}
			finally
			{
				context.Dispose();
				Unlock();
			}

			return userData;
		}

		public async Task<UserData> GetReadOnlyUserData(guid serverId, guid userId)
		{
			await Lock();
			UserData userData = null;
			ServerContext context = ServerContext.Create(this.DbConnectionString);
			try
			{
				userData = context.UserDatabase.FirstOrDefault(u => u.ServerId == serverId && u.UserId == userId);
				if( userData == null )
				{
					userData = new UserData(){
						ServerId = serverId,
						UserId = userId
					};
				}
			}
			catch(Exception e)
			{
				await this.Client.LogException(e, "GetReadOnlyUserData");
			}
			finally
			{
				context.Dispose();
				Unlock();
			}

			return userData;
		}

		public async Task ModifyUserData(guid serverId, guid userId, Func<UserData, Task> action)
		{
			await Lock();
			ServerContext context = ServerContext.Create(this.DbConnectionString);
			try
			{
				UserData userData = context.UserDatabase.FirstOrDefault(u => u.ServerId == serverId && u.UserId == userId);
				if( userData == null )
				{
					userData = new UserData(){
						ServerId = serverId,
						UserId = userId
					};
					context.UserDatabase.Add(userData);
				}

				await action(userData);

				context.SaveChanges();
			}
			catch(Exception e)
			{
				await this.Client.LogException(e, "ModifyUserData");
			}
			finally
			{
				context.Dispose();
				Unlock();
			}
		}

		public async Task ModifyUserData(Func<UserData, bool> query, Func<UserData, Task> action)
		{
			await Lock();
			ServerContext context = ServerContext.Create(this.DbConnectionString);
			try
			{
				IEnumerable<UserData> users = context.UserDatabase.Where(query);
				foreach( UserData userData in users )
					await action(userData);

				context.SaveChanges();
			}
			catch(Exception e)
			{
				await this.Client.LogException(e, "ModifyUserData");
			}
			finally
			{
				context.Dispose();
				Unlock();
			}
		}

		public async Task<int> ForEachModifyUserData(guid serverId, IEnumerable<guid> userIds, Func<guid, UserData, Task<bool>> action)
		{
			return await ForEachModifyUserData<guid>(userIds,
				(o, u) => serverId == u.ServerId && o == u.UserId,
				(o, u) => {
					u.UserId = o;
					u.ServerId = serverId;
				}, action);
		}

		public async Task<int> ForEachModifyUserData(IEnumerable<SocketGuildUser> collection, Func<SocketGuildUser, UserData, Task<bool>> action)
		{
			return await ForEachModifyUserData<SocketGuildUser>(collection,
				(o, u) => o.Guild.Id == u.ServerId && o.Id == u.UserId,
				(o, u) => {
					u.UserId = o.Id;
					u.ServerId = o.Guild.Id;
				}, action);
		}

		/// <summary>
		/// Iterate through <see cref="IEquatable{T}"/> and modify matching <see cref="UserData"/> with the action.
		/// </summary>
		/// <param name="collection">Collection to iterate over.</param>
		/// <param name="getUserDataQuery">Query to fetch the <see cref="UserData"/> based on the <typeparam name="T">enumerated object.</typeparam></param>
		/// <param name="initUserData">Action to initialize newly created <see cref="UserData"/> object if not found in the database.</param>
		/// <param name="action">Action to modify the data, return true to break the loop.</param>
		/// <returns>Total count of modified objects.</returns>
		public async Task<int> ForEachModifyUserData<T>(IEnumerable<T> collection, Func<T, UserData, bool> getUserDataQuery, Action<T, UserData> initUserData, Func<T, UserData, Task<bool>> action)
		{
			int count = 0;
			await Lock();
			ServerContext context = ServerContext.Create(this.DbConnectionString);
			try
			{
				foreach( T o in collection )
				{
					UserData userData = context.UserDatabase.FirstOrDefault(u => getUserDataQuery(o, u));
					if( userData == null )
					{
						userData = new UserData();
						initUserData(o, userData);
						context.UserDatabase.Add(userData);
					}

					if( await action(o, userData) )
						break;

					count++;
				}

				context.SaveChanges();
			}
			catch(Exception e)
			{
				await this.Client.LogException(e, "ForEachModifyUserData");
			}
			finally
			{
				context.Dispose();
				Unlock();
			}

			return count;
		}


		public async Task UpdateUsernames(SocketGuild guild)
		{
			ServerContext context = ServerContext.Create(this.DbConnectionString);
			try
			{
				foreach( SocketGuildUser user in guild.Users )
				{
					UpdateNickname(user, context); //Not UserData - Doesn't need a lock? Different lock?
				}

				context.SaveChanges();
			}
			catch(Exception e)
			{
				await this.Client.LogException(e, "UpdateUsernames");
			}
			finally
			{
				context.Dispose();
			}
		}

		public async Task UpdateUsername(SocketGuildUser user)
		{
			if( user == null )
				return;

			await Lock();
			ServerContext context = ServerContext.Create(this.DbConnectionString);
			try
			{
				UpdateNickname(user, context);
				UpdateUsername(user, context);

				context.SaveChanges();
			}
			catch(Exception e)
			{
				await this.Client.LogException(e, "UpdateUsername");
			}
			finally
			{
				context.Dispose();
				Unlock();
			}
		}

		private void UpdateUsername(SocketGuildUser user, ServerContext dbContext)
		{
			UserData userData = dbContext.UserDatabase.FirstOrDefault(u => u.ServerId == user.Guild.Id && u.UserId == user.Id);
			if( userData == null )
			{
				userData = new UserData(){
					ServerId = user.Guild.Id,
					UserId = user.Id
				};
				dbContext.UserDatabase.Add(userData);
			}

			userData.LastUsername = user.Username;
		}

		private void UpdateNickname(SocketGuildUser user, ServerContext dbContext)
		{
			if( !dbContext.Usernames.Any(u => u.ServerId == user.Guild.Id && u.UserId == user.Id && u.Name == user.Username) )
			{
				dbContext.Usernames.Add(new Username(){
					ServerId = user.Guild.Id,
					UserId = user.Id,
					Name = user.Username
				});
			}

			if( !string.IsNullOrEmpty(user.Nickname) &&
			    !dbContext.Nicknames.Any(u => u.ServerId == user.Guild.Id && u.UserId == user.Id && u.Name == user.Nickname) )
			{
				dbContext.Nicknames.Add(new Nickname(){
					ServerId = user.Guild.Id,
					UserId = user.Id,
					Name = user.Nickname
				});
			}
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private async Task Lock()
		{
			await this.UserDatabaseLock.WaitAsync();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Unlock()
		{
			this.UserDatabaseLock.Release();
		}
	}
}

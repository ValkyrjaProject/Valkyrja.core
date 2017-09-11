using System;
using System.Linq;
using System.Threading.Tasks;
using Botwinder.entities;

namespace Botwinder.core
{
	public class Operation<TUser> where TUser : UserData, new()
	{
		public enum State
		{
			Ready = 0,
			Awaiting,
			AwaitDone,
			Running,
			Finished,
			Canceled
		}

		public CommandArguments<TUser> CommandArgs = null;
		public State CurrentState = State.Ready;
		public DateTime TimeCreated = DateTime.UtcNow;
		public DateTime TimeStarted = DateTime.MinValue;
		public float AllocatedMemoryStarted = 0f;
		public bool IsLarge = false;

		private Operation(CommandArguments<TUser> commandArgs, float memory, bool isLarge)
		{
			this.CommandArgs = commandArgs;
			this.AllocatedMemoryStarted = memory;
			this.IsLarge = isLarge;
		}

		/// <summary> Create a new Operation and add it to the queue. </summary>
		public static Operation<TUser> Create(BotwinderClient<TUser> client, CommandArguments<TUser> e, bool isLarge = false)
		{
			Operation<TUser> op = new Operation<TUser>(e, GC.GetTotalMemory(false) / 1000000f, isLarge);
			lock(client.OperationsLock)
				client.CurrentOperations.Add(op);
			return op;
		}

		/// <summary> Execute this operation. </summary>
		public async Task Execute(BotwinderClient<TUser> client)
		{
			
		}

		/// <summary> This is blocking call that will await til there are less than config.MaximumConcurrentOperations.
		/// Returns true if it was canceled, false otherwise. </summary>
		public async Task<bool> Await(BotwinderClient<TUser> client, Func<Task> onAwaitStarted)
		{
			client.CurrentShard.OperationsRan++;

			Operation<TUser> alreadyInQueue = null;
			lock(client.OperationsLock)
				alreadyInQueue = client.CurrentOperations.FirstOrDefault(o => o.CommandArgs.Command.Id == this.CommandArgs.Command.Id && o.CommandArgs.Channel.Id == this.CommandArgs.Channel.Id && o != this);

			if( alreadyInQueue != null )
			{
				lock(client.OperationsLock)
				{
					client.CurrentOperations.Remove(this);
					this.CurrentState = State.Canceled;
				}
				return true;
			}

			if( client.IsGlobalAdmin(this.CommandArgs.Message.Author.Id) )
			{
				lock(client.OperationsLock)
				{
					client.CurrentOperations.Remove(this);
					client.CurrentOperations.Insert(0, this);
				}
			}
			else
			{
				bool ShouldAwait()
				{
					int index = 0;
					lock(client.OperationsLock)
						return !((index = client.CurrentOperations.IndexOf(this)) < client.GlobalConfig.OperationsMax ||
						         (index < client.GlobalConfig.OperationsMax + client.GlobalConfig.OperationsExtra && !this.IsLarge));
				}

				while( this.CurrentState != State.Canceled && ShouldAwait() )
				{
					if( this.CurrentState == State.Ready )
					{
						this.CurrentState = State.Awaiting;
						if( onAwaitStarted != null )
							await onAwaitStarted();
					}

					await Task.Delay(1000);
				}
			}

			if( this.CurrentState != State.Canceled )
			{
				this.TimeStarted = DateTime.UtcNow;
				this.CurrentState = State.AwaitDone;
			}

			return this.CurrentState == State.Canceled;
		}

		/// <summary> This is blocking call that will await til the connection is peachy.
		/// Returns true if the operation was canceled, false otherwise. </summary>
		public async Task<bool> AwaitConnection(BotwinderClient<TUser> client)
		{
			while( this.CurrentState != State.Canceled && !client.IsConnected )
				await Task.Delay(1000);

			return this.CurrentState == State.Canceled;
		}

		/// <summary> Gracefully cancel this operation and remove it from the queue. </summary>
		public void Cancel(BotwinderClient<TUser> client)
		{
			this.CurrentState = State.Canceled;
			lock(client.OperationsLock)
				client.CurrentOperations.Remove(this);
		}

		/// <summary> Gracefully finalise this operation and remove it from the list. </summary>
		public void Finalise(BotwinderClient<TUser> client)
		{
			this.CurrentState = State.Finished;
			lock(client.OperationsLock)
				client.CurrentOperations.Remove(this);
		}
	}
}

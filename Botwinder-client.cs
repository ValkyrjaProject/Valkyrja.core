using System;
using Botwinder.entities;

namespace Botwinder.core
{
    public partial class BotwinderClient<TUser>: IDisposable where TUser: UserData, new()
    {
    }
}

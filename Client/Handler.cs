using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Lynx.Client
{
    public abstract partial class Handler
    {
        readonly Dictionary<string, IRaiser>? raiserMap;

        public string Name { get; private set; }
        public Server Server { get; private set; } = null!;

        protected Handler()
        {
            Type interfaceType = GetType().GetInterfaces().Single();
            Name = interfaceType.GetCustomAttribute<HandlerAttribute>()!.Name;
            raiserMap = MakeRaisers(this)?.ToDictionary(raiser => raiser.Name);
        }

        public void SetServer(Server server)
        {
            Server = server;
        }

        public void Receive(string command, byte[] bytes)
        {
            raiserMap?[command].Raise(bytes);
        }

        public async Task<TResult> Send<T, TResult>(Func<T, Task<TResult>> Command, T content)
        {
            string command = Command.Method.Name;
            byte[] bytes = Packer.Pack(content!);
            bytes = await Server.Send(Name, command, bytes);
            return Packer.Unpack<TResult>(bytes);
        }
    }
}

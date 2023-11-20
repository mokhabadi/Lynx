using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Lynx.Client
{
    public abstract partial class Handler
    {
        readonly Dictionary<string, Raiser> raiserMap;
        readonly MemoryStream contentStream = new();

        public string Name { get; private set; }
        public Server Server { get; private set; } = null!;

        protected Handler()
        {
            Type interfaceType = GetType().GetInterfaces().Single();
            Name = interfaceType.GetCustomAttribute<HandlerAttribute>()!.Name;
            raiserMap = MakeRaisers(this)?.ToDictionary(raiser => raiser.Name)!;
        }

        public void SetServer(Server server)
        {
            Server = server;
        }

        public void Receive(string command, MemoryStream stream)
        {
            raiserMap[command].Raise(stream);
        }

        public async Task<TResult> Send<T, TResult>(Func<T, Task<TResult>> Command, T content)
        {
            string command = Command.Method.Name;
            await Packer.Pack(content, contentStream);
            MemoryStream resultStream = await Server.Send(Name, command, contentStream);
            return await Packer.Unpack<TResult>(resultStream);
        }
    }
}

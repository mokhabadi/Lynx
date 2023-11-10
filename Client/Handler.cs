using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lynx.Client
{
    public abstract partial class Handler : IHandler
    {
        readonly Dictionary<string, IRaiser>? raiserMap;

        public string Name { get; private set; }
        public Server? Server { get; private set; }

        protected Handler()
        {
            Name = GetType().GetInterfaces().Single(type => type != typeofIHandler).Name;
            raiserMap = CreateRaiserMap(this);
        }

        public void SetServer(Server server) => Server = server;

        public void Receive(string command, byte[] bytes)
        {
            raiserMap?[command].Raise(bytes);
        }

        public async Task<TResult> Send<T, TResult>(Func<T, Task<TResult>> Command, T content)
        {
            string command = Command.Method.Name;
            byte[] bytes = await Packer.Pack(content!);
            bytes = await Server!.Send(Name, command, bytes);
            return await Packer.Unpack<TResult>(bytes);
        }
    }
}

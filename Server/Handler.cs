using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Lynx.Server
{
    public abstract partial class Handler
    {
        readonly Dictionary<string, Executer> executerMap;

        public string Name { get; private set; }
        public Client Client { get; private set; } = null!;

        protected Handler()
        {
            Type interfaceType = GetType().GetInterfaces().Single();
            Name = interfaceType.GetCustomAttribute<HandlerAttribute>()!.Name;
            executerMap = MakeExecuters(this)?.ToDictionary(executer => executer.Name)!;
        }

        public void SetClient(Client client)
        {
            Client = client;
            MakeRaisers(this);
        }

        public virtual void Initialize()
        {
        }

        public virtual Task Finalize()
        {
            return Task.CompletedTask;
        }

        public Task<MemoryStream> Receive(string command, MemoryStream contentStream)
        {
            return executerMap[command].Run(contentStream);
        }
    }
}

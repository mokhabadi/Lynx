using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Lynx.Server
{
    public abstract partial class Handler : IHandler
    {
        readonly Dictionary<string, IExecuter>? executerMap;

        public string Name { get; private set; }
        public Client Client { get; private set; }

        protected Handler()
        {
            string interfaceName = GetType().GetInterfaces().Single(type => type != typeofIHandler).Name;
            Name = Regex.Match(interfaceName, @"(?<=I)(.*)(?=Handler)").Value;
            executerMap = CreateExecuterMap(this);
            CreateRaisers(this);
        }

        public void SetClient(Client client) => Client = client;

        public virtual void Initialize() { }

        public virtual Task Finalize()
        {
            return Task.CompletedTask;
        }

        public Task<byte[]> Receive(string command, byte[] bytes)
        {
            return executerMap![command].Run(bytes);
        }
    }
}

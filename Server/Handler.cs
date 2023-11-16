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
        public Client? Client { get; private set; }

        protected Handler()
        {
            string interfaceName = GetType().GetInterfaces().Single(type => type != typeofIHandler).Name;
            Name = Regex.Match(interfaceName, @"(?=I)(.*)(?=Handler)").Value;
            executerMap = MakeExecuters(this)?.ToDictionary(executer => executer.Name);
            MakeRaisers(this);
        }

        public void SetClient(Client client)
        {
            Client = client;
        }

        public abstract void Initialize();

        public abstract Task Finalize();

        public Task<byte[]> Receive(string command, byte[] bytes)
        {
            return executerMap![command].Run(bytes);
        }
    }
}

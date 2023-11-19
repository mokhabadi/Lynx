using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace Lynx.Server
{
    public abstract class Executer
    {
        public string Name { get; protected set; } = null!;

        public abstract Task<MemoryStream> Run(Stream stream);
    }

    public class Executer<T, TResult> : Executer
    {
        delegate Task<TResult> Command(T content);

        readonly Handler handler;
        readonly Command Method;
        readonly MemoryStream resultStream = new();

        protected Executer(Handler handler, MethodInfo commandMethod)
        {
            this.handler = handler;
            Name = commandMethod.Name;
            Method = (Command)Delegate.CreateDelegate(typeof(Command), handler, commandMethod);
        }

        public override async Task<MemoryStream> Run(Stream stream)
        {
            T content = await Packer.Unpack<T>(stream);
            handler.Initialize();
            TResult result = await Method(content);
            await handler.Finalize();
            await Packer.Pack(result, resultStream);
            return resultStream;
        }
    }
}

using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Lynx.Server
{
    public interface IExecuter
    {
        string Name { get; }

        Task<byte[]> Run(byte[] bytes);
    }

    public class Executer<T, TResult> : IExecuter
    {
        delegate Task<TResult> Command(T content);

        readonly Handler handler;
        readonly Command Method;

        public string Name { get; }

        protected Executer(Handler handler, MethodInfo commandMethod)
        {
            this.handler = handler;
            Name = commandMethod.Name;
            Method = (Command)Delegate.CreateDelegate(typeof(Command), handler, commandMethod);
        }

        public async Task<byte[]> Run(byte[] bytes)
        {
            T content = await Packer.Unpack<T>(bytes);
            handler.Initialize();
            TResult result = await Method(content);
            await handler.Finalize();
            return await Packer.Pack(result);
        }
    }
}

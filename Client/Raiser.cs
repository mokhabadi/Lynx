using System;
using System.Reflection;

namespace Lynx.Client
{
    public interface IRaiser
    {
        string Name { get; }

        void Raise(byte[] bytes);
    }

    public class Raiser<T> : IRaiser
    {
        readonly Handler handler;
        readonly FieldInfo fieldInfo;

        public string Name { get; }

        protected Raiser(Handler handler, FieldInfo fieldInfo)
        {
            this.handler = handler;
            this.fieldInfo = fieldInfo;
            Name = fieldInfo.Name;
        }

        public void Raise(byte[] bytes)
        {
            Action<T>? EventAction = (Action<T>?)fieldInfo.GetValue(handler);

            if (EventAction == null)
                return;

            T content = Packer.Unpack<T>(bytes);
            EventAction(content);
        }
    }
}

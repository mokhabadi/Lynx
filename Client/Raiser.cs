using System;
using System.IO;
using System.Reflection;

namespace Lynx.Client
{
    public abstract class Raiser
    {
        public string Name { get; protected set; } = null!;

        public abstract void Raise(Stream stream);
    }

    public class Raiser<T> : Raiser
    {
        readonly Handler handler;
        readonly FieldInfo fieldInfo;

        protected Raiser(Handler handler, FieldInfo fieldInfo)
        {
            this.handler = handler;
            this.fieldInfo = fieldInfo;
            Name = fieldInfo.Name;
        }

        public override async void Raise(Stream stream)
        {
            Action<T>? EventAction = (Action<T>?)fieldInfo.GetValue(handler);

            if (EventAction == null)
                return;

            T content = await Packer.Unpack<T>(stream);
            EventAction(content);
        }
    }
}

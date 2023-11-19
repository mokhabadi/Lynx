using System;
using System.Reflection;

namespace Lynx.Server
{
    class Raiser<T>
    {
        readonly Handler handler;
        readonly string name;

        public Raiser(Handler handler, EventInfo eventInfo)
        {
            this.handler = handler;
            name = eventInfo.Name;
            eventInfo.AddEventHandler(handler, (Action<T>)EventInvoked);
        }

        public async void EventInvoked(T content)
        {
            Header header = new(0, MessageType.Event, handler.Name, name);
            byte[] bytes = Packer.Pack(content!);
            await handler.Client!.link.Send(header, bytes);
        }
    }
}

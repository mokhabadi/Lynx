using System;
using System.IO;
using System.Reflection;

namespace Lynx.Server
{
    class Raiser<T>
    {
        readonly Link link;
        readonly Header header;
        readonly MemoryStream stream = new();

        public Raiser(Handler handler, EventInfo eventInfo)
        {
            link = handler.Client.link;
            eventInfo.AddEventHandler(handler, (Action<T>)EventInvoked);
            header = new(0, MessageType.Event, handler.Name, eventInfo.Name);
        }

        public async void EventInvoked(T content)
        {
            Packer.Pack(content!, stream);
            await link.Send(header, stream);
        }
    }
}

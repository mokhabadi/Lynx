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
            link = handler.Client.Link;
            eventInfo.AddEventHandler(handler, (Action<T>)EventInvoked);
            header = new() { Type = MessageType.Event, Handler = handler.Name, Command = eventInfo.Name };
        }

        public async void EventInvoked(T content)
        {
            await Packer.Pack(content!, stream);
            await link.Send(header, stream);
        }
    }
}

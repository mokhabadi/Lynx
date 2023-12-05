using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Lynx.Server
{
    public class Client
    {
        readonly Dictionary<string, Handler> handlerMap;

        public long Id { get; private set; }
        public Link Link { get; private set; }

        public event Action<bool>? Closed;

        public Client(Link link)
        {
            Link = link;
            link.Received += Received;
            link.Closed += Close;
            Handler[] handlers = Handler.MakeHandlers(this);
            handlerMap = handlers.ToDictionary(handler => handler.Name);
        }

        public void SetId(long id)
        {
            Id = id;
        }

        public T Get<T>() where T : class
        {
            string Name = typeof(T).GetCustomAttribute<HandlerAttribute>()!.Name;
            return (handlerMap[Name] as T)!;
        }

        async void Received(Header header, MemoryStream contentStream)
        {
            MemoryStream resultStream = await handlerMap[header.Handler].Receive(header.Command, contentStream);
            await Link.Send(header, resultStream);
        }

        void Close(bool proper)
        {
            Link.Received -= Received;
            Link.Closed -= Close;
            Closed?.Invoke(proper);
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Lynx.Server
{
    public class Client
    {
        readonly Dictionary<string, Handler> handlerNameMap;
        readonly Dictionary<Type, Handler> handlerTypeMap;

        public long Id { get; private set; }
        public Link Link { get; private set; }

        public event Action<bool>? Closed;

        public Client(Link link)
        {
            Link = link;
            link.Received += Received;
            link.Closed += Close;
            Handler[] handlers = Handler.MakeHandlers(this);
            handlerNameMap = handlers.ToDictionary(handler => handler.Name);
            handlerTypeMap = handlers.ToDictionary(handler => handler.GetType());
        }

        public void SetId(long id)
        {
            Id = id;
        }

        public T Get<T>() where T : Handler
        {
            return (T)handlerTypeMap[typeof(T)];
        }

        async void Received(Header header, Stream contentStream)
        {
            MemoryStream resultStream = await handlerNameMap[header.Handler].Receive(header.Command, contentStream);
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

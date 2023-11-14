using System;
using System.Collections.Generic;
using System.Linq;

namespace Lynx.Server
{
    public class Client
    {
        readonly public Link link;
        readonly Dictionary<string, Handler>? handlerNameMap;
        readonly Dictionary<Type, Handler>? handlerTypeMap;

        public long Id { get; private set; }

        public event Action<bool>? Went;

        public Client(Link link)
        {
            this.link = link;
            link.Received += Received;
            link.Ended += End;
            Handler[]? handlers = Handler.MakeHandlers(this);
            handlerNameMap = handlers?.ToDictionary(handler => handler.Name);
            handlerTypeMap = handlers?.ToDictionary(handler => handler.GetType());
        }

        public void SetId(long id) => Id = id;

        public T Get<T>() where T : Handler
        {
            return (T)handlerTypeMap![typeof(T)];
        }

        void End(bool proper)
        {
            link.Received -= Received;
            link.Ended -= End;
            Went?.Invoke(proper);
        }

        async void Received(Header header, byte[] bytes)
        {
            bytes = await handlerNameMap![header.Handler].Receive(header.Command, bytes);
            await link.Send(header, bytes);
        }
    }
}

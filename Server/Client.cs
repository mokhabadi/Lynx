using System;
using System.Collections.Generic;

namespace Lynx.Server
{
    public class Client
    {
        readonly public Link link;
        readonly Dictionary<string, Handler>? handlerMap;

        public long Id { get; private set; }

        public event Action<bool> Went = _ => { };

        public Client(Link link)
        {
            this.link = link;
            link.Received += Received;
            link.Ended += End;
            handlerMap = Handler.CreateHandlerMap(this);
        }

        public void SetId(long id) => Id = id;

        public T Get<T>() where T : IHandler
        {
            IHandler handler = handlerMap![typeof(T).Name];
            return (T)handler;
        }

        void End(bool proper)
        {
            link.Received -= Received;
            link.Ended -= End;
            Went(proper);
        }

        async void Received(Header header, byte[] bytes)
        {
            bytes = await handlerMap![header.Handler].Receive(header.Command, bytes);
            await link.Send(header, bytes);
        }
    }
}

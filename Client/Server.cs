using System.Collections.Generic;
using System.Net.Security;
using System.Net.Sockets;
using System.Net;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace Lynx.Client
{
    public class Server
    {
        Link? link;
        long serial;
        readonly Dictionary<string, Handler>? handlerNameMap;
        readonly Dictionary<Type, Handler>? handlerTypeMap;

        public Server()
        {
            Handler[]? handlers = Handler.MakeHandlers(this);
            handlerNameMap = handlers?.ToDictionary(handler => handler.Name);
            handlerTypeMap = handlers?.ToDictionary(handler => handler.GetType());
        }

        public async Task<bool> ConnectAsync(string domain, int port)
        {
            try
            {
                ServicePointManager.DnsRefreshTimeout = 0;
                IPAddress[] addresses = await Dns.GetHostAddressesAsync(domain);
                TcpClient tcpClient = new() { NoDelay = true };
                await tcpClient.ConnectAsync(IPAddress.Loopback, port);
                SslStream sslStream = new(tcpClient.GetStream());
                await sslStream.AuthenticateAsClientAsync(domain);
                link = new(sslStream);
                link.Received += Received;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public T Get<T>() where T : Handler
        {
            return (T)handlerTypeMap![typeof(T)];
        }

        void Received(Header header, byte[] bytes)
        {
            if (header.Type != MessageType.Event) return;
            handlerNameMap![header.Handler].Receive(header.Command, bytes);
        }

        public async Task<byte[]> Send(string handler, string command, byte[] contentBytes)
        {
            Header header = new(++serial, MessageType.Request, handler, command);
            await link!.Send(header, contentBytes);
            Waiter waiter = new(header.Id);
            return await waiter.Wait(link);
        }
    }
}

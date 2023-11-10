using System.Collections.Generic;
using System.Net.Security;
using System.Net.Sockets;
using System.Net;
using System.Threading.Tasks;
using System.Linq;

namespace Lynx.Client
{
    public class Server
    {
        Link link;
        long serial;
        readonly Dictionary<string, Handler>? handlerMap;

        public Server()
        {
            handlerMap = Handler.MakeHandlers(this)?.ToDictionary(handler => handler.Name);
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

        public T Get<T>() where T : IHandler
        {
            IHandler handler = handlerMap![typeof(T).Name];
            return (T)handler;
        }

        void Received(Header header, byte[] bytes)
        {
            if (header.Type != MessageType.Event) return;
            handlerMap![header.Handler].Receive(header.Command, bytes);
        }

        public async Task<byte[]> Send(string handler, string command, byte[] contentBytes)
        {
            Header header = new(++serial, MessageType.Request, handler, command);
            await link.Send(header, contentBytes);
            Waiter waiter = new(header.Id);
            return await waiter.Wait(link);
        }
    }
}

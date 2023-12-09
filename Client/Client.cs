using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Reflection;
using System.Threading.Tasks;

namespace Lynx.Client
{
    public class Client
    {
        readonly Dictionary<string, Handler> handlerMap;
        Link link = null!;
        long serial;

        public Client()
        {
            Handler[] handlers = Handler.MakeHandlers(this);
            handlerMap = handlers.ToDictionary(handler => handler.Name);
        }

        public async Task<bool> ConnectAsync(string domain, int port)
        {
            try
            {
                ServicePointManager.DnsRefreshTimeout = 0;
                IPAddress[] addresses = await Dns.GetHostAddressesAsync(domain);
                TcpClient tcpClient = new() { NoDelay = true };
                await tcpClient.ConnectAsync(addresses, port);
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

        public T Get<T>() where T : class
        {
            string Name = typeof(T).GetCustomAttribute<HandlerAttribute>()!.Name;
            return (handlerMap[Name] as T)!;
        }

        void Received(Header header, MemoryStream stream)
        {
            if (header.Type != MessageType.Event)
                return;

            handlerMap[header.Handler].Receive(header.Command, stream);
        }

        public async Task<MemoryStream> Send(string handler, string command, MemoryStream contentStream)
        {
            serial++;
            Header header = new() { Id = serial, Type = MessageType.Request, Handler = handler, Command = command };
            await link.Send(header, contentStream);
            Waiter waiter = new(header.Id);
            return await waiter.Wait(link);
        }

        public void Close()
        {
            link.Close();
        }
    }
}

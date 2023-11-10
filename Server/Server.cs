using System;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;

namespace Lynx.Server
{
    public class Server
    {
        readonly X509Certificate x509Certificate;

        public event Action<Client>? ClientCome;

        public Server(X509Certificate x509Certificate)
        {
            this.x509Certificate = x509Certificate;
        }

        public void Start(int port)
        {
            Listener listener = new(port);
            listener.Accepted += Accepted;
            listener.Start();
        }

        async void Accepted(TcpClient tcpClient)
        {
            SslStream stream = new(tcpClient.GetStream());
            await stream.AuthenticateAsServerAsync(x509Certificate, false, true);
            Link link = new(stream);
            Client client = new(link);
            ClientCome?.Invoke(client);
        }
    }
}

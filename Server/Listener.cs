using System.Net.Sockets;
using System.Net;
using System;

namespace Lynx.Server
{
    public class Listener
    {
        readonly TcpListener tcpListener;

        public event Action<TcpClient>? Accepted;

        public Listener(int port)
        {
            tcpListener = new(IPAddress.Any, port);
            tcpListener.Server.NoDelay = true;
        }

        public void Start()
        {
            tcpListener.Start();
            Accept();
        }

        async void Accept()
        {
            try
            {
                TcpClient tcpClient = await tcpListener.AcceptTcpClientAsync();
                Accepted?.Invoke(tcpClient);
                Accept();
            }
            catch
            {
            }
        }

        public void Stop()
        {
            tcpListener.Stop();
        }
    }
}

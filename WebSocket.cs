using System.IO;
using System.Net.WebSockets;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Text;

namespace Lynx
{
    public class WebSocket
    {
        ClientWebSocket clientWebSocket = new();
        WebSocketReceiveResult? webSocketReceiveResult;
        MemoryStream memoryStream = new();

        public event Action Disconnected = delegate { };
        public event Action<byte[]> Received = delegate { };

        public async Task<bool> Connect(Uri uri)
        {
            clientWebSocket = new ClientWebSocket();

            try
            {
                await clientWebSocket.ConnectAsync(uri, CancellationToken.None);
            }
            catch
            {
                return false;
            }

            memoryStream = new();
            Receive();
            return true;
        }

        public async Task Send(string value)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(value);

            try
            {
                await clientWebSocket.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);
            }
            catch
            {
                Disconnected();
            }
        }

        public async Task Send(byte[] bytes)
        {
            try
            {
                await clientWebSocket.SendAsync(bytes, WebSocketMessageType.Binary, true, CancellationToken.None);
            }
            catch
            {
                Disconnected();
            }
        }

        async void Receive()
        {
            byte[] bytes = new byte[1048576];

            try
            {
                webSocketReceiveResult = await clientWebSocket.ReceiveAsync(bytes, CancellationToken.None);
            }
            catch
            {
                Disconnected();
                return;
            }

            memoryStream.Write(bytes, 0, webSocketReceiveResult.Count);

            if (webSocketReceiveResult.EndOfMessage == true)
            {
                Received(memoryStream.ToArray());
                memoryStream = new();
            }

            Receive();
        }

        public void Disconnect()
        {
            clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, new CancellationToken());
        }
    }
}
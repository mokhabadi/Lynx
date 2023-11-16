using System.Net.WebSockets;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Text;
using System.IO;

namespace Lynx
{
    public class WebSocket
    {
        ClientWebSocket clientWebSocket = null!;
        WebSocketReceiveResult result = null!;
        readonly byte[] bytes = new byte[65536];
        readonly MemoryStream stream = new();

        public event Action? Disconnected;
        public event Action<byte[]>? Received;

        public async Task<bool> Connect(Uri uri)
        {
            clientWebSocket = new();

            try
            {
                await clientWebSocket.ConnectAsync(uri, CancellationToken.None);
            }
            catch
            {
                return false;
            }

            Receive();
            return true;
        }

        public Task Send(string value)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(value);
            return Send(bytes, WebSocketMessageType.Text);
        }

        public Task Send(byte[] bytes)
        {
            return Send(bytes, WebSocketMessageType.Binary);
        }

        async Task Send(byte[] bytes, WebSocketMessageType type)
        {
            try
            {
                await clientWebSocket.SendAsync(bytes, type, true, CancellationToken.None);
            }
            catch
            {
                Disconnected?.Invoke();
            }
        }

        async void Receive()
        {
            try
            {
                result = await clientWebSocket.ReceiveAsync(bytes, CancellationToken.None);
            }
            catch
            {
                Disconnected?.Invoke();
                return;
            }

            if (result.MessageType == WebSocketMessageType.Close)
            {
                Disconnected?.Invoke();
                return;
            }

            if (result.EndOfMessage && stream.Position == 0)
            {
                Received?.Invoke(bytes[..result.Count]);
                Receive();
                return;
            }

            if (!result.EndOfMessage)
            {
                stream.Write(bytes);
                Receive();
                return;
            }

            stream.Write(bytes, 0, result.Count);
            Received?.Invoke(stream.ToArray());
            stream.Position = 0;
            Receive();
        }

        public void Disconnect()
        {
            clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, new CancellationToken());
            Disconnected?.Invoke();
        }
    }
}

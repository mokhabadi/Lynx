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
        readonly CancellationToken none = CancellationToken.None;
        readonly ClientWebSocket clientWebSocket = new();
        readonly MemoryStream stream = new(1024);
        WebSocketReceiveResult result = null!;
        ArraySegment<byte> segment;

        public event Action? Disconnected;
        public event Func<MemoryStream, Task>? Received;

        public async Task<bool> Connect(Uri uri)
        {
            try
            {
                await clientWebSocket.ConnectAsync(uri, none);
            }
            catch
            {
                return false;
            }

            segment = new(stream.GetBuffer());
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
                await clientWebSocket.SendAsync(bytes, type, true, none);
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
                stream.SetLength(stream.Capacity);
                result = await clientWebSocket.ReceiveAsync(segment, none);
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

            if (!result.EndOfMessage)
            {
                int size = stream.GetBuffer().Length;
                stream.SetLength(size * 2);
                segment = new(stream.GetBuffer(), size, size);
                Receive();
                return;
            }

            stream.SetLength(segment.Offset + result.Count);
            await Received?.Invoke(stream);
            segment = new(stream.GetBuffer());
            Receive();
            return;
        }

        public void Disconnect()
        {
            clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, none);
            Disconnected?.Invoke();
        }
    }
}

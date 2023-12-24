using System.Net.WebSockets;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Segment = System.ArraySegment<byte>;

namespace Lynx
{
    public class WebSocket
    {
        readonly CancellationToken none = CancellationToken.None;
        readonly ClientWebSocket clientWebSocket = new();
        readonly MemoryStream stream = new(65536);
        WebSocketReceiveResult result = null!;
        Segment segment;

        public event Action? Disconnected;
        public event Func<MemoryStream, Task>? Received;

        public async Task<bool> Connect(Uri uri)
        {
            try
            {
                CancellationTokenSource tokenSource = new(TimeSpan.FromSeconds(5));
                await clientWebSocket.ConnectAsync(uri, tokenSource.Token);
            }
            catch
            {
                return false;
            }

            segment = new(stream.GetBuffer());
            Receive();
            return true;
        }

        public async Task Send(Segment bytes)
        {
            try
            {
                await clientWebSocket.SendAsync(bytes, WebSocketMessageType.Text, true, none);
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

            if (Received != null)
            {
                IEnumerable<Task> tasks = Received.GetInvocationList().Select(method => ((Func<MemoryStream, Task>)method)(stream));
                await Task.WhenAll(tasks);
            }

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

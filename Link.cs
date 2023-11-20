using System;
using System.IO;
using System.Net.Security;
using System.Threading.Tasks;

namespace Lynx
{
    public class Link
    {
        readonly SslStream stream;
        readonly MemoryStream headerStream = new(256);
        readonly MemoryStream sendStream = new(65536);
        readonly MemoryStream receiveStream = new(65536);

        public event Action<Header, MemoryStream>? Received;
        public event Action<bool>? Closed;

        public Link(SslStream stream)
        {
            this.stream = stream;
            Receive();
        }

        async void Receive()
        {
            try
            {
                await Receive(1);
                byte headerSize = receiveStream.GetBuffer()[0];

                if (headerSize == 0)
                {
                    Closed?.Invoke(true);
                    return;
                }

                await Receive(headerSize);
                Header header = await Packer.Unpack<Header>(receiveStream);
                await Receive(header.ContentSize);
                Received?.Invoke(header, receiveStream);
            }
            catch
            {
                Closed?.Invoke(false);
                return;
            }

            Receive();
        }

        async Task Receive(long size)
        {
            receiveStream.Position = 0;
            receiveStream.SetLength(size);
            await stream.ReadAsync(receiveStream.GetBuffer().AsMemory(0, (int)size));
        }

        public async Task Send(Header header, MemoryStream contentStream)
        {
            header.ContentSize = contentStream.Position;
            await Packer.Pack(header, headerStream);
            sendStream.Position = 0;
            sendStream.WriteByte((byte)headerStream.Position);
            sendStream.Write(headerStream.GetBuffer(), 0, (int)headerStream.Position);
            sendStream.Write(contentStream.GetBuffer(), 0, (int)contentStream.Position);
            await stream.WriteAsync(sendStream.GetBuffer().AsMemory(0, (int)sendStream.Position));
        }

        public void Close()
        {
            stream.Close();
        }
    }
}

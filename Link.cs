using System;
using System.IO;
using System.Threading.Tasks;

namespace Lynx
{
    public class Link
    {
        readonly Stream stream;
        readonly MemoryStream headerStream = new();
        readonly MemoryStream sendStream = new();
        readonly MemoryStream receiveStream = new();

        public event Action<Header, MemoryStream>? Received;
        public event Action<bool>? Closed;

        public Link(Stream stream)
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
            receiveStream.SetLength(size);
            await stream.ReadAsync(receiveStream.GetBuffer().AsMemory(0, (int)size));
        }

        public async Task Send(Header header, MemoryStream contentStream)
        {
            header.ContentSize = contentStream.Length;
            await Packer.Pack(header, headerStream);
            sendStream.WriteByte((byte)headerStream.Length);
            sendStream.Write(headerStream.GetBuffer(), 0, (int)headerStream.Length);
            sendStream.Write(contentStream.GetBuffer(), 0, (int)contentStream.Length);
            await stream.WriteAsync(sendStream.GetBuffer().AsMemory(0, (int)sendStream.Length));
        }

        public void Close()
        {
            stream.Close();
        }
    }
}

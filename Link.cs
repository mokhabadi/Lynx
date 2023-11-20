using System;
using System.IO;
using System.Net.Security;
using System.Threading.Tasks;

namespace Lynx
{
    public class Link
    {
        readonly SslStream stream;
        readonly MemoryStream sendHeader = new(256);
        readonly MemoryStream sendContent = new(65536);
        readonly MemoryStream receiveHeader = new(256);
        readonly MemoryStream receiveContent = new(65536);

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
                await Receive(receiveHeader, 1);
                byte headerSize = receiveHeader.GetBuffer()[0];

                if (headerSize == 0)
                {
                    Closed?.Invoke(true);
                    return;
                }

                await Receive(receiveHeader, headerSize);
                Header header = await Packer.Unpack<Header>(receiveHeader);
                await Receive(receiveContent, header.ContentSize);
                Received?.Invoke(header, receiveContent);
            }
            catch
            {
                Closed?.Invoke(false);
                return;
            }

            Receive();
        }

        async Task Receive(MemoryStream stream, long size)
        {
            stream.Position = 0;
            stream.SetLength(size);
            await stream.ReadAsync(stream.GetBuffer().AsMemory(0, (int)size));
        }

        public async Task Send(Header header, MemoryStream contentStream)
        {
            header.ContentSize = contentStream.Length;
            await Packer.Pack(header, sendHeader);
            sendContent.SetLength(0);
            sendContent.WriteByte((byte)sendHeader.Length);
            sendContent.Write(sendHeader.GetBuffer(), 0, (int)sendHeader.Length);
            sendContent.Write(contentStream.GetBuffer(), 0, (int)contentStream.Length);
            await stream.WriteAsync(sendContent.GetBuffer().AsMemory(0, (int)sendContent.Length));
        }

        public void Close()
        {
            stream.Close();
        }
    }
}

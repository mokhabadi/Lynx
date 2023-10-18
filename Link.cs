using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Lynx
{
    public class Link
    {
        readonly Stream stream;

        public event Action<Header, byte[]>? Received;
        public event Action<bool>? Ended;

        public Link() { }

        public Link(Stream stream)
        {
            this.stream = stream;
            Receive();
        }

        async void Receive()
        {
            try
            {
                byte[] bytes = await Receive(1);

                if (bytes[0] == 0)
                {
                    Ended?.Invoke(true);
                    return;
                }

                bytes = await Receive(bytes[0]);
                Header header = await Packer.Unpack<Header>(bytes);
                bytes = await Receive(header.ContentSize);
                Received?.Invoke(header, bytes);
            }
            catch
            {
                Ended?.Invoke(false);
                return;
            }

            Receive();
        }

        async Task<byte[]> Receive(int size)
        {
            byte[] bytes = new byte[size];
            await stream.ReadAsync(bytes);
            return bytes;
        }

        public async Task Send(Header header, byte[] contentBytes)
        {
            header.ContentSize = contentBytes.Length;
            byte[] headerBytes = await Packer.Pack(header);
            byte[] bytes = new byte[] { (byte)headerBytes.Length }.Concat(headerBytes).Concat(contentBytes).ToArray();
            await stream.WriteAsync(bytes);
        }
    }
}

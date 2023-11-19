using System.IO;
using System.Threading.Tasks;

namespace Lynx.Client
{
    public class Waiter
    {
        readonly long requestId;
        readonly TaskCompletionSource<Stream> taskCompletionSource = new();

        public Waiter(long requestId)
        {
            this.requestId = requestId;
        }

        public async Task<Stream> Wait(Link link)
        {
            link.Received += Received;
            Stream stream = await taskCompletionSource.Task;
            link.Received -= Received;
            return stream;
        }

        void Received(Header header, Stream stream)
        {
            if (header.Id != requestId)
                return;

            taskCompletionSource.SetResult(stream);
        }
    }
}

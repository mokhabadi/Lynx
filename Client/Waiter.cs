using System.IO;
using System.Threading.Tasks;

namespace Lynx.Client
{
    public class Waiter
    {
        readonly long requestId;
        readonly TaskCompletionSource<MemoryStream> taskCompletionSource = new();

        public Waiter(long requestId)
        {
            this.requestId = requestId;
        }

        public async Task<MemoryStream> Wait(Link link)
        {
            link.Received += Received;
            MemoryStream stream = await taskCompletionSource.Task;
            link.Received -= Received;
            return stream;
        }

        void Received(Header header, MemoryStream stream)
        {
            if (header.Id != requestId)
                return;

            taskCompletionSource.SetResult(stream);
        }
    }
}

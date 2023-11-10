using System.Threading.Tasks;

namespace Lynx.Client
{
    public class Waiter
    {
        readonly long requestId;
        readonly TaskCompletionSource<byte[]> taskCompletionSource = new();

        public Waiter(long requestId)
        {
            this.requestId = requestId;
        }

        public async Task<byte[]> Wait(Link link)
        {
            link.Received += Received;
            byte[] bytes = await taskCompletionSource.Task;
            link.Received -= Received;
            return bytes;
        }

        void Received(Header header, byte[] bytes)
        {
            if (header.Id != requestId)
                return;

            taskCompletionSource.SetResult(bytes);
        }
    }
}

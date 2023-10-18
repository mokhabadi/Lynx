using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Lynx
{
    public static class Packer
    {
        static readonly JsonSerializerOptions options = new() { IncludeFields = true };

        public static async Task<byte[]> Pack(object obj)
        {
            MemoryStream stream = new();
            await JsonSerializer.SerializeAsync(stream, obj, options);
            return stream.ToArray();
        }

        public static async Task<T> Unpack<T>(byte[] bytes)
        {
            T? obj = await JsonSerializer.DeserializeAsync<T>(new MemoryStream(bytes), options);
            return obj!;
        }
    }
}

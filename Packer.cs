using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Lynx
{
    public static class Packer
    {
        static readonly JsonSerializerOptions options = new() { IncludeFields = true };

        public static async Task<byte[]> Pack<T>(T @object)
        {
            MemoryStream stream = new();
            await JsonSerializer.SerializeAsync(stream, @object, options);
            return stream.ToArray();
        }

        public static async Task<T> Unpack<T>(byte[] bytes)
        {
            T? @object = await JsonSerializer.DeserializeAsync<T>(new MemoryStream(bytes), options);
            return @object!;
        }
    }
}

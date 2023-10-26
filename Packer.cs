using System.IO;
using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Lynx
{
    public static class Packer
    {
        static readonly JsonSerializerOptions options = new()
        {
            AllowTrailingCommas = true,
            Converters = { new JsonStringEnumConverter() },
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
            IncludeFields = true,
        };

        public static async Task<byte[]> Pack<T>(T @object)
        {
            MemoryStream memoryStream = new();
            using DeflateStream deflateStream = new(memoryStream, CompressionLevel.SmallestSize);
            await JsonSerializer.SerializeAsync(deflateStream, @object, options);
            deflateStream.Close();
            return memoryStream.ToArray();
        }

        public static async Task<T> Unpack<T>(byte[] bytes)
        {
            MemoryStream memoryStream = new(bytes);
            using DeflateStream deflateStream = new(memoryStream, CompressionMode.Decompress);
            T? @object = await JsonSerializer.DeserializeAsync<T>(deflateStream, options);
            return @object!;
        }
    }
}

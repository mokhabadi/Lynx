using System.IO;
using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Lynx
{
    public static class Packer
    {
        readonly static JsonSerializerOptions options = new()
        {
            AllowTrailingCommas = true,
            Converters = { new JsonStringEnumConverter() },
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
            IncludeFields = true,
        };

        public static async Task Pack<T>(T @object, Stream stream)
        {
            stream.Position = 0;
            using DeflateStream deflateStream = new(stream, CompressionLevel.Optimal, true);
            await JsonSerializer.SerializeAsync(deflateStream, @object, options);
        }

        public static async Task<T> Unpack<T>(Stream stream)
        {
            stream.Position = 0;
            using DeflateStream deflateStream = new(stream, CompressionMode.Decompress, true);
            T? @object = await JsonSerializer.DeserializeAsync<T>(deflateStream, options);
            return @object!;
        }

        public static string ToJson<T>(T @object)
        {
            return JsonSerializer.Serialize(@object, options);
        }

        public static T FromJson<T>(string json)
        {
            return JsonSerializer.Deserialize<T>(json, options)!;
        }
    }
}

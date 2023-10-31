using System.Diagnostics;
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
            using DeflateStream deflateStream = new(memoryStream, CompressionLevel.SmallestSize, true);
            await JsonSerializer.SerializeAsync(deflateStream, @object, options);
            deflateStream.Close();
            string json = JsonSerializer.Serialize(@object, options);//////////////////////
            Debug.WriteLine($"Pack: {json.Length}->{memoryStream.Length}\n{json}");////////
            return memoryStream.ToArray();
        }

        public static async Task<T> Unpack<T>(byte[] bytes)
        {
            MemoryStream memoryStream = new(bytes);
            using DeflateStream deflateStream = new(memoryStream, CompressionMode.Decompress, true);
            T? @object = await JsonSerializer.DeserializeAsync<T>(deflateStream, options);
            string json = JsonSerializer.Serialize(@object, options);//////////////////////
            Debug.WriteLine($"Unpack: {json.Length}->{memoryStream.Length}\n{json}");//////
            return @object!;
        }

        public static string ToJson(object @object)
        {
            return JsonSerializer.Serialize(@object, options);
        }

        public static T FromJson<T>(string json)
        {
            return JsonSerializer.Deserialize<T>(json, options)!;
        }
    }
}

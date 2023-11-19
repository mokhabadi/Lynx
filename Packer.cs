using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Serialization;

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

        public static byte[] Pack<T>(T @object)
        {
            MemoryStream memoryStream = new();
            using DeflateStream deflateStream = new(memoryStream, CompressionLevel.Optimal, true);
            JsonSerializer.Serialize(deflateStream, @object, options);
            deflateStream.Close();
            string json = ToJson(@object!);//////////////////////
            Debug.WriteLine($"Pack: {json.Length}->{memoryStream.Length}\n{json}");////////
            return memoryStream.ToArray();
        }

        public static T Unpack<T>(byte[] bytes)
        {
            MemoryStream memoryStream = new(bytes);
            using DeflateStream deflateStream = new(memoryStream, CompressionMode.Decompress, true);
            T? @object = JsonSerializer.Deserialize<T>(deflateStream, options);
            string json = ToJson(@object!);//////////////////////
            Debug.WriteLine($"Unpack: {json.Length}->{memoryStream.Length}\n{json}");//////
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

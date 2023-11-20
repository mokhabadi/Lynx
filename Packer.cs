using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text;
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

        public static async Task Pack<T>(T @object, MemoryStream stream)
        {
            stream.SetLength(0);
            //using DeflateStream deflateStream = new(stream, CompressionLevel.Optimal, true);
            await JsonSerializer.SerializeAsync(stream, @object, options);
            Log(nameof(Pack), stream);
        }

        static void Log(string method, MemoryStream stream)
        {
            Debug.WriteLine(method + ": " + Encoding.UTF8.GetString(stream.ToArray()));
        }

        public static async Task<T> Unpack<T>(MemoryStream stream)
        {
            Log(nameof(Unpack), stream);
            //using DeflateStream deflateStream = new(stream, CompressionMode.Decompress, true);
            T? @object = await JsonSerializer.DeserializeAsync<T>(stream, options);
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

using System.Text.Json.Serialization;

namespace Lynx
{
    public class Header
    {
        [JsonPropertyName("I")] public long Id { get; set; }
        [JsonPropertyName("T")] public MessageType Type { get; set; }
        [JsonPropertyName("H")] public string Handler { get; set; }
        [JsonPropertyName("C")] public string Command { get; set; }
        [JsonPropertyName("S")] public long ContentSize { get; set; }
    }
}

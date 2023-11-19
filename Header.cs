using System.Text.Json.Serialization;

namespace Lynx
{
    public class Header
    {
        [JsonPropertyName("I")] public long Id { get; }
        [JsonPropertyName("T")] public MessageType Type { get; }
        [JsonPropertyName("H")] public string Handler { get; }
        [JsonPropertyName("C")] public string Command { get; }
        [JsonPropertyName("S")] public long ContentSize { get; set; }

        public Header(long id, MessageType type, string handler, string command)
        {
            Id = id;
            Type = type;
            Handler = handler;
            Command = command;
        }
    }
}

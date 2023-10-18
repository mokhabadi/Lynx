namespace Lynx
{
    public class Header
    {
        public long Id { get; }
        public MessageType Type { get; }
        public string Handler { get; }
        public string Command { get; }
        public int ContentSize { get; set; }

        public Header(long id, MessageType type, string handler, string command)
        {
            Id = id;
            Type = type;
            Handler = handler;
            Command = command;
        }
    }
}

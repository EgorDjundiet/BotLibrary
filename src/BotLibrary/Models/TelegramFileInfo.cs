namespace BookCollectorWebAPI.Models
{
    public class TelegramFileInfo
    {
        public byte[] Bytes { get; private set; }
        public string Name { get; private set; }
        public TelegramFileInfo(byte[] bytes, string name)
        {
            Bytes = bytes;
            Name = name;
        }
    }
}

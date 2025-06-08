
namespace WebServer.Objects
{
    internal class ExtensionInfo
    {
        public string ContentType { get; set; }
        public Func<string, string, ExtensionInfo, ResponsePacket> Loader { get; set; }
    }
}

using System.Text;
using WebServer.Enums;

namespace WebServer.Objects
{
    internal class ResponsePacket
    {
        public string Redirect { get; set; }
        public byte[] Data { get; set; }
        public string ContentType { get; set; }
        public Encoding Encoding { get; set; }
        public ServerError Error { get; set; }
    }
}

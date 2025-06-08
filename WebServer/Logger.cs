using System.Net;

namespace WebServer
{
    internal class Logger
    {
        public Logger()
        {
            
        }

        public void Log(HttpListenerRequest request)
        {
            Console.WriteLine($"{request.RemoteEndPoint} {request.HttpMethod} /{request.Url.AbsoluteUri}"); 
        }

        public void Log(string message)
        {
            Console.WriteLine(message);
        }
    }
}

using System.Net;
using WebServer.Objects;

namespace WebServer
{
    public static class Server
    {
        private static Router _router = new Router();
        private static readonly Logger _logger = new Logger();
        private static HttpListener listener;
        public static int maxSimultaneousConnections = 20;
        private static Semaphore semaphore = new Semaphore(maxSimultaneousConnections, maxSimultaneousConnections);

        public static void Start(string siteRootPath)
        {
            _router.WebsitePath = siteRootPath;
            var localHostIPs = GetLocalHostIps();
            var listener = InitializeListener(localHostIPs);
            Start(listener);
        }

        private static List<IPAddress> GetLocalHostIps()
        {
            IPHostEntry host;
            host = Dns.GetHostEntry(Dns.GetHostName());
            var addresses = host.AddressList.Where(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).ToList();

            return addresses;
        }

        private static HttpListener InitializeListener(List<IPAddress> localHostIPs)
        {
            var listener = new HttpListener();
            listener.Prefixes.Add("http://localhost/");

            localHostIPs.ForEach(ip =>
                {
                    Console.WriteLine($"Listening on IP: http://{ip.ToString()}/");
                    listener.Prefixes.Add($"http://{ip.ToString()}/");
                });

            return listener;
        }

        private static void Start(HttpListener listener)
        {
            try
            {
                listener.Start();
            }
            catch(Exception ex)
            {
                _logger.Log(ex.ToString());
            }
            Task.Run(() => RunServer(listener));
        }

        private static async void RunServer(HttpListener listener)
        {
            while (true)
            {
                semaphore.WaitOne();
                StartConnectionListener(listener);
            }
        }

        private static async void StartConnectionListener(HttpListener listener)
        {
            HttpListenerContext context = await listener.GetContextAsync();

            semaphore.Release();

            _logger.Log(context.Request);

            var responsePacket = _router.Route(context.Request);

            Respond(context.Response, responsePacket);
        }

        private static void Respond(HttpListenerResponse response, ResponsePacket responsePacket)
        {
            response.ContentType = responsePacket.ContentType;
            response.ContentLength64 = responsePacket.Data.Length;
            response.OutputStream.Write(responsePacket.Data, 0, responsePacket.Data.Length);
            response.ContentEncoding = responsePacket.Encoding;
            response.StatusCode = (int)HttpStatusCode.OK;
            response.OutputStream.Close();
        }
    }
}

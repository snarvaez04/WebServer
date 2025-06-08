using System.Reflection;
using WebServer;

namespace WebServerConsole
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Server.Start(GetWebsitePath());
            Console.ReadLine();
        }

        private static string GetWebsitePath()
        {
            var executionPath = Assembly.GetExecutingAssembly().Location;

            var sliceIndex = 0;
            var pathDelimSpan = "\\".AsSpan();
            var pathSpan = executionPath.AsSpan();

            for (int i = 0; i < 4; i++)
            {
                sliceIndex = pathSpan.LastIndexOf(pathDelimSpan, StringComparison.InvariantCulture);
                pathSpan = pathSpan.Slice(0, sliceIndex);
            }

            var websitePath = pathSpan.ToString();

            return @$"{websitePath}\www";
        }
    }
}

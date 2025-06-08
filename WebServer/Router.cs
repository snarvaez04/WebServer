using System.Net;
using System.Text;
using WebServer.Extensions;
using WebServer.Objects;

namespace WebServer
{
    internal class Router
    {
        public string WebsitePath { get; set; }

        public Dictionary<string, ExtensionInfo> extFolderMap;

        public Router()
        {
            extFolderMap = new Dictionary<string, ExtensionInfo>()
            {
                { "ico", new ExtensionInfo(){ Loader = ImageLoader, ContentType = "image/ico" } },
                { "png", new ExtensionInfo(){ Loader = ImageLoader, ContentType = "image/png" } },
                { "jpg", new ExtensionInfo(){ Loader = ImageLoader, ContentType = "image/jpg" } },
                { "gif", new ExtensionInfo(){ Loader = ImageLoader, ContentType = "image/gif" } },
                { "bmp", new ExtensionInfo(){ Loader = ImageLoader, ContentType = "image/bmp" } },
                { "html", new ExtensionInfo(){ Loader = PageLoader, ContentType = "text/html" } },
                { "css", new ExtensionInfo(){ Loader = FileLoader, ContentType = "text/css" } },
                { "js", new ExtensionInfo(){ Loader = FileLoader, ContentType = "text/javascript" } },
                { "", new ExtensionInfo(){ Loader = PageLoader, ContentType = "text/html" } },
            };
        }

        public ResponsePacket Route(HttpListenerRequest request)
        {
            var verb = request.HttpMethod;
            var path = GetUrlPath(request.RawUrl, out string queryString);
            var parameters = GetKeyValues(queryString);
            return RouteToDestinatin(verb, path, parameters);
        }

        private string GetUrlPath(string? rawUrl, out string queryString)
        {
            queryString = string.Empty;

            if (rawUrl == null)
                return string.Empty;

            var splitIndex = rawUrl.IndexOf('?');

            if (splitIndex == -1)
                return rawUrl;

            queryString = rawUrl.Substring(splitIndex + 1);
            return rawUrl.Substring(0, splitIndex);
        }

        private Dictionary<string, string> GetKeyValues(string queryString)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            var queryStrSplit = queryString.Split('&');

            if(queryStrSplit.Any())
            {
                foreach(var parm in queryStrSplit)
                {
                    var parmDelimIndex = parm.IndexOf('=');

                    if (parmDelimIndex == -1)
                        continue;

                    if (parmDelimIndex > 0)
                    {
                        var key = parm.Substring(0, parmDelimIndex);
                        var value = parm.Substring(parmDelimIndex + 1);

                        parameters.Add(key, value);
                    }
                }
            }

            return parameters;
        }

        private ResponsePacket RouteToDestinatin(string httpVerb, string urlPath, Dictionary<string, string>? queryStrings)
        {
            var ext = urlPath.RightOf(".");

            if (extFolderMap.TryGetValue(ext, out ExtensionInfo? extInfo))
            {
                var fullPath = WebsitePath;
                urlPath = urlPath.Replace("/", "\\");

                if (urlPath != "\\")
                {
                    if (urlPath.StartsWith("\\"))
                        urlPath = urlPath.Remove(0, 1);

                    fullPath = Path.Combine(WebsitePath, urlPath);
                }
                
                return extInfo.Loader(fullPath, ext, extInfo);
            }
            else
            {
                return new ResponsePacket() { Error = Enums.ServerError.UnknownType };
            }
        }

        private ResponsePacket ImageLoader(string filePath, string ext, ExtensionInfo extInfo)
        {
            var fstream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            var breader = new BinaryReader(fstream);
            var response = new ResponsePacket() { Data = breader.ReadBytes((int)fstream.Length), ContentType = extInfo.ContentType };

            breader.Close();
            fstream.Close();

            return response;
        }

        private ResponsePacket FileLoader(string filePath, string ext, ExtensionInfo extInfo)
        {
            var text = File.ReadAllText(filePath);
            var response = new ResponsePacket() { Data = Encoding.UTF8.GetBytes(text), ContentType = extInfo.ContentType, Encoding = Encoding.UTF8 };
            
            return response;
        }

        private ResponsePacket PageLoader(string filePath, string ext, ExtensionInfo extInfo)
        {
            if (filePath == WebsitePath)
                return RouteToDestinatin("Get", "/index.html", null);
            else
            {
                if (string.IsNullOrEmpty(ext))
                    filePath = $"{filePath}.html";

                string fullPath = string.Empty;

                if (filePath.Contains("ErrorPages"))
                    fullPath = $@"{WebsitePath}{filePath.RightOf(WebsitePath)}";
                else
                    fullPath = $@"{WebsitePath}\Pages{filePath.RightOf(WebsitePath)}";

                return FileLoader(fullPath, ext, extInfo);
            }
        }
    }
}

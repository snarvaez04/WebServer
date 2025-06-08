
namespace WebServer.Extensions
{
    internal static class FilePathExtensions
    {
        public static string RightOf(this string source, string comparer)
        {
            var result = string.Empty;

            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(comparer))
                return result;

            if(!source.Contains(comparer))
                return result;

            var comparerIndex = source.LastIndexOf(comparer);
            result = source.Substring(comparerIndex + comparer.Length);

            return result;
        }
    }
}

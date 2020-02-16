using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace endpointTests
{
    public static class TokenStore
    {
        private static string dir = "tokens";

        public static Task Write(string user, Access a)
        {
            return File.WriteAllTextAsync($"{dir}/{user}", JsonSerializer.Serialize(a));
        }

        public static async Task<Access> Read(string usr)
        {
            if (File.Exists($"{dir}/{usr}"))
            {
                var json = await File.ReadAllTextAsync($"{dir}/{usr}");
                return JsonSerializer.Deserialize<Access>(json);
            }
            else
            {
                return null;
            }
        }
    }
}
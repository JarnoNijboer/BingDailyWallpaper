using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Xml;

namespace BingDailyWallpaper
{
    public static class Program
    {
        private static async Task Main()
        {
            const string url = "https://www.bing.com/HPImageArchive.aspx?format=rss&idx=0&n=1";

            var xmlDoc = new XmlDocument();
            xmlDoc.Load(url);

            var rawUrl = xmlDoc.SelectSingleNode("rss/channel/item/link")?.InnerText;

            if (rawUrl == null)
                return;

            var imageUrl = $"https://bing.com{rawUrl.Replace("1366x768", "1920x1080")}";

            using var httpClient = new HttpClient();
            using var sha256 = SHA256Managed.Create();

            var bytes = await httpClient.GetByteArrayAsync(imageUrl).ConfigureAwait(false);

            var hash = BitConverter.ToString(sha256.ComputeHash(bytes)).Replace("-", string.Empty);
            
            var dirPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Wallpapers");
            var directory = new DirectoryInfo(dirPath);

            if (!directory.Exists)
            {
                directory.Create();
            }
            
            var imagePath = Path.Combine(directory.FullName, $"{hash}.jpg");

            if (!File.Exists(imagePath))
            {
                File.WriteAllBytes(imagePath, bytes);
            }

            // Cleanup old files
            directory.GetFiles("*.jpg").OrderByDescending(x => x.CreationTimeUtc).Skip(10).ToList().ForEach(x => x.Delete());
        }
    }
}

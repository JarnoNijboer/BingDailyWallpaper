using System;
using System.IO;
using System.Net.Http;
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

            var imagePath = $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}/Wallpapers/BingWallpaper.jpg";

            using (var httpClient = new HttpClient())
            {
                var bytes = await httpClient.GetByteArrayAsync(imageUrl).ConfigureAwait(false);

                File.WriteAllBytes(imagePath, bytes);
            }
        }
    }
}

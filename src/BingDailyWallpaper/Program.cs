using System;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Win32;

namespace BingDailyWallpaper
{
    public static class Program
    {
        const string BingRssFeed = "https://www.bing.com/HPImageArchive.aspx?format=rss&idx=0&n=1";

        static async Task Main()
        {
            Console.WriteLine("Get info from RSS feed");

            var imageUrl = GetImageUrl();

            if (imageUrl == null)
                return;

            Console.WriteLine($"Downloading image from: ${imageUrl}");

            var imagePath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}\Wallpapers\BingWallpaper.jpg";
            //imageUrl = new Uri("https://images.unsplash.com/photo-1460602692976-8eab38c11f9d");

            var downloadResult = await DownloadImage(imageUrl, imagePath).ConfigureAwait(false);

            if (!downloadResult)
                return;

            await SetWallpaperAsync(imagePath).ConfigureAwait(false);

            Console.WriteLine("Done");
            Console.ReadLine();
        }

        [DllImport("user32.dll")]
        static extern Int32 SystemParametersInfo(UInt32 action, UInt32 uParam, String vParam, UInt32 winIni);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SendMessageTimeout(IntPtr hWnd, int Msg, IntPtr wParam, string lParam, uint fuFlags, uint uTimeout, IntPtr lpdwResult);

        private static readonly IntPtr HWND_BROADCAST = new IntPtr(0xffff);
        private const int WM_SETTINGCHANGE = 0x1a;
        private const int SMTO_ABORTIFHUNG = 0x0002;

        static async Task SetWallpaperAsync(string imagePath)
        {
            using (var regKey = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true))
            {
                regKey.SetValue("WallpaperStyle", 10);
                regKey.SetValue("TileWallpaper", 0);
            }

            SystemParametersInfo(0x0014, 0, imagePath, 0x0001);
            await Task.Delay(250).ConfigureAwait(false);
            SendMessageTimeout(HWND_BROADCAST, WM_SETTINGCHANGE, IntPtr.Zero, null, SMTO_ABORTIFHUNG, 100, IntPtr.Zero);
        }

        static async Task<bool> DownloadImage(Uri imageUrl, string imagePath)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    var bytes = await httpClient.GetByteArrayAsync(imageUrl).ConfigureAwait(false);

                    File.WriteAllBytes(imagePath, bytes);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }

            return true;
        }

        static Uri GetImageUrl()
        {
            var xmlDoc = new XmlDocument();

            try
            {
                xmlDoc.Load(BingRssFeed);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while loading RSS feed: {ex.Message}");
                return null;
            }

            var rawUrl = xmlDoc.SelectSingleNode("rss/channel/item/link")?.InnerText;

            if (rawUrl == null)
                return null;

            return new Uri($"https://bing.com{rawUrl.Replace("1366x768", "1920x1080")}");
        }
    }
}

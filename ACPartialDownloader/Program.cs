using System;
using System.Threading.Tasks;

namespace ACPartialDownloader
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            string user = Environment.GetEnvironmentVariable("STEAM_USER");
            string pass = Environment.GetEnvironmentVariable("STEAM_PASS");

            await SteamKitDownloader.RunAsync(user, pass);
        }
    }
}

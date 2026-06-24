using SteamKit2;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ACPartialDownloader
{
    internal class SteamKitDownloader
    {
        private const uint APP_ID = 813780; // Replace with your app ID
        private const uint DEPOT_ID = 4551041; // Replace with your depot ID
        private const ulong MANIFEST_ID = 0; // Replace with your manifest ID

        public static async Task RunAsync(string user, string pass)
        {
            Console.WriteLine("[*] Connecting to Steam...");

            var client = new SteamClient();
            var manager = new CallbackManager(client);

            var steamUser = client.GetHandler<SteamUser>();
            var steamApps = client.GetHandler<SteamApps>();

            bool loggedIn = false;

            manager.Subscribe<SteamUser.LoggedOnCallback>(cb =>
            {
                if (cb.Result == EResult.OK)
                {
                    Console.WriteLine("[✓] Logged in!");
                    loggedIn = true;
                }
                else
                {
                    Console.WriteLine($"[X] Login failed: {cb.Result}");
                }
            });

            client.Connect();

            while (!loggedIn)
                manager.RunWaitCallbacks(TimeSpan.FromSeconds(1));

            Console.WriteLine("[*] Requesting depot key...");

            var depotKeyResponse = await steamApps.GetDepotKeyAsync(APP_ID, DEPOT_ID);
            if (depotKeyResponse.Result != EResult.OK)
            {
                Console.WriteLine("[X] Failed to get depot key.");
                return;
            }

            var depotKey = depotKeyResponse.DepotKey;

            Console.WriteLine("[*] Fetching manifest...");

            var manifest = await steamApps.GetDepotManifestAsync(DEPOT_ID, MANIFEST_ID, depotKey);
            if (manifest == null)
            {
                Console.WriteLine("[X] Failed to load manifest.");
                return;
            }

            Console.WriteLine("[*] Manifest loaded.");

            Directory.CreateDirectory("output");

            foreach (var file in manifest.Files)
            {
                if (file.FileName.Contains("UnityPlayer.dll") ||
                    file.FileName.Contains("globalgamemanagers"))
                {
                    Console.WriteLine($"[*] Downloading {file.FileName}...");

                    using var fs = File.OpenWrite(Path.Combine("output", file.FileName));
                    await steamApps.DownloadDepotFileAsync(DEPOT_ID, file, depotKey, fs);

                    Console.WriteLine($"[✓] Downloaded {file.FileName}");
                }
            }

            Console.WriteLine("[✓] Done.");
        }
    }
}

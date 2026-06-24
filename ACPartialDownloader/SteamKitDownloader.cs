using SteamKit2;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ACPartialDownloader
{
    internal class SteamKitDownloader
    {
        private const uint APP_ID = 813780; // Example, replace with your app
        private const uint DEPOT_ID = 4551041;
        private const ulong MANIFEST_ID = 7419120047354550096;

        public static async Task RunAsync(string user, string pass)
        {
            Console.WriteLine("[*] Logging into Steam...");

            var steamClient = new SteamClient();
            var callbackManager = new CallbackManager(steamClient);

            var steamUser = steamClient.GetHandler<SteamUser>();
            var steamApps = steamClient.GetHandler<SteamApps>();

            bool loggedIn = false;

            callbackManager.Subscribe<SteamUser.LoggedOnCallback>(cb =>
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

            steamClient.Connect();

            while (!loggedIn)
            {
                callbackManager.RunWaitCallbacks(TimeSpan.FromSeconds(1));
            }

            Console.WriteLine("[*] Requesting depot manifest...");

            var manifestRequest = await steamApps.GetDepotDecryptionKeyAsync(APP_ID, DEPOT_ID);
            if (manifestRequest.Result != EResult.OK)
            {
                Console.WriteLine("[X] Failed to get depot key.");
                return;
            }

            var depotKey = manifestRequest.DepotKey;

            var manifest = await steamApps.GetManifestAsync(DEPOT_ID, MANIFEST_ID, depotKey);
            if (manifest == null)
            {
                Console.WriteLine("[X] Failed to fetch manifest.");
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
                    await steamApps.DownloadFileAsync(DEPOT_ID, file, depotKey, fs);

                    Console.WriteLine($"[✓] Downloaded {file.FileName}");
                }
            }

            Console.WriteLine("[✓] Done.");
        }
    }
}

using SteamKit2;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ACPartialDownloader
{
    public static class DepotPuller
    {
        private const uint APPID = 4551040;
        private const uint DEPOTID = 4551041;
        private const ulong MANIFESTID = 7419120047354550096;

        private static readonly string[] TargetFiles =
        {
            "UnityPlayer.dll",
            "globalgamemanagers"
        };

        public static async Task RunAsync()
        {
            Console.WriteLine("[*] Logging into Steam...");

            var steamClient = new SteamClient();
            var callbackManager = new CallbackManager(steamClient);

            var auth = steamClient.GetHandler<SteamUser>();
            var depot = steamClient.GetHandler<SteamContent>();

            bool loggedIn = false;

            callbackManager.Subscribe<SteamUser.LoggedOnCallback>(cb =>
            {
                if (cb.Result == EResult.OK)
                {
                    Console.WriteLine("[+] Logged in!");
                    loggedIn = true;
                }
                else
                {
                    Console.WriteLine("[-] Login failed: " + cb.Result);
                }
            });

            steamClient.Connect();

            while (!loggedIn)
            {
                callbackManager.RunWaitCallbacks(TimeSpan.FromSeconds(1));
            }

            Console.WriteLine("[*] Fetching manifest...");
            var manifest = await depot.GetManifestAsync(APPID, DEPOTID, MANIFESTID);

            Console.WriteLine("[*] Searching for target files...");
            var files = manifest.Files
                .Where(f => TargetFiles.Contains(Path.GetFileName(f.FileName)))
                .ToList();

            if (!files.Any())
            {
                Console.WriteLine("[-] No target files found in manifest.");
                return;
            }

            Directory.CreateDirectory("output");

            foreach (var file in files)
            {
                Console.WriteLine($"[*] Downloading {file.FileName}...");

                using var fs = new FileStream(Path.Combine("output", Path.GetFileName(file.FileName)), FileMode.Create);

                foreach (var chunk in file.Chunks)
                {
                    var data = await depot.DownloadChunkAsync(APPID, DEPOTID, chunk.ChunkID);
                    await fs.WriteAsync(data);
                }

                Console.WriteLine($"[+] Saved {file.FileName}");
            }

            Console.WriteLine("[✓] Done!");
        }
    }
}

import os
import glob

def find_game_folder():
    # Prefer DepotDownloader output
    if os.path.exists("downloaded"):
        return "downloaded"

    # Fallback for old SteamKit2 output
    if os.path.exists("output"):
        return "output"

    raise RuntimeError("No game folder found. Expected 'downloaded/' or 'output/'.")

def find_required_files(root):
    unity = None
    ggm = None

    for path in glob.glob(root + "/**/*", recursive=True):
        lower = path.lower()
        if "unityplayer.dll" in lower:
            unity = path
        if "globalgamemanagers" in lower and not lower.endswith(".assets"):
            ggm = path

    if not unity:
        raise RuntimeError("UnityPlayer.dll not found in depot files.")

    if not ggm:
        raise RuntimeError("globalgamemanagers not found in depot files.")

    return unity, ggm

def main():
    print("[*] Starting symbol puller...")

    root = find_game_folder()
    print(f"[*] Using game folder: {root}")

    unity, ggm = find_required_files(root)
    print(f"[*] Found UnityPlayer.dll: {unity}")
    print(f"[*] Found globalgamemanagers: {ggm}")

    # Continue with your existing logic...
    # parse files, generate maps, etc.

if __name__ == "__main__":
    main()

import os
import re
import json
import subprocess

BASE = os.path.dirname(os.path.abspath(__file__))
DEPOT_DIR = os.path.join(BASE, "output")

UNITY_DLL = os.path.join(DEPOT_DIR, "UnityPlayer.dll")
GGM_FILE  = os.path.join(DEPOT_DIR, "globalgamemanagers")

OUT_MAP   = os.path.join(BASE, "SymbolMap.json")
OUT_FRIDA = os.path.join(BASE, "Frida-Map.js")

def extract_symbols(path):
    print(f"[*] Extracting symbols from {path}...")
    if not os.path.exists(path):
        raise FileNotFoundError(f"Missing file: {path}")

    # strings-like extraction
    result = subprocess.run(
        ["strings", path],
        stdout=subprocess.PIPE,
        stderr=subprocess.PIPE,
        text=True
    )

    lines = result.stdout.splitlines()
    symbols = [s for s in lines if "il2cpp" in s.lower()]
    return symbols

def build_symbol_map(symbols):
    print("[*] Building SymbolMap.json...")
    data = {"symbols": symbols}
    with open(OUT_MAP, "w") as f:
        json.dump(data, f, indent=2)

def build_frida_map(symbols):
    print("[*] Building Frida-Map.js...")
    with open(OUT_FRIDA, "w") as f:
        f.write("/* Auto-generated */\n")
        f.write("var SymbolMap = [\n")
        for s in symbols:
            f.write(f'  "{s}",\n')
        f.write("];\n")

def main():
    print("[*] Starting symbol puller...")

    if not os.path.exists(DEPOT_DIR):
        raise RuntimeError("output/ folder missing — SteamKit downloader did not run.")

    # extract from UnityPlayer.dll
    unity_syms = extract_symbols(UNITY_DLL)

    # extract from globalgamemanagers
    ggm_syms = extract_symbols(GGM_FILE)

    # combine
    all_syms = sorted(set(unity_syms + ggm_syms))

    # write outputs
    build_symbol_map(all_syms)
    build_frida_map(all_syms)

    print("[✓] Symbol pull complete.")

if __name__ == "__main__":
    main()

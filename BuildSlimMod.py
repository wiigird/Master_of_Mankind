#!/usr/bin/env python3
"""Build a smaller runtime PCK without editor caches and unused source assets."""

from __future__ import annotations

import os
import re
import subprocess
import sys
from pathlib import Path


ROOT = Path(__file__).resolve().parent
PROJECT = ROOT / "Master_of_Mankind.csproj"
MANIFEST = ROOT / "Master_of_Mankind.json"
MOD_NAME = "Master_of_Mankind"
DEFAULT_MOD_DIR = Path(r"D:\SteamLibrary\steamapps\common\Slay the Spire 2\mods") / MOD_NAME


def run(command: list[str]) -> None:
    print(">", " ".join(f'"{part}"' if " " in part else part for part in command))
    subprocess.run(command, cwd=ROOT, check=True)


def game_is_running() -> bool:
    result = subprocess.run(
        ["tasklist", "/FI", "IMAGENAME eq SlayTheSpire2.exe"],
        capture_output=True,
        text=True,
        check=False,
    )
    return "SlayTheSpire2.exe" in result.stdout


def godot_path() -> Path:
    props = (ROOT / "Directory.Build.props").read_text(encoding="utf-8")
    match = re.search(r"<GodotPath>(.*?)</GodotPath>", props)
    if not match:
        raise RuntimeError("GodotPath was not found in Directory.Build.props.")

    path = Path(match.group(1).strip())
    if not path.is_file():
        raise RuntimeError(f"Godot executable was not found: {path}")
    return path


def main() -> int:
    if game_is_running():
        print("ERROR: Close Slay the Spire 2 before building so the mod files are not locked.")
        return 1

    mod_dir = Path(os.environ.get("STS2_MOD_DIR", DEFAULT_MOD_DIR))
    mod_dir.mkdir(parents=True, exist_ok=True)
    temporary_pck = mod_dir / f"{MOD_NAME}.slim.tmp.pck"
    output_pck = mod_dir / f"{MOD_NAME}.pck"

    try:
        print("Building DLL and manifest...")
        run(["dotnet", "build", str(PROJECT), "-c", "Release", "-v:minimal"])

        print("Exporting slim PCK...")
        temporary_pck.unlink(missing_ok=True)
        run([str(godot_path()), "--headless", "--export-pack", "SlimExport", str(temporary_pck)])
        os.replace(temporary_pck, output_pck)

        print("BUILD SUCCEEDED")
        print(f"Output: {mod_dir}")
        print(f"PCK size: {output_pck.stat().st_size / 1024 / 1024:.1f} MB")
        return 0
    except (OSError, RuntimeError, subprocess.CalledProcessError) as error:
        temporary_pck.unlink(missing_ok=True)
        print(f"BUILD FAILED: {error}", file=sys.stderr)
        return 1


if __name__ == "__main__":
    raise SystemExit(main())

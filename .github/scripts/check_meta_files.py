#!/usr/bin/env python3
"""Verify every tracked asset has a .meta file and every .meta has a matching asset.

Unity requires a .meta file next to every asset (file or folder) it tracks.
A missing or orphaned .meta breaks GUID references and can silently corrupt
the package when re-imported. This has no Unity dependency; it just diffs
the file tree against Unity's own naming convention.
"""
import subprocess
import sys
from pathlib import Path

ASSET_ROOTS = ["Runtime", "Editor", "Tests"]
IGNORED_SUFFIXES = {".meta"}


def tracked_paths() -> list[str]:
    result = subprocess.run(
        ["git", "ls-files"],
        check=True,
        capture_output=True,
        text=True,
    )
    all_files = [line for line in result.stdout.splitlines() if line]
    return [
        f
        for f in all_files
        if any(f == root or f.startswith(f"{root}/") or f == f"{root}.meta" for root in ASSET_ROOTS)
    ]


def main() -> int:
    files = tracked_paths()
    file_set = set(files)

    meta_files = {f for f in files if f.endswith(".meta")}
    asset_files = {f for f in files if not f.endswith(".meta")}

    # Every directory under the asset roots also needs a .meta file.
    dirs = set()
    for f in asset_files:
        parent = Path(f).parent
        while str(parent) not in (".",) and str(parent) not in dirs:
            dirs.add(str(parent))
            parent = parent.parent

    missing_meta = []
    for asset in sorted(asset_files | dirs):
        expected_meta = f"{asset}.meta"
        if expected_meta not in meta_files:
            missing_meta.append(asset)

    orphaned_meta = []
    for meta in sorted(meta_files):
        asset = meta[: -len(".meta")]
        if asset not in asset_files and asset not in dirs:
            orphaned_meta.append(meta)

    if missing_meta:
        print("Assets missing a .meta file:")
        for a in missing_meta:
            print(f"  {a}")

    if orphaned_meta:
        print("Orphaned .meta files (no matching asset):")
        for m in orphaned_meta:
            print(f"  {m}")

    if missing_meta or orphaned_meta:
        return 1

    print(f"OK: {len(asset_files) + len(dirs)} assets, all with matching .meta files.")
    return 0


if __name__ == "__main__":
    sys.exit(main())

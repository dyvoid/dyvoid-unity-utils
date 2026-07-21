#!/usr/bin/env python3
"""Validate package.json and all .asmdef files are well-formed JSON,
and that package.json's version field is valid semver."""
import json
import re
import subprocess
import sys

SEMVER_RE = re.compile(r"^\d+\.\d+\.\d+(-[0-9A-Za-z-.]+)?(\+[0-9A-Za-z-.]+)?$")


def json_files() -> list[str]:
    result = subprocess.run(
        ["git", "ls-files", "*.json", "*.asmdef"],
        check=True,
        capture_output=True,
        text=True,
    )
    return [line for line in result.stdout.splitlines() if line]


def main() -> int:
    errors = []

    for path in json_files():
        with open(path, encoding="utf-8") as f:
            try:
                data = json.load(f)
            except json.JSONDecodeError as e:
                errors.append(f"{path}: invalid JSON ({e})")
                continue

        if path == "package.json":
            version = data.get("version")
            if not version or not SEMVER_RE.match(version):
                errors.append(f"{path}: missing or invalid semver 'version' field: {version!r}")
            for field in ("name", "displayName", "unity"):
                if not data.get(field):
                    errors.append(f"{path}: missing required field '{field}'")

    if errors:
        print("JSON validation failed:")
        for e in errors:
            print(f"  {e}")
        return 1

    print(f"OK: {len(json_files())} JSON/asmdef files valid.")
    return 0


if __name__ == "__main__":
    sys.exit(main())

#!/usr/bin/env bash
# Copy platform DLLs from the sparse-checkout (.platform-libs-src/<version>/<edition>)
# flat into .platform-libs/ — the stable dir the build references via CoreLibPath.
# Inputs (env): PLATFORM_VERSION, EDITION.
set -euo pipefail

: "${PLATFORM_VERSION:?PLATFORM_VERSION is required}"
: "${EDITION:?EDITION is required}"

src=".platform-libs-src/${PLATFORM_VERSION}/${EDITION}"
if [[ -z "$(find "$src" -maxdepth 1 -name '*.dll' -print -quit)" ]]; then
  echo "::error::No DLLs found in $src" >&2
  exit 1
fi

mkdir -p .platform-libs
cp "$src"/*.dll .platform-libs/
echo "Copied $(find "$src" -maxdepth 1 -name '*.dll' | wc -l) DLL(s) from $src"

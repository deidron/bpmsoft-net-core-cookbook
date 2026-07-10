#!/usr/bin/env bash
# Fetch BPMSoft platform reference assemblies into .platform-libs/ for local /
# devcontainer use — the same thing CI's restore-platform-libs action does.
# Needs PLATFORM_LIBS_REPO (owner/name of the private libs repo) and GH_PAT (a token
# with read access to it; or gh auth) — set them in .devcontainer/devcontainer.env.
# Either unset -> exits non-zero. No repo name is hardcoded here.
# Note: assumes platform.json's `ref` is a branch/tag (the cookbook uses "main").
set -euo pipefail

repo="${PLATFORM_LIBS_REPO:-}"
token="${GH_PAT:-$(gh auth token 2>/dev/null || true)}"
if [ -z "$repo" ] || [ -z "$token" ]; then
  echo "::error::PLATFORM_LIBS_REPO and GH_PAT are required — set them in .devcontainer/devcontainer.env" >&2
  exit 1
fi

read_json() {
  if command -v jq >/dev/null 2>&1; then jq -r "$1" platform.json
  else yq -p=json "$1" platform.json; fi
}
ver="$(read_json '.version')"
ed="$(read_json '.edition')"
ref="$(read_json '.ref // "main"')"

echo "Fetching $repo @ $ref -> $ver/$ed"
rm -rf .platform-libs-src
git clone --depth 1 --filter=blob:none --sparse --branch "$ref" \
  "https://x-access-token:${token}@github.com/${repo}" .platform-libs-src
git -C .platform-libs-src sparse-checkout set "$ver/$ed"

src=".platform-libs-src/$ver/$ed"
if [ -z "$(find "$src" -maxdepth 1 -name '*.dll' -print -quit 2>/dev/null)" ]; then
  echo "::error::no DLLs found in $src ($repo@$ref)" >&2
  rm -rf .platform-libs-src
  exit 1
fi

mkdir -p .platform-libs
cp "$src"/*.dll .platform-libs/
n="$(find "$src" -maxdepth 1 -name '*.dll' | wc -l)"
rm -rf .platform-libs-src   # drop the token-bearing clone
echo "Copied $n DLL(s) into .platform-libs/"

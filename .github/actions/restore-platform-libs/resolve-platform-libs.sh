#!/usr/bin/env bash
# Resolve platform-libs version/edition/SHA from platform.json and expose them as
# step outputs (version, edition, sha) for the cache key and the fetch.
# Inputs (env): GH_TOKEN, LIBS_REPO. Writes to $GITHUB_OUTPUT.
set -euo pipefail

: "${GH_TOKEN:?GH_TOKEN is required}"
: "${LIBS_REPO:?LIBS_REPO is required}"

echo "version=$(jq -r .version platform.json)" >> "$GITHUB_OUTPUT"
echo "edition=$(jq -r .edition platform.json)" >> "$GITHUB_OUTPUT"
ref=$(jq -r '.ref // "main"' platform.json)

# actions/checkout persists an Authorization header for GITHUB_TOKEN in the local
# git config; it overrides the in-URL token and GITHUB_TOKEN can't see the private
# libs repo (-> "Repository not found"). Reset it so GH_TOKEN is actually used.
sha=$(git -c http.https://github.com/.extraheader= ls-remote \
  "https://x-access-token:${GH_TOKEN}@github.com/${LIBS_REPO}" "refs/heads/${ref}" | cut -f1)

if [ -z "$sha" ]; then
  echo "::error::Could not resolve refs/heads/$ref in $LIBS_REPO (check token / repo access)" >&2
  exit 1
fi
echo "sha=$sha" >> "$GITHUB_OUTPUT"

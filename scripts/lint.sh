#!/usr/bin/env bash
# Run the CI linters locally via Docker (no install needed): actionlint over the
# workflows + shellcheck over the .sh scripts. Works on Linux/macOS and on Windows
# Git Bash. Requires Docker. Usage: bash scripts/lint.sh
set -euo pipefail

# Stop Git Bash/MSYS from rewriting the in-container paths (/repo, /mnt) on Windows.
export MSYS_NO_PATHCONV=1 MSYS2_ARG_CONV_EXCL='*'

# Docker Desktop on Windows wants a Windows-style host path; `pwd -W` gives it.
# On Linux/macOS `pwd -W` fails, so fall back to plain `pwd`.
host="$(pwd)"
if (pwd -W) >/dev/null 2>&1; then host="$(pwd -W)"; fi

echo "==> actionlint (workflows)"
docker run --rm -v "${host}:/repo" -w /repo rhysd/actionlint:1.7.12 -color

echo "==> shellcheck (scripts)"
docker run --rm -v "${host}:/mnt" -w /mnt koalaman/shellcheck-alpine:stable \
  sh -c "find .github -name '*.sh' -print0 | xargs -0 shellcheck --shell=bash"

# actionlint does not lint composite action.yml files, so parse-check them here.
echo "==> yaml parse (action.yml)"
docker run --rm -v "${host}:/w" -w /w --entrypoint sh mikefarah/yq:4 \
  -c 'for f in $(find .github/actions -name action.yml); do yq e "true" "$f" >/dev/null || { echo "PARSE FAIL: $f"; exit 1; }; done'

echo "==> Lint OK"

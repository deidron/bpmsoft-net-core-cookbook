#!/usr/bin/env bash
# Runs the CI schema format check (.github/scripts/format-schema.sh) for every
# package with staged schema code, so imports ordering and style fail here
# instead of in the "Format schemas" job. Checks the working tree, not the
# staged content, so partially staged files are checked as they are on disk.
set -euo pipefail

mapfile -t pkgs < <(git diff --cached --name-only --diff-filter=ACMR \
  | grep -E '^[^/]+/Schemas/.+\.cs$' \
  | cut -d/ -f1 \
  | sort -u)

if [[ ${#pkgs[@]} -eq 0 ]]; then
  exit 0
fi

for pkg in "${pkgs[@]}"; do
  echo "Checking schema format in $pkg ..."
  bash .github/scripts/format-schema.sh "$pkg"
done

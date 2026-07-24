#!/usr/bin/env bash
# Enforces a Conventional Commits header: <type>(<scope>)?: <description>.
# Same type vocabulary as the branch prefixes in CONTRIBUTING.md.
set -euo pipefail

msg_file="$1"
first_line="$(head -n1 "$msg_file")"
pattern='^(feat|fix|docs|style|refactor|perf|test|build|ci|chore|revert)(\([a-z0-9,_-]+\))?!?: .+'

if ! [[ "$first_line" =~ $pattern ]]; then
  echo "✖ Commit message must follow Conventional Commits: <type>(<scope>)?: <description>" >&2
  echo "  Allowed types: feat fix docs style refactor perf test build ci chore revert" >&2
  echo "  Got: $first_line" >&2
  exit 1
fi

set -euo pipefail

# Refuse a production release unless the tagged commit is contained in main.
# A git tag belongs to no branch, so "is the tag on main?" is answered by commit
# ancestry, not by a branch name. Usage: guard-tag-on-main.sh <commit-sha>

sha="${1:?usage: guard-tag-on-main.sh <commit-sha>}"

git fetch origin main --quiet
if git merge-base --is-ancestor "$sha" FETCH_HEAD; then
  echo "Commit $sha is on main ✓"
else
  echo "::error::Commit $sha is not on main — production deploy refused" >&2
  exit 1
fi

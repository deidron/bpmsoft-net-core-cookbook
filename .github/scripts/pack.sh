set -euo pipefail

INPUT_PACKAGES="${1:-}"
FAILED=0

if [ -z "$INPUT_PACKAGES" ] || [ "$INPUT_PACKAGES" == "[]" ]; then
  echo "No packages provided to pack."
  exit 0
fi

PACKAGES=$(echo "$INPUT_PACKAGES" | jq -r '.[]')

for pkg in $PACKAGES; do
  echo "::group::Packing $pkg"
  
  if ! ubs zip "$GITHUB_WORKSPACE/$pkg" -d "./artifacts/$pkg.gz"; then
    echo "::error::Failed to pack package: $pkg"
    FAILED=1
  fi
  
  echo "::endgroup::"
done

if [ $FAILED -ne 0 ]; then
  echo "One or more packages failed to pack."
  exit 1
fi

set -euo pipefail

# Secrets are passed via env (BPMSOFT_URL/USER/PASSWORD) — not interpolated into
# the command line, so they don't end up in the rendered command.
shopt -s nullglob
artifacts=(./artifacts/*.gz)

if [[ ${#artifacts[@]} -eq 0 ]]; then
  echo "::error::No .gz packages found in ./artifacts" >&2
  exit 1
fi

# Pre-flight: validate every artifact before touching the target, so a corrupt
# package fails the deploy up front instead of leaving the environment in a
# half-installed state.
for pkg in "${artifacts[@]}"; do
  if ! gzip -t "$pkg" 2>/dev/null; then
    echo "::error::Corrupt or unreadable artifact: $pkg" >&2
    exit 1
  fi
done
echo "Pre-flight OK: ${#artifacts[@]} artifact(s) validated"

for pkg in "${artifacts[@]}"; do
  echo "[STUB] ubs install $pkg"
  echo "[STUB] URL: ${BPMSOFT_URL:-<unset>}"
  # ubs install "$pkg" \
  #   -u "$BPMSOFT_URL" -l "$BPMSOFT_USER" -p "$BPMSOFT_PASSWORD" \
  #   -o true -i true
done

echo "[STUB] Deploy completed successfully"

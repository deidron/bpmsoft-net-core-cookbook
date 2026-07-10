#!/usr/bin/env bash
# `ubs register` configures an interactive shell (~/.bashrc), which the
# non-interactive bash of later workflow steps does not source. Add a thin
# wrapper to $GITHUB_PATH so `ubs` is available in every subsequent step.
set -euo pipefail

bin_dir="$RUNNER_TEMP/ubs-bin"
mkdir -p "$bin_dir"
cat > "$bin_dir/ubs" <<EOF
#!/usr/bin/env bash
exec dotnet "$GITHUB_WORKSPACE/.ubs/app/ubs.dll" "\$@"
EOF
chmod +x "$bin_dir/ubs"
echo "$bin_dir" >> "$GITHUB_PATH"

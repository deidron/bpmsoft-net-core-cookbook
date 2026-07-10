set -euo pipefail

pkg="$1"
sln=$(find "$pkg" -maxdepth 1 -name '*.sln'   | head -1)
prj=$(find "$pkg" -maxdepth 1 -name '*.csproj' | head -1)
target="${sln:-$prj}"

# Platform DLLs live in .platform-libs (populated at CI time by the
# restore-platform-libs action; gitignored, never committed). *.props wire them in:
# when GITHUB_ACTIONS=true, CoreLibPath -> .platform-libs and the target switches
# to netstandard2.0 (builds on Linux, no .NET Framework ref-assemblies).
if [[ ! -f .platform-libs/BPMSoft.Core.dll ]]; then
  echo "::error::.platform-libs is missing platform DLLs — ensure PLATFORM_LIBS_REPO is set and the restore-platform-libs step ran (GH_PAT must read the private libs repo)." >&2
  exit 1
fi

dotnet restore "$target"
dotnet build   "$target" --no-restore --configuration Release

[[ -z "$sln" ]] && exit 0
dotnet test "$sln" --configuration Release \
  --logger "trx;LogFileName=test-results.trx" \
  --results-directory TestResults

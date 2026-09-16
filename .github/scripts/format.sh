set -euo pipefail

pkg="$1"
sln=$(find "$pkg" -maxdepth 1 -name '*.sln'    | head -1)
prj=$(find "$pkg" -maxdepth 1 -name '*.csproj'  | head -1)

# dotnet format loads Debug by default; match build.sh (Release).
export Configuration=Release

dotnet format "${sln:-$prj}" --verify-no-changes --verbosity diagnostic --severity info

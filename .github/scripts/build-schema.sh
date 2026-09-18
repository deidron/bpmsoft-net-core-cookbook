set -euo pipefail

pkg="$1"
csproj="$pkg/_schemas.csproj"

if [[ "${GITHUB_ACTIONS:-}" == "true" ]]; then
  echo "::error::build-schema.sh is local only." >&2
  exit 1
fi

if ! compgen -G "$pkg/Schemas/*/*.cs" > /dev/null; then
  echo "No schema code in $pkg, skipping."
  exit 0
fi

cp .github/templates/schema-package.csproj "$csproj"
trap 'rm -f "$csproj"' EXIT

# ConfigurationRefsPath may come from the environment, containerEnv or the package's
# local.props; ask MSBuild for the result instead of guessing.
if [[ "$(dotnet msbuild "$csproj" -getProperty:SchemaFullBuild | tr -d '\r')" != "true" ]]; then
  refs="$(dotnet msbuild "$csproj" -getProperty:ConfigurationRefsPath | tr -d '\r')"
  echo "ConfigurationRefsPath is not set or does not exist: '${refs}'." >&2
  echo "Point it to the configuration references, e.g. .configuration-refs (see .devcontainer/README.md)." >&2
  exit 1
fi

# Local only: full compile of schema code against the configuration references, with the
# package's analyzer settings (see schema-build.props).
dotnet build "$csproj" --configuration Release

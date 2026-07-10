set -euo pipefail

pkg="$1"
csproj="$pkg/_schemas.csproj"

cp .github/templates/schema-package.csproj "$csproj"
trap 'rm -f "$csproj"' EXIT

# Schema code references the platform, but the generated csproj has no assembly
# references, so it does not bind. Limit checks to whitespace + style (syntactic /
# IDE rules); analyzers (--severity) need a successful compilation and are skipped.
dotnet format whitespace "$csproj" --verify-no-changes --verbosity normal
dotnet format style      "$csproj" --verify-no-changes --verbosity normal

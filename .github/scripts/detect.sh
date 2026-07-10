set -euo pipefail

package_project=()
all_packages=()

for pkg_dir in */; do
  pkg_dir="${pkg_dir%/}"
  [[ ! -f "$pkg_dir/descriptor.json" ]] && continue

  all_packages+=("$pkg_dir")
  [[ -n "$(find "$pkg_dir" -maxdepth 1 -name '*.csproj')" ]] && package_project+=("$pkg_dir")
done

package_project_json="[]"
all_packages_json="[]"

[[ ${#package_project[@]}  -gt 0 ]] && package_project_json=$(jq -cn '$ARGS.positional' --args -- "${package_project[@]}")
[[ ${#all_packages[@]} -gt 0 ]] && all_packages_json=$(jq -cn '$ARGS.positional' --args -- "${all_packages[@]}")

echo "package_project=$package_project_json"   >> "$GITHUB_OUTPUT"
echo "all_packages=$all_packages_json" >> "$GITHUB_OUTPUT"

echo "With .csproj : ${package_project[*]:-none}"
echo "All packages : ${all_packages[*]:-none}"

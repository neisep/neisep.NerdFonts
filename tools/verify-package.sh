#!/usr/bin/env bash
# Checks a packed neisep.NerdFonts .nupkg: required files, and the nuspec metadata nuget.org relies on.
# Usage (from the repository root): tools/verify-package.sh <directory with the .nupkg>
set -euo pipefail

out=${1:?Usage: tools/verify-package.sh <directory with the .nupkg>}
version=$(sed -n 's|.*<Version>\(.*\)</Version>.*|\1|p' src/neisep.NerdFonts/neisep.NerdFonts.csproj)
shopt -s nullglob
packages=("$out"/*.nupkg)
[ ${#packages[@]} -gt 0 ] || { echo "::error::No .nupkg in $out"; exit 1; }

failed=0
fail() { echo "::error::$1"; failed=1; }

for pkg in "${packages[@]}"; do
  listing=$(unzip -Z1 "$pkg")
  for f in lib/netstandard2.0/neisep.NerdFonts.dll lib/netstandard2.0/neisep.NerdFonts.xml \
           LICENSE README.md THIRD-PARTY-NOTICES.md icon.png licenses/*; do
    grep -qxF "$f" <<< "$listing" || fail "$f is missing from $pkg"
  done

  nuspec=$(unzip -p "$pkg" '*.nuspec')
  for element in \
      '<id>neisep.NerdFonts</id>' \
      "<version>$version</version>" \
      '<license type="file">LICENSE</license>' \
      '<readme>README.md</readme>' \
      '<icon>icon.png</icon>'; do
    grep -qF "$element" <<< "$nuspec" || fail "$element is missing from the nuspec of $pkg"
  done
done

[ $failed -eq 0 ] && echo "Package check passed: ${packages[*]}"
exit $failed

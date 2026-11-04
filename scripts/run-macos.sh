#!/bin/zsh
set -euo pipefail

project_root="$(cd "$(dirname "$0")/.." && pwd)"
dotnet_bin="${DOTNET_ROOT:-$HOME/.dotnet}/dotnet"
configuration="${1:-Debug}"
build_output="$project_root/src/PixelMate/bin/$configuration/net10.0"
app_bundle="$project_root/.local/PixelMate.app"
contents_dir="$app_bundle/Contents"
macos_dir="$contents_dir/MacOS"

if [[ ! -x "$dotnet_bin" ]]; then
  print -u2 "PixelMate: dotnet was not found at $dotnet_bin"
  exit 1
fi

"$dotnet_bin" build \
  "$project_root/src/PixelMate/PixelMate.csproj" \
  -c "$configuration" \
  --no-restore \
  -m:1 \
  -nodeReuse:false \
  -p:UseSharedCompilation=false \
  -v:minimal

rm -rf "$app_bundle"
mkdir -p "$macos_dir"
cp -R "$build_output/." "$macos_dir/"
mv "$macos_dir/PixelMate" "$macos_dir/PixelMate.Host"
cp "$project_root/scripts/macos/pixelmate-launcher.sh" "$macos_dir/PixelMate"
chmod +x "$macos_dir/PixelMate"
cp "$project_root/scripts/macos/Info.plist" "$contents_dir/Info.plist"

open "$app_bundle"

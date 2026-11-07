#!/bin/zsh
set -euo pipefail

project_root="$(cd "$(dirname "$0")/.." && pwd)"
dotnet_bin="${DOTNET_ROOT:-$HOME/.dotnet}/dotnet"
output_dir="$project_root/artifacts/PixelMate-devhost-win-x64"
archive_path="$project_root/artifacts/PixelMate-1.0.1-win-x64-devhost.zip"

export AVALONIA_TELEMETRY_OPTOUT=1
export DOTNET_CLI_TELEMETRY_OPTOUT=1

if [[ ! -x "$dotnet_bin" ]]; then
  print -u2 "PixelMate: dotnet was not found at $dotnet_bin"
  exit 1
fi

rm -rf "$output_dir"

"$dotnet_bin" publish \
  "$project_root/src/PixelMate/PixelMate.csproj" \
  -c Release \
  -r win-x64 \
  --self-contained false \
  -p:DebugType=None \
  -p:DebugSymbols=false \
  -p:NuGetAudit=false \
  -o "$output_dir" \
  -m:1 \
  -nodeReuse:false \
  -p:UseSharedCompilation=false \
  -v:minimal

required_files=(
  "PixelMate.dll"
  "PixelMate.runtimeconfig.json"
  "libSkiaSharp.dll"
  "libHarfBuzzSharp.dll"
  "av_libglesv2.dll"
)
for required_file in "${required_files[@]}"; do
  if [[ ! -f "$output_dir/$required_file" ]]; then
    print -u2 "PixelMate: devhost publish output is missing $required_file"
    exit 1
  fi
done

find "$output_dir" -maxdepth 1 -type f -name '*.pdb' -delete
cp "$project_root/scripts/windows/START-PIXELMATE-DEVHOST.cmd" \
  "$output_dir/START-PIXELMATE-DEVHOST.cmd"
cp "$project_root/docs/windows-devhost-testing.md" \
  "$output_dir/DEVHOST-README.md"

rm -f "$archive_path"
(
  cd "$project_root/artifacts"
  zip -r -q -X "$(basename "$archive_path")" "$(basename "$output_dir")"
)

print "Windows x64 devhost output: $output_dir"
print "Windows x64 devhost archive: $archive_path"

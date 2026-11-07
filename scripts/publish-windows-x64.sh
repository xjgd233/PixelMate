#!/bin/zsh
set -euo pipefail

project_root="$(cd "$(dirname "$0")/.." && pwd)"
dotnet_bin="${DOTNET_ROOT:-$HOME/.dotnet}/dotnet"
output_dir="$project_root/artifacts/PixelMate-win-x64"
archive_path="$project_root/artifacts/PixelMate-1.0.1-win-x64.zip"
offline_archive_path="$project_root/artifacts/PixelMate-1.0.1-win-x64-offline.zip"

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
  --self-contained true \
  -p:DebugType=None \
  -p:DebugSymbols=false \
  -p:NuGetAudit=false \
  -o "$output_dir" \
  -m:1 \
  -nodeReuse:false \
  -p:UseSharedCompilation=false \
  -v:minimal

required_files=(
  "PixelMate.exe"
  "PixelMate.dll"
  "libSkiaSharp.dll"
  "libHarfBuzzSharp.dll"
  "av_libglesv2.dll"
)
for required_file in "${required_files[@]}"; do
  if [[ ! -f "$output_dir/$required_file" ]]; then
    print -u2 "PixelMate: publish output is missing $required_file"
    exit 1
  fi
done

find "$output_dir" -maxdepth 1 -type f -name '*.pdb' -delete
cp "$project_root/README.md" "$output_dir/README.md"
mkdir -p "$output_dir/docs"
cp "$project_root/docs/phase-status.md" "$output_dir/docs/phase-status.md"
cp "$project_root/docs/release-notes-1.0.1.md" \
  "$output_dir/docs/release-notes-1.0.1.md"
cp "$project_root/docs/windows-acceptance-checklist.md" \
  "$output_dir/docs/windows-acceptance-checklist.md"
cp -R "$project_root/docs/images" "$output_dir/docs/images"
cp "$project_root/docs/release-notes-1.0.1.md" "$output_dir/RELEASE-NOTES.md"
cp "$project_root/docs/windows-acceptance-checklist.md" \
  "$output_dir/WINDOWS-ACCEPTANCE.md"
cp "$project_root/docs/windows-offline-package.md" \
  "$output_dir/OFFLINE-README.md"
cp "$project_root/scripts/windows/START-PIXELMATE.cmd" \
  "$output_dir/START-PIXELMATE.cmd"

rm -f "$archive_path" "$offline_archive_path"
(
  cd "$project_root/artifacts"
  zip -r -q -X "$(basename "$archive_path")" "$(basename "$output_dir")"
)
cp "$archive_path" "$offline_archive_path"

print "Windows x64 publish output: $output_dir"
print "Windows x64 release archive: $archive_path"
print "Windows x64 offline archive: $offline_archive_path"

#!/bin/zsh
set -euo pipefail

project_root="$(cd "$(dirname "$0")/.." && pwd)"
dotnet_bin="${DOTNET_ROOT:-$HOME/.dotnet}/dotnet"
output_dir="$project_root/artifacts/PixelMate-win-x64"

if [[ ! -x "$dotnet_bin" ]]; then
  print -u2 "PixelMate: dotnet was not found at $dotnet_bin"
  exit 1
fi

"$dotnet_bin" publish \
  "$project_root/src/PixelMate/PixelMate.csproj" \
  -c Release \
  -r win-x64 \
  --self-contained true \
  -p:DebugType=None \
  -p:DebugSymbols=false \
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
cp "$project_root/docs/release-notes-1.0.0.md" "$output_dir/RELEASE-NOTES.md"

print "Windows x64 publish output: $output_dir"

#!/bin/zsh
set -euo pipefail

bundle_macos_dir="$(cd "$(dirname "$0")" && pwd)"

if [[ -z "${DOTNET_ROOT:-}" ]]; then
  for dotnet_root_candidate in "$HOME/.dotnet" "/usr/local/share/dotnet"; do
    if [[ -x "$dotnet_root_candidate/dotnet" ]]; then
      export DOTNET_ROOT="$dotnet_root_candidate"
      break
    fi
  done
fi

exec "$bundle_macos_dir/PixelMate.Host" "$@"

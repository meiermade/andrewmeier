#!/usr/bin/env bash
set -euo pipefail

script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
target="${1:-Default}"
if [[ $# -gt 0 ]]; then
  shift
fi

dotnet run --project "$script_dir/src/Build" -- --target "$target" "$@"

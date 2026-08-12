#!/usr/bin/env bash
set -euo pipefail

script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
e2e_dir="$(cd "$script_dir/.." && pwd)"
repo_dir="$(cd "$e2e_dir/.." && pwd)"
app_dir="$repo_dir/app"
server_port="${E2E_SERVER_PORT:-5051}"
log_dir="${TMPDIR:-/tmp}/andymeier-e2e"
mkdir -p "$log_dir"
app_pid=""

cleanup() {
  if [[ -n "$app_pid" ]]; then
    kill "$app_pid" 2>/dev/null || true
    wait "$app_pid" 2>/dev/null || true
  fi
}
trap cleanup EXIT INT TERM

cd "$app_dir"

if [[ "${E2E_SKIP_DOTNET_BUILD:-0}" != "1" ]]; then
  dotnet build src/App/App.fsproj --nologo --verbosity minimal
fi

./fake.sh BuildCss

ASPNETCORE_ENVIRONMENT=Development \
GOOGLE_ANALYTICS_MEASUREMENT_ID=G-LOCAL \
OTEL_RESOURCE_ATTRIBUTES="deployment.environment.name=e2e" \
OTEL_SERVICE_NAME="andymeier-e2e" \
SEQ_ENDPOINT="http://127.0.0.1:5341" \
SERVER_URL="http://127.0.0.1:${server_port}" \
  dotnet run --no-build --project src/App/App.fsproj > "$log_dir/app.log" 2>&1 &
app_pid=$!

for _ in $(seq 1 60); do
  if curl --fail --silent "http://127.0.0.1:${server_port}/health" >/dev/null; then break; fi
  if ! kill -0 "$app_pid" 2>/dev/null; then
    cat "$log_dir/app.log" >&2 || true
    exit 1
  fi
  sleep 1
done

if ! curl --fail --silent "http://127.0.0.1:${server_port}/articles" | grep -q 'F# Semantic Kernel'; then
  cat "$log_dir/app.log" >&2 || true
  exit 1
fi

wait "$app_pid"

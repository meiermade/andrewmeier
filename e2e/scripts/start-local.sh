#!/usr/bin/env bash
set -euo pipefail

script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
e2e_dir="$(cd "$script_dir/.." && pwd)"
repo_dir="$(cd "$e2e_dir/.." && pwd)"
app_dir="$repo_dir/app"
server_port="${E2E_SERVER_PORT:-5051}"
mock_port="${E2E_MOCK_NOTION_PORT:-5052}"
log_dir="${TMPDIR:-/tmp}/andymeier-e2e"
mkdir -p "$log_dir"
mock_pid=""
app_pid=""

cleanup() {
  for pid in "$app_pid" "$mock_pid"; do
    if [[ -n "$pid" ]]; then kill "$pid" 2>/dev/null || true; fi
  done
  for pid in "$app_pid" "$mock_pid"; do
    if [[ -n "$pid" ]]; then wait "$pid" 2>/dev/null || true; fi
  done
}
trap cleanup EXIT INT TERM

cd "$app_dir"

if [[ "${E2E_SKIP_DOTNET_BUILD:-0}" != "1" ]]; then
  dotnet build src/App/App.fsproj --nologo --verbosity minimal
  dotnet build src/MockNotion/MockNotion.fsproj --nologo --verbosity minimal
fi

./fake.sh BuildCss

MOCK_NOTION_URL="http://127.0.0.1:${mock_port}" \
  dotnet run --no-build --project src/MockNotion/MockNotion.fsproj > "$log_dir/mock-notion.log" 2>&1 &
mock_pid=$!

for _ in $(seq 1 60); do
  if curl --fail --silent "http://127.0.0.1:${mock_port}/healthz" >/dev/null; then break; fi
  sleep 1
done

if ! curl --fail --silent "http://127.0.0.1:${mock_port}/healthz" >/dev/null; then
  cat "$log_dir/mock-notion.log" >&2 || true
  exit 1
fi

sqlite_path="$log_dir/articles-${server_port}.db"
rm -f "$sqlite_path"
ASPNETCORE_ENVIRONMENT=Development \
GOOGLE_ANALYTICS_MEASUREMENT_ID=G-LOCAL \
NOTION_API_KEY=mock-notion-token \
NOTION_ARTICLES_DATABASE_ID=mock-articles \
NOTION_BASE_URL="http://127.0.0.1:${mock_port}/v1" \
SEQ_ENDPOINT="http://127.0.0.1:5341" \
SERVER_URL="http://127.0.0.1:${server_port}" \
SQLITE_PATH="$sqlite_path" \
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

for _ in $(seq 1 60); do
  if curl --fail --silent "http://127.0.0.1:${server_port}/articles" | grep -q 'Mock engineering notes'; then break; fi
  sleep 1
done

if ! curl --fail --silent "http://127.0.0.1:${server_port}/articles" | grep -q 'Mock engineering notes'; then
  cat "$log_dir/app.log" >&2 || true
  exit 1
fi

wait "$app_pid"

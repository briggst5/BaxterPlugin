#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

DOC_FILES=(
  "$ROOT_DIR/README.md"
  "$ROOT_DIR/docs/OPERATIONS.md"
  "$ROOT_DIR/polarion-mcp.env.example"
)

code_keys="$(
  rg --no-heading --no-filename --only-matching "POLARION_[A-Z0-9_]+" "$ROOT_DIR/dotnet" \
    | sort -u || true
)"

doc_text="$(
  for file in "${DOC_FILES[@]}"; do
    [[ -f "$file" ]] && cat "$file"
  done
)"

missing_keys=()
while IFS= read -r key; do
  [[ -z "$key" ]] && continue
  if ! grep -Fq "$key" <<<"$doc_text"; then
    missing_keys+=("$key")
  fi
done <<<"$code_keys"

workflow_file="$ROOT_DIR/.github/workflows/release.yml"
missing_rids=()
if [[ -f "$workflow_file" ]]; then
  rids="$(rg --no-heading --only-matching "win-x64|linux-x64|osx-x64|osx-arm64|linux-musl-x64" "$workflow_file" | sort -u || true)"
  while IFS= read -r rid; do
    [[ -z "$rid" ]] && continue
    if ! grep -Fq "$rid" <<<"$doc_text"; then
      missing_rids+=("$rid")
    fi
  done <<<"$rids"
fi

if [[ ${#missing_keys[@]} -eq 0 && ${#missing_rids[@]} -eq 0 ]]; then
  echo "Docs drift check passed."
  exit 0
fi

if [[ ${#missing_keys[@]} -gt 0 ]]; then
  echo "Missing POLARION_* docs entries:"
  for key in "${missing_keys[@]}"; do
    echo " - $key"
  done
fi

if [[ ${#missing_rids[@]} -gt 0 ]]; then
  echo "Release RIDs missing from docs:"
  for rid in "${missing_rids[@]}"; do
    echo " - $rid"
  done
fi

exit 1

#!/usr/bin/env bash
# One-time machine baseline for Baxter plugins (Python MCP via uv).
set -euo pipefail

MIN_PYTHON="3.10"

echo "==> Baxter dev machine bootstrap"

require_cmd() {
  if ! command -v "$1" >/dev/null 2>&1; then
    echo "Missing required command: $1" >&2
    return 1
  fi
}

python_ok=false
for py in python3 python; do
  if command -v "$py" >/dev/null 2>&1; then
    version="$("$py" -c 'import sys; print(".".join(map(str, sys.version_info[:2])))')"
    if "$py" -c "import sys; sys.exit(0 if sys.version_info >= (3, 10) else 1)"; then
      echo "Found Python ${version} ($py)"
      python_ok=true
      break
    fi
  fi
done

if [[ "$python_ok" != true ]]; then
  echo "Python ${MIN_PYTHON}+ is required. Install from https://www.python.org/downloads/" >&2
  exit 1
fi

if ! command -v uv >/dev/null 2>&1; then
  echo "Installing uv..."
  curl -LsSf https://astral.sh/uv/install.sh | sh
  export PATH="${HOME}/.local/bin:${PATH}"
fi

require_cmd uv
echo "uv: $(uv --version)"

echo ""
echo "Bootstrap complete."
echo "Install Baxter plugins from the team marketplace; MCP deps sync on first server start."

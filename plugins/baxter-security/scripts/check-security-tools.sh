#!/usr/bin/env bash
# Verify baxter-security baseline and optional scanner installs (Linux / macOS).
set -euo pipefail

OK=0
WARN=0
FAIL=0

check() {
  local label="$1"
  local status="$2"
  local detail="${3:-}"
  case "$status" in
    ok)   echo "  [OK]   $label${detail:+ — $detail}"; ((OK++)) || true ;;
    warn) echo "  [WARN] $label${detail:+ — $detail}"; ((WARN++)) || true ;;
    fail) echo "  [FAIL] $label${detail:+ — $detail}"; ((FAIL++)) || true ;;
  esac
}

version_of() {
  local cmd="$1"
  if command -v "$cmd" >/dev/null 2>&1; then
    "$cmd" --version 2>/dev/null | head -1 || "$cmd" version 2>/dev/null | head -1 || echo "installed"
  else
    echo ""
  fi
}

echo "==> Baxter Security tool check (Linux / macOS)"
echo ""

# --- Baseline ---
if command -v python3 >/dev/null 2>&1; then
  pyv=$(python3 -c 'import sys; print(".".join(map(str, sys.version_info[:3])))')
  if python3 -c "import sys; sys.exit(0 if sys.version_info >= (3, 10) else 1)"; then
    check "Python 3.10+" ok "$pyv"
  else
    check "Python 3.10+" fail "found $pyv"
  fi
else
  check "Python 3.10+" fail "python3 not found"
fi

if command -v uv >/dev/null 2>&1; then
  check "uv" ok "$(uv --version)"
else
  check "uv" warn "not found — run ../../scripts/bootstrap-dev-machine.sh from BaxterPlugin root"
fi

NVD_ENV="${HOME}/.config/nvd-api.env"
if [[ -f "$NVD_ENV" ]]; then
  if grep -qE '^NVD_API_KEY=.+' "$NVD_ENV" 2>/dev/null; then
    check "NVD API key file" ok "$NVD_ENV"
  else
    check "NVD API key file" warn "$NVD_ENV exists but NVD_API_KEY may be empty"
  fi
else
  check "NVD API key file" warn "missing — run: node scripts/setup-nvd-env.mjs"
fi

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
NVD_SCRIPT="${SCRIPT_DIR}/../skills/nvd-cve-search/scripts/nvd_search.py"
if [[ -f "$NVD_SCRIPT" ]]; then
  check "nvd_search.py" ok "$NVD_SCRIPT"
else
  check "nvd_search.py" fail "not found at expected path"
fi

echo ""
echo "--- Universal scanners (recommended) ---"

if command -v grype >/dev/null 2>&1; then
  check "grype" ok "$(version_of grype)"
else
  check "grype" warn "not installed — see docs/INSTALL.md"
fi

if command -v syft >/dev/null 2>&1; then
  check "syft" ok "$(version_of syft)"
else
  check "syft" warn "not installed — see docs/INSTALL.md"
fi

if command -v trivy >/dev/null 2>&1; then
  check "trivy" ok "$(version_of trivy)"
else
  check "trivy" warn "not installed (optional if grype+syft present)"
fi

echo ""
echo "--- SAST ---"

if command -v semgrep >/dev/null 2>&1; then
  check "semgrep" ok "$(version_of semgrep)"
else
  check "semgrep" warn "not installed — uv tool install semgrep"
fi

echo ""
echo "--- Ecosystem (optional) ---"

for tool in npm dotnet cargo go; do
  if command -v "$tool" >/dev/null 2>&1; then
    check "$tool" ok "$(version_of "$tool")"
  else
    check "$tool" warn "not on PATH (skip if stack unused)"
  fi
done

if command -v pip-audit >/dev/null 2>&1; then
  check "pip-audit" ok "$(version_of pip-audit)"
else
  check "pip-audit" warn "not installed — uv tool install pip-audit"
fi

if command -v cargo-audit >/dev/null 2>&1; then
  check "cargo-audit" ok "$(version_of cargo-audit)"
else
  check "cargo-audit" warn "not installed — cargo install cargo-audit"
fi

if command -v govulncheck >/dev/null 2>&1; then
  check "govulncheck" ok "$(version_of govulncheck)"
else
  check "govulncheck" warn "not installed — go install golang.org/x/vuln/cmd/govulncheck@latest"
fi

echo ""
echo "==> Summary: $OK ok, $WARN warnings, $FAIL failures"
if [[ "$FAIL" -gt 0 ]]; then
  exit 1
fi
exit 0

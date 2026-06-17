#!/usr/bin/env bash
# fix-wsl-corp-certs.sh — Trust corporate TLS roots in WSL (Zscaler/Baxter + internal HRC).
#
# Problem this solves:
#   WSL does not inherit the Windows certificate store. Python, Node, curl, git, MCP servers,
#   and Chromium then fail TLS with CERTIFICATE_VERIFY_FAILED when traffic is intercepted by
#   Zscaler (Baxter proxy roots) or when connecting to internal hosts signed by corporate CAs
#   (e.g. polarion.hrc.corp → btvschrc03).
#
# Usage:
#   bash scripts/fix-wsl-corp-certs.sh              # install + verify (recommended)
#   bash scripts/fix-wsl-corp-certs.sh --verify     # verify only, no changes
#   bash scripts/fix-wsl-corp-certs.sh --chromium   # also import into Chromium NSS db
#   bash scripts/fix-wsl-corp-certs.sh --dry-run    # show actions without sudo writes
#
# Requirements:
#   - WSL2 with Windows interop (for cert export from Windows store)
#   - sudo (to install into /usr/local/share/ca-certificates and run update-ca-certificates)
#   - openssl, curl (usually preinstalled)
#   - Optional: libnss3-tools (certutil) for --chromium
#
# Safe to re-run: idempotent.

set -euo pipefail

SCRIPT_NAME="$(basename "$0")"
DRY_RUN=0
VERIFY_ONLY=0
SETUP_CHROMIUM=0

CA_DIR="/usr/local/share/ca-certificates/corp"
BUNDLE="/etc/ssl/certs/ca-certificates.crt"
WORKDIR="${TMPDIR:-/tmp}/wsl-corp-certs-$$"
BASHRC="${HOME}/.bashrc"
MARKER_BEGIN="# >>> wsl-corp-certs >>>"
MARKER_END="# <<< wsl-corp-certs <<<"

# Internal HRC root — used when Polarion and other *.hrc.corp hosts send only a leaf cert.
HRC_CA_URL_PRIMARY="http://indsacert01v.HRC.CORP/CertEnroll/indsacert01v.HRC.CORP_btvschrc03(2).crt"
HRC_CA_URL_FALLBACK="http://www.hill-rom.com/crl/indsacert01v.HRC.CORP_btvschrc03(2).crt"
HRC_CA_NAME="hrc-root-btvschrc03.crt"

# Windows cert subjects to export (regex matched against LocalMachine\Root).
WIN_CERT_PATTERN='Zscaler|Baxter|Proxy Baxter|btvschrc03|Hillrom|HRC'

# Hosts used for post-install verification.
VERIFY_HOSTS=(
  "google.com:443"
  "dev.azure.com:443"
  "polarion.hrc.corp:443"
)

log()  { printf '[%s] %s\n' "$SCRIPT_NAME" "$*"; }
warn() { printf '[%s] WARN: %s\n' "$SCRIPT_NAME" "$*" >&2; }
die()  { printf '[%s] ERROR: %s\n' "$SCRIPT_NAME" "$*" >&2; exit 1; }

usage() {
  sed -n '2,20p' "$0" | sed 's/^# \{0,1\}//'
  exit 0
}

run() {
  if [[ "$DRY_RUN" -eq 1 ]]; then
    log "DRY-RUN: $*"
  else
    "$@"
  fi
}

run_sudo() {
  if [[ "$DRY_RUN" -eq 1 ]]; then
    log "DRY-RUN sudo: $*"
  else
    sudo "$@"
  fi
}

need_cmd() {
  command -v "$1" >/dev/null 2>&1 || die "Required command not found: $1"
}

parse_args() {
  while [[ $# -gt 0 ]]; do
    case "$1" in
      --dry-run)   DRY_RUN=1 ;;
      --verify)    VERIFY_ONLY=1 ;;
      --chromium)  SETUP_CHROMIUM=1 ;;
      -h|--help)   usage ;;
      *) die "Unknown option: $1 (try --help)" ;;
    esac
    shift
  done
}

find_powershell() {
  if command -v powershell.exe >/dev/null 2>&1; then
    command -v powershell.exe
    return 0
  fi
  local win_ps="/mnt/c/Windows/System32/WindowsPowerShell/v1.0/powershell.exe"
  if [[ -x "$win_ps" ]]; then
    echo "$win_ps"
    return 0
  fi
  return 1
}

slugify() {
  # Make a filesystem-safe slug from a cert subject DN.
  echo "$1" | tr '[:upper:]' '[:lower:]' | sed -E 's/[^a-z0-9]+/-/g; s/^-+|-+$//g' | cut -c1-80
}

cert_install_name() {
  # Prefer stable, human-readable filenames for known corporate roots.
  local cert_file="$1"
  local subject cn
  subject="$(openssl x509 -in "$cert_file" -noout -subject 2>/dev/null || true)"
  cn="$(echo "$subject" | sed -n 's/.*CN *= *\([^,]*\).*/\1/p' | sed 's/^ *//;s/ *$//')"

  case "$cn" in
    "Baxter Root CA SHA2")              echo "baxter-root-ca-sha2.crt" ;;
    "Proxy Baxter Healthcare"*)       echo "proxy-baxter-healthcare.crt" ;;
    "Proxy Baxter Healthcare (t) "*)   echo "proxy-baxter-healthcare.crt" ;;
    "btvschrc03")                      echo "hrc-root-btvschrc03.crt" ;;
    "Baxter Products Root CA MS")       echo "baxter-products-root-ca-ms.crt" ;;
    *)
      if [[ -n "$cn" ]]; then
        echo "$(slugify "$cn").crt"
      else
        echo "$(basename "$cert_file")"
      fi
      ;;
  esac
}

export_windows_roots() {
  local ps win_mount count f base pem
  ps="$(find_powershell || true)"
  if [[ -z "$ps" ]]; then
    warn "PowerShell not found — skipping Windows cert export."
    warn "Place PEM/DER roots in: ${WORKDIR}/manual-drop-in/"
    return 1
  fi

  win_mount="/mnt/c/Users/Public/wsl-corp-certs-export"
  log "Exporting matching roots from Windows LocalMachine\\Root ..."

  # Export each matching root to a unique .cer file (deduped by thumbprint).
  "$ps" -NoProfile -Command "
    \$pattern = '${WIN_CERT_PATTERN}'
    \$out = 'C:\\Users\\Public\\wsl-corp-certs-export'
    New-Item -ItemType Directory -Force -Path \$out | Out-Null
    Get-ChildItem Cert:\\LocalMachine\\Root |
      Where-Object { \$_.Subject -match \$pattern } |
      Sort-Object Thumbprint -Unique |
      ForEach-Object {
        \$slug = (\$_.Subject -replace '[^A-Za-z0-9]+','-').Trim('-').Substring(0, [Math]::Min(60, (\$_.Subject -replace '[^A-Za-z0-9]+','-').Trim('-').Length))
        \$path = Join-Path \$out (\$slug + '-' + \$_.Thumbprint + '.cer')
        Export-Certificate -Cert \$_ -FilePath \$path -Force | Out-Null
      }
  " >/dev/null 2>&1 || {
    warn "Windows cert export failed."
    return 1
  }

  if [[ ! -d "$win_mount" ]]; then
    warn "Windows export directory not visible from WSL: ${win_mount}"
    return 1
  fi

  count=0
  mkdir -p "${WORKDIR}/windows"
  for f in "${win_mount}"/*.cer; do
    [[ -f "$f" ]] || continue
    base="$(basename "$f" .cer)"
    pem="${WORKDIR}/windows/${base}.crt"
    if openssl x509 -inform DER -in "$f" -out "$pem" 2>/dev/null; then
      count=$((count + 1))
    else
      warn "Could not convert Windows cert: $f"
    fi
  done

  log "Exported ${count} certificate(s) from Windows."
  [[ "$count" -gt 0 ]]
}

fetch_hrc_root() {
  local der pem
  der="${WORKDIR}/hrc-root.der"
  pem="${WORKDIR}/hrc/${HRC_CA_NAME}"
  mkdir -p "${WORKDIR}/hrc"

  log "Fetching internal HRC root CA (btvschrc03) ..."
  if curl -fsS --max-time 20 -o "$der" "$HRC_CA_URL_PRIMARY" 2>/dev/null; then
    log "Downloaded HRC CA from internal CertEnroll URL."
  elif curl -fsS --max-time 20 -o "$der" "$HRC_CA_URL_FALLBACK" 2>/dev/null; then
    log "Downloaded HRC CA from fallback URL."
  else
    warn "Could not download HRC CA (need corporate network/VPN?)."
    warn "If Polarion TLS fails later, connect to VPN and re-run."
    return 1
  fi

  if openssl x509 -inform DER -in "$der" -out "$pem" 2>/dev/null; then
    :
  elif openssl x509 -in "$der" -out "$pem" 2>/dev/null; then
    :
  else
    warn "Could not parse downloaded HRC CA."
    return 1
  fi

  # Sanity check: subject should mention btvschrc03.
  if ! openssl x509 -in "$pem" -noout -subject 2>/dev/null | grep -qi 'btvschrc03'; then
    warn "Downloaded HRC CA subject unexpected: $(openssl x509 -in "$pem" -noout -subject 2>/dev/null || true)"
  fi
  return 0
}

collect_manual_dropins() {
  local drop="${WORKDIR}/manual-drop-in"
  mkdir -p "$drop"
  if compgen -G "${drop}/*" >/dev/null; then
    log "Processing manual drop-in certificates ..."
    local f out
    for f in "${drop}"/*; do
      [[ -f "$f" ]] || continue
      out="${WORKDIR}/manual/$(slugify "$(basename "$f")").crt"
      mkdir -p "${WORKDIR}/manual"
      if openssl x509 -inform DER -in "$f" -out "$out" 2>/dev/null \
        || openssl x509 -in "$f" -out "$out" 2>/dev/null; then
        log "  accepted: $(basename "$f")"
      else
        warn "  skipped (not a cert): $(basename "$f")"
      fi
    done
  fi
}

install_certs() {
  local src_dirs=("${WORKDIR}/windows" "${WORKDIR}/hrc" "${WORKDIR}/manual")
  local installed=0

  run_sudo mkdir -p "$CA_DIR"

  for dir in "${src_dirs[@]}"; do
    [[ -d "$dir" ]] || continue
    local f dest
    for f in "${dir}"/*.crt; do
      [[ -f "$f" ]] || continue
      dest="${CA_DIR}/$(cert_install_name "$f")"
      if [[ -f "$dest" ]] && cmp -s "$f" "$dest"; then
        log "unchanged: $(basename "$dest")"
      else
        run_sudo install -m 0644 "$f" "$dest"
        log "installed: $(basename "$dest")"
        installed=$((installed + 1))
      fi
    done
  done

  if [[ "$installed" -eq 0 ]] && [[ "$DRY_RUN" -eq 0 ]]; then
    log "No new certificates to install (already up to date)."
  fi

  log "Updating system CA bundle (update-ca-certificates) ..."
  run_sudo update-ca-certificates
}

configure_shell_env() {
  local block
  block=$(cat <<EOF
${MARKER_BEGIN}
# Corporate TLS roots for WSL — added by ${SCRIPT_NAME}
# Re-run the script after cert rotations; safe to source multiple times.
export SSL_CERT_FILE=${BUNDLE}
export REQUESTS_CA_BUNDLE=${BUNDLE}
export CURL_CA_BUNDLE=${BUNDLE}
export NODE_EXTRA_CA_CERTS=${BUNDLE}
export GIT_SSL_CAINFO=${BUNDLE}
${MARKER_END}
EOF
)

  if [[ -f "$BASHRC" ]] && grep -qF "$MARKER_BEGIN" "$BASHRC"; then
    log "~/.bashrc already configured (marker present)."
    return 0
  fi

  log "Appending CA environment variables to ~/.bashrc ..."
  if [[ "$DRY_RUN" -eq 1 ]]; then
    log "DRY-RUN: would append env block to ${BASHRC}"
  else
    printf '\n%s\n' "$block" >>"$BASHRC"
  fi
}

setup_chromium_nss() {
  if ! command -v certutil >/dev/null 2>&1; then
    warn "certutil not found. Install libnss3-tools for Chromium support:"
    warn "  sudo apt-get install -y libnss3-tools"
    return 1
  fi

  local nssdb="${HOME}/.pki/nssdb"
  mkdir -p "$nssdb"

  if [[ ! -f "${nssdb}/cert9.db" ]]; then
    log "Creating Chromium NSS database (empty password) ..."
    if [[ "$DRY_RUN" -eq 1 ]]; then
      log "DRY-RUN: certutil -N --empty-password"
    else
      certutil -d "sql:${nssdb}" -N --empty-password
    fi
  fi

  local f nick
  for f in "${CA_DIR}"/*.crt; do
    [[ -f "$f" ]] || continue
    nick="$(basename "$f" .crt)"
    if certutil -d "sql:${nssdb}" -L 2>/dev/null | grep -q "^${nick}$"; then
      log "NSS already has: ${nick}"
      continue
    fi
    log "Importing into Chromium NSS: ${nick}"
    if [[ "$DRY_RUN" -eq 1 ]]; then
      log "DRY-RUN: certutil -A -n ${nick} -i ${f}"
    else
      certutil -d "sql:${nssdb}" -A -t "C,," -n "$nick" -i "$f"
    fi
  done
}

verify_tls() {
  local entry host port
  local failures=0

  log "Verifying TLS against system bundle: ${BUNDLE}"
  for entry in "${VERIFY_HOSTS[@]}"; do
    host="${entry%%:*}"
    port="${entry##*:}"
    if echo | openssl s_client -connect "${host}:${port}" -CAfile "$BUNDLE" 2>/dev/null \
      | grep -q "Verify return code: 0 (ok)"; then
      log "  OK  ${host}"
    else
      warn "  FAIL ${host}"
      failures=$((failures + 1))
    fi
  done

  if command -v python3 >/dev/null 2>&1; then
    SSL_CERT_FILE="$BUNDLE" REQUESTS_CA_BUNDLE="$BUNDLE" python3 - <<'PY' || failures=$((failures + 1))
import os, sys, urllib.request
bundle = os.environ["SSL_CERT_FILE"]
for url in ("https://google.com", "https://dev.azure.com", "https://polarion.hrc.corp/polarion"):
    try:
        urllib.request.urlopen(url, timeout=15)
        print(f"  OK  python {url}")
    except Exception as e:
        print(f"  FAIL python {url}: {type(e).__name__}", file=sys.stderr)
        raise
PY
  fi

  if [[ "$failures" -gt 0 ]]; then
    warn "${failures} verification check(s) failed."
    warn "If polarion.hrc.corp failed: connect VPN and re-run."
    warn "If google.com failed: ensure Baxter/Zscaler roots exported from Windows."
    return 1
  fi

  log "All verification checks passed."
  return 0
}

cleanup() {
  rm -rf "$WORKDIR"
}

main() {
  parse_args "$@"

  need_cmd openssl
  need_cmd curl
  need_cmd sudo

  if [[ "$VERIFY_ONLY" -eq 1 ]]; then
    verify_tls
    exit $?
  fi

  trap cleanup EXIT
  mkdir -p "$WORKDIR/manual-drop-in"
  log "Work directory: ${WORKDIR}"
  log "Manual override: drop PEM/DER files in ${WORKDIR}/manual-drop-in/ before install."

  export_windows_roots || true
  fetch_hrc_root || true
  collect_manual_dropins

  local cert_count=0
  for dir in "${WORKDIR}/windows" "${WORKDIR}/hrc" "${WORKDIR}/manual"; do
    [[ -d "$dir" ]] && cert_count=$((cert_count + $(find "$dir" -maxdepth 1 -name '*.crt' 2>/dev/null | wc -l)))
  done
  if [[ "$cert_count" -eq 0 ]]; then
    die "No certificates collected. Check VPN, Windows export, or manual drop-in."
  fi

  install_certs
  configure_shell_env

  if [[ "$SETUP_CHROMIUM" -eq 1 ]]; then
    setup_chromium_nss || true
  fi

  # Export for current shell session too.
  export SSL_CERT_FILE="$BUNDLE"
  export REQUESTS_CA_BUNDLE="$BUNDLE"
  export CURL_CA_BUNDLE="$BUNDLE"
  export NODE_EXTRA_CA_CERTS="$BUNDLE"
  export GIT_SSL_CAINFO="$BUNDLE"

  verify_tls

  cat <<EOF

Done.

Next steps for this shell:
  source ~/.bashrc

For Chromium / browser TLS:
  bash scripts/fix-wsl-corp-certs.sh --chromium

After running, restart Cursor so MCP servers pick up the new environment.

EOF
}

main "$@"

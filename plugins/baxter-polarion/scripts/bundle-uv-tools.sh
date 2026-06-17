#!/usr/bin/env bash
# Download platform uv binaries into the baxter-product-owner plugin for offline bootstrap.
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
TOOLS="${ROOT}/bin/tools"
UV_VERSION="${UV_VERSION:-0.6.14}"

mkdir -p "${TOOLS}"

download_uv() {
  local platform="$1"
  local arch="$2"
  local out="$3"
  local url="https://github.com/astral-sh/uv/releases/download/${UV_VERSION}/uv-${arch}-${platform}.tar.gz"

  if [[ -f "${out}" ]]; then
    echo "Already present: ${out}"
    return
  fi

  echo "Downloading uv ${UV_VERSION} for ${platform}-${arch}..."
  tmp="$(mktemp -d)"
  curl -fsSL "${url}" | tar -xz -C "${tmp}"
  install -m 755 "${tmp}/uv-${arch}-${platform}/uv" "${out}"
  rm -rf "${tmp}"
  echo "Installed ${out}"
}

download_uv_windows() {
  local out="${TOOLS}/uv.exe"
  local url="https://github.com/astral-sh/uv/releases/download/${UV_VERSION}/uv-x86_64-pc-windows-msvc.zip"

  if [[ -f "${out}" ]]; then
    echo "Already present: ${out}"
    return
  fi

  echo "Downloading uv ${UV_VERSION} for windows..."
  tmp="$(mktemp -d)"
  curl -fsSL "${url}" -o "${tmp}/uv.zip"
  unzip -q "${tmp}/uv.zip" -d "${tmp}"
  install -m 755 "${tmp}/uv-${arch}-pc-windows-msvc/uv.exe" "${out}"
  rm -rf "${tmp}"
  echo "Installed ${out}"
}

download_uv "unknown-linux-gnu" "x86_64" "${TOOLS}/uv"
download_uv_windows

echo ""
echo "Bundled uv tools installed under ${TOOLS}/"
echo "Commit these for self-contained Linux + Windows plugin installs."

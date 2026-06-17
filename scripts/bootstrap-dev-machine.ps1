#Requires -Version 5.1
$ErrorActionPreference = "Stop"

Write-Host "==> Baxter dev machine bootstrap"

$python = Get-Command python -ErrorAction SilentlyContinue
if (-not $python) {
    $python = Get-Command python3 -ErrorAction SilentlyContinue
}
if (-not $python) {
    throw "Python 3.10+ is required. Install from https://www.python.org/downloads/"
}

$version = & $python.Source -c "import sys; print(f'{sys.version_info.major}.{sys.version_info.minor}')"
Write-Host "Found Python $version"

$uv = Get-Command uv -ErrorAction SilentlyContinue
if (-not $uv) {
    Write-Host "Installing uv..."
    irm https://astral.sh/uv/install.ps1 | iex
    $env:Path = "$env:USERPROFILE\.local\bin;$env:Path"
}

$uv = Get-Command uv -ErrorAction Stop
Write-Host "uv: $(& uv --version)"

Write-Host ""
Write-Host "Bootstrap complete."
Write-Host "Install Baxter plugins from the team marketplace; MCP deps sync on first server start."

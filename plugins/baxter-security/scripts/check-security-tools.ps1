#Requires -Version 5.1
# Verify baxter-security baseline and optional scanner installs (Windows).
$ErrorActionPreference = "Continue"

$ok = 0
$warn = 0
$fail = 0

function Check-Tool {
    param(
        [string]$Label,
        [string]$Status,
        [string]$Detail = ""
    )
    $suffix = if ($Detail) { " — $Detail" } else { "" }
    switch ($Status) {
        "ok"   { Write-Host "  [OK]   $Label$suffix"; $ok++ }
        "warn" { Write-Host "  [WARN] $Label$suffix"; $warn++ }
        "fail" { Write-Host "  [FAIL] $Label$suffix"; $fail++ }
    }
}

function Get-VersionLine {
    param([string]$Command)
    $cmd = Get-Command $Command -ErrorAction SilentlyContinue
    if (-not $cmd) { return "" }
    try {
        $out = & $cmd.Source --version 2>$null
        if (-not $out) { $out = & $cmd.Source version 2>$null }
        if ($out -is [array]) { return $out[0] }
        return "$out"
    } catch {
        return "installed"
    }
}

Write-Host "==> Baxter Security tool check (Windows)"
Write-Host ""

# --- Baseline ---
$python = Get-Command python -ErrorAction SilentlyContinue
if (-not $python) { $python = Get-Command python3 -ErrorAction SilentlyContinue }

if ($python) {
    $pyVer = & $python.Source -c "import sys; print('.'.join(map(str, sys.version_info[:3])))"
    $pyOk = & $python.Source -c "import sys; sys.exit(0 if sys.version_info >= (3, 10) else 1)"
    if ($LASTEXITCODE -eq 0) {
        Check-Tool "Python 3.10+" "ok" $pyVer
    } else {
        Check-Tool "Python 3.10+" "fail" "found $pyVer"
    }
} else {
    Check-Tool "Python 3.10+" "fail" "python not found"
}

$uv = Get-Command uv -ErrorAction SilentlyContinue
if ($uv) {
    Check-Tool "uv" "ok" (Get-VersionLine "uv")
} else {
    Check-Tool "uv" "warn" "not found — run ..\..\scripts\bootstrap-dev-machine.ps1 from BaxterPlugin root"
}

$nvdEnv = Join-Path $env:USERPROFILE ".config\nvd-api.env"
if (Test-Path $nvdEnv) {
    $content = Get-Content $nvdEnv -Raw -ErrorAction SilentlyContinue
    if ($content -match 'NVD_API_KEY=\S+') {
        Check-Tool "NVD API key file" "ok" $nvdEnv
    } else {
        Check-Tool "NVD API key file" "warn" "$nvdEnv exists but NVD_API_KEY may be empty"
    }
} else {
    Check-Tool "NVD API key file" "warn" "missing — run: node scripts\setup-nvd-env.mjs"
}

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$nvdScript = Join-Path $scriptDir "..\skills\nvd-cve-search\scripts\nvd_search.py"
if (Test-Path $nvdScript) {
    Check-Tool "nvd_search.py" "ok" $nvdScript
} else {
    Check-Tool "nvd_search.py" "fail" "not found at expected path"
}

Write-Host ""
Write-Host "--- Universal scanners (recommended) ---"

foreach ($tool in @("grype", "syft", "trivy")) {
    $cmd = Get-Command $tool -ErrorAction SilentlyContinue
    if ($cmd) {
        Check-Tool $tool "ok" (Get-VersionLine $tool)
    } else {
        if ($tool -eq "trivy") {
            Check-Tool $tool "warn" "not installed (optional if grype+syft present)"
        } else {
            Check-Tool $tool "warn" "not installed — see docs\INSTALL.md"
        }
    }
}

Write-Host ""
Write-Host "--- SAST ---"

$semgrep = Get-Command semgrep -ErrorAction SilentlyContinue
if ($semgrep) {
    Check-Tool "semgrep" "ok" (Get-VersionLine "semgrep")
} else {
    Check-Tool "semgrep" "warn" "not installed — uv tool install semgrep"
}

Write-Host ""
Write-Host "--- Ecosystem (optional) ---"

foreach ($tool in @("npm", "dotnet", "cargo", "go")) {
    $cmd = Get-Command $tool -ErrorAction SilentlyContinue
    if ($cmd) {
        Check-Tool $tool "ok" (Get-VersionLine $tool)
    } else {
        Check-Tool $tool "warn" "not on PATH (skip if stack unused)"
    }
}

foreach ($tool in @("pip-audit", "cargo-audit", "govulncheck")) {
    $cmd = Get-Command $tool -ErrorAction SilentlyContinue
    if ($cmd) {
        Check-Tool $tool "ok" (Get-VersionLine $tool)
    } else {
        $hint = switch ($tool) {
            "pip-audit" { "uv tool install pip-audit" }
            "cargo-audit" { "cargo install cargo-audit" }
            "govulncheck" { "go install golang.org/x/vuln/cmd/govulncheck@latest" }
        }
        Check-Tool $tool "warn" "not installed — $hint"
    }
}

Write-Host ""
Write-Host "==> Summary: $ok ok, $warn warnings, $fail failures"
if ($fail -gt 0) { exit 1 }
exit 0

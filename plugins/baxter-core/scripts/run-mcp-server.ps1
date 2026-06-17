#Requires -Version 5.1
param(
    [Parameter(Mandatory = $true, Position = 0)]
    [string]$ServerName
)

$ErrorActionPreference = "Stop"

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$PluginRoot = Split-Path -Parent $ScriptDir
$ServerDir = Join-Path $PluginRoot "mcp-servers" $ServerName

if (-not (Test-Path $ServerDir)) {
    Write-Error "MCP server directory not found: $ServerDir"
}

$uv = Get-Command uv -ErrorAction SilentlyContinue
if (-not $uv) {
    Write-Error "uv is not installed. Run scripts/bootstrap-dev-machine.ps1 first."
}

& uv --directory $ServerDir run server.py

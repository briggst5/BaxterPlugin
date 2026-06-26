# Baxter Core — Installation

Required baseline for all Baxter engineers. Covers machine bootstrap, MCP connectivity, and local plugin testing.

## Prerequisites

| Requirement | Purpose |
|-------------|---------|
| **Cursor** or **VS Code Copilot** with plugins enabled | Host IDE |
| **Python 3.10+** | Python MCP servers (`uv` runtime) |
| **uv** | Dependency sync on first MCP start |
| **baxter-core** plugin installed | From team marketplace or local symlink |

Node.js is **not** required for baxter-core (only for domain plugins with Node launchers).

## Step 1 — Install the plugin

**Marketplace:** Cursor → Settings → Plugins → install **Baxter Core** (org sets as **Required**).

**Local dev:**

```bash
mkdir -p ~/.cursor/plugins/local
ln -sfn "$(pwd)/plugins/baxter-core" ~/.cursor/plugins/local/baxter-core
```

Windows (PowerShell):

```powershell
New-Item -ItemType Directory -Force -Path "$env:USERPROFILE\.cursor\plugins\local"
cmd /c mklink /J "$env:USERPROFILE\.cursor\plugins\local\baxter-core" "$(Resolve-Path plugins\baxter-core)"
```

Reload Cursor after install.

## Step 2 — Machine bootstrap

From **BaxterPlugin repo root** (not the plugin subfolder):

### Linux

```bash
./scripts/bootstrap-dev-machine.sh
export PATH="$HOME/.local/bin:$PATH"
python3 --version   # 3.10+
uv --version
```

### macOS

```bash
./scripts/bootstrap-dev-machine.sh
export PATH="$HOME/.local/bin:$PATH"
```

Install Python from https://www.python.org/downloads/ or `brew install python` if missing.

### Windows (PowerShell)

```powershell
.\scripts\bootstrap-dev-machine.ps1
$env:Path = "$env:USERPROFILE\.local\bin;$env:Path"
python --version
uv --version
```

Enable **Add python.exe to PATH** when installing Python on Windows.

## Step 3 — Enable MCP (baxter-echo)

1. Cursor → **Settings** → **MCP**
2. Enable **baxter-echo** (connectivity smoke test)
3. First start runs `uv sync` in `mcp-servers/baxter-echo/` (30–60 seconds)

### Manual MCP test

**Linux / macOS:**

```bash
cd plugins/baxter-core
./scripts/run-mcp-server.sh baxter-echo
```

**Windows:**

```powershell
cd plugins\baxter-core
.\scripts\run-mcp-server.ps1 baxter-echo
```

Expected: server starts without `ModuleNotFoundError` or `uv: command not found`.

## Step 4 — Verify skills and agents

In Cursor chat:

- *"Apply baxter-standards to this change"*
- Invoke agent **baxter-code-reviewer** on a small diff

| Component | Name |
|-----------|------|
| Skills | `baxter-standards`, `baxter-mcp-setup` |
| Agent | `baxter-code-reviewer` |
| Rules (Cursor) | `baxter-engineering-standards`, plugin quality gates |

## Cursor vs Copilot

| Feature | Cursor | Copilot |
|---------|--------|---------|
| Skills / agents | Yes | Yes |
| `.mdc` rules | Yes | No — use `baxter-standards` skill |
| MCP | `.mcp.json` | `.mcp.copilot.json` |

## Troubleshooting

| Symptom | Fix |
|---------|-----|
| MCP red / failed to create client | Run `baxter-mcp-setup` skill in chat |
| `uv: command not found` | Re-run bootstrap; add local bin to PATH |
| `ModuleNotFoundError` on MCP start | Delete `~/.cache/uv` for baxter-echo project; retry |
| First MCP start slow | Normal — `uv` syncing dependencies |

## Related

- [README.md](../README.md) — full component list
- [Repo getting started](../../docs/getting-started.md)
- [CONTRIBUTING.md](../../CONTRIBUTING.md) — adding MCP servers

# Baxter Polarion — Installation

Standalone **Polarion MCP** server with bundled Linux, Windows, and macOS binaries — no local compile required.

## Prerequisites

| Requirement | Purpose |
|-------------|---------|
| **Node.js** | 20+ | Launcher script |
| **Polarion credentials** | URL, user, PAT or password |
| **baxter-product-owner** | Recommended for traceability skills/agents |

## Config file location

| OS | Path |
|----|------|
| Linux / macOS | `~/.config/polarion-mcp.env` |
| Windows | `%USERPROFILE%\.config\polarion-mcp.env` |

## Step 1 — Install plugin

1. Install **baxter-polarion** from marketplace (often Default On with product-owner).
2. Reload Cursor.

## Step 2 — Create Polarion env file

### Automated (all platforms)

```bash
node plugins/baxter-polarion/scripts/setup-polarion-env.mjs
```

Creates `~/.config/polarion-mcp.env` from example if missing.

### Linux / macOS — edit and secure

```bash
# Edit credentials
nano ~/.config/polarion-mcp.env
chmod 600 ~/.config/polarion-mcp.env
```

### Windows (PowerShell)

```powershell
notepad "$env:USERPROFILE\.config\polarion-mcp.env"
```

### Required variables

```bash
POLARION_URL=https://your-server/polarion
POLARION_USER=your_username
POLARION_PAT=your_personal_access_token
POLARION_PROJECT=YOUR_PROJECT
```

`POLARION_PASSWORD` works if PAT is not used. PAT is preferred when both are set.

See `mcp-servers/polarion-mcp.env.example` for optional TLS and session settings.

## Step 2b — Generate a Polarion Personal Access Token (PAT)

Use a **PAT** instead of your login password when possible. The MCP sends the PAT as a Bearer token to the Polarion REST API.

> **Expiry:** Baxter Polarion PATs are valid for **90 days** (org default). Plan to renew before expiry — expired PATs cause `401` errors with no warning in Cursor until MCP calls fail.

### Create a new PAT

1. Sign in to your Polarion instance (e.g. `https://polarion.hrc.corp/polarion` — use your org URL).
2. Open **My Account** / user settings:
   - Click your **user avatar** or **settings** icon (top bar).
   - Select **My Account** (or **Personal settings**).
3. Open the **Personal Access Token** tab (sometimes labeled **Access Tokens**).
4. Click **Generate** (or **Create token**).
5. Enter a **label** (e.g. `cursor-mcp-laptop`, `expires 2026-06`).
6. Set **lifetime** if prompted — maximum is typically **90 days** on Baxter instances.
7. **Copy the token immediately** — Polarion does not show the full value again after you close the dialog.
8. Paste into `POLARION_PAT=` in `polarion-mcp.env` (not into source control).

### Renew before expiry

| When | Action |
|------|--------|
| ~2 weeks before expiry | Generate a new PAT, update `polarion-mcp.env`, reload Cursor MCP |
| After expiry | MCP auth fails — regenerate PAT and update env file |
| Token compromised | Revoke old token in Polarion → generate new PAT |

In Polarion you can **renew** an existing token from the same Personal Access Token screen (Siemens default renewed lifetime is also **90 days**).

### If PAT menu is missing

Personal Access Tokens may be disabled by your Polarion admin. Contact your Polarion administrator to enable PATs, or use `POLARION_PASSWORD` temporarily (less ideal for automation).

**Reference:** [Siemens Polarion authentication — PAT](https://developer.siemens.com/polarion/authentication.html)

### Minimum access

The PAT must allow REST API access for the projects you use with MCP (read work items, documents, links; write only if you use tools that update Polarion). Use the minimum scope your workflow needs.


## Step 3 — Enable Polarion MCP

1. Cursor → **Settings** → **MCP**
2. Enable **polarion-mcp**
3. Server starts from bundled binary:
   - Linux: `bin/linux-x64/polarion-mcp`
   - Windows: `bin/win-x64/polarion-mcp.exe`
   - macOS (Intel): `bin/osx-x64/polarion-mcp`
   - macOS (Apple Silicon): `bin/osx-arm64/polarion-mcp`

### Manual launcher test

```bash
node plugins/baxter-polarion/scripts/launch-polarion-mcp.mjs
```

## Step 4 — Verify

In Cursor chat (with baxter-product-owner skills):

- *"List Polarion requirements in project X"*
- Invoke **traceability-auditor** agent for link audits

## WSL2

Run Polarion MCP from the same environment as Cursor (WSL or Windows native). Store `polarion-mcp.env` in that environment's home directory.

## Troubleshooting

| Symptom | Fix |
|---------|-----|
| Binary not found | Reinstall plugin; confirm `bin/linux-x64`, `bin/win-x64`, `bin/osx-x64`, or `bin/osx-arm64` present |
| TLS / certificate errors | Set `POLARION_TLS_CA_FILE` or consult IT for corp Polarion CA |
| 401 auth | PAT expired (90-day lifetime) — generate new PAT; update `polarion-mcp.env`; see [Generate PAT](#step-2b--generate-a-polarion-personal-access-token-pat) |
| Tools not in chat | Enable MCP server; reload Cursor |

## Maintainers

```bash
./scripts/sync-polarion-dotnet.sh      # sync upstream source
./scripts/build-polarion-binaries.sh   # rebuild bin/{linux-x64,win-x64,osx-x64,osx-arm64}
```

Commit updated binaries so users never compile locally.

## Related

- [README.md](../README.md)
- [Product Owner INSTALL](../../baxter-product-owner/docs/INSTALL.md)
- [OPERATIONS.md](../mcp-servers/docs/OPERATIONS.md)

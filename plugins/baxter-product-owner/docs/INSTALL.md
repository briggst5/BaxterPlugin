# Baxter Product Owner — Installation

FutureState SAFe plugin with Azure DevOps MCP. Pair with **baxter-polarion** for Polarion-backed skills.

## Prerequisites

| Requirement | Version | Purpose |
|-------------|---------|---------|
| **baxter-core** | Latest | Baseline standards |
| **Node.js** | 20+ | ADO MCP launcher (`npx @azure-devops/mcp`) |
| **Azure DevOps PAT** | Read/write work items, wiki | Stored in env file |
| **baxter-polarion** | Optional but recommended | Polarion skills and traceability agents |

## Config file location

| OS | Path |
|----|------|
| Linux / macOS | `~/.config/azure-devops-mcp.env` |
| Windows | `%USERPROFILE%\.config\azure-devops-mcp.env` |

## Step 1 — Install plugins

1. Install **baxter-product-owner** from marketplace.
2. Install **baxter-polarion** if you use Polarion requirements or traceability agents. Generate a Polarion PAT — [steps](../../baxter-polarion/docs/INSTALL.md#step-2b--generate-a-polarion-personal-access-token-pat) (expires after **90 days**).
3. Reload Cursor.

## Step 2 — Create Azure DevOps env file

### Linux / macOS

```bash
cd plugins/baxter-product-owner
cp azure-devops-mcp.env.example ~/.config/azure-devops-mcp.env
chmod 600 ~/.config/azure-devops-mcp.env
# Edit: ADO_ORG, AZURE_DEVOPS_EXT_PAT (or ADO_PAT)
```

### Windows (PowerShell)

```powershell
cd plugins\baxter-product-owner
$configDir = "$env:USERPROFILE\.config"
New-Item -ItemType Directory -Force -Path $configDir
Copy-Item azure-devops-mcp.env.example "$configDir\azure-devops-mcp.env"
notepad "$configDir\azure-devops-mcp.env"
```

### Required variables

```bash
ADO_ORG=FLC-NPD
ADO_AUTH=pat
AZURE_DEVOPS_EXT_PAT=your_pat_here
```

Accepted PAT variable names: `AZURE_DEVOPS_EXT_PAT`, `ADO_PAT`, `ADO_MCP_AUTH_TOKEN`, `PERSONAL_ACCESS_TOKEN`.

Create PAT in Azure DevOps → User settings → Personal access tokens. Scope: Work Items (read/write), Wiki (read), Project and team (read).

## Step 3 — Enable Azure DevOps MCP

1. Cursor → **Settings** → **MCP**
2. Enable the Azure DevOps MCP server (from baxter-product-owner)
3. First start runs `npx @azure-devops/mcp` — requires network access

### Manual launcher test

**Linux / macOS:**

```bash
node plugins/baxter-product-owner/scripts/launch-azure-devops-mcp.mjs
```

**Windows:**

```powershell
node plugins\baxter-product-owner\scripts\launch-azure-devops-mcp.mjs
```

## Step 4 — Wiki sync (optional)

Refresh local process docs from Platform.wiki:

```bash
python3 plugins/baxter-product-owner/scripts/sync_wiki.py
```

Requires valid PAT in `azure-devops-mcp.env`. Or invoke the `wiki-sync` skill in chat.

## Step 5 — Verify

In Cursor chat:

- *"List my team's active stories"* (ADO MCP)
- *"Run backlog readiness audit"* (`backlog-readiness-audit` skill)
- *"Navigate SAFe process for story DoR"* (`safe-process-navigator` skill)

## WSL2

Store `azure-devops-mcp.env` in the Linux home (`~/.config/`) when running Cursor against WSL, not the Windows profile.

## Troubleshooting

| Symptom | Fix |
|---------|-----|
| `ADO_ORG is missing` | Set `ADO_ORG` in env file |
| `Azure DevOps MCP is not configured` | Create env file at correct path for your OS |
| `npx` / Node errors | Install Node 20+; check corporate proxy |
| 401 / auth failed | PAT expired or insufficient scopes — regenerate PAT |
| Polarion tools missing | Install and enable **baxter-polarion** |

## Related

- [README.md](../README.md) — skills and agents catalog
- [Polarion INSTALL](../../baxter-polarion/docs/INSTALL.md)
- [Getting started](../../../docs/getting-started.md)

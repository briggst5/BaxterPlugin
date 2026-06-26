# Baxter GQP — Installation

GQP Knowledge MCP for compliance Q&A with verified GQP/GQT citations. Authentication uses **Baxter Entra sign-in** (device code) — no PAT in env file for typical users.

## Prerequisites

| Requirement | Purpose |
|-------------|---------|
| **baxter-gqp** plugin | From marketplace |
| **Baxter Entra account** | Access to GQP search backend |
| **Node.js** | 20+ optional — launcher uses bundled binary |

Pair with **baxter-product-owner** when answers must also trace to Polarion or ADO.

## Config file location

| OS | Path |
|----|------|
| Linux / macOS | `~/.config/gqp-mcp.env` |
| Windows | `%USERPROFILE%\.config\gqp-mcp.env` |

Non-secret settings only. **Do not store API keys in the file** for standard users.

## Step 1 — Install plugin

1. Install **baxter-gqp** from marketplace.
2. Reload Cursor.

## Step 2 — Enable MCP

1. Cursor → **Settings** → **MCP**
2. Enable **gqp-knowledge**
3. On **first use**, complete **device code sign-in** (check MCP logs for URL and code)

No terminal required for most users. Launcher creates `gqp-mcp.env` automatically.

### Optional manual env setup

```bash
node plugins/baxter-gqp/scripts/setup-gqp-env.mjs
```

## Step 3 — Authenticate (if prompted)

### Linux / macOS

```bash
plugins/baxter-gqp/bin/linux-x64/gqp-mcp authenticate --device-code
plugins/baxter-gqp/bin/linux-x64/gqp-mcp check-auth
```

### Windows (PowerShell)

```powershell
plugins\baxter-gqp\bin\win-x64\gqp-mcp.exe authenticate --device-code
plugins\baxter-gqp\bin\win-x64\gqp-mcp.exe check-auth
```

**Alternative:** `az login` if you already use Azure CLI with the right tenant.

## Step 4 — Verify

In Cursor chat:

- *"What V&V does GQP require for software changes?"*
- *"Verify GQP citations in this design section"*
- Invoke **gqp-compliance-reviewer** agent before gate reviews

## Maintainer / Key Vault setup

Maintainers may set `GQP_KEYVAULT_NAME` in `gqp-mcp.env` for Key Vault-backed API keys. See GQP repo `docs/keyvault-setup.md`.

## Troubleshooting

| Symptom | Fix |
|---------|-----|
| Sign-in loop | Run `authenticate --device-code` manually |
| MCP won't start | Confirm binary exists for your OS under `bin/` |
| No search results | Check Entra RBAC for Azure Search / OpenAI access |
| Auth works in CLI but not MCP | Reload Cursor; check MCP logs |

## Related

- [README.md](../README.md)
- [Getting started](../../../docs/getting-started.md)

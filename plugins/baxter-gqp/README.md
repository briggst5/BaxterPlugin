# Baxter GQP Knowledge Plugin

Standalone Cursor/Copilot plugin for the **GQP Knowledge MCP** ù hybrid RAG search over Baxter GQP/GQT documents, plus skills and rules for **compliance Q&A with verified citations**.

This plugin is **not** related to Polarion or other Baxter MCP plugins. It ships its own `gqp-mcp` .NET binary.

Pair with **`baxter-product-owner`** when answers must also trace to Polarion requirements or ADO delivery items.

## User setup

1. Install `baxter-gqp` from the Baxter Team Marketplace.
2. Enable **gqp-knowledge** in Cursor ? Settings ? MCP.
3. On first use, complete **Baxter sign-in** via device code (shown in MCP logs) ù no custom IT Entra app required.

No terminal commands required. The launcher creates `~/.config/gqp-mcp.env` and handles sign-in automatically. Alternatively, run `az login` once if you already use Azure CLI.

## Compliance features

| Component | Purpose |
|-----------|---------|
| `gqp-compliance-advisor` skill | Route compliance questions to the right MCP tools and format cited answers |
| `gqp-compliance-citations` rule | Require GQP/GQT doc_id, revision, and section citations; flag unsupported claims |
| `gqp-compliance-reviewer` agent | Audit plans and documents for citation accuracy before gate reviews |

Example prompts:

- "What V&V does GQP-1234 require for software?"
- "Verify the GQP citations in this design plan"
- "What deliverables are required if we change the sterilization method?"

## Configuration

`~/.config/gqp-mcp.env` - non-secret settings only. End users authenticate with Entra RBAC to Azure Search and OpenAI; maintainers may set `GQP_KEYVAULT_NAME=kv-flc-copilot` to load API keys from Key Vault.

See the GQP repo [keyvault-setup.md](https://github.com/briggst5/GQP/blob/main/docs/keyvault-setup.md) (or local `docs/keyvault-setup.md` in the GQP project).

## MCP tools

- `search_gqp_documents`
- `find_testing_requirements`
- `find_risk_mitigations`
- `find_security_tasks`
- `find_required_deliverables`
- `assess_change_impact`
- `build_project_plan`

## Troubleshooting

Manual config (if auto-setup fails):

```bash
node scripts/setup-gqp-env.mjs
```

Re-authenticate:

```bash
bin/linux-x64/gqp-mcp authenticate --device-code
# Windows: bin\win-x64\gqp-mcp.exe authenticate --device-code
```

Check auth status:

```bash
bin/linux-x64/gqp-mcp check-auth
```

## Maintainer: sync from GQP repo

```bash
./scripts/sync-gqp-dotnet.sh
# Or: SOURCE_PATH=/path/to/GQP ./scripts/sync-gqp-dotnet.sh
# Or: SOURCE_REPO=https://github.com/briggst5/GQP.git ./scripts/sync-gqp-dotnet.sh
```

Syncs `mcp-servers/dotnet/` from the GQP repo, rebuilds binaries, and runs plugin validation.

Commit updated `mcp-servers/` and `bin/` artifacts after source changes.

Source of truth for MCP logic: GQP repo `mcp-servers/dotnet/`.

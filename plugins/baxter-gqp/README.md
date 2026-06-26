# Baxter GQP Knowledge Plugin

Standalone plugin for **GQP Knowledge MCP** — hybrid RAG search over Baxter GQP/GQT documents, plus skills and rules for **compliance Q&A with verified citations**.

| | |
|--|--|
| **Audience** | Quality, regulatory, engineering leads, compliance reviewers |
| **Install guide** | [docs/INSTALL.md](docs/INSTALL.md) (Linux, macOS, Windows) |
| **Not related to** | Polarion MCP — ships its own `gqp-mcp` .NET binary |

## Quick start

1. Install **baxter-gqp** from marketplace.
2. Enable **gqp-knowledge** in Cursor → Settings → MCP.
3. Complete the Baxter **browser sign-in** on first use (a browser window opens automatically).
4. Try: *"What deliverables are required if we change the sterilization method?"*

Pair with **baxter-product-owner** when answers must also trace to Polarion requirements or ADO delivery items.

## What's included

### Skills

| Skill | Purpose |
|-------|---------|
| `gqp-compliance-advisor` | Route compliance questions to MCP tools; format cited answers |

### Agents

| Agent | Purpose |
|-------|---------|
| `gqp-compliance-reviewer` | Audit plans and documents for citation accuracy before gate reviews |

### Rules (Cursor only)

| Rule | Purpose |
|------|---------|
| `gqp-compliance-citations` | Require GQP/GQT doc_id, revision, section citations; flag unsupported claims |

### MCP tools

| Tool | Purpose |
|------|---------|
| `search_gqp_documents` | General GQP/GQT search |
| `find_testing_requirements` | V&V requirements |
| `find_risk_mitigations` | Risk-related tasks |
| `find_security_tasks` | Security activities |
| `find_required_deliverables` | Deliverable lists |
| `assess_change_impact` | Change impact on quality system |
| `build_project_plan` | Project planning support |

## Authentication

Authentication mirrors the Baxter CFCT tool: the MCP signs in to **Entra** using your **ambient Baxter identity** (Windows SSO/WAM, Visual Studio, or the Azure CLI) and reads the backend API keys from **Azure Key Vault** (`kv-flc-copilot`). There is no custom IT Entra app, no client secret, and no per-user consent prompt — access is governed by your Azure RBAC on the vault (`Key Vault Secrets User`).

On first use a browser window opens for sign-in. After the first successful read, keys are cached locally (`~/.config/gqp-mcp.secrets.json`, owner-only) so later sessions need no sign-in. If backend access later fails (e.g. rotated keys), the cache is flushed and sign-in is retried automatically.

Optional config: `~/.config/gqp-mcp.env` (non-secret settings only).

## Binaries

| OS | Path |
|----|------|
| Linux | `bin/linux-x64/gqp-mcp` |
| macOS (Apple Silicon) | `bin/osx-arm64/gqp-mcp` |
| macOS (Intel) | `bin/osx-x64/gqp-mcp` |
| Windows | `bin/win-x64/gqp-mcp.exe` |

## Example prompts

- "What V&V does GQP-1234 require for software?"
- "Verify the GQP citations in this design plan"
- "What deliverables are required if we change the sterilization method?"

## Maintainers

```bash
./scripts/sync-gqp-dotnet.sh
# Or: SOURCE_PATH=/path/to/GQP ./scripts/sync-gqp-dotnet.sh
```

Syncs `mcp-servers/dotnet/` from GQP repo, rebuilds binaries, runs validation. Commit updated `mcp-servers/` and `bin/` artifacts.

Source of truth: GQP repo `mcp-servers/dotnet/`.

## Changelog

See [CHANGELOG.md](CHANGELOG.md).

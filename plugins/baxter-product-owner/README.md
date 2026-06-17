# Baxter Product Owner

FutureState SAFe skills, rules, and agents for Product Owners, RTEs, and agile team members — vendored from [ProductOwner](https://github.com/briggst5/ProductOwner).

## Contents

- **26 skills** in `skills/` (PO, RTE, ceremonies, ADO/Polarion stewards)
- **6 rules** in `rules/` (Cursor only)
- **6 agents** in `agents/` (PO Coach, RTE Coordinator, etc.)
- **Process docs** in `docs/` and wiki exports in `reference/`
- **MCP:** Azure DevOps only (this plugin)

## Related plugins

| Plugin | Provides |
|--------|----------|
| [**baxter-polarion**](../baxter-polarion/) | Polarion MCP server (install alongside this plugin) |
| **baxter-core** | Org-wide standards |

Skills such as `polarion-requirements-steward` and agents like `traceability-auditor` expect **baxter-polarion** to be installed and enabled.

## Azure DevOps MCP

Uses `scripts/launch-azure-devops-mcp.mjs` (Node + `npx @azure-devops/mcp`).

Create `~/.config/azure-devops-mcp.env` — see [ProductOwner INSTALL](https://github.com/briggst5/ProductOwner/blob/main/INSTALL.md) (`ADO_ORG=FLC-NPD`, base64 PAT, etc.).

Requires **Node.js 20+**.

## Wiki refresh

```bash
python3 scripts/sync_wiki.py
```

Or invoke the `wiki-sync` skill in chat.

## Cursor distribution

Optional plugin — set **Default On** for PO/RTE/agile SCIM groups. Set **baxter-polarion** Default On for the same groups.

## Agents

| Agent | Use when |
|-------|----------|
| `po-coach` | Story refinement, prioritization |
| `rte-coordinator` | PI Planning, ART Sync, dependencies |
| `traceability-auditor` | Polarion ↔ ADO link audits |
| `readiness-gatekeeper` | DoR gates before planning |
| `metrics-analyst` | PI metrics and I&A prep |
| `stakeholder-brief-writer` | Leadership status updates |

## Updating from ProductOwner

Copy changed files from `../ProductOwner` into this plugin tree (skills, rules, docs, reference, scripts — not Polarion MCP). Bump `version` in both manifest files. See [CONTRIBUTING.md](../../CONTRIBUTING.md).

Polarion MCP updates go to [**baxter-polarion**](../baxter-polarion/README.md).

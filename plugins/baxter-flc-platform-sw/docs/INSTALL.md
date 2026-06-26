# Baxter FLC Platform SW — Installation

Code review and PR review skills with organization checklists. Optional Azure DevOps CLI for PR workflows and Polarion for IEC 62304 requirements.

## Prerequisites

| Requirement | Purpose |
|-------------|---------|
| **baxter-core** | Baseline standards |
| **baxter-flc-platform-sw** | This plugin |
| **Azure CLI + azure-devops extension** | PR review skill (`az repos pr`) |
| **baxter-polarion** | IEC 62304 requirements rule (Polarion MCP) |

## Step 1 — Install plugin

Install **baxter-flc-platform-sw** from marketplace. Reload Cursor.

## Step 2 — Azure DevOps CLI (for PR review)

Required when using `pr-review` skill against ADO pull requests.

### Linux / macOS

```bash
# Azure CLI — install per https://learn.microsoft.com/cli/azure/install-azure-cli
az extension add --name azure-devops
az devops login --organization https://dev.azure.com/FLC-NPD
az devops configure --defaults organization=https://dev.azure.com/FLC-NPD project=<your-project>
```

### Windows (PowerShell)

```powershell
az extension add --name azure-devops
az devops login --organization https://dev.azure.com/FLC-NPD
az devops configure --defaults organization=https://dev.azure.com/FLC-NPD project=<your-project>
```

Alternatively use PAT in env — same as [baxter-product-owner ADO setup](../../baxter-product-owner/docs/INSTALL.md).

## Step 3 — Polarion (IEC 62304 only)

If you use Class C requirements workflows:

1. Install **baxter-polarion**
2. Configure [polarion-mcp.env](../../baxter-polarion/docs/INSTALL.md)
3. Enable Polarion MCP

## Step 4 — IDE-specific setup (optional)

| IDE | Guide |
|-----|-------|
| Cursor (plugin) | You're here — skills auto-load from plugin |
| Cursor (project copy) | [docs/cursor-setup.md](../docs/cursor-setup.md) |
| Copilot | [docs/copilot-setup.md](../docs/copilot-setup.md) |
| Continue | [docs/continue-setup.md](../docs/continue-setup.md) |

Plugin install replaces per-project `.cursor/skills` copy for most users.

## Step 5 — Verify

In Cursor chat:

- *"Code review this file using org checklists"*
- *"PR review for ADO PR #123 in repo X"* (requires Azure CLI)
- Open a `.kt` file and ask for Kotlin-specific review (kotlin-review rule)

## Scripts (advanced)

| Script | Purpose |
|--------|---------|
| `scripts/fetch_pr_diff.py` | Fetch PR diff for automation |
| `scripts/post_pr_comment.py` | Post review comments |
| `scripts/run_ai_review.py` | Batch AI review runner |

Run from repo with Python 3.10+ and dependencies per script headers.

## Troubleshooting

| Symptom | Fix |
|---------|-----|
| PR review can't fetch diff | `az login`; confirm azure-devops extension and project default |
| Checklists not referenced | Invoke `code-review` or `pr-review` skill explicitly |
| Polarion IEC workflow fails | Install baxter-polarion; enable MCP |

## Related

- [README.md](../README.md)
- [checklists/README.md](../checklists/README.md)
- [Integration strategy](../docs/INTEGRATION_STRATEGY.md)

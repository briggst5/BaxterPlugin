# BaxterPlugin

Enterprise team marketplace for Baxter engineering: shared MCP servers, skills, rules, and agents for **Cursor** and **VS Code GitHub Copilot**.

## Plugins

| Plugin | Cursor install mode | Description |
|--------|---------------------|-------------|
| [baxter-core](plugins/baxter-core/) | **Required** | Baseline standards, core skills, example MCP, shared agents |
| [baxter-security](plugins/baxter-security/) | Default On/Off by SCIM group | Security review skill and agent |
| [baxter-product-owner](plugins/baxter-product-owner/) | Default On/Off by SCIM group | FutureState SAFe PO/RTE skills, Azure DevOps MCP |
| [baxter-polarion](plugins/baxter-polarion/) | Default On/Off by SCIM group | Standalone Polarion MCP (pair with product-owner) |

## Quick start (local dev)

```bash
# Validate marketplace
./scripts/validate-plugin.sh

# Machine baseline (Python 3.10+, uv) — not installed by plugins
./scripts/bootstrap-dev-machine.sh

# Symlink for local Cursor testing
mkdir -p ~/.cursor/plugins/local
ln -sfn "$(pwd)/plugins/baxter-core" ~/.cursor/plugins/local/baxter-core
ln -sfn "$(pwd)/plugins/baxter-security" ~/.cursor/plugins/local/baxter-security
```

Reload Cursor and enable the `baxter-echo` MCP server in Settings → MCP.

## Cursor Team Marketplace rollout

1. Push this repo to GitHub (for example `briggst5/BaxterPlugin`).
2. Cursor Dashboard → Settings → Plugins → Team Marketplaces → **Import from Repo**.
3. Set `baxter-core` to **Required**; set optional plugins per SCIM group.
4. Enable **Auto Refresh**.

### Private marketplace readiness checklist

- Use a dedicated repo (`BaxterPlugin`) with branch protection on `main`.
- Restrict who can edit plugin install modes in Cursor dashboard.
- Keep optional plugins (`baxter-security`, `baxter-product-owner`, `baxter-polarion`) mapped to the right SCIM groups.
- Enable Auto Refresh so merged PRs propagate without manual republish.
- Run `./scripts/validate-plugin.sh` in CI on every PR before merge.

## VS Code Copilot rollout

1. Enable `chat.plugins.enabled` in org or user settings.
2. Add marketplace:

```json
"chat.plugins.marketplaces": [
  "your-org/BaxterPlugin"
]
```

3. Copy [templates/copilot-settings.json](templates/copilot-settings.json) into project templates.

**Note:** Cursor `.mdc` rules are not available in Copilot. Use the `baxter-standards` skill for equivalent guidance.

## Adding skills, MCP, rules, and agents

Assets are **copied into this repo** and committed — they do not sync from upstream at install time.

See [CONTRIBUTING.md](CONTRIBUTING.md) for where to put files and the release checklist.

```bash
# Example: copy a skill and MCP from local upstream repos
cp -r ../my-skills-repo/skills/foo plugins/baxter-core/skills/foo
cp -r ../my-mcp-repo plugins/baxter-core/mcp-servers/foo
./scripts/add-mcp-entry.sh baxter-core foo
./scripts/validate-plugin.sh
```

## Python MCP prerequisites

Plugin install delivers MCP **config and source**, not Python packages. See [plugins/baxter-core/README.md](plugins/baxter-core/README.md).

## Repository layout

```
.cursor-plugin/marketplace.json   # marketplace index
plugins/baxter-core/              # required core plugin
plugins/baxter-security/          # optional domain plugin
scripts/                          # bootstrap, validate, add-mcp-entry
CONTRIBUTING.md                   # how to copy assets into plugins
templates/                        # Copilot workspace settings
```

## License

MIT — see [LICENSE](LICENSE).

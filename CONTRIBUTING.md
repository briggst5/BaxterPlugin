# Contributing to BaxterPlugin

Skills, MCP servers, rules, and agents **live in this repo**. They are copied (vendored) here and committed — not fetched at install time or synced from upstream on each release.

When you update an asset in its source repo, copy the changes into BaxterPlugin and open a PR.

## Where things go

| Asset | Location | Notes |
|-------|----------|--------|
| Skill | `plugins/<plugin>/skills/<skill-name>/SKILL.md` | Include `name` and `description` frontmatter; kebab-case names |
| Agent | `plugins/<plugin>/agents/<agent-name>.md` | Shared by Cursor and Copilot |
| Rule (Cursor only) | `plugins/<plugin>/rules/<rule-name>.mdc` | Use a skill mirror for Copilot parity when needed |
| Python MCP | `plugins/<plugin>/mcp-servers/<server-name>/` | `pyproject.toml` + entrypoint for uv/python launchers |
| Polarion MCP (.NET) | `plugins/baxter-polarion/mcp-servers/` + `plugins/baxter-polarion/bin/<rid>/` | Bundle prebuilt `polarion-mcp` binaries; no end-user local compile |

**Plugins:** put shared assets in `baxter-core`; domain-specific assets in optional plugins like `baxter-security`.

## Adding a skill

1. Copy the skill directory into the target plugin:

```bash
cp -r /path/to/upstream/skills/my-skill plugins/baxter-core/skills/my-skill
```

2. Confirm `SKILL.md` frontmatter has `name` and `description` (plain kebab-case, no org prefix).
3. Run `./scripts/validate-plugin.sh`.
4. Bump `version` in the plugin's `.cursor-plugin/plugin.json` and `.plugin/plugin.json`.
5. Update the plugin `CHANGELOG.md`.

> Adding or removing a whole plugin? Update both marketplace indexes:
> `.cursor-plugin/marketplace.json` (Cursor) and `.github/plugin/marketplace.json` (VS Code Copilot).

Skills are auto-discovered from `skills/*/SKILL.md` — no manifest edit needed unless you use custom paths.

## Adding a Python MCP server

1. Copy the MCP project into the plugin:

```bash
cp -r /path/to/upstream-mcp-repo plugins/baxter-core/mcp-servers/my-server
```

2. Ensure the server has `pyproject.toml` (and ideally `uv.lock`) plus a `server.py` entrypoint compatible with:

```bash
./scripts/run-mcp-server.sh my-server
```

3. Register the server in MCP config (both files):

```bash
./scripts/add-mcp-entry.sh baxter-core my-server
```

Or add entries manually to `plugins/baxter-core/.mcp.json` and `.mcp.copilot.json`.

4. Add `mcp-servers/my-server/README.md` documenting auth env vars.
5. Run `./scripts/validate-plugin.sh`.
6. Bump plugin version and changelog.

## Adding an agent or rule

```bash
cp /path/to/agent.md plugins/baxter-core/agents/my-agent.md
cp /path/to/rule.mdc plugins/baxter-core/rules/my-rule.mdc
```

Validate, bump version, update changelog.

## Updating from an upstream repo (example: ProductOwner)

When the source repo changes, copy the updated directories into the plugin and commit:

```bash
PO=../ProductOwner
PLUGIN=plugins/baxter-product-owner

cp -r "$PO/.cursor/skills/"* "$PLUGIN/skills/"
cp -r "$PO/.cursor/rules/"* "$PLUGIN/rules/"
cp -r "$PO/docs/"* "$PLUGIN/docs/"
cp -r "$PO/scripts/"* "$PLUGIN/scripts/"
cp -r "$PO/reference/"* "$PLUGIN/reference/"

# Polarion MCP → baxter-polarion plugin only
./scripts/sync-polarion-dotnet.sh

./scripts/validate-plugin.sh
```

- [ ] `./scripts/validate-plugin.sh` passes
- [ ] Plugin `version` bumped where contents changed
- [ ] `CHANGELOG.md` updated
- [ ] MCP README documents any new auth/env requirements
- [ ] PR reviewed; merge to default branch for Cursor Auto Refresh

## What plugin install does *not* do

- Install Python packages for Python MCP servers (`uv sync` runs lazily on first MCP start)
- Build Polarion MCP locally (baxter-polarion requires bundled binaries in `bin/linux-x64` and `bin/win-x64`)

See [README.md](README.md) for Enterprise rollout steps.

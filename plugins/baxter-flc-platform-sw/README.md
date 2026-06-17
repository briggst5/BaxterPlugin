# Baxter FLC Platform Software

Optional plugin for FLC platform software engineering — code review, PR review, Kotlin standards, and IEC 62304 Class C requirements workflows.

Vendored from [ai-skills](https://dev.azure.com/FLC-NPD/Proof%20Of%20Concept/_git/ai-skills) (Proof Of Concept project).

## Contents

| Component | Location |
|-----------|----------|
| **Skills** | `skills/code-review`, `skills/pr-review` |
| **Checklists** | `checklists/` (11 review frameworks) |
| **Rules** | `rules/` — foundation standards, Kotlin, IEC 62304 |
| **Scripts** | `scripts/` — PR diff fetch, AI review automation |
| **Docs** | `docs/` — Cursor, Copilot, Continue setup guides |

## Skills

| Skill | Use when |
|-------|----------|
| `code-review` | Reviewing files, diffs, or individual code changes |
| `pr-review` | Reviewing PRs, branches, or merge readiness |

## Rules

| Rule | Scope |
|------|-------|
| `foundation-standards` | Always on — core review checklists |
| `kotlin-review` | `**/*.kt` — Kotlin-specific checklists |
| `iec62304-requirements` | Polarion requirements generation/review (Class C) |

## Prerequisites

**PR Review skill** requires Azure CLI with the Azure DevOps extension:

```bash
az extension add --name azure-devops
az devops login --organization https://dev.azure.com/FLC-NPD
az devops configure --defaults organization=https://dev.azure.com/FLC-NPD project=<your-project>
```

**IEC 62304 requirements** rule expects **baxter-polarion** to be installed for Polarion MCP access.

## Cursor distribution

Set to **Default On** for platform software engineering SCIM groups.

## Updating from ai-skills

When the upstream repo changes, copy updated directories into this plugin and commit:

```bash
SRC=/path/to/ai-skills
PLUGIN=plugins/baxter-flc-platform-sw

cp -r "$SRC/skills/code-review" "$SRC/skills/pr-review" "$PLUGIN/skills/"
cp -r "$SRC/checklists/"* "$PLUGIN/checklists/"
cp "$SRC/scripts/"* "$PLUGIN/scripts/"
cp "$SRC/templates/cursor-project-template/.cursor/rules/"* "$PLUGIN/rules/"
```

Re-apply path updates (`.agents/checklists/` → `checklists/`). Bump `version` in both manifest files. Run `./scripts/validate-plugin.sh`.

## Changelog

See [CHANGELOG.md](CHANGELOG.md).

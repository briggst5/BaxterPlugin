# Cursor IDE Integration

Cursor uses **Agent Skills** (`.cursor/skills/`) and **project rules** (`.cursor/rules/*.mdc`) for persistent AI guidance.

## Prerequisites

Clone the ai-skills repository into `.agents/` at your project root. See [Integration Strategy](INTEGRATION_STRATEGY.md#prerequisites).

## Install

Copy skills from the `.agents/` folder and rules from the foundation template.

### Windows (PowerShell)

```powershell
New-Item -ItemType Directory -Force -Path .cursor\skills | Out-Null
New-Item -ItemType Directory -Force -Path .cursor\rules | Out-Null
Copy-Item -Path .\.agents\skills\* -Destination .cursor\skills\ -Recurse -Force
Copy-Item -Path .\.agents\templates\cursor-project-template\.cursor\rules\* -Destination .cursor\rules\ -Force
```

### Linux / macOS

```bash
mkdir -p .cursor/skills .cursor/rules
cp -r ./.agents/skills/* .cursor/skills/
cp ./.agents/templates/cursor-project-template/.cursor/rules/* .cursor/rules/
```

Or follow [cursor-project-template README](../templates/cursor-project-template/README.md).

## Agent Skills

Skills live in `.cursor/skills/<skill-name>/SKILL.md`. Cursor discovers them from the `description` in each file’s YAML frontmatter.

Included skills (from `.agents/skills/`):

| Skill | Use when |
|-------|----------|
| `code-review` | Reviewing files, diffs, or individual changes |
| `pr-review` | Reviewing PRs, branches, or merge readiness |

**Invocation:** Ask naturally (“review this file”, “is this PR ready to merge?”) or refer to the skill by name. The agent selects skills based on `description` trigger terms.

**Checklists:** Skills link to `.agents/checklists/...`. The agent should read those files during a review.

## Project rules

Rules are `.mdc` files in `.cursor/rules/` with YAML frontmatter:

| Field | Purpose |
|-------|---------|
| `description` | Shown in the rule picker |
| `alwaysApply: true` | Applies to every conversation |
| `globs` | Applies when matching files are in context (e.g. `**/*.kt`) |

The template provides:

- `foundation-standards.mdc` — always on; points to core `.agents/checklists/`
- `kotlin-review.mdc` — applies to `**/*.kt`; Kotlin-specific checklists
- `iec62304-requirements.mdc` — activates for Polarion requirements generation/review (Class C)

Add project-specific rules in your repo’s `.cursor/rules/` (do not edit files inside `.agents/`).

## Personal skills

Optional: place skills in `~/.cursor/skills/` for use across all projects. Do not use `~/.cursor/skills-cursor/` (reserved by Cursor).

## Updating the foundation

When you update the ai-skills clone in `.agents/`:

```bash
cd .agents
git pull origin main
cd ..
```

Re-copy skills into `.cursor/skills/` (see Install above). Re-copy rules only when release notes recommend it.

## Tips

- Be specific: “security review of the auth module” vs. “review this”
- Provide branch names and target branch for PR reviews
- For Kotlin, open a `.kt` file so `kotlin-review.mdc` applies

## Troubleshooting

**Skills not appearing**

- Confirm `.cursor/skills/<name>/SKILL.md` exists (re-run Install copy)
- Restart Cursor after first install

**Checklists not found**

- Confirm `.agents/` exists at the project root (`git clone <url> .agents`)
- Paths in skills use `.agents/checklists/...` (project root relative)

**Rules not applying**

- Check `.cursor/rules/*.mdc` frontmatter (`alwaysApply` or `globs`)
- For glob rules, ensure a matching file is in context

## Next steps

1. Complete [Integration Strategy](INTEGRATION_STRATEGY.md) setup (clone into `.agents/`)
2. Run Install commands above
3. Commit `.cursor/` to your project repository
4. Test with a code review and a PR review request

# Baxter FLC Platform Software

Optional plugin for **FLC platform software engineering** — code review, PR review, Kotlin standards, and IEC 62304 Class C requirements workflows.

| | |
|--|--|
| **Audience** | Platform software, embedded, Kotlin teams |
| **Install guide** | [docs/INSTALL.md](docs/INSTALL.md) (Linux, macOS, Windows) |
| **Upstream** | Vendored from [ai-skills](https://dev.azure.com/FLC-NPD/Proof%20Of%20Concept/_git/ai-skills) |

## Quick start

1. Install plugin from marketplace.
2. For PR reviews: install Azure CLI + `azure-devops` extension — see [INSTALL.md](docs/INSTALL.md).
3. Try: *"Review this diff using code-review checklists"* or *"PR review for my open PR"*.

## What's included

### Skills

| Skill | Use when |
|-------|----------|
| `code-review` | Reviewing files, diffs, or individual code changes |
| `pr-review` | Reviewing PRs, branches, or merge readiness |

### Checklists (11 frameworks)

Located in `checklists/` — see [checklists/README.md](checklists/README.md).

| Checklist | Focus |
|-----------|-------|
| `code-review-checklist.md` | General code review |
| `pr-review-checklist.md` | Pull request merge readiness |
| `class-c-code-checklist.md` | IEC 62304 Class C |
| Plus Kotlin, security, API, concurrency, etc. | Scoped reviews |

### Rules (Cursor only)

| Rule | Scope |
|------|-------|
| `foundation-standards` | Always on — points to checklists |
| `kotlin-review` | `**/*.kt` |
| `iec62304-requirements` | Polarion Class C requirements (needs **baxter-polarion**) |

### Scripts

| Script | Purpose |
|--------|---------|
| `fetch_pr_diff.py` | ADO PR diff fetch |
| `post_pr_comment.py` | Post review to PR |
| `run_ai_review.py` | Automated review batch |

### Docs

| Doc | Purpose |
|-----|---------|
| [cursor-setup.md](docs/cursor-setup.md) | Project-level Cursor install |
| [copilot-setup.md](docs/copilot-setup.md) | Copilot parity |
| [continue-setup.md](docs/continue-setup.md) | Continue IDE |
| [INTEGRATION_STRATEGY.md](docs/INTEGRATION_STRATEGY.md) | Upstream integration |

## Prerequisites

| Need | For |
|------|-----|
| Azure CLI + `azure-devops` extension | `pr-review` against ADO |
| **baxter-polarion** | IEC 62304 Polarion workflows |

## Example prompts

- "Run code-review on `src/foo.rs`"
- "PR review — is this branch ready to merge?"
- "Apply class-c-code-checklist to this module"

## Updating from ai-skills

```bash
SRC=/path/to/ai-skills
PLUGIN=plugins/baxter-flc-platform-sw
cp -r "$SRC/skills/code-review" "$SRC/skills/pr-review" "$PLUGIN/skills/"
cp -r "$SRC/checklists/"* "$PLUGIN/checklists/"
cp "$SRC/scripts/"* "$PLUGIN/scripts/"
```

Re-apply path fixes (`checklists/` not `.agents/checklists/`). Bump version; run `./scripts/validate-plugin.sh`.

## Changelog

See [CHANGELOG.md](CHANGELOG.md).

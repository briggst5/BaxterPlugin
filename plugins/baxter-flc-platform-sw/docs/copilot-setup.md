# GitHub Copilot Integration

Copilot reads agent skills and instructions from the `.agents/` folder after you clone the ai-skills foundation into your project.

## Prerequisites

Clone ai-skills into `.agents/` at your project root. See [Integration Strategy](INTEGRATION_STRATEGY.md#prerequisites).

```bash
git clone <your-ai-skills-repo-url> .agents
```

## Setup

No extra Copilot configuration is required for the foundation checklists and skills. Copilot discovers content under `.agents/`, including:

- `.agents/skills/*/SKILL.md` — code review and PR review workflows
- `.agents/checklists/*.md` — evaluation checklists

## Onboarding

When setting up a new machine or project workspace:

1. Clone your application repository.
2. Clone ai-skills into `.agents/` if the folder is not already present.

## Optional project instructions

You may add `.github/copilot-instructions.md` in your project for **project-specific** standards that extend (not replace) the shared foundation. Reference shared assets by path:

```markdown
Follow code review standards in `.agents/checklists/code-review-checklist.md`.
For PRs, also use `.agents/checklists/pr-review-checklist.md`.
```

Do not copy or modify checklist files from `.agents/`.

## Updating the foundation

See [Integration Strategy — Version Management](INTEGRATION_STRATEGY.md#updating-to-a-new-version).

## Related guides

- [Continue setup](continue-setup.md)
- [Cursor setup](cursor-setup.md)

# Integration Strategy: Clone to `.agents`

**This is the standard approach for integrating the ai-skills foundation into consumer projects.**

## Overview

Each project keeps a **clone** of the ai-skills repository in a folder named `.agents/` at the project root. This approach:

- Keeps checklists immutable and version-controlled in the foundation repository
- Works with private repositories using normal `git clone` authentication
- Provides a single source of truth teams can update on their own schedule
- Avoids Git submodule complexity
- Prevents skill and checklist drift when teams pin or pull explicit versions

## Prerequisites

From your **project root**, clone the ai-skills repository into `.agents/`:

```bash
git clone https://FLC-NPD@dev.azure.com/FLC-NPD/Proof%20Of%20Concept/_git/ai-skills .agents
```

## How It Works

### Step 1: Foundation folder

The clone creates:

- `.agents/` — a full copy of the ai-skills repository (with its own `.git` directory)
- Skills under `.agents/skills/`
- Checklists under `.agents/checklists/`

Reference foundation files from your project using paths under `.agents/`.

### Step 2: Configure AI tools

**For Copilot:**

GitHub Copilot reads the `.agents` folder automatically. See [copilot-setup.md](copilot-setup.md).

**For Continue** (`.continue/prompts/`):

See [continue-setup.md](continue-setup.md) for complete setup instructions.

**For Cursor** (`.cursor/skills/` and `.cursor/rules/`):

1. Copy skills: `cp -r ./.agents/skills/* .cursor/skills/`
2. Copy rules from `.agents/templates/cursor-project-template/.cursor/rules/`

See [cursor-setup.md](cursor-setup.md) for full install commands.

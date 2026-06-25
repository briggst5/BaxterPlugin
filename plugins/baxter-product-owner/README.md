# Baxter Product Owner

FutureState **SAFe** skills, rules, and agents for Product Owners, RTEs, and agile team members — plus **Azure DevOps MCP** for backlog, wiki, and work items.

| | |
|--|--|
| **Audience** | PO, RTE, Scrum Master, agile team members |
| **Install guide** | [docs/INSTALL.md](docs/INSTALL.md) (Linux, macOS, Windows) |
| **Upstream** | Vendored from [ProductOwner](https://github.com/briggst5/ProductOwner) |

## Quick start

1. Install **baxter-product-owner** (+ **baxter-polarion** for requirements/traceability).
2. Create `~/.config/azure-devops-mcp.env` — see [INSTALL.md](docs/INSTALL.md) or `azure-devops-mcp.env.example`.
3. Enable Azure DevOps MCP in Cursor Settings → MCP.
4. Try: *"Prepare sprint planning assist for my team"* or *"Run safe-process-navigator — what is our story format?"*

## Related plugins

| Plugin | Provides |
|--------|----------|
| **baxter-core** | Required baseline |
| [**baxter-polarion**](../baxter-polarion/) | Polarion MCP — required for `polarion-requirements-steward`, `traceability-auditor` |
| **baxter-gqp** | GQP compliance when answers need quality system citations |

## Skills (26)

### Backlog & refinement

| Skill | Use when |
|-------|----------|
| `acceptance-criteria-coach` | Rewrite AC into testable Given/When/Then |
| `backlog-readiness-audit` | Pre-refinement / pre-PI DoR gate |
| `backlog-refinement-facilitator` | Refinement pre-read pack |
| `feature-refinement` | Feature split/merge, PI prep |
| `story-breakdown` | Decompose Features into INVEST stories |
| `definition-of-done-check` | Pre-close DoD validation |

### PI planning

| Skill | Use when |
|-------|----------|
| `pi-planning-prep` | RTE pre-PI readiness |
| `pi-planning-facilitator` | PI event materials |
| `pi-objectives-writer` | Draft PI Objectives |
| `pi-confidence-calculator` | Confidence vote synthesis |
| `capacity-planner` | Capacity vs load |
| `wsjf-prioritization` | Feature ranking |

### Ceremonies & flow

| Skill | Use when |
|-------|----------|
| `daily-standup-prep` | Personal standup brief from ADO |
| `sprint-planning-assist` | Sprint goal and overcommit flags |
| `scrum-of-scrums-brief` | ART Sync talking points |
| `inspect-adapt-synthesis` | I&A workshop synthesis |
| `solution-demo-prep` | System Demo script outline |
| `iteration-health-check` | Mid-PI / end-of-iteration health |

### Tracking & architecture

| Skill | Use when |
|-------|----------|
| `dependency-tracker` | Cross-team dependencies |
| `impediment-logger` | Structured impediments |
| `architectural-runway-review` | Enabler vs feature balance |

### ADO, Polarion, process

| Skill | Use when |
|-------|----------|
| `ado-work-item-steward` | Create/update ADO work items |
| `polarion-requirements-steward` | Polarion SRS, reviews, links |
| `wiki-sync` | Refresh `reference/wiki/` from Platform.wiki |
| `safe-process-navigator` | FutureState process questions |
| `release-notes-draft` | Release notes from completed work |

## Agents

| Agent | Use when |
|-------|----------|
| `po-coach` | Story refinement, prioritization |
| `rte-coordinator` | PI Planning, ART Sync, dependencies |
| `traceability-auditor` | Polarion ↔ ADO link audits |
| `readiness-gatekeeper` | DoR before planning |
| `metrics-analyst` | PI metrics and I&A prep |
| `stakeholder-brief-writer` | Leadership status updates |

## Rules (Cursor only)

Process boundaries: ADO vs Polarion, communication tone, no unapproved status changes, traceability standards, FutureState process. See `rules/` directory.

## Process documentation

| Resource | Location |
|----------|----------|
| Consolidated process summary | `docs/futurestate-process.md` |
| DoR / DoD checklists | `docs/dor-dod-checklists.md` |
| Story/feature templates | `docs/templates/` |
| Wiki exports (synced) | `reference/wiki/` |

## Azure DevOps MCP

Launcher: `scripts/launch-azure-devops-mcp.mjs`  
Config: `~/.config/azure-devops-mcp.env` — see [INSTALL.md](docs/INSTALL.md).

Requires **Node.js 20+**.

## Maintainers

Copy updates from upstream ProductOwner (skills, rules, docs, scripts — not Polarion MCP). Bump version in both manifest files. Polarion updates go to **baxter-polarion**.

See [CONTRIBUTING.md](../../CONTRIBUTING.md).

## Changelog

See [CHANGELOG.md](CHANGELOG.md).

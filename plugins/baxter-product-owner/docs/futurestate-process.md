# FutureState Agile Process (Summary)

Condensed from [Platform.wiki](https://dev.azure.com/FLC-NPD/FutureState/_wiki/wikis/Platform.wiki). Full page copies are synced locally to `reference/wiki/` (not in git — run `scripts/sync_wiki.py`). See `reference/wiki-index.md` after sync.

---

## Timeboxes

| Interval | Duration |
|----------|----------|
| Planning Interval (PI) | One quarter — 6 iterations |
| Iteration (Sprint) | Half month (days 1–15 or 16–last day of month) |

PI structure: 5 development iterations + 1 Innovation & Planning (IP) iteration.

---

## Work Item Hierarchy

```
Epic → Feature → User Story → Task
```

| Type | Story Points | Duration | Owner |
|------|-------------|----------|-------|
| Epic | > 100 | No fixed bound; may span project | Business Owner, Architect, Product Manager |
| Feature | 20–100 | Single PI | Product Manager, Product Owner |
| User Story | 1–20 | Single sprint | Product Owner |
| Task | 1–3 | Single sprint | Agile Team |

### Required fields (Azure DevOps)

**Epic / Feature:** Description, Technical Readiness

**User Story:** Description (As a… I want… so that…), Acceptance Criteria, Platform Decision and Rationale

**Task:** Description, Definition of Done ("I know that I am done when…"), Planned/Remaining/Completed story points

### Sizing smells

- Feature extending beyond one PI → split, reduce risk, fix priority mismatch
- User story not completable in one sprint → decompose further
- Task > individual capacity (~3–9 points per person per iteration, avg 5) → decompose

### SAFe Category field

Values: Capability, Enabler, Feature, Solution. See [wiki/agile-practices/safe-agile/safe-category.md](wiki/agile-practices/safe-agile/safe-category.md).

---

## Work Item States

Typical flow: **New → Active → Resolved → Closed**

Special states: **Waiting** (blocked), **Removed** (obsolete)

- Active items must be assigned to the current sprint; otherwise move to backlog
- Waiting requires a comment explaining the blocker and owner
- See [wiki/future-state/work-item-states.md](wiki/future-state/work-item-states.md)

---

## Definition of Ready / Done

Quick gate cards: **[dor-dod-checklists.md](dor-dod-checklists.md)**

## Standard Work Item Tags

Governance tags for backlog hygiene:

| Tag | Meaning |
|-----|---------|
| NeedsScope | All items — needs scoping |
| NeedsPO | Needs PO review for scope & priority |
| NeedsAC | Story needs acceptance criteria |
| NeedsDoD | Task needs Definition of Done |
| NeedsPI | Needs PI planning review |
| NeedSAFe | Needs SAFe fields updated |
| DevSecOps, HW, REG, RSK, SEC, SW, SYS, UX | Discipline assignment |
| Review* | Discipline review tasks (ReviewSW, ReviewUX, etc.) |

See [wiki/future-state/standard-work-item-tags.md](wiki/future-state/standard-work-item-tags.md).

---

## User Stories

- Format: `As a [user role], I want [goal] so that [benefit]`
- Must be **INVEST**: Independent, Negotiable, Valuable, Estimable, Small, Testable
- Acceptance criteria: Given/When/Then; created by agile team
- Every story maps to a feature; every feature to an epic
- Enabler stories follow same INVEST model
- See [wiki/agile-practices/guide-to-writing-user-stories.md](wiki/agile-practices/guide-to-writing-user-stories.md)

---

## Effort & Capacity

- Story points use Fibonacci: 1, 1, 2, 3, 5, 8, 13, 21, 34, 55
- Individual capacity: ~3–9 points per iteration (avg 5); never use as performance metric
- Team capacity = sum of individual capacities; do not over-plan
- Update Planned, Remaining, Completed on work items during sprint
- Reflect on estimates at sprint end

---

## SAFe Ceremonies (ART level)

| Event | Purpose |
|-------|---------|
| **PI Planning** (2 days) | Shared plan, dependencies, PI Objectives, program board, confidence vote |
| **IP Iteration** | Innovation, PI prep, education, hardening, demo prep |
| **Inspect & Adapt** | System demo, metrics, problem-solving, improvement backlog |
| **ART Sync (Scrum of Scrums)** | Cross-team impediments, dependencies, risks — facilitated by RTE |
| **PO Sync** | Feature scope, priorities, backlog refinement — PM + POs |
| **Coach Sync** | SM community, agile practice improvement — RTE + SMs |

Team-level ceremonies: Sprint Planning, Daily Stand-up, Sprint Review, Sprint Retrospective. See [wiki/agile-practices/safe-agile/agile-ceremonies-scrum.md](wiki/agile-practices/safe-agile/agile-ceremonies-scrum.md) and [wiki/agile-practices/safe-agile/agile-ceremonies-safe.md](wiki/agile-practices/safe-agile/agile-ceremonies-safe.md).

---

## Platform Governance

User stories include **Platform Decision and Rationale** — aligns delivery to platform vs product-specific work. Principle: *"It's Platform until it isn't."* See [wiki/future-state/platform-decision-matrix.md](wiki/future-state/platform-decision-matrix.md).

---

## Key Roles (ownership summary)

| Role | Focus |
|------|-------|
| Business Owner / PM | Epics, program backlog, feature prioritization |
| Product Owner | Stories, team backlog, acceptance criteria, demos |
| RTE | ART sync, PI execution, cross-team coordination |
| Scrum Master | Team ceremonies, impediment removal, process improvement |
| Agile Team | Tasks, estimation, delivery, Definition of Done |

See [wiki/future-state/work-item-ownership.md](wiki/future-state/work-item-ownership.md). RACI matrix: external SharePoint link in [wiki/agile-practices/safe-agile/safe-raci.md](wiki/agile-practices/safe-agile/safe-raci.md).

---

## Glossary highlights

- **PI Goal**: Stakeholder-level goal for a PI (product vision alignment)
- **PI Objective**: Team/ART objective from PI planning; SMART; assigned business value; reviewed at I&A
- **Shippable Product**: Meets Definition of Done; IEC 62304 alignment includes QA and documentation
- Full glossary: [wiki/agile-practices/agile-glossary.md](wiki/agile-practices/agile-glossary.md)

---
name: pi-planning-facilitator
description: Produces PI Planning meeting materials — agenda, breakout guides, program board outline, ROAM template, and final plan summary. Use when facilitating or preparing FutureState PI Planning.
---

# PI Planning Facilitator

Reference: `reference/wiki/agile-practices/safe-agile/agile-ceremonies-safe.md`

Prerequisite data: `pi-planning-prep`, `pi-objectives-writer`, `dependency-tracker`

## Day 1 materials

### Agenda
- Business context (PM/BO)
- Product/architecture vision
- Planning context & PI Goals
- Team breakouts: draft plans
- Draft program board
- ROAM risks initial pass

### Team breakout guide
- Review assigned Features (ready only)
- Draft team PI Objectives
- Identify dependencies — add to program board
- Capacity check per iteration (`capacity-planner`)
- Draft risks

## Day 2 materials

- Management review & problem-solving slot
- Final plan presentation template per team
- **Confidence vote** (`pi-confidence-calculator` template)
- PI Planning retro prompts

## Program board outline

| Feature | Team | Iteration | Dependency arrows | Milestone |

(Text/table representation if physical board not available)

## Final plan summary template

```
# PI Plan — [ART] — [PI name]

## Committed PI Objectives
| Team | Objective | BV | Features |

## Program dependencies
[from dependency-tracker]

## ROAM board
| Risk | R/O/A/M | Owner |

## Confidence vote
[from pi-confidence-calculator]

## Management issues resolved
- ...
```

Offer to pull live Features from ADO when project/team known.

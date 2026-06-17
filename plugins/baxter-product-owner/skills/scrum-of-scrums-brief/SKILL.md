---
name: scrum-of-scrums-brief
description: Generates ART Sync (Scrum of Scrums) talking points — blockers, dependencies, risks, and decisions needed. Use before or during ART Sync for RTE facilitation.
---

# Scrum of Scrums Brief

Ceremony: ART Sync — `reference/wiki/agile-practices/safe-agile/agile-ceremonies-safe.md`

## Inputs

- Teams in scope (user-provided or query ADO teams)
- Run `dependency-tracker` (summary) for cross-team deps
- Query Waiting items across teams (WIQL State = Waiting)
- PI Objectives progress if mid-PI (Features/stories closed vs planned)

## Output structure (15-min sync friendly)

```
## ART Sync Brief — [date]

### PI Objective pulse
- [Objective 1]: on track | at risk — [evidence]

### Cross-team dependencies (top 5)
| Consumer | Provider | Status | Ask |

### Program impediments
| Impediment | Team | Owner | Age | Escalation? |

### Risks emerged
- [Risk] — ROAM suggestion

### Decisions needed today
1. [Decision] — options: A / B — recommend: 

### Parking lot
- ...
```

## Facilitation tips

- Each SM: 2 min max per team in live session
- Decisions captured with owner + date
- Escalate items matching ART Escalations path to wiki process

Pair with `impediment-logger` for new blockers discovered in sync.

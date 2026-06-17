---
name: dependency-tracker
description: Tracks cross-team and cross-ART dependencies for FutureState — aging deps, escalation candidates, and dependency board snapshots. Use for PI planning, ART Sync, and RTE coordination.
---

# Dependency Tracker

Template: `docs/templates/dependency-record.md`

Escalation path: `reference/wiki/future-state/platform-decision-matrix.md`, wiki page ART Escalations

## Steps

1. Define scope: PI, iteration, or ART
2. Query ADO for dependency link types (Predecessor/Successor, Related, or custom — inspect sample work items)
3. Build dependency list:
   - Consumer team / item
   - Provider team / item
   - Need-by date / iteration
   - Status (Open / Met / At risk)
4. Age dependencies from created/changed dates (`wit_list_work_item_revisions` if needed)
5. Flag **>1 iteration overdue** or on critical path for PI Objectives
6. Recommend escalation per ART Escalations wiki when teams cannot resolve

## Output

```
## Dependency Snapshot — [scope] — [date]

### Critical (need action this week)
| ID | Consumer | Provider | Need by | Days open | Action |

### Watch list
| ... |

### Recently resolved
| ... |

## Escalation recommendations
- [Item]: escalate to [RTE/ART leadership] because [reason]
```

## ART Sync input

Feed top 5 critical deps into `scrum-of-scrums-brief`.

Do not create/modify ADO links without user approval.

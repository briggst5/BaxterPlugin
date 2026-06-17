---
name: iteration-health-check
description: Mid-PI and end-of-iteration health report — committed vs delivered, spillover, scope creep, and impediment themes. Use for RTE reviews, I&A prep, and team coaching.
---

# Iteration Health Check

## Metrics to collect (ADO)

1. **Iteration scope:** `wit_get_work_items_for_iteration` — committed at sprint start vs current
2. **Completion:** Closed story points vs committed
3. **Spillover:** Active/New items still in iteration past end date
4. **Scope creep:** Items added after sprint start (revisions)
5. **Waiting:** Count and age of Waiting items
6. **Predictability:** (delivered / committed) × 100%

Capacity context: `work_get_team_capacity`

## Qualitative themes

From comments and stand-up notes if provided:
- Recurring blockers (UX, REG, cross-team)
- DoD/AC churn
- Platform vs product decision delays

## Output

```
## Iteration Health — [Team] — [Iteration]

### Scorecard
| Metric | Value | Target | Status |
| Committed SP | | | |
| Delivered SP | | | |
| Predictability | | ~80%+ | |
| Spillover items | | 0 ideal | |
| Waiting items | | | |

### Impediment themes
1. ...

### Recommendations
- [For RTE / SM / PO — specific actions]

### I&A inputs
- [Facts for inspect-adapt-synthesis]
```

Follow `communication-tone` rule — facts first, no blame.

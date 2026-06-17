---
name: capacity-planner
description: Matches team capacity to planned backlog load for an iteration or PI using Azure DevOps capacity tools. Use for sprint planning, PI planning, and overcommit detection.
---

# Capacity Planner

Reference: `reference/wiki/agile-practices/safe-agile/safe-timebox-planning.md` — individual 3–9 pts/iteration (avg 5)

## Steps

1. Get team + iteration(s): `work_list_team_iterations`, user confirms target
2. Fetch capacity: `work_get_team_capacity` per iteration
3. Sum **available capacity** (per person × days off if shown)
4. Query committed backlog for iteration: `wit_get_work_items_for_iteration` or WIQL
5. Sum **planned story points** on committed Stories/Tasks
6. Compare load % = planned / capacity
7. Flag overcommit (>100%) and underutilization (<70% committed may indicate readiness gap)

## PI rollup

Repeat for each of 5 development iterations in PI; exclude IP iteration from feature delivery load unless user includes innovation work.

## Output

```
## Capacity Plan — [Team] — [Iteration/PI]

| Member | Capacity (days) | Allocated SP | Notes |

**Team capacity:** X SP
**Committed load:** Y SP
**Load:** Z%

### Over capacity by
- [Item ID] — N SP — suggest: defer / split / swap

### Recommendations
- ...
```

Never use capacity data to rank individuals — FutureState policy forbids performance use of story points.

## Related

- `sprint-planning-assist`
- `pi-planning-prep`

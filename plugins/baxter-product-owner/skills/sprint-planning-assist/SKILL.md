---
name: sprint-planning-assist
description: Pulls iteration backlog and team capacity, flags overcommit, and drafts sprint goal for FutureState teams. Use at sprint planning and backlog commitment.
---

# Sprint Planning Assist

Ceremony: `reference/wiki/agile-practices/safe-agile/agile-ceremonies-scrum.md`

## Steps

1. Confirm team, iteration, attendees
2. Run `capacity-planner` for the iteration
3. Pull candidate stories: team backlog filtered by priority (`wit_list_backlog_work_items`)
4. Run `backlog-readiness-audit` on candidates — only sprint-ready stories
5. Select stories up to capacity; leave buffer (~10–15%) for unplanned work
6. Draft **Sprint Goal** — one sentence tying selected stories to Feature/PI objective
7. Flag stories needing task breakdown (team activity, not pre-created unless asked)

## Output

```
## Sprint Planning Pack — [Team] — [Iteration]

**Proposed Sprint Goal:** [sentence]

| Story | SP | Ready | Feature | Notes |
|-------|-----|-------|---------|-------|

**Total SP:** X / Capacity Y (Z%)

### Not ready — defer
| Story | Reason |

### Task breakdown prompts
- Story [ID]: suggest tasks for ...
```

Do not assign iteration or change states without user approval.

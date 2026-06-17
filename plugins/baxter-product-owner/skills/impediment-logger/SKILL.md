---
name: impediment-logger
description: Structures impediments with owner and ART visibility for FutureState teams. Use when logging blockers, Waiting state transitions, or ART Sync escalations.
---

# Impediment Logger

States: `reference/wiki/future-state/work-item-states.md` — use **Waiting** with comment

## Steps

1. Capture impediment from user:
   - What is blocked?
   - Which work item(s)?
   - External dependency or internal?
   - Who owns resolution?
   - Target resolution date?
2. Draft Waiting comment (required):

```
Blocked: [description]
Waiting on: [person/team/system]
Impact: [PI objective / sprint goal / story at risk]
Requested action: [specific ask]
Expected resolution: [date if known]
```

3. Recommend visibility:
   - Team-level: comment on work item
   - ART-level: include in `scrum-of-scrums-brief`
   - Escalation: ART Escalations wiki path if unresolved >1 iteration

4. Propose ADO update (State → Waiting) only after user approval

## Output

Impediment record + optional link to `docs/templates/dependency-record.md` if cross-team.

## Related

- `dependency-tracker` — systemic deps
- `scrum-of-scrums-brief` — program visibility

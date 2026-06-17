---
name: backlog-readiness-audit
description: Pre-refinement and pre-PI gate audit of backlog items against FutureState Definition of Ready. Use before PI Planning, sprint planning, or backlog refinement to find gaps.
---

# Backlog Readiness Audit

Full checklists: `docs/dor-dod-checklists.md`

## Definition of Ready (FutureState)

### Feature (PI ready)

- [ ] Description + Technical Readiness
- [ ] Program-level AC
- [ ] Sized for one PI (20–100 pts guideline)
- [ ] SAFe Category set
- [ ] No NeedsPI / NeedsPO / NeedSAFe / NeedsScope tags (or documented exception)
- [ ] Predecessors/enablers identified

### User Story (sprint ready)

- [ ] Parent Feature linked
- [ ] INVEST story format
- [ ] Acceptance criteria present
- [ ] Platform Decision and Rationale
- [ ] Sized 1–20 pts
- [ ] No NeedsAC / NeedsPO tags
- [ ] Polarion link if regulated requirement

### Task (execution ready)

- [ ] Parent Story linked
- [ ] Definition of Done ("I know that I am done when…")
- [ ] Sized 1–3 pts
- [ ] Assigned to current sprint if Active

## Steps

1. User provides scope: query, backlog ID, or iteration
2. Fetch items via `wit_query_by_wiql` or `wit_list_backlog_work_items`
3. Score each item Pass / Fail per checklist
4. Summarize: % ready, top blockers by tag type
5. Prioritize fix order for refinement session

## Output

```
## Readiness Summary
- Scope: [N items]
- Ready: X | Not ready: Y

## Not Ready (priority order)
| ID | Type | Fail reasons | Suggested fix |

## Recommended refinement agenda
1. ...
```

Do not fix items automatically unless user asks.

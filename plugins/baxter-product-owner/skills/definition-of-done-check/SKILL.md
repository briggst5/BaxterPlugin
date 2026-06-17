---
name: definition-of-done-check
description: Pre-close validation of Stories and Tasks against FutureState Definition of Done before Resolved/Closed transitions. Use before completing work items or sprint review.
---

# Definition of Done Check

## Story — ready for Resolved/Closed

- [ ] All acceptance criteria met (verify each Given/When/Then)
- [ ] Platform Decision honored in delivery
- [ ] Parent Feature linkage intact
- [ ] Polarion requirement links updated if applicable
- [ ] Discipline review tags/tasks complete (ReviewSW, ReviewUX, etc. if required)
- [ ] Demo-ready for Sprint Review
- [ ] No open Tasks under story (or explicitly deferred with PO approval)
- [ ] For shippable/regulated work: QA and documentation per IEC 62304 glossary note

## Task — ready for Closed

- [ ] Definition of Done checklist in description — each item demonstrable
- [ ] Planned/Remaining/Completed SP updated
- [ ] Work matches DoD scope only (no unapproved scope creep)

## Steps

1. `wit_get_work_item` + child tasks
2. Walk checklist; mark Pass/Fail/NA per item
3. List evidence gaps
4. Recommend state transition only if all Pass

## Output

```
## DoD Check — [ID] [Title]

| Criterion | Status | Evidence / Gap |
|-----------|--------|----------------|

**Verdict:** Ready for [Resolved/Closed] | Not ready — [actions]
```

Do not transition state without user approval.

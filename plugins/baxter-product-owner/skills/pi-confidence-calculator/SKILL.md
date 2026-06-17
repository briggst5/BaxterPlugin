---
name: pi-confidence-calculator
description: Summarizes team confidence vote inputs and weighted PI confidence score with gaps. Use during PI Planning Day 2 or when assessing PI plan viability.
---

# PI Confidence Calculator

SAFe confidence vote: each team rates 1–5 (fist of five) on ability to meet PI Objectives.

## Steps

1. List teams and PI Objectives (from `pi-objectives-writer` output or ADO)
2. Collect confidence score per team (user input or meeting notes)
3. Optional weights: by team capacity share of ART
4. Calculate:
   - **Simple average** across teams
   - **Weighted average** if capacities provided
5. For any team ≤3, document **gaps**:
   - Dependencies unresolved
   - Features not ready
   - Capacity overcommit
   - Skill/architecture gaps
6. Recommend: proceed | replan scope | resolve blockers before commit

## Output

```
## PI Confidence Summary — [PI name]

| Team | Confidence (1–5) | Capacity | Key concern |
|------|-------------------|----------|-------------|

**ART average:** X.X
**Weighted average:** X.X

### Teams below 4
- [Team]: [gaps] → [recommended action]

### Conditions for raising confidence
1. ...
```

If scores not yet collected, output a **blank template** for facilitation during PI Planning.

## Related

- `pi-planning-prep` — pre-planning health
- `pi-planning-facilitator` — Day 2 agenda

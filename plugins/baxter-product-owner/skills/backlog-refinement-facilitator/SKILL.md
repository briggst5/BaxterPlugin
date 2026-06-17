---
name: backlog-refinement-facilitator
description: Prepares backlog refinement pre-read pack — top N items with gaps, suggested questions, and session agenda. Use before team backlog grooming or PO Sync.
---

# Backlog Refinement Facilitator

## Steps

1. User sets: team, N items (default 10), Feature or Story level
2. Pull top backlog items (`wit_list_backlog_work_items`)
3. Run readiness checks from `backlog-readiness-audit` (compact)
4. For each not-ready item, generate **refinement questions** for PO/team
5. Time-box agenda (default 60–90 min):
   - 5 min: goal & PI context
   - Per item: 5–10 min
   - 10 min: split/estimate parking lot

## Question bank (pick relevant)

**Feature:**
- What is the smallest demo slice for iteration 1?
- Which enablers are predecessors?
- Platform or product-specific?

**Story:**
- Who is the user? What is the benefit?
- Can we write Given/When/Then AC now?
- Does this fit one sprint at proposed points?

## Output

```
## Refinement Pre-read — [Team] — [date]

**Goal:** [PI objective or sprint prep focus]

| # | ID | Title | SP | Ready? | Key questions |
|---|-----|-------|-----|--------|---------------|

## Suggested agenda
1. ...

## Parking lot
- Items too large — schedule split session for [IDs]
```

Pair with `acceptance-criteria-coach` and `story-breakdown` during session.

---
name: acceptance-criteria-coach
description: Rewrites vague User Story acceptance criteria into testable Given/When/Then format per FutureState standards. Use when reviewing stories, preparing for refinement, or fixing NeedsAC items.
---

# Acceptance Criteria Coach

Reference: `reference/wiki/agile-practices/guide-to-writing-user-stories.md`

## Steps

1. Load story from ADO or user paste
2. Identify user role, goal, and observable outcomes
3. Rewrite AC as Given/When/Then blocks
4. Check each criterion is:
   - **Testable** — QA can verify without reading minds
   - **Independent** — not duplicating other stories
   - **Bounded** — no "etc.", "and other cases"
   - **Platform-aware** — includes platform/product expectations if relevant
5. Flag scope creep vs parent Feature AC
6. Remove implementation detail unless required for verification (prefer behavior over API names)

## Anti-patterns to fix

| Bad | Better |
|-----|--------|
| "Works correctly" | Given logged-in clinician, When viewing home screen, Then EWS updates within 60s |
| "Unit tests pass" | Separate task/DoD item unless story is test infrastructure |
| 15 bullet micro-tasks | Split story |

## Output

```markdown
## Story
[user story sentence]

## Revised Acceptance Criteria
### AC1: [title]
Given ...
When ...
Then ...

## Notes
- [Split recommendation if >7 AC or >13 points]
```

Offer to update ADO and remove NeedsAC tag after user approval.

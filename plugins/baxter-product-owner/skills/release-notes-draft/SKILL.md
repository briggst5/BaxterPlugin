---
name: release-notes-draft
description: Drafts customer-facing or stakeholder release notes from completed Features and Stories in a PI or release. Use after iteration, PI, or milestone completion.
---

# Release Notes Draft

Follow `communication-tone` rule — outcomes first.

## Steps

1. Define scope: iteration, PI, or date range
2. Query closed Features/Stories (`wit_query_by_wiql` State = Closed, changed in range)
3. Group by Feature or PI Objective
4. For each item write:
   - **User-facing headline** (not technical task name)
   - **Benefit** — so that…
   - **Polarion/req ref** if regulated (optional footnote)
5. Separate **New**, **Improved**, **Fixed** if Bugs included
6. Add **Known issues** if user provides

## Output

```markdown
# Release Notes — [Product/ART] — [PI/Version] — [date]

## Highlights
- [Top 3 outcomes]

## New capabilities
### [Feature name]
[User-facing description]

## Improvements
- ...

## Fixes
- ...

## Known issues
- ...
```

Internal appendix (optional): ADO ID table for support teams.

Do not publish externally without user review.

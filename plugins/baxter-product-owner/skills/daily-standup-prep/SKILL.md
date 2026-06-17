---
name: daily-standup-prep
description: Builds a personal standup brief from Azure DevOps — yesterday, today, blockers. Use before daily stand-up for any team member.
---

# Daily Standup Prep

## Steps

1. `wit_my_work_items` — user's assigned items
2. Filter Active + recently Closed (last 2 days if query allows, else manual from list)
3. For each Active item: summarize title, state, remaining SP if present
4. Identify **Waiting** items and draft blocker one-liner
5. Format standup answers:

## Output

```
## Standup — [User] — [date]

**Yesterday:** [completed items or progress notes]

**Today:** [Active items in priority order]

**Blockers:**
- [ID Title]: [blocker] — waiting on [who]

**Board walk order:** [ID list for walking the board]
```

## Tips

- Stand-up is not for problem-solving — note "take offline" for deep issues
- If Active item not in current sprint, flag for SM/PO per work item states rule

If `wit_my_work_items` empty, ask user for team/iteration filter and use WIQL assigned to user.

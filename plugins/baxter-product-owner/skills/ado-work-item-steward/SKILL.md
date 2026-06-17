---
name: ado-work-item-steward
description: Creates, queries, and updates Azure DevOps work items in FutureState following hierarchy and field standards. Use for backlog queries, Features, Stories, Tasks, capacity, iterations, comments, and linking work items to PRs.
---

# ADO Work Item Steward

Project: **FutureState** unless user specifies otherwise.

## Standards

Read `docs/futurestate-process.md` and `.cursor/rules/work-item-writing-standards.mdc` before writes.

Templates: `docs/templates/`

## MCP tools (azure-devops)

| Action | Tool |
|--------|------|
| Query | `wit_query_by_wiql`, `wit_list_backlog_work_items`, `wit_my_work_items` |
| Read | `wit_get_work_item`, `wit_list_work_item_comments` |
| Create | `wit_create_work_item`, `wit_add_child_work_items` |
| Update | `wit_update_work_item` (confirm with user per no-unapproved-status-changes rule) |
| Iteration/capacity | `work_list_team_iterations`, `work_get_team_capacity`, `work_list_iterations` |
| Link PR | `wit_link_work_item_to_pull_request` |

Always read tool schemas before calling.

## Create workflow

1. Confirm work item type and parent (Epic→Feature→Story→Task)
2. Draft fields from template; show user before create
3. Apply governance tags if gaps (NeedsAC, NeedsDoD, etc.)
4. Set iteration only when user commits to sprint

## Query patterns

**My work:** `wit_my_work_items`

**Backlog:** `wit_list_backlog_work_items` — need project, team, backlogId

**Blocked / Waiting:** WIQL on State = Waiting

**Missing AC:** tag contains NeedsAC or empty AC field (field name may vary — inspect `wit_get_work_item_type`)

## WIQL examples

```
SELECT [System.Id], [System.Title], [System.State]
FROM WorkItems
WHERE [System.TeamProject] = 'FutureState'
  AND [System.WorkItemType] = 'User Story'
  AND [System.State] <> 'Closed'
ORDER BY [Microsoft.VSTS.Common.Priority] ASC
```

Adjust fields per actual process template.

## Never

- Change state without user approval
- Create Stories without parent Feature (unless user explicitly allows orphan)
- Use hours instead of story points for estimation

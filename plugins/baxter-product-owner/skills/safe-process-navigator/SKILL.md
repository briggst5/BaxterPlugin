---
name: safe-process-navigator
description: Answers FutureState SAFe process questions using local wiki exports and routes to the right skill. Use when asked about DoR, DoD, work item hierarchy, ceremonies, ownership, tags, platform governance, or which tool (ADO vs Polarion) to use.
---

# Safe Process Navigator

## When to use

Process questions, ceremony prep, "what's our standard for X", tool routing.

## Steps

1. Read `docs/futurestate-process.md` for the answer
2. For DoR/DoD gates read `docs/dor-dod-checklists.md`
3. Drill into `reference/wiki/` for detail (see `reference/wiki-index.md`)
4. If local exports may be stale, run `wiki-sync` skill or `python3 scripts/sync_wiki.py`
5. If live wiki needed, use `wiki_get_page_content` (project: `FutureState`, wiki: Platform.wiki)
6. Route to a specialized skill when the user needs action, not explanation:

| Topic | Skill |
|-------|-------|
| Create/query ADO items | `ado-work-item-steward` |
| Requirements/Polarion | `polarion-requirements-steward` |
| PI planning prep | `pi-planning-prep` |
| Story writing | `acceptance-criteria-coach`, `story-breakdown` |
| Backlog hygiene | `backlog-readiness-audit` |
| Dependencies | `dependency-tracker` |
| Refresh wiki | `wiki-sync` |

## DoR / DoD quick reference

**Story ready for sprint (DoR):** INVEST story, AC present, parent Feature, Platform Decision, sized ≤20 pts, no NeedsAC/NeedsPO tags

**Task ready (DoD defined):** "I know that I am done when…" list agreed, sized 1–3 pts, assigned to iteration

**Feature ready for PI:** Description, Technical Readiness, AC, sized for one PI, predecessors identified

## Output format

- Direct answer citing FutureState specifics (not generic SAFe)
- Link to wiki page path when helpful
- Suggest next skill if user should take action

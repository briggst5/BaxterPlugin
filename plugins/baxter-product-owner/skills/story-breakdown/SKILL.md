---
name: story-breakdown
description: Decomposes Features into INVEST-compliant User Stories with acceptance criteria, sizing, and Polarion links. Use when breaking down Features for team backlog or sprint readiness.
---

# Story Breakdown

Templates: `docs/templates/story.md`, `docs/templates/feature.md`

## Steps

1. Load parent Feature from ADO; load related Polarion requirements if IDs provided (`polarion-requirements-steward`)
2. Identify user-visible outcomes (not tasks)
3. For each story draft:
   - User story sentence (As a / I want / so that)
   - Given/When/Then AC (3–7 criteria typical)
   - Platform Decision and Rationale
   - Fibonacci estimate (1–20, one sprint)
   - Polarion requirement link(s)
4. Run **INVEST** check on each story
5. Flag enabler stories separately (same format, indirect value)
6. Ensure no story hides multi-sprint scope

## Example decomposition pattern

Feature: Mobile ECG Viewer →
- View ECG data on tablet
- Annotate ECG trace
- Filter by date range
- Export PDF for EMR

## Output table

| # | Title | Points | INVEST | Polarion | Notes |
|---|-------|--------|--------|----------|-------|

Offer `wit_add_child_work_items` after user approves drafts.

## Reference

`reference/wiki/agile-practices/guide-to-writing-user-stories.md`, `reference/wiki/agile-practices/safe-agile/work-item-hierarchy.md`

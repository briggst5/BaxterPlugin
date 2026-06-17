---
name: pi-objectives-writer
description: Drafts and refines PI Objectives from Features and business context for FutureState teams. Use for PI planning, SMART objectives, business value assignment, and committed vs stretch objectives.
---

# PI Objectives Writer

Template: `docs/templates/pi-objective.md`

## Inputs

Gather from user or ADO:
- PI name / iteration
- Team or ART name
- PI Goals (stakeholder level) if available
- Committed Features for the PI (`wit_list_backlog_work_items`, feature query)

## Steps

1. List candidate Features with IDs and story points
2. Distill 3–7 **team PI Objectives** in business language (not task lists)
3. Make each objective **SMART**
4. Map Features/Stories to each objective
5. Assign **business value** (1–10) per objective for PI planning
6. Separate **Committed** vs **Stretch** objectives
7. Note dependencies and ROAM risks per objective

## Quality checks

- Objectives demo-able at System Demo / I&A
- No single objective that is pure enabler without stated outcome
- Align with PI Goals wiki pages if team-specific goals exist (`reference/wiki-index.md` → PI and Sprint Goals)

## Output

Use `pi-objective.md` template structure. Offer to create/update ADO items only after user approval.

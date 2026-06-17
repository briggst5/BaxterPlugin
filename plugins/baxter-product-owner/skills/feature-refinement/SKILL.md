---
name: feature-refinement
description: Prepares Features for PI Planning or backlog refinement — split/merge guidance, acceptance criteria, dependencies, enablers, and sizing. Use when grooming program backlog Features in FutureState.
---

# Feature Refinement

Template: `docs/templates/feature.md`

## Preconditions

Feature should fit **one PI** (20–100 story points). Flag multi-PI features for split.

## Steps

1. Fetch Feature from ADO (`wit_get_work_item`) including children and links
2. Validate fields: Description, Technical Readiness, SAFe Category, AC
3. Check tags: NeedsPI, NeedsPO, NeedSAFe, NeedsScope
4. Assess sizing smell — if children sum > PI capacity or span iterations, recommend split
5. Identify **enabler predecessors** (architecture, compliance, spikes)
6. Draft/refine program-level acceptance criteria
7. Confirm Platform vs product alignment at feature level when stories will inherit

## Split guidance

Split by:
- Independent user outcomes
- Release slice / demo milestone
- Enabler vs business delivery
- ART boundary (if cross-ART, escalate to Capability level per SAFe Category)

## Output

| Section | Content |
|---------|---------|
| Health | Ready / Needs work |
| Gaps | Missing fields, tags |
| Split recommendation | If applicable |
| Refined description & AC | Draft text |
| Predecessors | List |

Apply updates via `ado-work-item-steward` after user approval.

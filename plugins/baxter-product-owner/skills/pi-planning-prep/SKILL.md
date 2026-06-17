---
name: pi-planning-prep
description: Prepares RTE and ART leadership for PI Planning — feature readiness, capacity vs load, dependency matrix, and risk register draft. Use 2–4 weeks before PI Planning in FutureState.
---

# PI Planning Prep

Ceremony reference: `reference/wiki/agile-practices/safe-agile/agile-ceremonies-safe.md`

## Data to gather

1. **Features** targeted for PI — `wit_query_by_wiql` or program backlog
2. **Readiness** — run checklist from `backlog-readiness-audit` (summary mode)
3. **Capacity** — per team: `work_list_team_iterations`, `work_get_team_capacity` for PI iterations
4. **Dependencies** — query linked items, predecessor/successor relations; use `dependency-tracker`
5. **PI Goals** — wiki pages under PI and Sprint Goals (`reference/wiki-index.md`)

## Deliverables

### 1. Feature readiness summary

| Feature | Team | Points | Ready? | Blockers |

### 2. Capacity vs load

| Team | PI capacity (pts) | Committed load | Load % | Over/under |

Rule: team load ≤ 100% of capacity for committed scope; buffer for IP iteration.

### 3. Dependency matrix

Use `docs/templates/dependency-record.md` columns — consumer × provider grid.

### 4. Risk register (ROAM draft)

| Risk | Teams | ROAM | Mitigation |

### 5. PI Planning agenda gaps

- Features needing split before planning
- Teams missing PO/SM attendance
- Architecture/enabler gaps

## Output

Executive summary (per communication-tone rule) + detailed tables. Hand off to `pi-planning-facilitator` for session materials.

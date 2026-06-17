---
name: wsjf-prioritization
description: Applies WSJF-style prioritization to rank Features in the FutureState program backlog. Use when prioritizing Features, comparing epics, or preparing PI Planning scope.
---

# WSJF Prioritization

WSJF = Cost of Delay / Job Size (SAFe). FutureState may use custom ADO fields — **discover fields first** via `wit_get_work_item` on sample Features before scoring.

## Steps

1. Confirm WSJF fields with user if not visible in ADO (Common: Business Value, Time Criticality, Risk Reduction/Opportunity Enablement, Job Size/Story Points)
2. Query candidate Features (`wit_query_by_wiql` or backlog)
3. For each Feature, collect or elicit:
   - **User/Business Value** (1–10)
   - **Time Criticality** (1–10)
   - **RR/OE** (1–10)
   - **Job Size** (story points or relative)
4. Calculate: WSJF = (BV + TC + RR/OE) / Job Size
5. Rank descending; note ties and missing data
6. Flag Features with high WSJF but not PI-ready (`backlog-readiness-audit`)

## Output

| Rank | ID | Title | BV | TC | RR/OE | Size | WSJF | Ready? |
|------|-----|-------|----|----|-------|------|------|--------|

Include narrative: top 3 recommendations and any "high WSJF but blocked" items.

## Caution

- Do not update ADO priority fields without user approval
- WSJF informs conversation; PO/PM makes final call
- Job size 0 or unknown — exclude from numeric rank, list separately

## If no WSJF fields exist

Use simplified CoD ranking: (BV + TC) / Size with user-provided scores in chat.

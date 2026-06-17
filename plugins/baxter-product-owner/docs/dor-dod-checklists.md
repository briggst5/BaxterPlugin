# Definition of Ready & Definition of Done — Quick Reference

FutureState SAFe quick gate cards. Full detail: [futurestate-process.md](futurestate-process.md). Wiki exports (local): `reference/wiki/` — run `scripts/sync_wiki.py`.

---

## Feature — Definition of Ready (PI Planning)

Use before committing a Feature to a PI.

| # | Criterion | Pass? |
|---|-----------|-------|
| 1 | Description states value and scope | ☐ |
| 2 | Technical Readiness assessed | ☐ |
| 3 | Program-level Acceptance Criteria defined | ☐ |
| 4 | SAFe Category set (Feature / Enabler / Capability) | ☐ |
| 5 | Sized 20–100 story points; fits **one PI** | ☐ |
| 6 | Predecessors / enabler dependencies identified in ADO | ☐ |
| 7 | Owner assigned (PM / PO) | ☐ |
| 8 | Tags cleared: NeedsPI, NeedsPO, NeedSAFe, NeedsScope | ☐ |

**Smell:** Feature spans multiple PIs → split before planning.

---

## User Story — Definition of Ready (Sprint)

Use before committing a Story to an iteration.

| # | Criterion | Pass? |
|---|-----------|-------|
| 1 | Parent Feature linked | ☐ |
| 2 | INVEST: Independent, Negotiable, Valuable, Estimable, Small, Testable | ☐ |
| 3 | Story format: As a [role], I want [goal], so that [benefit] | ☐ |
| 4 | Acceptance criteria (Given/When/Then) — testable | ☐ |
| 5 | Platform Decision and Rationale completed | ☐ |
| 6 | Sized 1–20 points; fits **one sprint** | ☐ |
| 7 | Polarion requirement link if regulated / specified behavior | ☐ |
| 8 | Tags cleared: NeedsAC, NeedsPO, NeedsScope | ☐ |

**Smell:** Cannot envision demo in one sprint → decompose.

---

## Task — Definition of Ready (Execution)

| # | Criterion | Pass? |
|---|-----------|-------|
| 1 | Parent User Story linked | ☐ |
| 2 | Definition of Done: "I know that I am done when…" + demonstrable list | ☐ |
| 3 | Sized 1–3 story points | ☐ |
| 4 | Assignee accepts task | ☐ |
| 5 | Tag cleared: NeedsDoD | ☐ |

---

## User Story — Definition of Done (Close)

Use before Resolved → Closed.

| # | Criterion | Pass? |
|---|-----------|-------|
| 1 | All acceptance criteria met and demonstrable | ☐ |
| 2 | Platform Decision honored in delivery | ☐ |
| 3 | All child Tasks closed or explicitly deferred (PO approved) | ☐ |
| 4 | Discipline reviews complete (ReviewSW, ReviewUX, etc. if required) | ☐ |
| 5 | Demo-ready for Sprint Review | ☐ |
| 6 | Polarion traceability updated if applicable | ☐ |
| 7 | For CfC stories: PO confirms DoD + process check + team agreement | ☐ |
| 8 | For shippable work: QA + documentation per project plans (IEC 62304) | ☐ |

---

## Task — Definition of Done (Close)

| # | Criterion | Pass? |
|---|-----------|-------|
| 1 | Every DoD bullet demonstrable | ☐ |
| 2 | Planned / Remaining / Completed SP updated | ☐ |
| 3 | Scope matches agreed DoD only (no unapproved extras) | ☐ |

---

## Feature — Definition of Done (PI complete)

| # | Criterion | Pass? |
|---|-----------|-------|
| 1 | Program AC met | ☐ |
| 2 | Demonstrated in System Demo / PI review | ☐ |
| 3 | Feature owner (PM/PO) accepts completion | ☐ |
| 4 | Enabler / successor Features unblocked | ☐ |

---

## Epic — Definition of Done

| # | Criterion | Pass? |
|---|-----------|-------|
| 1 | Epic Hypothesis outcomes achieved / MVP defined met | ☐ |
| 2 | Decomposed Features delivered or explicitly descoped | ☐ |
| 3 | Epic owner accepts; demonstrated to stakeholders | ☐ |

---

## Code-for-Credit (CfC) Story — Additional gates

From wiki: CfC Story Standard Work.

**Ready:** Copy template story #7083 with child tasks; cross-functional scope confirmed (Systems, SW, SW Test, UX if UI).

**Done:**
- PO confirms story DoD
- Process check: CfC goals met
- Team agrees story is complete
- Feature impact assessment still accurate (Systems-driven)

---

## Waiting state — required comment

When blocked, add before setting **Waiting**:

```
Blocked: [what]
Waiting on: [person/team]
Impact: [sprint/PI objective at risk]
Requested action: [specific ask]
Expected resolution: [date if known]
```

---

## Governance tags (remediation)

| Tag | Fix |
|-----|-----|
| NeedsScope | Scope the item with PO/team |
| NeedsPO | PO review for priority and scope |
| NeedsAC | Add Given/When/Then acceptance criteria |
| NeedsDoD | Add task Definition of Done |
| NeedsPI | Review in PI Planning |
| NeedSAFe | Update SAFe Category and related fields |

---

## Skills for automated checks

| Gate | Skill |
|------|-------|
| Backlog / PI readiness | `backlog-readiness-audit` |
| Story AC quality | `acceptance-criteria-coach` |
| Pre-close validation | `definition-of-done-check` |
| Process questions | `safe-process-navigator` |

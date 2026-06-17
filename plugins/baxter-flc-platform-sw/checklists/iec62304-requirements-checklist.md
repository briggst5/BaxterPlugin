---
name: IEC 62304 Class C Software Requirements Checklist
description: >
  Generation and review checklist for software requirements under IEC 62304:2006+Amd1:2015
  §5.1–§5.2 and ISO 14971 risk-control traceability. Used with Polarion work items via MCP.
alwaysApply: false
---

When generating or reviewing software requirements, apply every item below.
Flag each issue: 🔴 Blocking / 🟡 Warning / 🔵 Suggestion.

---

## 1. Requirement identity and structure (IEC 62304 §5.2.1)

- [ ] Each requirement has a unique, stable Polarion ID (e.g. `PLT1-XXXX`) — never reuse or duplicate IDs
- [ ] Title is a concise summary; description holds the full requirement statement
- [ ] One primary behavior or constraint per requirement — split compound "and/or" requirements
- [ ] Mandatory behavior uses **shall**; **should** only for non-mandatory guidance explicitly marked as such
- [ ] Requirement type, status, and priority fields are set correctly in Polarion

---

## 2. Content completeness (IEC 62304 §5.2.2)

Verify the requirement set collectively covers applicable categories:

- [ ] Functional and performance capability
- [ ] Software inputs, outputs, and data definitions
- [ ] Interfaces to other software, hardware, and operators
- [ ] Alarms, warnings, and operator messages driven by software
- [ ] Security and access-control requirements
- [ ] Usability / human-factors requirements (when applicable)
- [ ] Installation, deployment, and acceptance criteria
- [ ] Operation, maintenance, and user-documentation needs
- [ ] Regulatory and labeling obligations referenced explicitly

For a single requirement, confirm it states **what** the software must do — not **how** to implement it.

---

## 3. Quality attributes (IEC 62304 §5.2.3 — verifiable and unambiguous)

- [ ] **Unambiguous:** No vague terms without measurable definition (`fast`, `robust`, `user-friendly`, `adequate`, `as needed`, `etc.`, `TBD`)
- [ ] **Verifiable:** Acceptance criteria or verification method is stated or linked — a reviewer can derive a test or inspection
- [ ] **Feasible:** Requirement is achievable within platform, safety class, and regulatory constraints
- [ ] **Complete:** Preconditions, triggers, expected behavior, and postconditions are defined for behavioral requirements
- [ ] **Consistent:** No conflict with linked parent requirements or sibling requirements in the same document/module
- [ ] **Bounded:** Inputs, outputs, limits, units, and timing constraints are specified where relevant

---

## 4. Class C rigor (IEC 62304 §5.2 — safety class C)

- [ ] Safety-relevant requirements explicitly identify the hazard or risk control they address
- [ ] Failure modes and safe-state behavior are specified — software failure must not leave patient-affecting state unknown
- [ ] Sequences involving alarms, clinical data, or therapy delivery include explicit confirmation/acknowledgement requirements
- [ ] Security requirements for patient data (confidentiality, integrity, availability) are present where PHI/PII is handled
- [ ] SOUP dependencies referenced by requirements identify the SOUP item and version constraints

---

## 5. Traceability (IEC 62304 §5.1, §5.2.6, ISO 14971)

Use Polarion links — verify with `get_work_item_links`:

- [ ] **Upstream:** Linked to system requirement(s) or risk control measure(s) that drive this software requirement
- [ ] **Downstream:** Linked or planned links to software design items, implementation work, and verification test cases
- [ ] **Risk:** Risk items (hazard, harm, severity) trace to requirements that implement risk controls — no orphaned risk controls
- [ ] **Change impact:** Modified requirements note affected downstream links; flag broken traceability after edits

---

## 6. Review process (IEC 62304 §5.2.5)

When performing a requirements review:

1. Fetch the work item (`get_work_item`) and links (`get_work_item_links`)
2. For document/module reviews, list items (`get_document_work_items` or `list_document_work_items`)
3. Apply sections 1–5 above to each requirement
4. Check reviewer status (`list_reviewers`) — do not approve on the user's behalf
5. Post findings as a structured Polarion comment (`add_work_item_comment`) with severity tags
6. Do **not** change status, links, or field values without explicit user approval

### Review comment format

```
## Requirements Review — [WORK-ITEM-ID]

### Summary
[Pass / Pass with findings / Fail — one sentence]

### Findings
- 🔴 [ID] — [issue] — [recommended fix]
- 🟡 [ID] — [issue] — [recommended fix]

### Traceability gaps
- [missing upstream/downstream links]

### Questions for author
- [clarifications needed]
```

---

## 7. Generation guidelines

When drafting new or revised requirements for Polarion:

1. Confirm upstream system requirement or risk control via `query_work_items` or user input
2. Draft using this template:

```
The software shall [behavior/capability] when [condition/trigger].

Acceptance criteria:
- [measurable criterion 1]
- [measurable criterion 2]

Rationale: [risk control / system requirement / regulatory source]
Verification method: [test / inspection / analysis / demonstration]
```

3. Propose link targets (parent system req, risk item, planned test case) — do not create links without approval
4. Present the draft for human review before any Polarion write operation

---

## 8. Common anti-patterns

| Anti-pattern | Fix |
|---|---|
| "The system shall handle errors gracefully" | Specify each error condition, expected response, and safe state |
| "Response time shall be fast" | State measurable latency (e.g. ≤ 200 ms at P95 under defined load) |
| "Shall comply with applicable regulations" | Name the regulation/clause or link to a regulatory requirement item |
| Design detail in SRS ("shall use Redis cache") | Move implementation to design; keep SRS behavioral |
| Duplicate requirements with different wording | Merge or cross-link; one authoritative statement |
| Missing negative requirements | Add explicit "shall not" or forbidden-behavior requirements where safety-relevant |

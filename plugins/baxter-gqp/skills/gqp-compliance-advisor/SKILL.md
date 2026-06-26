---
name: gqp-compliance-advisor
description: Answers Baxter GQP/GQT compliance questions with verified citations via gqp-knowledge MCP. Use for procedure lookups, V&V, risk, cybersecurity, deliverables, change impact, and project planning.
---

# GQP Compliance Advisor

## Standards

Apply `rules/gqp-compliance-citations.mdc` on every compliance answer.

For regulated **product requirements** and traceability, also use `baxter-product-owner` skills (`polarion-requirements-steward`, `traceability-standards`).

## Prerequisites

- **gqp-knowledge** MCP enabled (baxter-gqp plugin)
- Baxter SSO completed on first use

## MCP tools (gqp-knowledge)

| Action | Tool | Key parameters |
|--------|------|----------------|
| Hybrid document search | `search_gqp_documents` | `query`; optional `doc_id`, `doc_family`, `doc_type`, `top` |
| V&V requirements | `find_testing_requirements` | `procedure_id` (e.g. `GQP-1234`) |
| Risk mitigations | `find_risk_mitigations` | `procedure_id` |
| Cybersecurity tasks | `find_security_tasks` | `procedure_id` |
| Required records/deliverables | `find_required_deliverables` | `procedure_id` |
| Change impact | `assess_change_impact` | `change_description` |
| Project plan | `build_project_plan` | `goal` |

## Common workflows

### Answer a compliance question

1. Identify whether the question targets a **specific procedure** or is **cross-cutting**.
2. Specific procedure → use the specialized tool for that domain when applicable; otherwise `search_gqp_documents` with `doc_id`.
3. Cross-cutting → `search_gqp_documents` with a focused query; narrow with `doc_family` / `doc_type` if needed.
4. Structure the response:
   - **Summary** (2–4 sentences)
   - **Requirements** (numbered, each with GQP/GQT citation)
   - **Gaps / follow-up** (missing procedure ID, ambiguous scope, revision questions)
5. If the user needs ADO/Polarion actions, draft stories or links separately — do not mutate work items without approval.

### Verify citations in a document or plan

1. List each GQP/GQT reference or implied obligation in the user's text.
2. For each, call `search_gqp_documents` with `doc_id` and a query for the claimed section/topic.
3. Produce a verification table: Claim | Status (Verified / Unsupported / Outdated / Overstated) | Citation evidence.

### Change control / impact assessment

1. `assess_change_impact` with a plain-language change description.
2. Present impacted procedures, deliverables, and recommended gate sequence with citations.
3. Offer to cross-check Polarion requirements if the change affects regulated product scope.

### New product or major initiative planning

1. `build_project_plan` with the development goal.
2. Present phased plan with GQP activities, GQT templates, and milestones — all cited.
3. Flag where team-specific tailoring or platform governance (FutureState) still applies.

## Response quality

- Prefer **procedure_id + section** over vague "per GQP" statements.
- When MCP returns "No results found", state that explicitly and ask for a procedure number or alternate keywords.
- Distinguish **corporate QMS** (GQP) from **product SRS** (Polarion) and **sprint execution** (ADO).

## Never

- Answer GQP compliance questions from memory without calling MCP
- Invent procedure numbers or requirements
- Present synthesized MCP output as verbatim regulatory text without noting synthesis

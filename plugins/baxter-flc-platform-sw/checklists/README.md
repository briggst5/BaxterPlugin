# Checklists Directory

Reusable code review evaluation frameworks in Markdown. All checklists are tool-agnostic and apply equally to AI-generated and hand-written code.

## Available Checklists

### code-review-checklist.md
Framework for reviewing individual code files or changes.

**Evaluation Areas:**
- Logic & Correctness
- Code Quality & Maintainability
- Security & Performance
- Testing & Error Handling
- Dependencies & Compatibility
- Documentation & Communication

### pr-review-checklist.md
Framework for reviewing complete pull requests.

**Evaluation Areas:**
- Change Scope & Intent
- Code Quality
- Testing Coverage
- Documentation Updates
- Security & Performance
- Integration & Compatibility

### iec62304-requirements-checklist.md
Framework for generating and reviewing software requirements under IEC 62304 Class C.

**Evaluation Areas:**
- Requirement identity and structure (§5.2.1)
- Content completeness (§5.2.2)
- Quality attributes — unambiguous, verifiable, feasible (§5.2.3)
- Class C safety rigor and risk-control traceability (ISO 14971)
- Polarion traceability links and review workflow

## Checklist Immutability

These checklists are **shared read-only assets**. Projects do NOT:

- Copy checklists to their own repositories
- Modify or customize checklist items
- Create project-specific versions

Projects reference these checklists from the `.agents/` folder (clone of the ai-skills repository):

- `.agents/checklists/code-review-checklist.md`
- `.agents/checklists/pr-review-checklist.md`

## Integration

| Tool | Reference path |
|------|----------------|
| Copilot | `.agents/checklists/` |
| Continue | `.agents/checklists/` (listed in prompts) |
| Cursor | `.agents/checklists/` (linked from skills and rules) |

Setup: [Integration Strategy](../docs/INTEGRATION_STRATEGY.md)

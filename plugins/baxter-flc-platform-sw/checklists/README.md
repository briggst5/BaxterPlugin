# Checklists Directory

Reusable code review evaluation frameworks in Markdown. All checklists are tool-agnostic and apply equally to AI-generated and hand-written code.

**Plugin users:** skills and rules reference `checklists/` in this plugin — no per-project copy required when **baxter-flc-platform-sw** is installed.

**Project clone users:** see [INTEGRATION_STRATEGY.md](../docs/INTEGRATION_STRATEGY.md) for `.agents/` layout.

## Available checklists

| File | Use with skill |
|------|----------------|
| [code-review-checklist.md](code-review-checklist.md) | `code-review` |
| [pr-review-checklist.md](pr-review-checklist.md) | `pr-review` |
| [class-c-code-checklist.md](class-c-code-checklist.md) | Class C / safety-critical review |
| [kotlin-checklist.md](kotlin-checklist.md) | Kotlin sources (`kotlin-review` rule) |
| Plus API, concurrency, error-handling, security, etc. | Scoped reviews in skill workflows |

### code-review-checklist.md

Framework for reviewing individual code files or changes.

- Logic & correctness
- Code quality & maintainability
- Security & performance
- Testing & error handling
- Dependencies & compatibility
- Documentation & communication

### pr-review-checklist.md

Framework for reviewing complete pull requests.

- Change scope & intent
- Code quality
- Testing coverage
- Documentation updates
- Security & performance
- Integration & compatibility

### IEC 62304 / Class C

Use `class-c-code-checklist.md` and the `iec62304-requirements` rule with **baxter-polarion** for Polarion-backed requirements workflows.

## Checklist immutability

These checklists are **shared read-only assets** in the ai-skills / plugin distribution. Projects should not fork modified checklist copies into app repos.

| Distribution | Path |
|--------------|------|
| **baxter-flc-platform-sw plugin** | `plugins/baxter-flc-platform-sw/checklists/` |
| ai-skills `.agents` clone | `.agents/checklists/` |

## Integration

| Tool | How checklists are used |
|------|-------------------------|
| **Cursor + plugin** | `code-review` / `pr-review` skills + `foundation-standards` rule |
| Cursor (project copy) | [cursor-setup.md](../docs/cursor-setup.md) |
| Copilot | [copilot-setup.md](../docs/copilot-setup.md) |
| Continue | [continue-setup.md](../docs/continue-setup.md) |

Install: [INSTALL.md](../docs/INSTALL.md)

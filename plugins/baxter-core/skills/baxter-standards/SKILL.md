---
name: baxter-standards
description: Apply Baxter engineering standards for code changes, reviews, and documentation. Use for all implementation work so Copilot users get org rules without Cursor .mdc files.
---

# Baxter standards

## When to use

Apply on every implementation, refactor, or review task unless a domain-specific Baxter skill overrides it.

## Standards

1. **Minimize scope** — Smallest correct diff; avoid unrelated changes.
2. **Match conventions** — Read surrounding code before editing; reuse existing patterns.
3. **No secrets** — Never commit credentials, tokens, or `.env` values.
4. **Tests** — Add or update tests when behavior changes; skip trivial-only edits.
5. **Documentation** — Update README or inline docs when user-facing behavior changes.

## Output

- Explain what changed and why in plain language.
- Call out any auth or env prerequisites the user must configure locally.

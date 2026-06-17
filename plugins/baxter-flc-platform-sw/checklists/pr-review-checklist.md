---
name: PR Review Checklist
description: Language-agnostic review checklist applied to every PR review alongside the project context file.
alwaysApply: false
---

When performing a PR review, always check every item in this checklist regardless of the stated criteria.
Flag each issue with its severity: 🔴 Blocking / 🟡 Warning / 🔵 Suggestion.

## Secrets & Security

- [ ] No secrets, tokens, API keys, passwords, or credentials hardcoded anywhere in the diff
- [ ] No internal URLs, hostnames, or IP addresses hardcoded that should come from config or environment variables
- [ ] Authentication and authorisation logic is not bypassed or weakened
- [ ] No new attack surface introduced without corresponding input validation

## Code Hygiene

- [ ] No debug statements left in (`console.log`, `console.warn`, `console.error`, `print`, `println`, `System.out`, `Log.d`, `info!`, `dbg!`)
- [ ] No commented-out code blocks committed (stale code should be deleted, not commented out)
- [ ] No TODO / FIXME / HACK comments introduced without a linked ticket
- [ ] No dead code (unreachable branches, unused variables, unused imports, unused functions)

## Error Handling

- [ ] All fallible operations have error handling — no silent swallows (`catch {}`, `catch { /* ignore */ }`, `unwrap()` in non-test Rust code)
- [ ] Errors surfaced to callers or logged with enough context to diagnose in production
- [ ] Timeouts and resource cleanup (connections, streams, subscriptions) are handled on both happy and error paths

## Testing

- [ ] New logic has corresponding unit tests
- [ ] Existing tests are not deleted or disabled without explanation
- [ ] Tests assert meaningful behaviour — not just that code runs without throwing
- [ ] Test-only code (mocks, stubs, fixtures) is not present in production source directories

## API & Interface Changes

- [ ] Public API changes (function signatures, exported types, published module interfaces) are intentional and documented
- [ ] Breaking changes are explicitly flagged — callers or consumers may need updating
- [ ] Deprecations are marked and a migration path is provided

## General Quality

- [ ] No copy-pasted blocks that should be extracted into a shared function or utility
- [ ] Naming is clear and consistent with the surrounding codebase
- [ ] Complex logic has an explanatory comment
- [ ] No unnecessary dependencies added (new packages, new imports from heavy libraries)
- [ ] Code must follow the Clean Architecture and SOLID principles 
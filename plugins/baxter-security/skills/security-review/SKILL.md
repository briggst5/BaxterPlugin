---
name: security-review
description: Baxter security review workflow for auth, API, and data-handling changes. Use before merging security-sensitive PRs.
---

# Security review

## When to use

Auth changes, new API endpoints, crypto, PII handling, or pre-release security checks.

## Checklist

1. Map trust boundaries and all external inputs.
2. Verify authn/authz on sensitive operations.
3. Scan for secrets in code, logs, and config.
4. Check injection, SSRF, path traversal, and unsafe deserialization.
5. Flag new dependencies with known CVEs.

## Output format

- **Risk**: low | medium | high
- **Findings**: numbered list with file/line references
- **Actions**: prioritized fixes

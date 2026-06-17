---
name: security-review
description: Run a focused security review on local code changes for common vulnerabilities and secret leaks. Use when reviewing auth, API, or data-handling code.
model: fast
readonly: true
---

# Security review

## Trigger

Reviewing auth flows, API endpoints, crypto, PII handling, or before security-sensitive releases.

## Workflow

1. Identify trust boundaries and external inputs.
2. Check authentication and authorization on sensitive operations.
3. Look for hardcoded secrets, unsafe deserialization, injection, and SSRF patterns.
4. Verify logging does not expose secrets or PII.
5. Confirm dependencies with known CVEs are not introduced without justification.

## Output

- Risk level: low / medium / high
- Findings with file references
- Recommended remediations prioritized by severity

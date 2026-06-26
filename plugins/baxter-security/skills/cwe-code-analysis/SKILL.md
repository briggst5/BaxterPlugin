---
name: cwe-code-analysis
description: Maps CWE weakness types to codebase search patterns and reviews code for vulnerability classes. Use when analyzing a CWE ID against source, auditing for OWASP/CWE Top 25 issues, or tracing CVE CWE tags to local code.
---

# CWE code analysis

Given a CWE identifier (e.g. `CWE-89`) or weakness category, systematically search and review the codebase for matching vulnerability patterns.

## When to use

- CVE record lists a CWE and you need to find vulnerable code paths locally
- Security audit scoped to specific weakness classes (XSS, SQLi, path traversal)
- Pre-release review against CWE Top 25 or OWASP categories
- Threat model identifies abuse cases that map to CWE entries

## Workflow

Copy and track progress:

```
CWE analysis:
- [ ] Step 1: Normalize CWE ID and load weakness definition
- [ ] Step 2: Identify applicable languages, frameworks, and trust boundaries
- [ ] Step 3: Run pattern searches from cwe-patterns.md
- [ ] Step 4: Inspect hits and trace data flow from external input
- [ ] Step 5: Check mitigations (validation, encoding, authz, bounds)
- [ ] Step 6: Report findings with evidence
```

### Step 1: Understand the CWE

Normalize to `CWE-NNNN` (reject malformed IDs).

**Authoritative sources:**

1. MITRE CWE definition: `https://cwe.mitre.org/data/definitions/NNNN.html`
2. Related CVEs (optional): NVD search with `--cwe-id CWE-NNNN` via `nvd-cve-search` skill
3. OWASP cheat sheet for the weakness class when one exists

Capture: weakness name, typical consequences, common mitigations, and **observable manifestation** in code (APIs, patterns, missing controls).

### Step 2: Scope the analysis

Define before searching:

| Scope item | Example |
|------------|---------|
| Target paths | `src/`, specific service, PR diff |
| Languages | C#, TypeScript, Python, C |
| Entry points | HTTP handlers, CLI args, IPC, file uploads |
| Trust boundary | External user → app, device → cloud, guest → admin |

For PR reviews, search the diff first; expand to related modules if hits suggest systemic issues.

### Step 3: Pattern search

Open [cwe-patterns.md](cwe-patterns.md) for the target CWE. Run suggested `rg` commands, adapting file globs to the repo.

If the CWE is not listed, derive patterns from the MITRE definition:

- **Sources**: functions that read untrusted input
- **Sinks**: dangerous APIs (exec, SQL, file write, deserialize, render HTML)
- **Missing guards**: validation, encoding, authz, size limits

```bash
# Generic sink discovery when no preset exists
rg -n '<dangerous_api_from_cwe>' --glob '<project_globs>'
```

### Step 4: Data-flow review

For each high-signal hit:

1. Trace input from source to sink
2. Note sanitization, validation, or authz along the path
3. Check error handling (does failure expose data or skip checks?)
4. Consider alternate paths (async, background jobs, test-only code shipped in prod)

### Step 5: Mitigation check

| Control | CWE examples |
|---------|----------------|
| Input validation / allowlist | CWE-20, CWE-22 |
| Output encoding | CWE-79 |
| Parameterized queries | CWE-89 |
| Authn + authz on sensitive ops | CWE-287, CWE-862 |
| Bounds checks / safe APIs | CWE-119, CWE-787 |
| No hard-coded secrets | CWE-798 |

Confirm framework defaults (e.g. ASP.NET anti-forgery, ORM parameterization) are not bypassed.

### Step 6: Cross-link CVEs

When the analysis started from a CVE's CWE tag, run `cve-impact-analysis` for exposure and version context. CWE analysis answers **"could this pattern exist here?"**; CVE analysis answers **"is this specific flaw in our version?"**

## Output format

```markdown
# CWE analysis: CWE-NNNN — [Weakness name]

## Scope
- Paths: [...]
- Languages: [...]
- Entry points: [...]

## CWE summary
[One paragraph from MITRE + typical code manifestation]

## Search strategy
- Patterns used: [commands or APIs searched]
- Files examined: N

## Findings

### [F1] [Title] — [confirmed | likely | informational]
- **Location**: `path:line`
- **Pattern**: [CWE manifestation]
- **Data flow**: [source → sink]
- **Mitigation gap**: [what is missing]
- **Recommendation**: [specific fix]

## Summary
| Severity | Count |
|----------|-------|
| Critical | N |
| High | N |
| Medium | N |
| Informational | N |

## Recommended actions
1. [Prioritized fixes and further tests]
```

## Severity guidance

| Rating | Criteria |
|--------|----------|
| Critical | Exploitable path from untrusted input to sink without effective control |
| High | Weak control or authz gap on sensitive operation |
| Medium | Defense present but incomplete; requires chained conditions |
| Informational | Pattern match without clear exploit path; hardening opportunity |

## Rules

- **Pattern match ≠ vulnerability** — confirm exploitable data flow before rating Critical/High
- Prefer project conventions for validation and security middleware
- Check test and debug code paths that ship in production builds
- For embedded/Yocto: include recipes, init scripts, and default configs
- Do not claim absence of a CWE class after only grep — note search limits in the report
- Link related CVE IDs when CWE analysis is driven by NVD/CVE triage

## Additional resources

- Search patterns by CWE: [cwe-patterns.md](cwe-patterns.md)
- CVE exposure and patches: `cve-impact-analysis` skill
- NVD CWE-filtered CVE lists: `nvd-cve-search` skill

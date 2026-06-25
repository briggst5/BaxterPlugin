---
name: dependency-security-audit
description: Audits project dependencies for known vulnerabilities using lockfiles, ecosystem advisories, and NVD/OSV lookups. Use before releases, after dependency bumps, or when onboarding a new repo to Baxter security practices.
---

# Dependency security audit

Scan application and library dependencies for known vulnerabilities and produce a prioritized fix list.

## When to use

- Pre-release or sprint-end dependency health check
- After major version upgrades or new package additions
- CI lacks integrated dependency scanning
- Onboarding audit for a new repository

## Workflow

```
Dependency audit:
- [ ] Step 1: Discover manifests and lockfiles
- [ ] Step 2: Run native ecosystem scanners when available
- [ ] Step 3: Supplement with OSV and NVD for gaps
- [ ] Step 4: Classify findings (direct vs transitive, shipped vs dev)
- [ ] Step 5: Deep-dive critical items with cve-impact-analysis
- [ ] Step 6: Report with upgrade paths
```

### Step 1: Discover dependencies

```bash
# Find manifests
find . -name 'package.json' -o -name 'package-lock.json' -o -name 'yarn.lock' \
  -o -name 'pnpm-lock.yaml' -o -name 'requirements*.txt' -o -name 'poetry.lock' \
  -o -name 'Pipfile.lock' -o -name 'go.mod' -o -name 'go.sum' \
  -o -name 'Cargo.toml' -o -name 'Cargo.lock' -o -name 'pom.xml' \
  -o -name 'Gemfile.lock' -o -name 'composer.lock' | head -50
```

For monorepos, audit each deployable artifact separately.

### Step 2: Ecosystem scanners (preferred)

Run when the toolchain is present — faster and more accurate than manual lookup:

| Ecosystem | Command |
|-----------|---------|
| npm | `npm audit --json` or `pnpm audit` |
| Python | `pip-audit` or `poetry audit` |
| Rust | `cargo audit` |
| Go | `govulncheck ./...` |
| .NET | `dotnet list package --vulnerable` |
| Ruby | `bundle audit` |
| Container | `grype` / `trivy` on image or filesystem |

Capture JSON output for traceability. If scanners are unavailable, proceed to Step 3.

See [security-scanning-tools.md](../../docs/security-scanning-tools.md) for recommended installs and IT request template. Per-OS install steps: [INSTALL.md](../../docs/INSTALL.md).

### Step 3: OSV batch lookup

For packages without scanner coverage:

```bash
# Example OSV query (adjust package ecosystem)
curl -sS -X POST https://api.osv.dev/v1/query \
  -H 'Content-Type: application/json' \
  -d '{"package": {"name": "lodash", "ecosystem": "npm"}, "version": "4.17.15"}'
```

For CVE details on specific IDs, use `nvd-cve-search --cve-id CVE-YYYY-NNNN`.

### Step 4: Classify findings

| Dimension | Triage note |
|-----------|-------------|
| Direct vs transitive | Direct fixes are team-owned |
| Dev vs prod dependency | Dev-only lowers runtime priority |
| Reachable vs unreachable | Use `govulncheck` / scanner reachability when available |
| Fixed version available | Prefer minor/patch bumps |

### Step 5: Critical escalation

For CVSS ≥ 7, KEV-listed, or network-exposed paths:

1. Run `cve-impact-analysis` if exploitability in this codebase is unclear
2. Run `cwe-code-analysis` if vulnerability class affects custom integration code

## Output format

```markdown
# Dependency security audit: [repo / branch]

## Scope
- Manifests: [list]
- Scan tools: [npm audit, cargo audit, ...]
- Date: [date]

## Summary
| Severity | Direct | Transitive | Total |
|----------|--------|------------|-------|
| Critical | N | N | N |
| High | N | N | N |
| ... | | | |

## Fix now (P0/P1)

| Package | Current | CVE | Severity | Fixed in | Path |
|---------|---------|-----|----------|----------|------|
| ... | ... | CVE-... | Critical | 2.1.4 | direct |

## Accept / defer
| Package | CVE | Rationale |
|---------|-----|-----------|
| ... | CVE-... | dev-only, not in production bundle |

## Upgrade commands
```bash
npm update <pkg>@<version>
```

## Recommended CI additions
- [Dependabot, npm audit in pipeline, etc.]
```

## Rules

- Prefer **lockfile-resolved versions** over manifest ranges
- Do not auto-bump dependencies without user approval — propose upgrades
- Scanner silence does not guarantee safety — note scan coverage limits
- For Yocto/embedded, include recipe versions (see `sbom-cve-triage`)
- Never commit credentials found during audit — flag and rotate immediately

## Related skills

- `sbom-cve-triage` — full component inventory workflows
- `nvd-cve-search` — CVE record lookup
- `cve-impact-analysis` — exposure verification
- `cisa-kev-review` — KEV prioritization

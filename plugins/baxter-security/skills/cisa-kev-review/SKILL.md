---
name: cisa-kev-review
description: Reviews CISA Known Exploited Vulnerabilities catalog entries against product components and prioritizes mandatory remediation. Use for federal compliance checks, KEV alert triage, or verifying hasKev CVEs from NVD searches.
---

# CISA KEV review

The [CISA KEV catalog](https://www.cisa.gov/known-exploited-vulnerabilities-catalog) lists vulnerabilities with confirmed active exploitation. Federal agencies must remediate on published deadlines; product teams should treat KEV-listed CVEs affecting shipped components as **P0**.

## When to use

- CISA publishes new KEV additions (subscribe to RSS)
- NVD search returns CVEs with `hasKev=true`
- Compliance audit requires KEV remediation tracking
- Release gate: zero unmitigated KEV matches in shipped SBOM

## Data sources

1. **CISA KEV JSON** (authoritative list): https://www.cisa.gov/sites/default/files/feeds/known_exploited_vulnerabilities.json
2. **NVD filter**: `nvd-cve-search` with `--has-kev` plus date or CPE filters
3. **Vendor advisories** for patch availability on embedded/LTS branches

Fetch KEV feed:

```bash
curl -sS -o /tmp/kev.json \
  https://www.cisa.gov/sites/default/files/feeds/known_exploited_vulnerabilities.json
```

## Workflow

```
KEV review:
- [ ] Step 1: Load current KEV catalog (or delta since last review)
- [ ] Step 2: Match KEV CVEs to product component inventory
- [ ] Step 3: Assess version exposure per component
- [ ] Step 4: Verify fixes with cve-impact-analysis
- [ ] Step 5: Track remediation against CISA due dates
- [ ] Step 6: Report status
```

### Step 1: Catalog snapshot

Record `dateReleased` from KEV JSON and filter:

- **New since last review**: `dateAdded` after last review date
- **Due soon**: `dueDate` within 30 days
- **Overdue**: `dueDate` in the past and not remediated

### Step 2: Component matching

For each KEV entry, extract from `vulnerabilityName` and `product` fields:

- Vendor / product name
- Affected versions if stated
- Required action (typically vendor patch)

Cross-reference with SBOM, lockfiles, and Yocto recipes (see `sbom-cve-triage` inventory steps).

### Step 3: Exposure assessment

| Result | Action |
|--------|--------|
| Component not in product | Document not applicable |
| Component present, version safe | Verify with patch evidence |
| Component present, vulnerable | P0 — open remediation ticket |
| Uncertain | Run `cve-impact-analysis` |

### Step 4: Remediation tracking

For each open KEV match:

| Field | Value |
|-------|-------|
| CVE ID | CVE-YYYY-NNNN |
| CISA due date | [date] |
| Component | [name + version] |
| Fix version / patch | [from vendor] |
| Ticket | [ADO ID] |
| Status | Open / In progress / Mitigated / Not applicable |
| Evidence | [recipe bump, patch file, scan result] |

## Output format

```markdown
# CISA KEV review: [date]

## Catalog snapshot
- KEV total: N
- New since [last review]: N
- Due within 30 days: N

## Product matches

| CVE | Product (KEV) | Our component | Version | Due date | Status | Action |
|-----|---------------|---------------|---------|----------|--------|--------|
| CVE-... | ... | openssl | 3.0.12 | 2024-08-15 | Open | Bump to 3.0.14 |

## Not applicable (documented)
- CVE-... — [component absent / not shipped]

## Overdue / escalations
[List requiring program attention]

## Next review
[Recommended date — weekly during active KEV additions]
```

## Rules

- KEV listing means **exploitation in the wild** — do not downgrade solely on low CVSS
- CISA due dates apply to federal systems; product teams should align SLAs to risk policy
- "Not applicable" requires evidence (component absent or version verified fixed)
- Re-run after each image/build when recipes or dependencies change
- Subscribe to KEV updates: https://www.cisa.gov/known-exploited-vulnerabilities-catalog

## Related skills

- `nvd-cve-search` — `--has-kev` NVD queries
- `sbom-cve-triage` — component inventory
- `cve-impact-analysis` — patch and version verification

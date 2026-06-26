# Changelog

## 0.2.0

- Add `nvd-cve-search` skill with `scripts/nvd_search.py` for NVD API 2.0 date-range and filter queries
- Add `~/.config/nvd-api.env` support and `scripts/setup-nvd-env.mjs` for API key configuration
- Add `cwe-code-analysis` skill with CWE pattern reference for codebase weakness hunting
- Add `sbom-cve-triage` skill for SBOM-driven CVE prioritization
- Add `dependency-security-audit` skill for lockfile and ecosystem scanner workflows
- Add `cisa-kev-review` skill for CISA Known Exploited Vulnerabilities tracking
- Add `threat-model-sketch` skill for lightweight STRIDE threat modeling
- Cross-link existing `security-review` and `cve-impact-analysis` skills to new workflows
- Add `docs/security-scanning-tools.md` with recommended scanner installs and IT request template
- Add `docs/INSTALL.md` with Linux, macOS, and Windows installation instructions
- Add `scripts/check-security-tools.sh` and `scripts/check-security-tools.ps1` verification scripts

## 0.1.2

- Republish `security-review` agent for marketplace deployment

## 0.1.1

- Add `cve-impact-analysis` skill: authoritative CVE lookup, codebase exposure check, and Yocto patch verification

## 0.1.0

- Initial security skill and agent

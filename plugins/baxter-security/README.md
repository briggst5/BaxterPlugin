# Baxter Security

Optional domain plugin for security-focused skills, CVE/NVD triage, and product security workflows.

| | |
|--|--|
| **Audience** | Security engineers, release managers, embedded teams |
| **Install guide** | [docs/INSTALL.md](docs/INSTALL.md) (Linux, macOS, Windows) |
| **Scanner tools** | [docs/security-scanning-tools.md](docs/security-scanning-tools.md) |

See also [getting started](../../docs/getting-started.md) for install order with other plugins.

## Installation

**Linux, macOS, and Windows:** [docs/INSTALL.md](docs/INSTALL.md)

Quick start after cloning BaxterPlugin:

| OS | Baseline | Verify |
|----|----------|--------|
| Linux / macOS | `./scripts/bootstrap-dev-machine.sh` then `node plugins/baxter-security/scripts/setup-nvd-env.mjs` | `plugins/baxter-security/scripts/check-security-tools.sh` |
| Windows | `.\scripts\bootstrap-dev-machine.ps1` then `node plugins\baxter-security\scripts\setup-nvd-env.mjs` | `plugins\baxter-security\scripts\check-security-tools.ps1` |

Install recommended scanners (Grype, Syft, Semgrep, etc.) per platform in [docs/INSTALL.md](docs/INSTALL.md).

## Contents

### Skills

| Skill | Purpose |
|-------|---------|
| `security-review` | Auth, API, and data-handling PR review checklist |
| `cve-impact-analysis` | Single-CVE exposure and Yocto patch verification |
| `nvd-cve-search` | NVD API 2.0 queries by date range, CWE, CPE, KEV |
| `cwe-code-analysis` | Map CWE weakness types to codebase patterns |
| `sbom-cve-triage` | Prioritize CVEs against SBOM / component inventory |
| `dependency-security-audit` | Lockfile and ecosystem vulnerability scanning |
| `cisa-kev-review` | CISA Known Exploited Vulnerabilities remediation tracking |
| `threat-model-sketch` | Lightweight STRIDE threat modeling |

### Agents

- `security-review`

## NVD API key

Request a key at https://nvd.nist.gov/developers/request-an-api-key

Store in:

- Linux / macOS: `~/.config/nvd-api.env`
- Windows: `%USERPROFILE%\.config\nvd-api.env`

```bash
node scripts/setup-nvd-env.mjs
# Set NVD_API_KEY=your-key-here in the file created above
```

See [docs/INSTALL.md](docs/INSTALL.md) for full setup on each OS.

## Recommended scanning tools

- **Install guide:** [docs/INSTALL.md](docs/INSTALL.md) — per-OS commands
- **Tool rationale & IT template:** [docs/security-scanning-tools.md](docs/security-scanning-tools.md)

**Quick picks:** Grype + Syft (or Trivy) for universal CVE/SBOM scans; Semgrep for CWE/SAST; ecosystem tools (`npm audit`, `dotnet list package --vulnerable`, `pip-audit`, `cargo audit`, `govulncheck`) per stack.

## Typical workflows

**New CVE advisory** → `cve-impact-analysis` → `cwe-code-analysis` (if custom code involved)

**Release vulnerability gate** → `dependency-security-audit` → `sbom-cve-triage` → `cisa-kev-review`

**CVEs published this quarter** → `nvd-cve-search` (date range) → filter → `cve-impact-analysis`

**New feature design** → `threat-model-sketch` → `security-review` at implementation

## Cursor distribution

Set to **Default On** for security/engineering SCIM groups, **Default Off** for others.

## Changelog

See [CHANGELOG.md](CHANGELOG.md).

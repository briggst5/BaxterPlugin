# Recommended security scanning tools

Tools that pair well with **baxter-security** skills. Request these via IT/MDM or install locally where policy allows.

**Installation (Linux, macOS, Windows):** [INSTALL.md](INSTALL.md)

**Verify installs:** `scripts/check-security-tools.sh` (Linux/macOS) or `scripts/check-security-tools.ps1` (Windows).

## How skills use scanners

| Skill | Primary tools |
|-------|----------------|
| `dependency-security-audit` | Ecosystem scanners, Grype, Trivy, OSV |
| `sbom-cve-triage` | Syft (SBOM) + Grype/Trivy (CVE match) |
| `nvd-cve-search` | NVD API script + optional NVD key in `~/.config/nvd-api.env` |
| `cve-impact-analysis` | NVD/CVE.org + patch verification in repo |
| `cwe-code-analysis` | Semgrep, SonarQube, manual pattern search |
| `cisa-kev-review` | CISA KEV JSON feed + Grype/NVD filters |

Agents can run scanners and interpret JSON output. Without installed tools, skills fall back to NVD/OSV API calls and `rg` pattern searches — slower and less complete.

---

## Tier 1 — Baseline (all developers)

| Tool | Purpose | Install | IT notes |
|------|---------|---------|----------|
| **Python 3.10+** | `nvd_search.py`, pip-audit | `scripts/bootstrap-dev-machine.sh` in BaxterPlugin | Usually pre-approved |
| **uv** | Fast Python tool installs | Same bootstrap script | Astral.sh installer |
| **NVD API key** | Higher NVD rate limits | [INSTALL.md](INSTALL.md) — `~/.config/nvd-api.env` | No install; free registration |

See [INSTALL.md](INSTALL.md) for Linux, macOS, and Windows bootstrap steps instead of platform-specific snippets below.

---

## Tier 2 — Universal scanners (recommended for IT allowlist)

Best ROI for mixed stacks (npm, .NET, containers, firmware images). One install covers many repos.

### Grype + Syft (Anchore)

| | |
|--|--|
| **Why** | Filesystem, container image, and SBOM CVE scanning; excellent JSON for agent triage |
| **Skills** | `dependency-security-audit`, `sbom-cve-triage` |
| **Install** | https://github.com/anchore/grype#installation |
| **License** | Apache 2.0 |

```bash
# Example: scan repo root (lockfiles + binaries)
grype dir:. -o json

# SBOM then CVE match
syft . -o cyclonedx-json > sbom.json
grype sbom:sbom.json -o json
```

### Trivy (Aqua)

| | |
|--|--|
| **Why** | Similar to Grype; strong container/filesystem scanning; `trivy fs` for projects |
| **Skills** | `dependency-security-audit`, `sbom-cve-triage` |
| **Install** | https://trivy.dev/latest/getting-started/installation/ |
| **License** | Apache 2.0 |

```bash
trivy fs --scanners vuln --format json .
trivy image --format json myimage:tag
```

**IT request:** Allowlist **Grype + Syft** *or* **Trivy** (either pair is sufficient; Grype+Syft is slightly better for SBOM-first workflows).

---

## Tier 3 — Ecosystem-native scanners

Use when the repo is single-stack and CI already runs these.

| Ecosystem | Tool | Install | Command |
|-----------|------|---------|---------|
| **Node / npm** | npm (built-in) | Node.js LTS | `npm audit --json` |
| **.NET** | .NET SDK | https://dot.net | `dotnet list package --vulnerable` |
| **Python** | pip-audit | `uv tool install pip-audit` | `pip-audit -f json` |
| **Rust** | cargo-audit | `cargo install cargo-audit` | `cargo audit --json` |
| **Go** | govulncheck | `go install golang.org/x/vuln/cmd/govulncheck@latest` | `govulncheck -json ./...` |
| **Ruby** | bundler-audit | `gem install bundler-audit` | `bundle audit check` |

**IT request:** Ensure Node LTS, .NET SDK, and Go/Rust toolchains are on the standard dev image; add `pip-audit` and `cargo-audit` to optional security bundle.

---

## Tier 4 — Static analysis (CWE / code patterns)

| Tool | Purpose | Install | Skills |
|------|---------|---------|--------|
| **Semgrep** | SAST with CWE-aligned rules; fast local runs | https://semgrep.dev/docs/getting-started/ | `cwe-code-analysis`, `security-review` |
| **SonarQube / SonarCloud** | Org SAST + quality gates | SonarCloud or internal SonarQube | Baxter Gradle projects already reference `sonar.token` |

```bash
# Semgrep — auto rules + optional custom rules
semgrep scan --config auto --json .
semgrep scan --config p/owasp-top-ten .
```

**IT request:** Semgrep CLI for local pre-PR scans; Sonar token in `gradle.properties` or env (see FutureState dev environment wiki).

---

## Tier 5 — Embedded / Yocto / device images

| Tool | Purpose | Notes |
|------|---------|-------|
| **Grype / Trivy** | Scan built rootfs, SDK sysroot, or OCI image from CI | Primary recommendation for firmware teams |
| **Yocto `cve-check`** | Recipe-level CVE check at build | `INHERIT += "cve-check"` in distro config; uses NVD data |
| **cve-search** (optional) | Local CVE DB CLI | https://github.com/cve-search/cve-search — heavier; usually CI-only |

For Yocto exposure and patch verification, skills still need recipe access in the repo — scanners do not replace `cve-impact-analysis` patch matching.

---

## Tier 6 — CI / program-level (not per-developer)

| Tool | Purpose |
|------|---------|
| **Dependabot / GitHub Advanced Security** | Automated dependency PRs on ADO/GitHub |
| **Azure Defender for DevOps** | ADO-native scanning if org uses it |
| **OWASP Dependency-Check** | Jenkins/CI dependency scans (XML/JSON output) |
| **CISA KEV feed** | `curl` to JSON URL — no install (`cisa-kev-review` skill) |

---

## Suggested IT bundle

**Minimum security dev bundle**

1. Python 3.10+ and uv (Baxter bootstrap)
2. Grype + Syft *or* Trivy
3. Semgrep CLI
4. NVD API key policy (developers can register; key stored in `~/.config/nvd-api.env`)

**Full-stack bundle** (add per language use)

5. Node.js LTS (npm audit)
6. .NET SDK (`dotnet list package --vulnerable`)
7. `pip-audit`, `cargo-audit`, `govulncheck` via standard toolchain images

---

## Sample IT / MDM request

> **Request:** Security scanning tools for Baxter Cursor security plugin and local pre-PR checks.
>
> **Binaries to allowlist/install:**
> - Grype and Syft (https://github.com/anchore/grype, https://github.com/anchore/syft) — OR Trivy (https://trivy.dev)
> - Semgrep CLI (https://semgrep.dev)
>
> **Already on standard dev image (confirm):** Python 3.10+, Node LTS, .NET SDK as required by project.
>
> **Optional Python tools:** `pip-audit` via `uv tool install pip-audit`
>
> **Network:** Outbound HTTPS to `services.nvd.nist.gov`, `api.osv.dev`, `www.cisa.gov` (CVE/KEV feeds), Grype/Trivy vulnerability DB updates.
>
> **Credentials:** Developers obtain personal NVD API keys; stored locally in `~/.config/nvd-api.env` (not in source control).
>
> **License:** Grype, Syft, Trivy, Semgrep OSS — Apache 2.0 / LGPL / compatible OSS.

---

## Verification

After install, run the check script from `plugins/baxter-security`:

- Linux / macOS: `./scripts/check-security-tools.sh`
- Windows: `.\scripts\check-security-tools.ps1`

Full smoke tests and troubleshooting: [INSTALL.md](INSTALL.md#step-5--verify-installation).

Ask the agent: *"Run dependency-security-audit on this repo"* — it should detect installed scanners and use them automatically.

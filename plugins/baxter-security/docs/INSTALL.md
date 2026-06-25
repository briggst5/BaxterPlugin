# Baxter Security — Installation

Install the **baxter-security** Cursor plugin, baseline tooling, NVD API credentials, and optional security scanners on **Linux**, **macOS**, or **Windows**.

For tool rationale and IT allowlist text, see [security-scanning-tools.md](security-scanning-tools.md).

---

## What you are installing

| Layer | Components | Required? |
|-------|------------|-----------|
| **Plugin** | baxter-security skills/agents in Cursor | Yes |
| **Baseline** | Python 3.10+, uv, NVD API key file | Yes (for NVD scripts) |
| **Universal scanners** | Grype + Syft *or* Trivy | Recommended |
| **SAST** | Semgrep | Recommended |
| **Ecosystem** | npm, .NET SDK, pip-audit, cargo-audit, govulncheck | Per project stack |

---

## Config file locations (all platforms)

| Item | Linux / macOS | Windows |
|------|---------------|---------|
| NVD API key | `~/.config/nvd-api.env` | `%USERPROFILE%\.config\nvd-api.env` |
| uv / pip tools | `~/.local/bin` | `%USERPROFILE%\.local\bin` |

Example `nvd-api.env`:

```bash
NVD_API_KEY=your-key-here
```

Request a key: https://nvd.nist.gov/developers/request-an-api-key

---

## Step 1 — Install the Cursor plugin

1. Open **Cursor** → **Settings** → **Plugins** (or your org marketplace).
2. Install **Baxter Security** (`baxter-security`).
3. Enable the plugin for your user or team (org may set Default On for security/engineering groups).
4. Reload Cursor if skills do not appear immediately.

No separate plugin binary is installed — skills load from the plugin package.

---

## Step 2 — Baseline (Python + uv)

### Linux

```bash
# From BaxterPlugin repo root
./scripts/bootstrap-dev-machine.sh

# Ensure ~/.local/bin is on PATH (add to ~/.bashrc if needed)
export PATH="$HOME/.local/bin:$PATH"

python3 --version    # 3.10+
uv --version
```

**WSL2:** Use the Linux steps inside WSL. Store `nvd-api.env` in the Linux home (`~/.config/`), not the Windows profile, when running tools from WSL.

### macOS

```bash
# From BaxterPlugin repo root
./scripts/bootstrap-dev-machine.sh

# Apple Silicon / Intel — ensure local bin on PATH
export PATH="$HOME/.local/bin:$PATH"

python3 --version
uv --version
```

If Python is missing, install from https://www.python.org/downloads/ or `brew install python`.

### Windows (PowerShell)

```powershell
# From BaxterPlugin repo root
.\scripts\bootstrap-dev-machine.ps1

# Refresh PATH in this session
$env:Path = "$env:USERPROFILE\.local\bin;$env:Path"

python --version
uv --version
```

Install Python 3.10+ from https://www.python.org/downloads/ if needed (check **Add python.exe to PATH** during setup).

---

## Step 3 — NVD API key file

### Linux / macOS

```bash
cd plugins/baxter-security
node scripts/setup-nvd-env.mjs
# Edit ~/.config/nvd-api.env — set NVD_API_KEY=...

chmod 600 ~/.config/nvd-api.env
```

### Windows (PowerShell)

```powershell
cd plugins\baxter-security
node scripts\setup-nvd-env.mjs
# Edit %USERPROFILE%\.config\nvd-api.env — set NVD_API_KEY=...

notepad "$env:USERPROFILE\.config\nvd-api.env"
```

`chmod` is not required on Windows; restrict file ACLs via IT policy if needed.

### Test NVD access

**Linux / macOS:**

```bash
python3 plugins/baxter-security/skills/nvd-cve-search/scripts/nvd_search.py \
  --cve-id CVE-2024-21762 | head -20
```

**Windows:**

```powershell
python plugins\baxter-security\skills\nvd-cve-search\scripts\nvd_search.py `
  --cve-id CVE-2024-21762 | Select-Object -First 20
```

---

## Step 4 — Security scanners (recommended)

Pick **Grype + Syft** (SBOM-first) **or** **Trivy** (single tool). Install **Semgrep** for CWE/SAST. Use ecosystem scanners for your stack.

### Grype + Syft

#### Linux

```bash
# Install to ~/.local/bin (no root)
curl -sSfL https://raw.githubusercontent.com/anchore/grype/main/install.sh | sh -s -- -b ~/.local/bin
curl -sSfL https://raw.githubusercontent.com/anchore/syft/main/install.sh | sh -s -- -b ~/.local/bin

export PATH="$HOME/.local/bin:$PATH"
grype version
syft version
```

**apt (Debian/Ubuntu)** — if your org provides packages:

```bash
# Optional — only if IT distributes .deb installs
sudo apt-get update && sudo apt-get install -y grype syft
```

#### macOS

```bash
brew install grype syft
grype version && syft version
```

#### Windows (PowerShell)

**winget** (Windows 11 / current Windows 10):

```powershell
winget install Anchore.Grype
winget install Anchore.Syft
```

**Scoop** (if winget unavailable):

```powershell
scoop install grype syft
```

**Manual** — download releases from GitHub and add to PATH:

- https://github.com/anchore/grype/releases
- https://github.com/anchore/syft/releases

---

### Trivy (alternative to Grype + Syft)

#### Linux

```bash
# Official install script
curl -sfL https://raw.githubusercontent.com/aquasecurity/trivy/main/contrib/install.sh | sh -s -- -b ~/.local/bin
export PATH="$HOME/.local/bin:$PATH"
trivy --version
```

#### macOS

```bash
brew install trivy
trivy --version
```

#### Windows (PowerShell)

```powershell
winget install AquaSecurity.Trivy
# or: scoop install trivy
trivy --version
```

---

### Semgrep

#### Linux / macOS

```bash
# Via uv (matches Baxter Python baseline)
uv tool install semgrep
# Ensure ~/.local/bin on PATH
semgrep --version
```

**macOS alternative:**

```bash
brew install semgrep
```

#### Windows (PowerShell)

```powershell
uv tool install semgrep
$env:Path = "$env:USERPROFILE\.local\bin;$env:Path"
semgrep --version
```

**Alternative:** `pip install semgrep` if uv is not available.

---

### Ecosystem scanners (install what your repo uses)

| Tool | Linux / macOS | Windows (PowerShell) |
|------|---------------|----------------------|
| **npm audit** | Install Node.js LTS → `npm audit --json` | Same |
| **.NET** | Install .NET SDK → `dotnet list package --vulnerable` | Same |
| **pip-audit** | `uv tool install pip-audit` | `uv tool install pip-audit` |
| **cargo-audit** | `cargo install cargo-audit` | `cargo install cargo-audit` |
| **govulncheck** | `go install golang.org/x/vuln/cmd/govulncheck@latest` | Same (Go on PATH) |

---

## Step 5 — Verify installation

Run the check script from `plugins/baxter-security`:

### Linux / macOS

```bash
cd plugins/baxter-security
./scripts/check-security-tools.sh
```

### Windows (PowerShell)

```powershell
cd plugins\baxter-security
.\scripts\check-security-tools.ps1
```

### Manual smoke tests

**Linux / macOS:**

```bash
grype dir:. -o table | head -20          # or: trivy fs --scanners vuln .
semgrep scan --config auto --quiet .
python3 skills/nvd-cve-search/scripts/nvd_search.py --cve-id CVE-2024-21762
```

**Windows:**

```powershell
grype dir:. -o table | Select-Object -First 20
semgrep scan --config auto --quiet .
python skills\nvd-cve-search\scripts\nvd_search.py --cve-id CVE-2024-21762
```

In Cursor, ask the agent: *"Run dependency-security-audit on this repo"* — it should detect installed scanners automatically.

---

## Network requirements

Allow outbound HTTPS for:

| Endpoint | Used by |
|----------|---------|
| `services.nvd.nist.gov` | NVD CVE API |
| `api.osv.dev` | OSV vulnerability DB |
| `www.cisa.gov` | CISA KEV catalog |
| Grype/Trivy DB mirrors | Scanner vulnerability data updates |
| `semgrep.dev` | Semgrep rule sync (if using registry rules) |

Corporate proxies: configure `HTTP_PROXY` / `HTTPS_PROXY` for CLI tools per your IT docs.

---

## Troubleshooting

| Symptom | Fix |
|---------|-----|
| `python3: command not found` (Windows) | Use `python` or reinstall Python with PATH option |
| `uv: command not found` | Re-run bootstrap; add `%USERPROFILE%\.local\bin` or `~/.local/bin` to PATH |
| NVD returns 403 / rate limit | Set `NVD_API_KEY` in `nvd-api.env`; wait 30s between burst requests |
| `grype` / `trivy` not found after install | Open new terminal; confirm install dir is on PATH |
| Semgrep not found after `uv tool install` | Add uv tool bin dir: `export PATH="$HOME/.local/bin:$PATH"` |
| WSL vs Windows path confusion | Run scanners and NVD script from the same environment (WSL or native) consistently |
| Scanner DB download blocked | Ask IT to allowlist Trivy/Grype update URLs or use offline DB per vendor docs |

---

## Uninstall / cleanup

| Item | Action |
|------|--------|
| Cursor plugin | Disable or uninstall in Cursor plugin settings |
| `nvd-api.env` | Delete `~/.config/nvd-api.env` (or Windows equivalent) |
| uv tools | `uv tool uninstall semgrep pip-audit` |
| brew packages | `brew uninstall grype syft trivy semgrep` |
| winget | `winget uninstall <package-id>` |

---

## Related docs

- [security-scanning-tools.md](security-scanning-tools.md) — tool tiers, IT request template, skill mapping
- [README.md](../README.md) — skills list and workflows

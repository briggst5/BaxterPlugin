# Baxter Polarion

Standalone plugin for the vendored **Polarion MCP** server — Linux and Windows, no manual clone or local compile.

## Install

Install `baxter-polarion` from the Baxter team marketplace (often **Default On** alongside `baxter-product-owner` for teams using Polarion).

Pair with `baxter-product-owner` for PO/RTE skills that call Polarion tools (`polarion-requirements-steward`, traceability agents, etc.).

## One-time setup

```bash
node plugins/baxter-polarion/scripts/setup-polarion-env.mjs
# Edit ~/.config/polarion-mcp.env — POLARION_URL, POLARION_USER, POLARION_PAT, POLARION_PROJECT
```

Enable **polarion-mcp** in Cursor **Settings → MCP**.

Requires **Node.js 20+**.

## How it works

| Component | Location |
|-----------|----------|
| Polarion MCP source | `mcp-servers/` |
| Cross-platform launcher | `scripts/launch-polarion-mcp.mjs` |
| Required prebuilt binaries | `bin/linux-x64/polarion-mcp`, `bin/win-x64/polarion-mcp.exe` |

Server starts immediately from the bundled executable.

## Maintainers

### Update from upstream PolarionMCP

```bash
./scripts/sync-polarion-dotnet.sh
```

Syncs from the PolarionMCP `main` branch by default.

### Build and bundle Linux + Windows binaries

```bash
cd plugins/baxter-polarion
./scripts/build-polarion-binaries.sh
```

Commit the generated executables so users do not compile locally.

### CI automation

Use workflow `.github/workflows/sync-polarion-dotnet.yml` to auto-sync from upstream and open a PR with updated source + bundled binaries.
When sync changes are detected, it also auto-bumps plugin version in both manifests and prepends `CHANGELOG.md`.

See [CHANGELOG.md](CHANGELOG.md).

# PolarionMCP Operator Guide

This guide is for operators who need to install, configure, run, and troubleshoot
the `.NET` Polarion MCP server without reading source code.

## 1) Install

### Linux (x64)

1. Download the latest `linux-x64` release archive.
2. Extract it:
   ```bash
   mkdir -p ~/polarion-mcp
   tar -xzf polarion-mcp-linux-x64.tar.gz -C ~/polarion-mcp
   ```
3. Ensure executable bit is set:
   ```bash
   chmod +x ~/polarion-mcp/polarion-mcp
   ```

### Windows (x64)

1. Download the latest `win-x64` release archive.
2. Extract it to a folder (for example `C:\polarion-mcp`).
3. Confirm the executable exists: `C:\polarion-mcp\polarion-mcp.exe`.

## 2) Configure

Configuration file path:

- Linux: `~/.config/polarion-mcp.env`
- Windows: `%USERPROFILE%\.config\polarion-mcp.env`

Create the file from template and secure it:

```bash
mkdir -p ~/.config
cp polarion-mcp.env.example ~/.config/polarion-mcp.env
chmod 600 ~/.config/polarion-mcp.env
```

### Required variables

- `POLARION_URL`
- `POLARION_USER`
- One of:
  - `POLARION_PASSWORD`
  - `POLARION_PAT`

### Recommended baseline (self-signed cert + reconnect)

```dotenv
POLARION_URL=https://polarion.example.local/polarion
POLARION_USER=myuser
POLARION_PASSWORD=secret
POLARION_PROJECT=MY_PROJECT
POLARION_TLS_SKIP_VERIFY=true
POLARION_SESSION_MAX_RETRIES=1
POLARION_SESSION_IDLE_REFRESH_SECONDS=300
```

### More secure TLS option (custom CA file)

```dotenv
POLARION_TLS_SKIP_VERIFY=false
POLARION_TLS_CA_FILE=/path/to/polarion-ca.pem
```

## 3) Wire Into Cursor

Add/update `.cursor/mcp.json`:

```json
{
  "mcpServers": {
    "polarion-mcp": {
      "command": "/absolute/path/to/polarion-mcp",
      "args": []
    }
  }
}
```

For Windows use the full `.exe` path.

## 4) Verify

Use this checklist.

### Linux verify example

1. Start the server manually:
   ```bash
   /absolute/path/to/polarion-mcp
   ```
2. Confirm there are no immediate startup errors in the terminal.
3. In Cursor chat, type:
   ```text
   Call get_work_item with work_item_id "PLT1-2668"
   ```
4. Expected result:
   - Success: JSON for work item `PLT1-2668` (id/title/status/etc.).
   - Failure: JSON error with a clear reason (auth, URL, TLS, project, etc.).

### Windows verify example

1. Start the server manually (PowerShell):
   ```powershell
   C:\polarion-mcp\polarion-mcp.exe
   ```
2. Confirm there are no immediate startup errors in the PowerShell window.
3. In Cursor chat, type:
   ```text
   Call get_work_item with work_item_id "PLT1-2668"
   ```
4. Expected result:
   - Success: JSON for work item `PLT1-2668` (id/title/status/etc.).
   - Failure: JSON error with a clear reason (auth, URL, TLS, project, etc.).

## 5) Troubleshooting

### Symptom: `Not authorized`

- Likely causes:
  - Expired/invalid credentials
  - Session expired and reconnect failed
- Fix:
  1. Re-check `POLARION_USER` + `POLARION_PASSWORD`/`POLARION_PAT`
  2. Restart server
  3. If using PAT, verify token scope and validity

### Symptom: TLS / certificate validation error

- Likely causes:
  - Corporate CA not in the OS trust store (common on WSL2)
  - Self-signed cert with strict validation enabled
  - Wrong `POLARION_TLS_CA_FILE` path
  - **Linux/WSL (.NET 8):** older builds used `ServicePointManager`, which WCF ignores on .NET Core — `POLARION_TLS_SKIP_VERIFY=true` logged a warning but TLS still failed
- Fix:
  1. Use a build with `TlsHttpClientEndpointBehavior` (HttpClientHandler-based TLS)
  2. Set `POLARION_TLS_SKIP_VERIFY=false` and `POLARION_TLS_CA_FILE=/path/to/corporate-ca.pem` (preferred)
  3. Or set `POLARION_TLS_SKIP_VERIFY=true` for a quick dev workaround
  4. On WSL, install Windows-exported CAs into the Linux trust store if you rely on system validation

### Symptom: frequent reconnect loops / timeouts

- Likely causes:
  - Server idle timeout lower than your refresh settings
- Fix:
  1. Set `POLARION_SESSION_IDLE_REFRESH_SECONDS` below server idle timeout
  2. Keep `POLARION_SESSION_MAX_RETRIES=1` unless directed otherwise

### Symptom: `POLARION_PROJECT is required`

- Likely causes:
  - Project not set for document/query tools
- Fix:
  1. Set `POLARION_PROJECT=<project-id>`
  2. Retry request

## 6) Recovery Steps

Credential rotation:

1. Update credentials in `~/.config/polarion-mcp.env`
2. Restart the server process
3. Re-run a simple tool call

Hard reset:

1. Backup current env file
2. Recreate from `polarion-mcp.env.example`
3. Re-enter minimal required fields
4. Test again

## 7) Operator Changelog Template

For each release, include:

- What changed for operators
- New/changed env variables
- New required installation/configuration steps
- Known issues and workarounds

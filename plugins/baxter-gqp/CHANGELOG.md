# Changelog

## 0.3.1

- Device-code auth default — no custom IT Entra app or localhost redirect required
- End-user template uses Entra RBAC to Azure Search/OpenAI (Key Vault optional for maintainers)
- Credential chain: Azure CLI → device-code → DefaultAzureCredential → browser last
- Rebuilt bundled binaries for linux-x64, osx-arm64, osx-x64, win-x64

## 0.3.0

- Add macOS binary support (`osx-arm64`, `osx-x64`) with platform-aware launcher
- Expand maintainer build script to publish all four target RIDs
- Update MCP README platform list

## 0.2.0

- Sync GQP MCP source from upstream (TLS transport, Key Vault bootstrap, credential factory updates)
- Add `gqp-compliance-advisor` skill for cited compliance Q&A
- Add `gqp-compliance-citations` rule for citation verification workflow
- Add `gqp-compliance-reviewer` agent for document/plan citation audits
- Add `scripts/sync-gqp-dotnet.sh` maintainer sync script

## 0.1.0

- Initial baxter-gqp plugin with bundled gqp-mcp binary and gqp-knowledge MCP server

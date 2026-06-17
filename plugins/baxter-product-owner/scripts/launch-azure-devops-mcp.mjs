#!/usr/bin/env node
/**
 * Cross-platform Azure DevOps MCP launcher.
 * Loads ~/.config/azure-devops-mcp.env and runs npx @azure-devops/mcp.
 */
import { spawnSync } from "node:child_process";
import { existsSync, readFileSync } from "node:fs";
import { homedir, platform } from "node:os";
import { join } from "node:path";

function configPath() {
  return join(homedir(), ".config", "azure-devops-mcp.env");
}

function loadEnvFile(path) {
  if (!existsSync(path)) {
    return false;
  }
  for (const rawLine of readFileSync(path, "utf8").split(/\r?\n/)) {
    const line = rawLine.trim();
    if (!line || line.startsWith("#") || !line.includes("=")) {
      continue;
    }
    const idx = line.indexOf("=");
    const key = line.slice(0, idx).trim();
    let value = line.slice(idx + 1).trim();
    if (
      (value.startsWith('"') && value.endsWith('"')) ||
      (value.startsWith("'") && value.endsWith("'"))
    ) {
      value = value.slice(1, -1);
    }
    if (key) {
      process.env[key] = value;
    }
  }
  return true;
}

function isWindows() {
  return platform() === "win32";
}

function main() {
  if (!loadEnvFile(configPath())) {
    console.error(`Azure DevOps MCP is not configured. Create ${configPath()}`);
    console.error("See plugins/baxter-product-owner/README.md for ADO_ORG and PAT format.");
    process.exit(1);
  }

  const org = process.env.ADO_ORG;
  const auth = process.env.ADO_AUTH || "pat";
  if (!org) {
    console.error("ADO_ORG is missing in azure-devops-mcp.env");
    process.exit(1);
  }

  const args = [
    "-y",
    "@azure-devops/mcp",
    org,
    "-d",
    "core",
    "wiki",
    "work",
    "work-items",
    "--authentication",
    auth,
  ];

  const result = spawnSync("npx", args, {
    stdio: "inherit",
    env: process.env,
    shell: isWindows(),
  });

  if (result.error) {
    console.error(`Failed to run npx: ${result.error.message}`);
    process.exit(1);
  }
  process.exit(result.status ?? 1);
}

main();

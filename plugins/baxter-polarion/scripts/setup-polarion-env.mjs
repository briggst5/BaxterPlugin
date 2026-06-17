#!/usr/bin/env node
/**
 * Copy polarion-mcp.env.example to ~/.config/polarion-mcp.env if missing.
 */
import { copyFileSync, existsSync, mkdirSync } from "node:fs";
import { homedir } from "node:os";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";

const __dirname = dirname(fileURLToPath(import.meta.url));
const root = join(__dirname, "..");
const example = join(root, "mcp-servers", "polarion-mcp.env.example");
const configDir = join(homedir(), ".config");
const target = join(configDir, "polarion-mcp.env");

if (existsSync(target)) {
  console.log(`Already exists: ${target}`);
  console.log("Edit that file with your Polarion URL, user, and PAT/password.");
  process.exit(0);
}

if (!existsSync(example)) {
  console.error(`Example file not found: ${example}`);
  process.exit(1);
}

mkdirSync(configDir, { recursive: true });
copyFileSync(example, target);
console.log(`Created ${target}`);
console.log("Edit it with your Polarion credentials, then enable polarion-mcp in Cursor MCP settings.");

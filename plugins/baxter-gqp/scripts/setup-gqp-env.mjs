#!/usr/bin/env node
/**
 * Copy gqp-mcp.env.example to ~/.config/gqp-mcp.env if missing.
 */
import { copyFileSync, chmodSync, existsSync, mkdirSync } from "node:fs";
import { homedir } from "node:os";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";

const __dirname = dirname(fileURLToPath(import.meta.url));
const root = join(__dirname, "..");
const example = join(root, "mcp-servers", "gqp-mcp.env.example");
const configDir = join(homedir(), ".config");
const target = join(configDir, "gqp-mcp.env");

if (existsSync(target)) {
  console.log(`Already exists: ${target}`);
  process.exit(0);
}

if (!existsSync(example)) {
  console.error(`Example file not found: ${example}`);
  process.exit(1);
}

mkdirSync(configDir, { recursive: true });
copyFileSync(example, target);
try {
  chmodSync(target, 0o600);
} catch {
  // Windows may not support chmod; ignore.
}
console.log(`Created ${target}`);
console.log("Enable gqp-knowledge in Cursor MCP settings — browser sign-in runs automatically on first use.");

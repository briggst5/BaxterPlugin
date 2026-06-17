#!/usr/bin/env node
/**
 * Cross-platform Polarion MCP launcher for Cursor / VS Code plugins.
 *
 * Runtime model: binary-only.
 * - Requires vendored self-contained binary under bin/<rid>/polarion-mcp[.exe]
 * - Never builds or compiles on user machines
 *
 * Credentials are loaded by polarion-mcp from ~/.config/polarion-mcp.env.
 */
import { spawnSync } from "node:child_process";
import { existsSync } from "node:fs";
import { homedir, platform } from "node:os";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";

const __dirname = dirname(fileURLToPath(import.meta.url));

function pluginRoot() {
  return process.env.CURSOR_PLUGIN_ROOT || process.env.PLUGIN_ROOT || join(__dirname, "..");
}

function configPath() {
  return join(homedir(), ".config", "polarion-mcp.env");
}

function isWindows() {
  return platform() === "win32";
}

function rid() {
  return isWindows() ? "win-x64" : "linux-x64";
}

function binaryName() {
  return isWindows() ? "polarion-mcp.exe" : "polarion-mcp";
}

function bundledBinaryPath(root) {
  return join(root, "bin", rid(), binaryName());
}

function run(cmd, args) {
  const result = spawnSync(cmd, args, {
    stdio: "inherit",
    env: process.env,
    shell: isWindows(),
  });
  if (result.error) {
    console.error(`Failed to run ${cmd}: ${result.error.message}`);
    process.exit(1);
  }
  if (result.status !== 0) {
    process.exit(result.status ?? 1);
  }
}

function printSetupHelp(root) {
  const example = join(root, "mcp-servers", "polarion-mcp.env.example");
  console.error("");
  console.error("Polarion MCP is not configured.");
  console.error(`Create ${configPath()} with your Polarion credentials.`);
  console.error("");
  console.error("Quick setup:");
  console.error(`  node ${join(__dirname, "setup-polarion-env.mjs")}`);
  console.error("");
  console.error(`Example file: ${example}`);
  console.error("");
}

function main() {
  const root = pluginRoot();
  const serverRoot = join(root, "mcp-servers");

  if (!existsSync(serverRoot)) {
    console.error(`Polarion MCP source not found: ${serverRoot}`);
    process.exit(1);
  }

  if (!existsSync(configPath())) {
    printSetupHelp(root);
    process.exit(1);
  }

  const bundledBinary = bundledBinaryPath(root);
  if (existsSync(bundledBinary)) {
    run(bundledBinary, []);
    return;
  }

  console.error("Polarion MCP binary is missing from this plugin build.");
  console.error(`Expected bundled binary: ${bundledBinary}`);
  console.error("This plugin does not compile Polarion MCP on the user machine.");
  console.error("Maintainers must bundle prebuilt binaries for linux-x64 and win-x64.");
  process.exit(1);
}

main();

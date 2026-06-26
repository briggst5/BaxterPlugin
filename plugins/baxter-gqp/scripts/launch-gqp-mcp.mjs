#!/usr/bin/env node
/**
 * Cross-platform GQP Knowledge MCP launcher for Cursor / VS Code plugins.
 *
 * Runtime model: binary-only.
 * - Requires vendored self-contained binary under bin/<rid>/gqp-mcp[.exe]
 * - Never builds or compiles on user machines
 *
 * On first use: creates ~/.config/gqp-mcp.env and runs Baxter SSO via the
 * ambient Entra identity (Windows SSO, Azure CLI) with browser fallback,
 * then caches Key Vault keys locally so later sessions authenticate silently.
 */
import { spawnSync } from "node:child_process";
import { chmodSync, copyFileSync, existsSync, mkdirSync } from "node:fs";
import { homedir, platform } from "node:os";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";

const __dirname = dirname(fileURLToPath(import.meta.url));

function pluginRoot() {
  return process.env.CURSOR_PLUGIN_ROOT || process.env.PLUGIN_ROOT || join(__dirname, "..");
}

function userConfigPath() {
  return join(homedir(), ".config", "gqp-mcp.env");
}

function exampleConfigPath(root) {
  return join(root, "mcp-servers", "gqp-mcp.env.example");
}

function isWindows() {
  return platform() === "win32";
}

function rid() {
  if (isWindows()) {
    return "win-x64";
  }
  if (platform() === "darwin") {
    return process.arch === "arm64" ? "osx-arm64" : "osx-x64";
  }
  return "linux-x64";
}

function binaryName() {
  return isWindows() ? "gqp-mcp.exe" : "gqp-mcp";
}

function bundledBinaryPath(root) {
  return join(root, "bin", rid(), binaryName());
}

function ensureConfigFile(root) {
  if (existsSync(userConfigPath())) {
    return;
  }

  const example = exampleConfigPath(root);
  if (!existsSync(example)) {
    console.error(`GQP config template not found: ${example}`);
    process.exit(1);
  }

  const configDir = join(homedir(), ".config");
  mkdirSync(configDir, { recursive: true });
  copyFileSync(example, userConfigPath());
  try {
    chmodSync(userConfigPath(), 0o600);
  } catch {
    // Windows may not support chmod; ignore.
  }
  console.error(`Created ${userConfigPath()}`);
}

function spawnCommand(cmd, args, { inherit = false } = {}) {
  return spawnSync(cmd, args, {
    stdio: inherit ? "inherit" : "pipe",
    env: process.env,
    shell: isWindows(),
    encoding: "utf8",
  });
}

function runSubcommand(binary, subcommand, extraArgs = []) {
  const args = [subcommand, ...extraArgs];
  const result = spawnCommand(binary, args, { inherit: subcommand === "authenticate" });
  if (result.error) {
    console.error(`Failed to run ${binary}: ${result.error.message}`);
    process.exit(1);
  }
  return result.status ?? 1;
}

function ensureAuthenticated(binary) {
  if (runSubcommand(binary, "check-auth") === 0) {
    return;
  }

  console.error("GQP MCP: Baxter sign-in required (one time).");
  console.error("  A browser window will open for Baxter sign-in.");
  console.error("  (If no browser is available, a device code appears in these logs.)");
  if (runSubcommand(binary, "authenticate") !== 0) {
    console.error("GQP MCP authentication failed. See messages above.");
    process.exit(1);
  }
}

function main() {
  const root = pluginRoot();
  ensureConfigFile(root);

  const binary = bundledBinaryPath(root);
  if (!existsSync(binary)) {
    console.error("GQP MCP binary is missing from this plugin build.");
    console.error(`Expected bundled binary: ${binary}`);
    console.error("Maintainers must bundle prebuilt binaries for linux-x64, osx-arm64, osx-x64, and win-x64.");
    process.exit(1);
  }

  ensureAuthenticated(binary);

  const result = spawnCommand(binary, [], { inherit: true });
  if (result.error) {
    console.error(`Failed to run ${binary}: ${result.error.message}`);
    process.exit(1);
  }
  process.exit(result.status ?? 0);
}

main();

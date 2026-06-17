# Training and Development/Application Space Software Development/Development Environment Setup/Development Environment Setup - platform repo

[[_TOC_]]

# Kotlin Mono-Repo – Build, Quality, and Publishing Guide

  

## Introduction

  

This repository hosts a **multi‑module Kotlin monorepo** containing:

  

- Kotlin/JVM libraries (`libs/`)

- React UI Web components (`ui-web/`)

- Rust → WASM modules (`rust-wasm/`)

  

Centralized build logic lives in the **build-logic** module using Kotlin DSL (`.kts`) precompiled

plugins.

  

---

  

## Build Infrastructure

  

### Key Components

  

#### build-logic/

  

This directory is treated as an **included Gradle build**. It contains all **precompiled Kotlin

DSL (`.kts`) convention plugins** that define how every module in the repository builds, tests,

reports coverage, publishes artifacts, and integrates with SonarQube.

  

### Role of build-logic Module

  

The `build-logic` module acts as the **central orchestration layer** of the monorepo. It:

  

- Defines standard **build** tasks across all modules

- Provides unified **testing** and **coverage** configuration

- Generates consistent **HTML/XML reports**

- Handles **publishing** for Maven + NPM

- Applies **SonarQube** quality rules uniformly

- Ensures all modules use the same **toolchain**, **Java version**, **compiler flags**, and *

  *quality rules**

  

> **Implementation Note — Why `.kts`?**

> All convention plugins are written using **Kotlin DSL** (`.kts`) as *precompiled script plugins*.

> This provides full IDE auto‑completion and type checking.

> Learn the syntax here:

> - [Gradle Kotlin DSL Primer](https://docs.gradle.org/current/userguide/kotlin_dsl.html)

> - [Kotlin DSL Reference Guide](https://docs.gradle.org/current/kotlin-dsl/index.html)

  

**Example (`build.gradle.kts` in a module):**

  

```kotlin

plugins {

    id("platform.kotlin-library")

    id("platform.publishing-convention")

    id("platform.kotlin-quality")

}

  

dependencies {

    implementation(libs.kotlin.stdlib)

    testImplementation(libs.junit.jupiter)

}

```

  

---

  

## Setting up Android Studio for Pure Kotlin Development

  

You can use **Android Studio or IntelliJ IDEA** without installing Android SDK components.

  

1. Install Android Studio and open the project via `settings.gradle.kts`.

2. Ensure **Kotlin** and **Gradle** plugins are enabled.

3. Do *not* install Android SDK—this project does not use Android.

4. Configure Gradle JDK → JDK 21.

5. Use the Gradle tool window to run tasks.

---

## Project Layout & Build Logic

  

- `libs/` – Kotlin libraries

- `ui-web/` – React components published as NPM packages

- `rust-wasm/` – Rust crate built to WASM and published via NPM

- `gradle/libs.versions.toml` – Version catalog

 
---

## Building & Reports

```bash
./gradlew build

./gradlew :<project>:build
```

For a list of projects:
```
./gradlew projects 
```

  

Reports:

  

- Coverage: `build/reports/reports/jacoco/testCoverageReport/testCoverageReport.xml`

- Test reports: `build/libs/report/test/test/TEST-<module>.xml`

  

---

  

## Publishing
**Note:** Do not manually publish to nightly artifacts. It should be done inside the pipeline. 
```bash
./gradlew :<project>:publish
```

Kotlin libraries → Maven (Azure Artifacts).  

UI-Web / Rust-WASM → NPM registry (Azure Artifacts).

Uses `platformVersion` for unified versioning.

---

## UI-Web — Building the Rust WASM Component

```bash
rustup target add wasm32-unknown-unknown

cargo install wasm-bindgen-cli
```

```bash
./gradlew :rust-wasm:build

./gradlew :rust-wasm:publish
```

  

Workflow for UI:

```bash
./gradlew :rust-wasm:build

./gradlew :ui-web:build

./gradlew :ui-web:publish

```
---
## Static Analysis

- Kotlin: ktlint → `libs/build/reports/ktlint/...`

- UI-Web: eslint → `ui-web/build/reports/lint-results.html`

---

## SonarQube Integration

Plugins:

- `platform.base-quality`

- `platform.kotlin-quality`

- `platform.ui-web-quality`

- `platform.rust-quality`

Configure:

```properties
systemProp.sonar.token=<token>

sonarUri=<url>
```
---

## Troubleshooting

- Missing credentials → check `~/.gradle/gradle.properties`

- WASM build errors → ensure correct rust target + wasm tools

- Node/NPM mismatch → validate nodeVersion / npmVersion

# 🧩 ui-web -- @platform/component_library


A reusable **React component library** for internal projects, built with **TypeScript**, **Tailwind CSS**, and **Rollup**.  

It provides shared UI components and utilities to ensure consistent design and functionality across projects.

## 📚 Documentation & Visual Testing

**[View Latest Chromatic Build →](https://www.chromatic.com/build?appId=67a6682f783aab2655f1cf13)**

View the latest build with visual regression testing results, component snapshots, and Storybook documentation.

---

## 🧱 1. Developing the Component Library

### 🚀 Features

- ⚛️ **React + TypeScript** for strongly typed, reusable components

- 🎨 **Tailwind CSS** for utility-first styling

- 🧱 **Rollup** for optimized, tree-shakeable builds (ESM + UMD)

- 📘 **Storybook** for component visualization and documentation

- 🧩 Optional **Redux** and **React Router** support via peer dependencies

- 📦 Ready for publication to **Azure Artifacts npm feed**

- 🔤 **Noto Sans** font included

---

### 🧰 Development Setup

#### 1️⃣ Clone the repository

```bash
git clone https://FLC-NPD@dev.azure.com/FLC-NPD/Platform/_git/platform

cd ui-web
```

#### 2️⃣ Install dependencies

```bash
npm install
```

#### 3️⃣ Verify .env

Verify the `/.env` file at the root of the `platform` repo includes the MQTT URL.

```bash
VITE_MQTT_URL=ws://127.0.0.1:9001/mqtt
```

#### 4️⃣ Run Storybook

```bash
npm run storybook
```

Storybook will start at http://localhost:6006.  

Use it to preview and test components in isolation.

#### 5️⃣ Build the library

```bash
npm run build
```

This will create a production-ready build in the `dist/` folder:

```
dist/

 ├─ platform-component-library.es.js   (ESM)

 ├─ platform-component-library.umd.cjs (UMD)

 ├─ style.css

 ├─ index.d.ts (TypeScript definitions)

 ├─ files/ (Noto Sans font files)
```

#### 6️⃣ Run tests

```bash
npm run test
```

### 🧩 Adding a New Component

1. Create a new folder under `src/components/YourComponent/`

2. Add your `.tsx` file with Tailwind CSS classes

3. Export it from `src/index.ts`:

```typescript
export { YourComponent } from './components/YourComponent';
```

4. Add a Storybook story under `src/components/YourComponent/YourComponent.stories.tsx`

---

### 🧪 Local Testing (without publishing)

You can test your package locally before publishing it.

#### 1️⃣ Build the library

```bash
$env:VITE_MQTT_URL="ws://127.0.0.1:9001/mqtt"; npm run build
```

Note: Update `VITE_MQTT_URL` to match your local or target MQTT broker before building.

#### 2️⃣ Create a .tgz bundle

```bash
npm pack
```

This generates a file such as:

```
platform-component_library-0.1.0.tgz
```

#### 3️⃣ Install in another project

From your parent project:

```bash
npm install ../../platform/ui-web/platform-component_library-<version>.tgz
```

This installs the local build for testing before publishing.

---

### 📦 Publishing to Azure Artifacts

#### 1️⃣ Build the library

```bash
npm run build
```

#### 2️⃣ Publish to private feed

The component library is published to the private Azure Artifacts feed via the CI/CD pipeline.

The following command is shown for reference only and is not expected to be run manually:  

```bash
npm publish --registry=https://pkgs.dev.azure.com/FLC-NPD/_packaging/pltcore-nightly/npm/registry/
```

---

## 🧩 2. Using the Component Library in a Parent Project

After building or publishing, you can use the library in any project.

### 🧱 Installation

#### From Azure Artifacts

```bash
npm install @platform/component_library@0.1.0
```

Replace 0.1.0 with the desired version published to Azure feed.

If your `.npmrc` isn't configured:

```bash
npm install @platform/component_library@0.1.0 --registry=https://pkgs.dev.azure.com/FLC-NPD/_packaging/pltcore-nightly/npm/registry/
```

#### From Local .tgz

If you used `npm pack`:

```bash
npm install ../../platform/ui-web/platform-component_library-<version>.tgz
```

---

### 🧠 Usage Example

```tsx
import { Button } from '@platform/component_library';
import '@platform/component_library/style.css';

export default function App() {
  return <Button variant="primary" label="Click Me" />;
}
```

**Note:** The library uses Tailwind CSS internally. You may need to configure Tailwind in your parent project if you want to extend or customize the styling.

---

### 🧩 Optional Dependencies

The library has optional peer dependencies for:

- **Redux** (`react-redux` >=9.0.0) - if you use Redux-connected components

- **React Router** (`react-router` and `react-router-dom` >=7.0.0) - if you use routing components

Install these only if needed:

```bash
npm install react-redux react-router react-router-dom
```

### 📋 Requirements

- **Node.js**: >=18.17

- **React**: >=18.0.0

- **React DOM**: >=18.0.0

---

## 🌐 Internationalization (i18n)

The component library exposes translations but expects the parent project to provide and initialize the i18n runtime. This prevents duplicate i18n instances and lets the parent project control language loading and configuration.

What changed

- `i18next` and `react-i18next` are declared as **peerDependencies**. The parent project must install them.

How to integrate (parent project)

1. Install the peer dependencies in the parent project:

```bash
npm install i18next react-i18next
```

2. Initialize i18n in your parent project (example minimal setup):

```ts
// src/i18n.ts

import i18n from 'i18next';

import { initReactI18next } from 'react-i18next';

i18n.use(initReactI18next).init({

  lng: 'en',

  fallbackLng: 'en',

  interpolation: { escapeValue: false },

});

export default i18n;
```

3. Register the component-library translations into your parent project's i18n instance (library helper):

```ts
import i18n from './i18n';

import { registerPlatformUiResources } from '@platform/component_library';

registerPlatformUiResources(i18n);
```
Notes

- For namespaced usage, you can adapt the helper to register under a custom namespace and call `useTranslation('my-namespace')` in components.

### i18n Scripts

The repository provides helper scripts to extract and validate translation keys. Use these when adding or updating translations.

- `npm run i18n:extract-labels` — scans `src/` for data-driven keys (for example `labelSid: 'NAV_HOME'`) and adds any missing keys to `src/locales/en/translation.json` with empty values.

- `npm run i18n:scan` — runs the extractor then `i18next-scanner` to extract `t('KEY')` usages and update locale files. This is a single command that populates dynamic keys and extracts literal keys.

- `npm run i18n:check` — compares `src/locales/en/translation.json` keys against other locales and reports any missing translations (exit code non-zero on missing keys).

Usage example (local or CI):

```bash
# populate data-driven keys and extract all keys

npm run i18n:scan

# verify other locales cover keys

npm run i18n:check
```

CI recommendation:

- Run `npm run i18n:scan` and then fail the job if it produces uncommitted changes (so authors commit the new keys), or run `npm run i18n:check` to fail when translations are missing.

# **Rust-wasm Overview**

This project creates a wasm binary that attaches to an HTML5 canvas and draws accurate waveforms for use in displaying ECG data.

## How it works.

The HTML5 canvas is essentially just a render texture that the browser knows about. This enables many high performance solutions for computer graphics, as it can be used as a render target from Javascript/WASM. This library uses the WebGL graphics framework that has high compatability between web browsers.

If you're unfamiliar with graphics programming the gyst of it goes

 - Compile / Link Shaders against a context

 - Set shader as active program

 - Load buffers into GPU

 - Draw

 A more in depth explanation of graphics programming in general can be found here: [Learn OpenGL](https://learnopengl.com/)

 The link above is describing OpenGL not WebGL but the core principles (And sometimes function names) are the same.

## Setting up your build enviornment

1) Install the latest version of rust

``` bash
curl --proto '=https' --tlsv1.2 -sSf https://sh.rustup.rs | sh
```

If the link above is dead, follow the official Rust install steps on their website.

2) After cargo is installed (The rust package manager), install the correct toolchain for WASM

``` bash
rustup target add wasm32-unknown-unknown
```

3) Install the wasm-pack tool

``` bash
cargo install wasm-pack
```

## Building the project

To build just the WASM portion of the application

``` bash
cargo build --target wasm32-unknown-unknown
```

To build the project for use with Javascript, we need to generate the correct bindings
 in addition to the wasm file. To do this we need to use wasm-pack (which will call the

 cargo build command)

``` bash
wasm-pack build --target web
```

> Note: wasm-pack uses node.js under the hood and might take a bit to run for the first time. After the first time though, the build should run fairly quick.

## Running the test web server

To run a simple test web server run the simple_web_server.py script from the root directory

``` bash
python scripts/simple_web_server.py -d ./www
```

### Running the tests

To run the available unit tests

``` bash
cargo test
```

### Viewing the documentation

To view the documentation for the entire project

``` bash
cargo doc --open
```

To view the documentation for just a specific package

``` bash
cargo doc --open -p PACKAGE_NAME
```

To view the general information (as well as source links) about a package

``` bash
cargo info PACKAGE_NAME  
```
## Remote Debugging / Viewing On Tablet

### To see the website on a tablet device you can get the IP address of your machine and go to that IP address at port 8000

```
192.168.1.15:8000/www
```

### To use the remote debugger / screen share

First make sure that your device is configured for developer mode

[Configuring Developer Mode](https://developer.android.com/studio/debug/dev-options)

then connect your device via usb C to your laptop and open your Chrome Browser and go to the following URL

```
chrome://inspect/#devices
```

## Useful Links

- [WebGL cheat sheet](https://learnwebgl.brown37.net/12_shader_language/documents/webgl-reference-card-1_0.pdf) (Page 4 is really handy)

- [Configuring Developer Mode](https://developer.android.com/studio/debug/dev-options)

- [Learn OpenGL](https://learnopengl.com/)

# **Tests**

Overview and quick commands for the tests folder.

## Overview

End-to-end tests using Playwright + BDD (.feature files). Key items in this folder:

- e2e/

    - features/ — human-readable .feature files

    - components/ — shared UI helpers

    - pages/ — Page Object classes

    - steps/ — step definitions

    - bdd.ts — BDD test bootstrap

    - utils/ — helpers

- playwright.config.ts — Playwright configuration

- .env.test.local — local environment variables (test secrets; keep out of source control)

- playwright-report/, test-results/, data/, trace/ — reports, artifacts and traces

## Prerequisites

- Node >= required by repo

- pnpm (preferred package manager)

- Playwright browsers installed (see commands below)

## Install

```
pnpm install

pnpm exec playwright install
```

## Run tests

Run full test suite:

```
pnpm exec playwright test
```

Run a single generated spec (example):

```
pnpm exec playwright test tests/e2e/features-gen/e2e/features/login.feature.spec.js
```

Open last Playwright HTML report:

```
pnpm exec playwright show-report
```

## Environment (Local)

- Environment variables are read from `.env.test.local` and utils/getEnvironmentVariables.ts.

- Ensure any secrets or test accounts are present in the env file before running tests.

### Environment secrets (CI/CD)

- Store secrets only in your CI/CD provider's secret manager (do not commit .env.test.local).

- Use the same env var names expected by utils/getEnvironmentVariables.ts so CI mirrors local setup.

- Examples of common names: TEST_USERNAME, TEST_PASSWORD, TEST_API_KEY, PLAYWRIGHT_BROWSERS_PATH (if used).

- GitHub Actions: add repository secrets (Settings → Secrets) and inject in workflow:

```
yaml

    env:

        TEST_USERNAME: ${{ secrets.TEST_USERNAME }}

        TEST_PASSWORD: ${{ secrets.TEST_PASSWORD }}
```

- Azure DevOps: use pipeline/variable groups with secret variables and map them to env vars for the job.

- Ensure Playwright browsers are installed in the CI job (e.g., run `pnpm exec playwright install` or your CI-specific browser setup step).

- Best practices: use dedicated test accounts, least privilege, rotate secrets regularly, mask/avoid printing secrets in logs, and consider a secrets manager (Vault, Azure Key Vault, etc.) for advanced needs.

## Artifacts & Reports

- HTML report: tests/playwright-report/index.html

- Traces, screenshots, videos (if enabled): tests/data/ and tests/trace/

- Test results stored under tests/test-results/

## Debugging tips

- Use `pnpm exec playwright test --debug` or `--headed --timeout=0` for interactive debugging.

- Open report for failed traces/screenshots: `pnpm exec playwright show-report`

## Notes

- Keep .env.test.local out of source control; it contains test credentials.

- Generated specs live under features-gen — edit step definitions or .feature files, then regenerate if applicable.

### Example .env.test.local

A minimal local env file (DO NOT COMMIT — contains secrets):

```env
# Example .env.test.local

TEST_USERNAME=qa_user@example.com

TEST_PASSWORD=SuperSecretPassword123!

TEST_API_KEY=example-api-key-abcdef

BASE_URL=https://staging.example.com

PLAYWRIGHT_BROWSERS_PATH=/path/to/playwright/browsers
```

Replace values with real test credentials and keep this file out of source control.

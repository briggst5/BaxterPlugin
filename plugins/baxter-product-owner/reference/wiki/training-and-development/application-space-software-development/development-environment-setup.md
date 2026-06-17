# Training and Development/Application Space Software Development/Development Environment Setup

[[_TOC_]]

How to Develop on the Platform
==============================

This page is the **starting point for all platform development**.  
It covers **one‑time tool setup**, **workspace configuration**, and **shared development workflows** across all platform repositories.
* * *

## 1. Overview

The platform is split across **three repositories**, with shared frontend and backend technologies.
[Developing with Platform Repositories](https://myfiles.baxter.com/:p:/r/personal/medhi_venon_baxter_com/Documents/Documents/Platform/PlatformDevelopmentOverview.pptx?d=w8bf142b0a6a64ac580d055610ea27f29&csf=1&web=1&e=IoLCSh])

### Repositories

*   [platform](https://dev.azure.com/FLC-NPD/Platform/_git/platform)
*   [platform-web](https://dev.azure.com/FLC-NPD/Platform/_git/platform-web)
*   [platform-android](https://dev.azure.com/FLC-NPD/Platform/_git/platform-android)

### Shared Technology Stack

*   **Frontend:** React, TypeScript
*   **Backend:** Kotlin
*   **Build & Tooling:** Node.js, npm, Gradle

* * *

## 2. Prerequisites

### 2.1 Required Access

Before starting, ensure you have:
*   Access to all three repositories
*   Access to internal package registries (e.g. Azure Artifacts) https://dev.azure.com/FLC-NPD/Platform/_artifacts/feed/pltcore-nightly (Only component_library and rust_wasm are necessary for building. Maven is needed for publishing)
  
*   Go to [@platform/component_library](https://dev.azure.com/FLC-NPD/Platform/_artifacts/feed/pltcore-nightly/Npm/@platform%2Fcomponent_library/overview/1.0.0-9-N) and download component_library-1.0.0-9-N.tgz.
Run npm install component_library-1.0.0-9-N.tgz

*   Go to [@platform/rust-wasm](https://dev.azure.com/FLC-NPD/Platform/_artifacts/feed/pltcore-nightly/Npm/@platform%2Frust-wasm/overview/1.0.0-9-N) and download rust-wasm-1.0.0-9-N.tgz.
Run npm install rust-wasm-1.0.0-9-N.tgz

*   (Maven only needed for publishing)
Go to [com.baxter.flcp:libs](https://dev.azure.com/FLC-NPD/Platform/_artifacts/feed/pltcore-nightly/maven/com.baxter.flcp%2Flibs/overview/1.00.000.0060-N), under Get this package, select Connect to Feed, select Maven, select Get the Tools, Step 1 download binary apache-maven-3.9.15-bin.tar.gz, Step 2 Install Maven

![image.png](/.attachments/image-dc889b38-346c-4892-aaa1-65cafc6112a8.png)
* * *

### 2.2 Supported Development Environment

Use the IDE that matches the repo you are working in:

| Area | Recommended IDE |
| --- | --- |
| <br><br>Frontend (`platform-web`)<br><br> | <br><br>[VS Code](https://code.visualstudio.com/)<br><br> |
| <br><br>Backend (`platform`)<br><br> | <br><br>[IntelliJ IDEA](https://www.jetbrains.com/idea/)<br><br> |
| <br><br>Android (`platform-android`)<br><br> | <br><br>[Android Studio](https://developer.android.com/studio)<br><br> |

#### Android Studio: JDK environment (Windows)

Android builds and Gradle expect JDK 21. Point your shell and tools at the JBR bundled with Android Studio instead of installing a separate JDK.
1.  Open Settings → System → About → Advanced system settings → Environment Variables.
2.  Under User variables (or System variables if your team requires it), set or create:
    *   JAVA_HOME = `C:\Program Files\Android\Android Studio\jbr`
    *   Path — add (or move near the top): `C:\Program Files\Android\Android Studio\jbr\bin`
3.  Click OK, then close and reopen any terminal, IDE, or build tool so the new values load.
4.  In a new PowerShell or Command Prompt window, verify:
```
java -version
```
Expected result:
openjdk version "21.0.10" 2026-01-20
OpenJDK Runtime Environment (build 21.0.10+-14961533-b1163.108)
OpenJDK 64-Bit Server VM (build 21.0.10+-14961533-b1163.108, mixed mode)
* * *
## 3. Core Tooling Setup (All Repos)

This section applies to every developer, regardless of repo.

### 3.1 Node.js & Frontend Tooling

**Required**
*   Node.js (LTS version used by the platform) [Node.js — Download Node.js®](https://nodejs.org/en/download)
*   npm (bundled with Node)
Verify installation:


```
node -v
```
Expected result:
_v22.14.0_
```
npm -v
```
Expected result:
_10.9.2_
  


**Frontend stack**
*   React
*   TypeScript
*   ESLint / Prettier (configured per repo)

* * *

### 3.2 Backend Tooling (Kotlin)
 

The backend of the platform is built using **Kotlin on the JVM** with **Gradle** as the build system. This tooling applies to any repository containing backend or shared Kotlin libraries.

* * *

#### Required

**Java Development Kit (JDK)**
*   Only needed if Android Studio not installed (it has its own JDK)
*   JDK **21** [OpenJDK](https://jdk.java.net/archive/)
*   Must be configured as the Gradle JDK in your IDE
Verify:

```
java -version
```
Expected result:
_openjdk version "21" 2023-09-19
OpenJDK Runtime Environment (build 21+35-2513)
OpenJDK 64-Bit Server VM (build 21+35-2513, mixed mode, sharing)_
    

* * *

**Gradle**
*   Uses the **Gradle Wrapper** (`./gradlew`)
*   No separate Gradle installation is required
Verify:

```
./gradlew --version
```
Expected result:
\------------------------------------------------------------
_Gradle 9.2.1_
\------------------------------------------------------------

_Build time:    2025-11-17 13:40:48 UTC
Revision:      30ecdc708db275e8f8769ea0620f6dd919a58f76_

_Kotlin:        2.2.20
Groovy:        4.0.28
Ant:           Apache Ant(TM) version 1.10.15 compiled on August 25 2024
Launcher JVM:  21.0.10 (JetBrains s.r.o. 21.0.10+-14961533-b1163.108)
Daemon JVM:    C:\Program Files\Java\jdk-23 (from org.gradle.java.home)
OS:            Windows 11 10.0 amd64_

### 3.3 Package & Dependency Authentication

The platform uses private npm packages.
 
For Windows development, use **vsts-npm-auth**.

#### Step 1: Install vsts-npm-auth

    npm install -g vsts-npm-auth
    

Verify installation:

    npm list -g vsts-npm-auth
    
Expected result:
_C:\Users\<your-username>\AppData\Roaming\npm
`-- vsts-npm-auth@0.43.0_
* * *

#### Step 2: Run authentication

From the directory containing the project `.npmrc` (platform-web/ui-web):

    vsts-npm-auth -config .npmrc
    
Expected result:
_vsts-npm-auth v0.43.0.0
\-----------------------_

This command:
*   Reads the project-level `.npmrc`
*   Writes credentials to your **user-level npm config**
    *   `C:\Users\<your-username>\.npmrc`
*   Prompts for login if required

> The user-level `.npmrc` is **not part of the repository**

* * *

#### Step 3: Verify access

    npm install

Expected result:
_up to date, audited 536 packages in 4s

_118 packages are looking for funding
  run `npm fund` for details_

_9 vulnerabilities (3 moderate, 6 high)_

_To address all issues, run:
  npm audit fix_

_Run `npm audit` for details.__

* * * 
If authentication is successful, dependencies will install without errors.

For more details, see the [Azure Artifacts feed connection guide](https://dev.azure.com/FLC-NPD/Platform/_artifacts/feed/pltcore-nightly/connect) (select **npm**).

> Do not commit auth tokens or user‑level `.npmrc` files.

* * *

## 4. Workspace Setup

### 4.1 Repository Layout & Relationships
**`platform`**
*   Hosts **shared backend (Kotlin) libraries**
*   Hosts **shared frontend component library** (React + TypeScript)
*   Acts as the **source of truth** for reusable FE components and BE logic

**`platform-web`**
*   Contains a **frontend reference web application**
*   Consumes the shared FE component library published from `platform`

**`platform-android`**
*   Contains an **Android reference application**
*   Reuses:
    *   **Frontend reference app FE code from `platform-web`**
    *   **Backend Kotlin libraries from `platform`**
*   Includes **Android‑specific tooling, build logic, and integrations**

* * *

### 4.2 Cloning Repositories

Clone the repositories you need:

```
git clone https://FLC-NPD@dev.azure.com/FLC-NPD/Platform/_git/platform

git clone https://FLC-NPD@dev.azure.com/FLC-NPD/Platform/_git/platform-web  

git clone https://FLC-NPD@dev.azure.com/FLC-NPD/Platform/_git/platform-android
```

It is recommended to keep them under a **single workspace directory**.

* * *

### 4.3 Local Configuration
  
Create the file gradle.properties and store it in: C:/Users/<your-username>/.gradle
```
# ============================================================================
# Platform / Android Build Properties
# ============================================================================
# This file defines environment-specific configuration used for building,
# publishing, and securing the Android platform and related services.
#
# IMPORTANT
# - Do NOT commit real secrets to source control.
# - Replace values marked with <REDACTED> or <PATH> as appropriate.
# ============================================================================
# ----------------------------------------------------------------------------
# Azure Artifacts (Maven & NPM)
# ----------------------------------------------------------------------------
# Azure DevOps Maven feed used for resolving and publishing artifacts
# pltcore-nightly - Nightly build of the platform performed on version-modify branch
# Test-Configure - Sandbox, unmonitored for incremental development
#
# Note: Authorization errors can occur if you don't have access rights to the organization
#
azureUri= https://pkgs.dev.azure.com/FLC-NPD/_packaging/pltcore-nightly/maven/v1
#azureUri= https://pkgs.dev.azure.com/Test-Configure/_packaging/Test-Configure/maven/v1

# Azure DevOps user (typically email)
azureUser=<your-username>

# Azure DevOps access token (use environment variable in CI)
# Azure DevOps Personal Access Token (PAT) in plain text
azureToken=<your-token>

# ----------------------------------------------------------------------------
# Project Metadata
# ----------------------------------------------------------------------------
group=com.baxter

# Platform dependency version
platformVersion=0.0.1-SNAPSHOT

# Current project build version
projectVersion=0.0.1-SNAPSHOT

# ----------------------------------------------------------------------------
# Security
# ----------------------------------------------------------------------------
# JWT signing secret (must be at least 32 characters) made up by you
JWTSecret=<secret>

# ----------------------------------------------------------------------------
# Node / NPM Configuration
# ----------------------------------------------------------------------------
nodeVersion=25.9.0

npmVersion=11.12.1

# Azure Artifacts NPM registry
npmRegistry=https://pkgs.dev.azure.com/FLC-NPD/_packaging/pltcore-nightly/npm/registry/
#npmRegistry=https://pkgs.dev.azure.com/Test-Configure/_packaging/Test-Configure/npm/registry/

# NPM auth token (use environment variable or .npmrc)
# should be your Azure DevOps Personal Access Token (PAT) in plain text
npmAuth=<your-token>

# ----------------------------------------------------------------------------
# Vulnerability & Security Scanning
# ----------------------------------------------------------------------------
# NVD (National Vulnerability Database) API key
# Not necessary for building
# Can be obtained here: https://nvd.nist.gov/developers/request-an-api-key
nvdApiKey=<your-key>

# Local Rust advisory database path
rustAdvisatoryPath=C:/Users/<your-username>/.cargo/test_db

# See below how to generate sonar token
systemProp.sonar.token=<sonar-token>

sonarUri=https://sonarqube.example.com/

org.gradle.configuration-cache=false

org.gradle.java.installations.auto-download=true

org.gradle.java.home=C:/Program Files/Java/jdk-23

# (Android) Prevent OOM errors in large projects
org.gradle.jvmargs=-Xmx4g -XX:MaxMetaspaceSize=1g -XX:+HeapDumpOnOutOfMemoryError -Dfile.encoding=UTF-8

# ----------------------------------------------------------------------------
# Android Signing Configuration
# ----------------------------------------------------------------------------
# Debug signing configuration
# See below how to generate debug.keystore
debugKeyStorePath=C:/Users/<your-username>/.android/debug.keystore

debugKeyStorePasword=<password>

debugKeyAlias=key0

debugKeyPassword=<password>

# RELEASE signing config
releaseKeyStorePath=C:/Users/<your-username>/.gradle/release-key.jks

releaseKeyStorePasword=<password>

releaseKeyAlias=key0

releaseKeyPassword=<password>

# ----------------------------------------------------------------------------
# Gradle Configuration
# ----------------------------------------------------------------------------
# Show all warnings
org.gradle.warning.mode=all

# Always show stacktraces
org.gradle.logging.stacktrace=all

# ----------------------------------------------------------------------------
# Android Lint / Compiler
# ----------------------------------------------------------------------------
# Disable K2 UAST if compatibility issues exist
android.lint.useK2Uast=false

playTrack=internal
```
**Notes:**

- To create debug.keystore:
```
keytool -genkey -v -keystore C:/Users/<your-username>/.android/debug.keystore -storepass android -alias androiddebugkey -keypass android -keyalg RSA -keysize 2048 -validity 10000 -dname "CN=Android Debug,O=Android,C=US"
```

- `org.gradle.java.home` must point to JDK 21 (needed for `kotlin.jvmToolchain(21)`).

- If `projectVersion` is not set, the build expects it from `rootProject.extra["projectVersion"]`. You can set it in `gradle.properties` or via CLI:

```
  ./gradlew build -PprojectVersion=1.0.0
```

- You can use environment variables for `BUILD_NUMBER`.

- (Optional) Node.js settings let you point the Node Gradle plugin to your system install.  



- To generate sonar-token:
1. Go to https://sonarcloud.io and log in
1. Click your profile avatar (top right) → My Account
1. Go to the Security tab
1. Under Generate Tokens, enter a name (e.g. platform-local) and click Generate
1. Copy the token immediately — it won't be shown again

- If Android Studio fails to sync due to toolchain errors, verify `org.gradle.java.home` is set correctly and has the right permissions.

- See the repo‑specific wiki page for details. Each repo may require:
1. Environment variables
1. Local config files
1. Secrets provided through secure means

* * *

### 4.4 Running the Stack Locally
[[_TOSP_]]
 
### 4.5 Dependency Locking with gradle.lockfile

Gradle's dependency locking feature pins the exact versions of every resolved dependency (both direct and transitive) into a gradle.lockfile that lives alongside your build scripts. When locking is enabled, every build verifies the resolved dependency graph against this file. If a version drifts — because a dynamic version like 1.+ resolved differently, or a transitive dependency picked up a new minor release — the build fails fast instead of silently consuming a different artifact.
This gives us three guarantees:
*   **Reproducibility**. A build run today on CI produces the same dependency graph as a build run six months from now on a developer laptop.
*   **Auditability**. The lockfile is checked into source control, so any change in transitive dependencies shows up in code review as a diff.
*   **Supply-chain safety**. Surprise version bumps (including malicious ones) cannot enter the build without an explicit lockfile update.

**Relationship to the version catalog (libs.versions.toml)**
The TOML version catalog declares the intended coordinates and versions of dependencies in a single, human-edited place. The lockfile records the actually resolved versions for each configuration after Gradle walks the full dependency graph.
The two files have different jobs:
*   libs.versions.toml — what you want.
*   gradle.lockfile — what you got.

Because the catalog only describes top-level intent, any change you make there (bumping a version, adding a library, adjusting a bundle) can ripple through transitives in ways the catalog itself does not capture. The lockfile must be regenerated so it reflects the new resolved graph, otherwise the next build will fail the lock check.

**Updating the lockfile after editing the catalog**
After you change anything in libs.versions.toml — or in any other dependency declaration — regenerate the lockfile with:
```
./gradlew resolveAndLockAll --write-locks
```
What this does:
*   resolveAndLockAll is an aggregating task that resolves every lockable configuration across every project in the build, so a single invocation covers the whole repo rather than one subproject at a time.
*   --write-locks tells Gradle to overwrite gradle.lockfile with the freshly resolved versions instead of validating against the existing file. Without this flag, Gradle would resolve dependencies and then fail because the lockfile is out of date.

Recommended workflow
1.  Edit libs.versions.toml (or the relevant build.gradle(.kts)).
2.  Run ./gradlew resolveAndLockAll --write-locks.
```
./gradlew resolveAndLockAll --write-locks.
```
3.  Inspect the diff on gradle.lockfile. Pay attention to transitive bumps you did not explicitly request — they are legitimate, but worth a glance.
4.  Run a normal build (./gradlew build) without the flag to confirm the new lockfile validates cleanly.
```
./gradlew build
```
5.  Commit libs.versions.toml and gradle.lockfile together in the same change. Splitting them across commits will break bisection and produce broken intermediate states on CI.

**Common pitfalls**
*   **Forgetting to commit the lockfile**. CI will fail on the next build because the resolved graph no longer matches what is checked in. Always commit the lockfile alongside the catalog change.
*   **Running the command in only one subproject**. Use the root-level ./gradlew resolveAndLockAll so every project is covered in one shot.
```
./gradlew resolveAndLockAll
```
*   **Hand-editing gradle.lockfile**. The file is generated; manual edits will be overwritten and can introduce inconsistencies between configurations. Always regenerate via the task.
*   **Mixing lockfile updates with unrelated changes**. Keep the dependency change isolated so reviewers can tell at a glance which transitive bumps came along for the ride.

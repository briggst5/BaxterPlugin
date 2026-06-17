# Training and Development/Application Space Software Development/Development Environment Setup/Development Environment Setup - platform-android repo

[[_TOC_]]

# Reference Application

  

This project integrates a **ReactJS web application** as a WebView within a native Android application. It consumes both **Maven** and **NPM** artifacts.  

The React-based UI lives in a separate codebase, is built as static assets, and is packaged into the Android app’s `assets` folder during the build process.

  

---

  

## Project Organization

  

```

root/

├── app/         # Native Android Studio project (Java/Kotlin)

│   └── src/

│       └── main/

│           ├── cpp/        # Native C++ sources & build scripts

│           │   ├── MyHeader.h

│           │   ├── MyReceiver.cpp

│           │   ├── native_lib.i

│           │   └── CMakeLists.txt

│           └── kotlin/     # Main application logic

│   ├── build.gradle.kts

│   └── ... (other config)

├── ui-web/      # ReactJS application (submodule)

└── ...

```

  

**Details**

  

- **/ui-web/**: Contains the ReactJS (web) application (managed as a Git submodule).

- **/app/**: Contains the native Android Studio project.

  

---

  

## Prerequisites

  

### Tools & Dependencies

  

- **Git**: For version control and submodules.

- **Node.js (with NPM)**: LTS version recommended.

  

  **Authenticating to Private NPM Feeds**  

  If you use private npm feeds (e.g., Azure Artifacts), fetch credentials before building:

```
cd platform-android\reference_app\ui-web-link\ui-web
npx vsts-npm-auth -config .npmrc
```

  - Run this **from your project root directory**.

  - This must be done before any `npm install` or build step that requires access to private npm feeds.

  - Make sure [vsts-npm-auth](https://www.npmjs.com/package/vsts-npm-auth) is globally installed or use `npx` as above.

  

- **Android Studio**: IDE for Android development (includes SDK & tools).

- **Android SDK**: Managed via Android Studio.

- **NDK & CMake**:

  - Open Android Studio.

  - Go to *Settings* > *Languages & Frameworks* > *Android SDK* > *SDK Tools* tab.

  - Enable **NDK (Side by side)** (latest) and **CMake** (≥3.18.1 recommended for Kotlin DSL).

- **local.properties** (if building outside Android Studio or on CI/CD):

```properties
sdk.dir = C\:\\Users\\YourUser\\AppData\\Local\\Android\\Sdk
ndk.dir = C\:\\Users\\YourUser\\AppData\\Local\\Android\\Sdk\\ndk\\25.1.8937393
```

### Environment Variable

Set **ANDROID_HOME** for command-line and CI/CD builds:

1. Search *Edit System Environment Variables* (Windows).

2. Under *User variables*, click *New*.

3. Name: `ANDROID_HOME`, Value: `<path-to-android-sdk>\Android\Sdk`

---

## Git Submodule Setup & Updating

This project uses a Git submodule for the UI:

```bash

# Initialize submodules if not already done

git submodule update --init --recursive

  

# To update 'ui-web-link' to the latest on the 'integration' branch:

cd reference_app

cd ui-web-link

git fetch

git checkout integration

git pull origin integration

cd ..

git add ui-web-link

git commit -m "Update ui-web-link submodule to latest integration branch"

```

  

---

  

## Build Variants

  

- **Standard build**: Uses typical debug/release configurations.

- **DebugMinified variant** (this feature is currently disabled):  

  To test ProGuard, R8, and code minification rules in a debug-safe environment, use the `debugMinified` build variant.

  

#### Run the debugMinified build:

  

```bash
./gradlew installDebugMinified
```

This variant uses your debug keystore, keeps debugging enabled, but applies minification just like release builds.  

**Check the app’s functionality and inspect stack traces/UI for issues with removed or renamed code.**
---

## Building the Application

The React app is built first; its assets are then copied to the Android app’s `assets` folder before building the native app.

**Automatic build via Gradle:**

```bash
cd reference_app
./gradlew build
```

**Manual build (if needed):**

1. Build React app:

```bash
cd reference_app

cd ui-web-link

cd ui-web

npm install

npm run build

cd ..
```

2. The `copyWebAssetsToAndroid` Gradle task does this automatically.

---

## Running the Android Application

**Android Studio:**

- Open the **reference_app/** project.

- With a device or emulator running:

```bash
./gradlew installDebug
```

```bash
./gradlew run   # If supported in your build config
```
The app will launch on the connected device/emulator with the ReactJS UI inside a native WebView.

- If there is no virtual or physical device, use the green triangle or the green debug buttons in Android studio
![android_studio.png](/.attachments/android_studio-d2fed075-b836-4c23-9cb9-59f17bdf3679.png)

---

## Troubleshooting & Verification

- **Sync Gradle files** (click the *elephant* icon) after dependency changes.

- After build, a `.cxx/` folder should appear in `app/`, created by the Android C++ toolchain.

- If web content isn’t updating, ensure you rebuilt the React assets and ran `copyWebAssetsToAndroid`.

---

## Additional Information

- The **`ui-web-link`** directory is a submodule; check for updates regularly, especially on the `integration` branch.

- Use the `debugMinified` variant to test minification during development and catch potential ProGuard/R8 issues early.

---

## Contact & Support
  
For configuration, development, or troubleshooting help, file an issue or contact the project maintainer.

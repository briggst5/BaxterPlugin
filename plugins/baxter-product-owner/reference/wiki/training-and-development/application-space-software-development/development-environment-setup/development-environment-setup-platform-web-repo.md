# Training and Development/Application Space Software Development/Development Environment Setup/Development Environment Setup - platform-web repo

[[_TOC_]]

# UI Web

A modern web interface built with React, TypeScript, and Vite.

## Prerequisites
 
Before you begin, ensure you have the following installed:

- **Node.js** (v18 or higher recommended)

  - Download from [nodejs.org](https://nodejs.org/)

  - Verify installation: `node --version`

- **npm** (comes with Node.js)

  - Verify installation: `npm --version`

## Setup

1. **Navigate to the project directory:**

```bash
cd platform-web/ui-web
```

2. **Configure npm to use Platform Artifacts:**

   A `.npmrc` file is already included in this repository with the following configuration:

```
registry=https://registry.npmjs.org/

@platform:registry=https://pkgs.dev.azure.com/FLC-NPD/_packaging/pltcore-nightly/npm/registry/

always-auth=true
```

This configuration ensures:

- Public packages are resolved from npmjs.org

- `@platform` scoped packages are resolved from Azure Artifacts

- Authentication is always enforced for the private registry

3. **Authenticate with Azure Artifacts:**

   Run the following command to add an Azure Artifacts access token to your user-level `.npmrc` file:

```bash
vsts-npm-auth -config .npmrc
```

> You do not need to run this every time. Only rerun it if `npm install` returns a `401 Unauthorized` error.
>
> For more details, see the [Azure Artifacts feed connection guide](https://dev.azure.com/FLC-NPD/Platform/_artifacts/feed/pltcore-nightly/connect) (select **npm**).

4. **Install dependencies:**

```bash
npm install
```

## Available Scripts

### `npm run dev`

Starts the development server with hot module replacement (HMR).

- Opens at `http://localhost:5173` by default

- Changes are reflected instantly in the browser

- Use this for active development

### `npm run build`

Creates an optimized production build.

- Runs TypeScript compiler checks

- Bundles and minifies code for production

- Output is generated in the `dist/` directory

### `npm run lint`

Runs ESLint to check code quality and style.

- Identifies potential errors and code style issues

- Run this before committing code

### `npm run format`

Formats all code files using Prettier.

- Automatically fixes formatting issues

- Formats TypeScript, JavaScript, JSON, CSS, and Markdown files

- Run this to ensure consistent code style

### `npm run format:check`

Checks if code is formatted according to Prettier rules.

- Does not modify files, only reports issues

- Useful for CI/CD pipelines

- Returns exit code 1 if formatting issues are found

### `npm run preview`

Serves the production build locally for testing.

- Must run `npm run build` first

- Useful for testing the production build before deployment

## Development Workflow

1. Start the development server:

```bash
npm run dev
```

2. Make your changes in the `src/` directory

3. Format your code:

```bash
npm run format
```

4. Check for linting issues:

```bash
npm run lint
```

5. Build for production:

```bash
npm run build
```

6. Preview the production build:

```bash
npm run preview
```

## Tech Stack

- **React** - UI library

- **TypeScript** - Type-safe JavaScript

- **Vite** - Fast build tool and dev server

- **ESLint** - Code linting and quality checks

- **Prettier** - Code formatting

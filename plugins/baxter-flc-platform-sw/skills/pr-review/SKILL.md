---
name: pr-review
description: Reviews pull requests and branches for merge readiness using organization checklists. Every finding must cite repo-relative file path and line number(s). Use when reviewing PRs, comparing branches, or when the user asks if changes are ready to merge.
---

# PR Review

## Purpose
This skill provides comprehensive pull request reviews at the PR-level. It compares branches, assesses the complete change against coding standards and checklists, and determines merge readiness.

## How It Works

### Step 1: Get PR Metadata and Change Details
The only required input is the **PR ID**. Use the Azure CLI to retrieve all PR metadata, then use `git` to retrieve the actual change content.

**Fetch PR metadata from Azure DevOps:**
```bash
# Retrieve source branch, target branch, title, description, and current reviewers
az repos pr show --id <pr-id>

# Fetch any review comments already posted on the PR
# to avoid duplicating feedback that has already been raised
az repos pr reviewer list --id <pr-id>
```

From the `az repos pr show` output, extract:
- **`sourceRefName`** — the feature branch containing the changes
- **`targetRefName`** — the branch the PR will merge into
- **`title`** and **`description`** — the stated goals and scope of the PR
- **`workItemRefs`** — the list of linked work item IDs

**Fetch each linked work item:**
```bash
# For each work item ID found in workItemRefs, fetch its full details
az boards work-item show --id <work-item-id>
```

From each work item, extract:
- **`System.Title`** — the work item title
- **`System.Description`** — the detailed description of the task or requirement
- **`Custom.DefinitionofDone`** — the acceptance criteria / Definition of Done that must be satisfied before the work item is considered complete
- **`System.State`** — current state (e.g. Active, Resolved, Closed)
- **`System.Parent`** — parent work item, for understanding hierarchy and broader feature context

**Fetch change details using git:**
```bash
# Ensure both branches are up to date locally
git fetch origin

# View all changed files between branches
git diff --name-only origin/<target-branch>...origin/<source-branch>

# View detailed line-level changes
git diff origin/<target-branch>...origin/<source-branch>

# View a file-by-file summary of additions and deletions
git diff --stat origin/<target-branch>...origin/<source-branch>
```

> Use the three-dot (`...`) form to diff from the common ancestor, which reflects only what the PR branch changed rather than unrelated divergence on the target branch.

### Step 2: Understand PR Intent
- Read and analyze the PR title and description retrieved from Azure DevOps
- Identify the stated goals and motivation
- Review the scope of changes and affected systems
- Check for related issues or dependencies referenced in the description
- Assess appropriateness of PR scope against stated goals
- Note any feedback already posted by other reviewers so it is not duplicated in the final output

**Cross-reference with linked work items:**
- Verify the PR description and changed files are consistent with the work item's `System.Description` — flag any mismatch between stated intent and actual change
- Record the `Custom.DefinitionofDone` for each linked work item — this will be evaluated in Step 6
- If no work item is linked, flag it — a PR targeting `integration` without a linked work item violates traceability requirements (IEC 62304 §5.1)

### Step 3: Design Pass
Before executing any checklist, evaluate the overall design of the change:
- Does the change make sense architecturally and belong in this codebase?
- Do the interactions between changed components make sense as a whole?
- Is the approach appropriate, or is there a simpler or better-fitting solution?
- If a significant design problem is found at this stage, raise it immediately — do not proceed to detailed checklist review until it is resolved, as line-level findings may be irrelevant if the design changes

### Step 4: Compare Against Coding Standards
Read the tests first before reading production code — tests define what the change is supposed to do and provide the intent needed to evaluate the implementation accurately. Then review all changed files against the following coding standards:

- [Code Review Checklist](checklists/code-review-checklist.md)
- [Class C Code Checklist](checklists/class-c-code-checklist.md)
- [SDP Metric Checklist](checklists/sdp-metrics-checklist.md)

Review Kotlin code, files matching `*.kt`, against:

- [Kotlin Coroutines Checklist](checklists/kotlin-coroutines-checklist.md)
- [Kotlin Data Iteration Pattern](checklists/kotlin-dataiteration-pattern.md)
- [Kotlin Exception Safety](checklists/kotlin-exceptionsafetyauditor.md)
- [Kotlin Zombie Checklist](checklists/kotlin-zombie-checklist.md)

### Step 5: Execute PR-Level Review
Use the **PR Review Checklist** to evaluate PR-level concerns: scope, quality, testing, documentation, security, and integration readiness.

See: [PR Review Checklist](checklists/pr-review-checklist.md)

### Step 6: Evaluate Merge Readiness
- Confirm all checklist items are addressed
- Assess risk level (low/medium/high)
- Note any blockers vs. optional improvements
- Identify required fixes vs. nice-to-have improvements
- Provide clear merge recommendation

**Evaluate Definition of Done for each linked work item:**
- For each work item fetched in Step 1, explicitly assess whether the PR satisfies its `Custom.DefinitionofDone`
- If the DoD references a measurable threshold (e.g. "80% quality gate", "all acceptance tests passing"), assess whether the evidence available (build results, test additions, coverage reports) supports that the threshold has been met
- If the DoD cannot be confirmed from the diff and build results alone, flag it as an open question that the author must address before merge
- A PR whose linked work item DoD is not demonstrably satisfied is not ready to merge

### Step 7: Re-evaluate all findings before publishing
- For every finding at any severity level, verify it represents a real issue before including it in the review:
  - Examine the full context — not just the diff line in isolation — to confirm the problem genuinely exists
  - Discard any finding that cannot be concretely demonstrated to be an issue; do not publish findings based on surface appearance, speculation, or incomplete analysis
- **Confirm each finding has an accurate file path and line number(s)** — open the file on the PR branch (or use `git diff` with line context) and re-check line numbers before publishing; do not guess from the diff hunk alone

### Step 8: Provide Actionable Feedback

#### Finding citation requirements (mandatory)

**Every finding** at any severity (Blocking, Warning, Suggestion) **must** include:

1. **File path** — repo-relative path (e.g. `ui-web/src/store/authSlice.ts`)
2. **Line number(s)** — one line or an inclusive range for multi-line issues (e.g. `61` or `61–76`)

Use this format in the review body so findings are one-click navigable:

```61:76:ui-web/src/features/auth/LoginContainer.tsx
// relevant excerpt or single offending line
```

Rules:

- Cite the **line numbers in the PR branch** (current file), not only diff hunk offsets
- Prefer the **smallest range** that shows the problem; add a short excerpt in the citation block when it helps
- **Do not publish a finding without `path` + line** — if you cannot locate a line after reading the file, omit the finding or downgrade it to an open question for the author
- **PR-wide / process findings** (missing work item, CI not run, coordinated-merge risk): still list **affected file(s) with line(s)** when the issue ties to code (e.g. missing middleware wiring → cite `store.ts` where middleware should be registered). For purely process items with no code anchor, use: `File: (none — process)` and state why; do not invent line numbers

**Finding template** — use for each item in the Findings section:

```markdown
### N. Short title — 🔴 Blocking | 🟡 Warning | 🔵 Suggestion

**Location:** `path/to/file.ext` — lines `42` (or `42–48`)

\`\`\`42:48:path/to/file.ext
// excerpt
\`\`\`

**Issue:** …

**Recommendation:** …
```

#### Review output
- Summarize overall assessment against all checklists
- Group findings under **Blocking**, **Warnings**, and **Suggestions**; each bullet or numbered item must satisfy the citation requirements above
- Highlight critical issues blocking merge
- Note suggestions for improvement
- Communicate merge recommendation clearly
- Be constructive and supportive

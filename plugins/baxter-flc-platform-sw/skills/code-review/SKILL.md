---
name: code-review
description: Reviews code for quality, correctness, security, and maintainability using organization checklists. Use when reviewing files, diffs, or when the user asks for a code review.
---

# Code Review

## Purpose
This skill provides comprehensive code review following a structured approach. It evaluates code across multiple dimensions: logic and correctness, code quality, security, performance, testing, and compatibility.

## How It Works

### Step 1: Understand the Context
- Identify what the code is trying to accomplish
- Review the commit message or PR description
- Note any dependencies or related changes
- Understand the scope and intended functionality

### Step 2: Design Pass
Before executing any checklist, evaluate the overall design of the code:
- Does the design make sense for the problem being solved?
- Are the interactions between components appropriate?
- Is the approach overly complex or over-engineered for what is needed?
- If a significant design problem is found at this stage, raise it immediately — line-level findings may be irrelevant if the design needs to change

### Step 3: Execute Structured Review
Read tests first before reading production code — tests define what the code is supposed to do and provide the intent needed to evaluate the implementation accurately. Then review all files against the following checklists:

- [Code Review Checklist](checklists/code-review-checklist.md)
- [Class C Code Checklist](checklists/class-c-code-checklist.md)
- [SDP Metric Checklist](checklists/sdp-metrics-checklist.md)

Review Kotlin code, files matching `*.kt`, against:

- [Kotlin Coroutines Checklist](checklists/kotlin-coroutines-checklist.md)
- [Kotlin Data Iteration Pattern](checklists/kotlin-dataiteration-pattern.md)
- [Kotlin Exception Safety](checklists/kotlin-exceptionsafetyauditor.md)
- [Kotlin Zombie Checklist](checklists/kotlin-zombie-checklist.md)

### Step 4: Provide Structured Feedback
- Highlight strengths as well as issues
- Organize feedback by category (logic, style, security, etc.)
- Provide specific examples where possible
- Suggest concrete improvements
- Prioritize critical issues over stylistic preferences

### Step 5: Re-evaluate all findings before publishing
- For every finding at any severity level, verify it represents a real issue before including it in the review:
  - Examine the full context — not just the changed lines in isolation — to confirm the problem genuinely exists
  - Discard any finding that cannot be concretely demonstrated to be an issue; do not publish findings based on surface appearance, speculation, or incomplete analysis

### Step 6: Communicate Results
- Summarize key findings
- Note any blockers vs. nice-to-haves
- Provide clear next steps
- Be constructive and supportive

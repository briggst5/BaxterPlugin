---
name: PR Review Checklist - IEC 62304 Class C / IEC TR 80002-1
description: >
  Class C medical device software checklist combining IEC 62304:2006+Amd1:2015,
  ISO 14971:2019, IEC/TR 80002-1:2009 Table B.2, and project RCMs (PLT1-60 through
  PLT1-70) as defined in FLC_Platform_PDLM_Software_Development_Plan section 9.3.5.
  Applied alongside pr-review-checklist.md for every PR review.
  All items are scoped to what is directly observable in the diff only.
alwaysApply: false
---

When performing a PR review, apply every item in this checklist.
Flag each issue with its severity: 🔴 Blocking / 🟡 Warning / 🔵 Suggestion.

> **Scope note — Hardware defect categories (IEC/TR 80002-1 Table B.2):**
> The hardware categories from Table B.2 (EEPROM/NVRAM wear, CPU/hardware faults,
> peripheral anomalies, power loss/recovery, low-power mode transitions) are
> **not applicable** to Kotlin and TypeScript code in this repository and are
> excluded from this checklist. They remain applicable to any bare-metal or
> firmware components if introduced in future.

---

## 1. Traceability (IEC 62304 §5.1, §5.2)

- [ ] Every change is linked to a traceable work item — flag any change with no ticket reference in the code, commit, or PR description
- [ ] New `// @todo`, `// FIXME`, or `// HACK` comments include a ticket number — unlinked annotations are not acceptable in Class C code
- [ ] If a software requirement ID is referenced in a comment or doc string, the ID is present and correctly formatted (e.g. `PLT1-XXXX`)

---

## 2. Detailed Design Visibility (IEC 62304 §5.4 — Class C mandatory)

- [ ] New or significantly modified software units have sufficient inline documentation to reconstruct design intent: inputs, outputs, preconditions, postconditions, and non-obvious logic are described
- [ ] Interface contracts between units (function signatures, message schemas, MQTT topic names) are explicitly defined and consistent across both sides of the interface within the diff
- [ ] No interface contract change (function signature, MQTT topic, serialised payload shape) is made on one side without a corresponding update on the other side visible in the same diff

---

## 3. Risk Control Measures in Code (IEC 62304 §7 / ISO 14971)

- [ ] Code that handles patient data (identifiers, clinical values, alarms, medication) validates inputs before use — flag any path where patient data is consumed without a null, bounds, or format check
- [ ] Failure modes are explicit and safe — a software failure must not silently leave patient-affecting state (e.g. stale patient context, incorrect alarm state, missing alert) in an unknown condition
- [ ] Safety-relevant state changes (patient context set/clear, alarm raised/cleared) are confirmed via a response or acknowledgement — fire-and-forget is not acceptable for these paths
- [ ] Error conditions on safety-relevant operations are reported outward (published to an error topic, returned as a typed failure, or surfaced to the caller) — silent swallows on these paths are a blocking issue

---

## 4. Arithmetic Defects (IEC/TR 80002-1 Table B.2 — Arithmetic)

> These causes apply to all numeric operations in Kotlin, TypeScript, and Rust.

- [ ] **Divide by zero:** Every division operation where the divisor is derived from external input, user data, or a computed value has a zero-guard or assertion before use — flag any unguarded division
- [ ] **Numeric overflow/underflow:** Operations on values that could exceed type bounds (counters, accumulators, sample indices) have explicit range checks or use types that prevent overflow (e.g. `BigInteger`, saturating arithmetic) — flag unchecked accumulation on bounded types
- [ ] **Floating point rounding:** Code using floating point for clinical or safety-relevant calculations (dosage, waveform scaling, thresholds) uses robust algorithms and does not rely on exact equality comparisons (`==`) between floats — flag `Float ==` or `Double ==` comparisons on safety-relevant values
- [ ] **Improper range/bounds checking:** All array, list, and buffer accesses derived from external or computed indices have bounds validation before access — flag any index used without a prior bounds check
- [ ] **Off-by-one errors:** Loop bounds, slice ranges, and index calculations on safety-relevant collections are reviewed for off-by-one risk — flag any loop iterating to `length` rather than `length - 1` on a zero-indexed structure without clear justification

---

## 5. Timing Defects (IEC/TR 80002-1 Table B.2 — Timing)

> Applies to coroutines, threads, async/await, and MQTT message handlers in Kotlin and TypeScript.

- [ ] **Race conditions:** Shared mutable state accessed from multiple coroutines, threads, or async callbacks is protected by appropriate synchronisation (`Mutex`, `AtomicBoolean`, `StateFlow`, or equivalent) — flag any shared mutable variable accessed from concurrent contexts without synchronisation
- [ ] **Missed timing deadlines:** Operations with real-time or responsiveness requirements (MQTT message handling, UI state updates, alarm delivery) do not perform blocking I/O or long-running computation on the calling thread/dispatcher — flag blocking calls on `Dispatchers.Main` or equivalent UI threads

---

## 6. Data Defects (IEC/TR 80002-1 Table B.2 — Data Issues)

- [ ] **Data corruption:** Global or shared mutable state is minimised — flag new mutable global variables or singletons holding patient or clinical data that are not protected by a concurrency primitive
- [ ] **Resource contention:** Code that acquires shared resources (MQTT connections, coroutine scopes, file handles) defines explicit behaviour when the resource is unavailable — flag resource acquisition with no timeout, fallback, or error path
- [ ] **Errant pointers:** Rust code outside of test modules does not use `unwrap()` or `expect()` on `Option` or `Result` without a documented safety justification — flag every `unwrap()` in non-test Rust production code
- [ ] **Data conversion errors:** Type conversions on safety-relevant values use strongly typed wrappers, `Adapter`, or `Wrapper` patterns rather than raw casts — flag direct numeric casts (e.g. `as Int`, `as f32`, TypeScript `as SomeType`) on values derived from external input or patient data without range validation
- [ ] **Incorrect initialisation:** New classes and data structures initialise all fields to safe, defined default values — flag any field left uninitialised or defaulting to a value that could be mistaken for valid clinical data (e.g. `0` for an MRN, `true` for an alarm state)
- [ ] **Averaged data out of range:** Code that computes running averages or filtered values (e.g. waveform filtering, signal processing) validates that the sample count meets the minimum required before using the result — flag averaged computations with no minimum-sample guard
- [ ] **Rollovers:** Counters, timestamps, and sequence numbers that could roll over define explicit handling at the boundary (saturation, wrap detection, or use of a larger type) — flag counters on bounded integer types with no rollover handling

---

## 7. Interface Defects (IEC/TR 80002-1 Table B.2 — Interface Issues)

- [ ] **Failing to update display:** Code that pushes state to the UI or a downstream consumer detects and handles the case where the consumer is non-responsive, disconnected, or has not yet subscribed — flag fire-and-forget publishes to UI-facing topics with no error or disconnection handling
- [ ] **Misuse / failure reconstruction:** New code that handles safety-relevant operations (patient context changes, authentication, alarm state transitions) emits a structured log or event via the project event system (`EventManager`) or logging infrastructure — flag any safety-relevant operation that produces no observable log or event entry
- [ ] **Overload:** Code that processes unbounded input streams or queues (MQTT subscriptions, patient list fetches) has defined behaviour under high load — flag subscription handlers with no backpressure, rate limiting, or queue depth control

---

## 8. Miscellaneous Defects (IEC/TR 80002-1 Table B.2 — Miscellaneous)

- [ ] **Memory leaks:** Resources that must be explicitly released (coroutine scopes, MQTT subscriptions, streams, `Closeable` objects) are released in a `close()`, `finally`, `use {}`, or `DisposableEffect` block — flag any resource acquisition with no corresponding release path
- [ ] **Stack overflow — recursion as an anti-pattern:** 🔴 **Any recursive function introduced in this diff is a blocking issue.** Recursion is prohibited without a documented architectural exception. Flag all recursive calls unconditionally
- [ ] **Logic errors:** Boolean conditions, state machine transitions, and branching logic on safety-relevant paths are reviewed for inversion errors, missing branches, and unreachable cases — flag any `when`/`switch`/conditional chain on a safety-relevant enum or state that does not handle all cases explicitly

---

## 9. Logging and Event System Usage (PLT1-67, PLT1-68)

> Applies to all Kotlin, TypeScript, and Rust code. Replaces the general "no debug statements" rule for this codebase.

- [ ] **No raw print/debug statements in production code (PLT1-68):** `println`, `print`, `console.log`, `console.warn`, `console.error`, `System.out`, `Log.d`, `info!`, `dbg!`, `eprintln!` are not present in production source — these are a blocking issue; all runtime output must go through the project logging infrastructure
- [ ] **Safety-relevant operations use the event system (PLT1-67):** Operations that contribute to failure analysis (patient context changes, authentication outcomes, alarm transitions, MQTT connection state changes) emit an event via `EventManager` or equivalent — flag safety-relevant state changes with no corresponding event emission
- [ ] **Errors are logged with sufficient context (PLT1-68):** Caught exceptions and error conditions include enough contextual information (component name, operation, relevant input state) to reconstruct the failure scenario — flag `catch` blocks that log only a static string with no contextual data

---

## 10. SOUP Management (IEC 62304 §8.1)

- [ ] Any new third-party dependency introduced in this diff (`build.gradle.kts`, `package.json`, `Cargo.toml`) is flagged for SOUP assessment per GQP-09-24 — the reviewer cannot approve SOUP additions without a corresponding ticket
- [ ] No existing SOUP dependency has been silently upgraded — version changes must be explicit and intentional
- [ ] SOUP is not used directly in a safety-relevant execution path without an isolation layer or documented rationale visible in the diff

---

## 11. Unit Verification Completeness (IEC 62304 §5.5 — Class C mandatory / PLT1-63)

- [ ] Every new software unit (class, service, significant function) has at least one unit test — untested units are a blocking issue for Class C
- [ ] Tests cover boundary conditions, null/empty inputs, and failure paths in addition to the happy path
- [ ] Tests for safety-relevant behaviour (patient context rules, authentication outcomes, alarm logic, arithmetic bounds) assert the exact outcome — not just that no exception was thrown
- [ ] No existing test has been removed or commented out without a documented rationale and ticket reference

---

## 12. Configuration Management (IEC 62304 §8.3)

- [ ] No credentials, tokens, secrets, or environment-specific values are hardcoded in any committed file
- [ ] No internal hostnames, IP addresses, or hardcoded URLs that should be supplied by configuration at deployment time
- [ ] Build and configuration files do not contain developer-local overrides or machine-specific paths
- [ ] Version identifiers in `package.json`, `build.gradle.kts`, and `Cargo.toml` are consistent and intentional

---

## 13. Anomaly and Problem Resolution (IEC 62304 §9)

- [ ] Commented-out code is not present — dead code must be deleted; if a block is temporarily disabled a ticket reference is mandatory
- [ ] `// @todo`, `// FIXME`, `// HACK` comments without a ticket number are not acceptable in Class C code
- [ ] If this change fixes a previously reported defect, the fix addresses the identified root cause and not just the observable symptom

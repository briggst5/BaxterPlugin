# Training and Development/BDD‑Based Behavioral Specification and Traceability

BDD‑Based Behavioral Specification and Traceability
===================================================

Unit and System Test Validation
-------------------------------

* * *

> **BDD Contract**
> _Every approved behavior is defined once in Polarion, expressed in BDD language, and validated by traceable automated tests at the appropriate level (unit and/or system)._

* * *

Behavior‑Driven Development (BDD) provides the language and structure used to define behavior.

This document describes **how behavior is specified, validated, and traced** across unit and system test levels, while intentionally **avoiding unnecessary overhead for developer‑level and TDD tests**.

* * *

Behavioral Language: BDD and Gherkin
------------------------------------

* * *

| **Topic** | **Behavior‑Driven Development (BDD)** | **Gherkin Syntax** |
| --- | --- | --- |
| **What it is** | A development approach describing expected behavior using shared language | A structured, plain‑text language for expressing scenarios |
| **Purpose** | Establish shared understanding before implementation | Provide a consistent, tool‑readable format |
| **Format** | User‑focused examples and conversations | **Given / When / Then** |
| **Role** | Guides requirements, design, and validation | Executable or reference behavior |

* * *

Given / When / Then: Behavioral Structure
-----------------------------------------

* * *

BDD scenarios use **Given / When / Then** to clearly separate **context**, **action**, and **outcome**.

### Given – Context

Defines the initial state of the system.
Examples:
*   Preconditions
*   Existing data
*   Logged‑in user state
*   Environment configuration

> _Given the clinician is successfully logged in_

* * *

### When – Action

Describes the triggering action.
Examples:
*   API call
*   UI interaction
*   Request execution

> _When the clinician opens the patient list_

* * *

### Then – Outcome

Specifies observable results.
Examples:
*   Returned data
*   UI elements displayed
*   Business rules enforced

> _Then a table of patients is displayed_

* * *

### Why This Structure Matters

*   Makes behavior readable by business, QA, and developers
*   Clearly separates intent from implementation
*   Scales across unit, component, and system tests

* * *

What Gherkin Does for Automated Testing
---------------------------------------

* * *

Gherkin scenarios describe behavior in plain language. Automation frameworks map each **Given / When / Then** step to executable code.
When a scenario runs:
*   Steps are interpreted
*   Corresponding automation code is executed
*   Behavior is validated against expectations

* * *

System Test Example (Executable Gherkin)
----------------------------------------

* * *

**Polarion Work Item:** **[PLT1-1451 - Verify Patient List Table shows default columns: Patient Name, MRN, DOB](https://polarion.hrc.corp/polarion/redirect/project/flc.platform.01/workitem?id=PLT1-1451)**

    Feature: Platform Patient List
    
    Background:
      Given the clinician is successfully logged in
      And the clinician sees the home page
    
    @PatientListScreen @polarionID=PLT1-1451
    Scenario: Verify Patient List table shows default columns
      When the clinician opens the patient list
      Then a table of patients is displayed
      And the table header includes Patient, MRN, and DOB
    

* * *

System Test Step Definitions (Typescript)
----------------------------------------------

* * *

```
Given('the clinician is successfully logged in', async ({ page }) => {

  await logInAsClinician(page);

});

  
Given('the clinician sees the home page', async ({ page }) => {

  const { homePage } = usePageObjects(page);

  await homePage.waitForReady();

});

  

When('the clinician opens the patient list', async ({ page }) => {

  const { patientList } = usePageObjects(page);

  await patientList.open();

});


Then('a table of patients is displayed', async ({ page }) => {

  const { patientList } = usePageObjects(page);

  await expect(patientList.patientListTable).toBeVisible();

});

  
Then('the table header includes Patient, MRN, and DOB', async ({ page }) => {

  const { patientList } = usePageObjects(page);

  const locale = 'en'; // Patient, MRN, DOB are the same in both languages

  await expect(patientList.patientNameTableHeader).toContainText(getTranslation(locale, 'PATIENTS_TABLE_PATIENT'));

  await expect(patientList.mrnTableHeader).toContainText(getTranslation(locale, 'PATIENTS_TABLE_MRN'));

  await expect(patientList.dobTableHeader).toContainText(getTranslation(locale, 'PATIENTS_TABLE_DOB'));

});
```
  

These steps:
*   Execute the BDD scenario directly
*   Validate **user‑visible behavior**
*   Are traceable to the Polarion work item via ID

* * *

Behavioral Validation Across Test Levels
----------------------------------------

* * *

BDD applies across **unit, component, and system tests**.

> Tests describe **behavior**, regardless of test level.

What changes is the **scope of behavior being validated**, not the intent.

* * *

Test Level Responsibilities
---------------------------

* * *

| Area | System / End‑to‑End Tests | Unit / Component Tests |
| --- | --- | --- |
| **Scope** | Full workflows | Single rule or service |
| **Focus** | User/API‑visible behavior | Business logic |
| **Dependencies** | Integrated | Mocked |
| **Execution speed** | Slower | Fast |
| **Audience** | Business, QA, devs | Devs, QA |

* * *

Unit‑Level Behavioral Validation (BDD Principles)
-------------------------------------------------

* * *

For unit tests, BDD:
*   Emphasizes **what the unit should do**
*   Uses business‑meaningful names
*   Follows Given / When / Then semantics
*   Validates rules in isolation

* * *

Unit Test Example
--------------------------------------------

* * *

**Polarion Work Item:** **[PLT1-2298 - The system successfully retrieves a patient list](https://polarion.hrc.corp/polarion/redirect/project/flc.platform.01/workitem?id=PLT1-2298)**
```
Scenario:
  Given A FHIR server is available with 5 patients registered  
  When A request is made to fetch the patient list with a maximum limit of 5  
  Then the patient list is successfully retrieved  
  And all 5 registered patients are returned
```

* * *

Unit test code (Kotlin)
----------------------------------------------

* * *
```
@Test  
@DisplayName("PLT1-2298 - The system successfully retrieves a patient list")  
fun `fetchPatientList where dagger is used for DI and TestContainer is used for FHIRServer`() {  
    runBlocking {
 
        // ** Given **
        PatientListConfig.maxPatient = 5
       
        // ** When **  
        val patients = repo.fetchPatientList()  
        val list = patients.getOrNull()!!.list

        // ** Then **
        assertTrue(list.isNotEmpty())  
        assertEquals(list.size, 5)  
    }  
}
```
  



**Characteristics**
*   References Polarion ID explicitly
*   External dependencies are controlled via test containers
*   Validates business rules (maximum patient count)
*   No UI interaction required

This is suitable for **unit or component‑level verification** of approved behavior.

* * *

Traceability Source of Truth
----------------------------

* * *

*   **Polarion work items** are the authoritative source of behavioral requirements
*   Gherkin scenarios define **behavior specification**
*   Automated tests are **implementations of that specification**

A single Polarion scenario may be validated by:
*   Multiple unit/component tests
*   One or more system tests

* * *

Traceability Rules for Verified Behaviors
-----------------------------------------

* * *

### Mandatory Rules

*   Every **Polarion verification work item** must be covered by **at least one automated test**
*   Every automated test used to verify a Polarion work item **must reference a Polarion ID**
*   Tests validate **approved behavior only**
*   Each traced test validates **one primary behavior**

* * *

### Explicit Non‑Requirements

*   **Not all unit tests must be traced**
*   TDD‑level and developer acceptance tests **do not require Polarion linkage**
*   Traceability applies only to tests demonstrating compliance with:
    *   Requirements
    *   Responsibilities
    *   Mitigations

* * *

Diagram 1 — Behavioral Traceability (Approved Behaviors)
--------------------------------------------------------

* * *

> **Note:** This diagram illustrates traceability for **approved behavioral requirements** only.  
> It does **not** imply that all unit tests must be traced.

::: mermaid
graph LR  

  A[Business Requirement]

  B["Polarion Verification Work Item<br/>(Approved Behavior)"]

  C["BDD Specification<br/>Given / When / Then"]

  D["Unit / Component Tests<br/>(Used for Behavior Verification)"]

  E["System / End-to-End Tests<br/>(Executable Gherkin)"]

  F[Verified Approved Behavior]

  

A --> B  

B --> C  

C --> D  

C --> E  

D --> F  

E --> F  
:::


* * *

Diagram 2 — Developer‑Level Tests (Outside Traceability Scope)
--------------------------------------------------------------

* * *

::: mermaid

graph TD  

  A[Implementation Code]

  B[TDD-Level Tests]

  C[Developer Acceptance Tests]

  D[Fine-Grained Unit &<br>Edge-Case Tests]

  

A --> B  

A --> C  

A --> D  
:::

These tests:
*   Are created during TDD or refactoring
*   Improve internal quality and confidence
*   Are **intentionally not traced to Polarion**

* * *

Summary
-------

* * *

This approach:
*   Keeps **Polarion as the single source of truth for approved behavior**
*   Ensures **audit‑ready behavioral traceability**
*   Supports **BDD and TDD simultaneously**
*   Avoids duplication and test bloat
*   Scales cleanly across system and unit testing

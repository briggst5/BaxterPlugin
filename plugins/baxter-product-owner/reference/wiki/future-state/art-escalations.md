# Future State/ART Escalations

# Work Instruction: Managing ART Sync Escalations in DevOps


## Purpose

Establish a consistent and transparent method for escalating risks, impediments, and cross-team concerns into Azure DevOps using the **Issue** work item type. This ensures timely review and action during Scrum of Scrums / Governance Board and ART Sync ceremonies.
 

---

  

## 1. When to Raise an Issue

Create an Issue in Azure DevOps when:

  

- A team-level impediment requires ART-level support

- A dependency threatens team or PI commitments

- A cross-team decision, alignment, or trade-off is required

- A risk has materialized and needs ART visibility

- A topic requires discussion in a future ART Sync

  

---

  

## 2. How to Create the Issue in Azure DevOps

  

1. Navigate to:


    - **Boards → Work Items → New Work Item → Issue**

  

2. Complete required fields:

-    **Title:** Clear statement of the problem

-    **Area Path:** Team's Path, e.g. FutureState\ScrumofScrums

-    **Iteration Path:** Current PI, e.g. FutureState\PI-26.1

-    **Description:** Include:

  -   Current state

  -   Impact to teams, milestones, or deliverables

    - Relevant context

  - Acceptance Criteria / Desired Outcome, i.e. what decision or action is needed?

-    **Tags:** Use `ART-Sync`, `Escalation`, or ART-defined tags

  

3. Select **Save**.

  

---

  

## 3. Information To Consider Before Submission

Consider including the following information, as necessary:  


- A clear problem statement

- Impact assessment (teams, milestones, compliance needs)

- Dependencies and linked work items (Features, Stories, CRs, Bugs)

- The objective of escalation (what support/decision is needed)

- An assigned owner

- A target date or required decision deadline

  

---

  

## 4. Link the Issue to Relevant Work Items

  

Use **Add Link → Related** to connect:

  

- Features

- User Stories

- Change Requests

- Bugs

- Prior ART Sync topics

  

This ensures traceability across the ART.

  

---

  

## 5. Routing and Visibility for ART Sync

  

Once submitted:

  

- The Issue appears on the **ART-level Issue Board / Query**

- Scrum Masters review team issues before ART Sync

- ART Leadership (RTE + Governance Board) may review Issues during pre-sync preparation

  

The Issue board is the **single source of truth** for escalations.

  

---

  

## 6. Responsibilities

  

### Team / Scrum Master

- Identify impediments requiring ART escalation

- Document Issues completely and accurately

- Maintain status and comments

- Present the Issue in ART Sync

  

### Product Owner

- Provide business context and priority input

  

### RTE / Governance Board

- Review Issues in ART Sync

- Provide or drive decisions

- Assign owners and follow-up actions

- Escalate further if necessary

  

### System Team / Shared Services

- Provide architecture, UX, platform, or specialist support

  

---

  

## 7. Workflow States for Issues

  

Use standardized Azure DevOps states:

  

- **New** – Created and awaiting review

- **Active** – Under investigation or ART follow-up

- **Resolved** – Decision or action identified

- **Closed** – Fully addressed; no further action needed

  

---

  

## 8. Review Cadence

  

- **Weekly:** During ART Sync

- **As needed:** For urgent escalations

- **Pre‑PI Planning:** Ensure no unresolved cross-team risks carry forward

  

---

  

## 9. Closure Requirements

  

An Issue may be closed when:

  

- The decision or action is complete

- Dependencies are resolved

- No further ART-level action is required

  

Owner updates status and final resolution notes.

  

---

  

## 10. Communication Expectations

  

- All escalations and updates must be captured in Azure DevOps

- Avoid side-channel discussions unless urgent

- ART Sync references the Issue board as the authoritative source

---

## ART Escalation Query
  ::: query-table 94012fd3-8016-4cd0-bba8-a93eff1427be
:::

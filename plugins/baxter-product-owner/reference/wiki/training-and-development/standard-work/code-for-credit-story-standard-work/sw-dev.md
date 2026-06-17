# Training and Development/Standard Work/Code-for-Credit Story Standard Work/SW Dev

As a Software Developer, it is important to understand if the User Story you are working on is intended to be Code-for-Credit (CfC) or Proof-of-Concept (PoC).  If you are unsure, raise the issue and ask!

The guidance contained within this page applies specifically to CfC.  **_As a reminder, CfC User Stories must deliver production quality software and the associated QMS compliant DHF artifacts_**.  This page contains guidance and best practices on how to most effectively collaborate to achieve those objectives.  The project’s software development plan (in alignment with GQP-09-05) ultimately defines what must be delivered and the process rigor required.

---

**How do I know the story is ready for a CfC?**

Before accepting a User Story as CfC, understand if the User Story is ready for CfC.  Use the following as guidance to determine if the User Story is ready for CfC from a SW Dev perspective:
* Distinct focus on execution and implementation.  There should be very little remaining technical risk for implementation in CfC stories.
* SW architecture must be sufficiently prepared and mature to support implementation.  (i.e. structural components, interfaces, data flows, and integration points reasonably well defined.)  If significant architectural work is required a separate enabling User Story may be needed to build out the architecture.
<br/>

---
**As a Software Developer working on a CfC User Story, WHAT can I be expected to deliver?**

Responsibilities of a software developer include:
- Collaboration with cross functional team on design inputs (requirements) and test cases/acceptance criteria
- Collaboration with SW Architect on alignment to the Software Architecture Design Specification (SADS)
- Collaboration with SW Lead on alignment of the test cases (i.e. story acceptance criteria) to Software Requirement Specification (SRS)
- Development of the Software Design Specification (SDS), Unit Verification Report (UVR) and traceability ([SDS and UVR Guidance](https://dev.azure.com/FLC-NPD/FutureState/_wiki/wikis/Platform.wiki/178/SDS-and-UVR-Guidance))
- Software implementation (inclusive of automated unit verification and possibly automated integration/system test code)
- Unit verification report and traceability
- Coordinate or participate in code review/pull requests ([Pull Request Guidance](https://dev.azure.com/FLC-NPD/FutureState/_wiki/wikis/Platform.wiki/179/Pull-Request-Guidance))
---
**As a Software Developer working on a CfC User Story, HOW do I work to deliver effectively?**

**Align**

Ensure alignment with the cross-functional team is established early and maintained throughout.  As CfC stories are expected to be completed within a 2 week sprint, aligning with the cross-functional team early and maintaining that alignment throughout the sprint is critical.

Early in the sprint (ideally before sprint planning but minimally on Day 1 of the sprint), collaborate with the cross-functional team to identify the definition and constraints of the intended design for the User Story.  Aligned to Test Driven Development, the design should be constrained by test cases, initially identified without concern for the level (i.e. product requirement, software requirement, design detail, etc.).  Depending on maturity of the PoC and pre-existing knowledge (i.e. standards, pre-existing requirements), the constraints may also be captured in draft form in Polarion.

It's important to consider both the expected behavior (i.e. "happy-path") and failure scenarios throughout.  Defining the design and its constraints is not intended to be a one time event.  As the team progresses throughout the sprint and new information emerges (failure modes, risk mitigations, etc.), additional test cases should be identified, refined, and added to the test suite.

**Design**

While the process is iterative in nature, some natural dependencies are needed to ensure good software engineering practices are followed.  The most important of these is design before implementing.

Standard work includes two design related task for software developers:
- Software Architecture Confirmation
    - Coordinate with SW Architect to align design to the SADS
    - Update SADS (with traceability to drive implementation) as needed
- Software Design Specification (SDD)
    - Define unit responsibilities and verification strategy (TDD)
    - Capture in Polarion as Work Items and submit for Peer Review
    - [SDS and UVR Guidance](https://dev.azure.com/FLC-NPD/FutureState/_wiki/wikis/Platform.wiki/178/SDS-and-UVR-Guidance)

**Implement**

During implementation, standard work includes
- Implementation of automated tests (aligned to unit verification strategy and/or VVP)
- Implementation of product code
- Updates/additions to the SDD as necessary

Implementation
- Should take place on a dedicated feature branch
- Follow the PR process for approval ([Pull Request Guidance](https://dev.azure.com/FLC-NPD/FutureState/_wiki/wikis/Platform.wiki/179/Pull-Request-Guidance))
- Cannot be considered completed until
    - PR process is complete
    - All quality gates and tests are passing

**Deliver**

During this step, software developers should collaborate to ensure
- Test cases are refined to requirements/specification at the proper level  (Sys Req, SW Req, SDS, etc.)
- Traceability is established between all Polarion work items
- All Polarion work items have been approved

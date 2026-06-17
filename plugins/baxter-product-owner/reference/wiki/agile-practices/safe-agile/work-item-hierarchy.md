# Agile Practices/SAFe Agile/Work Item Hierarchy

[[_TOC_]]

# Process Overview

The Scaled Agile Framework (SAFe) is a process for planning and tracking work items. It's organized around Epics, Features, and Stories to track broad project goals, and Tasks for tracking action.  

# SAFe Overview

## Epics

An Epic is a significant solution development initiative that delivers business value or supports the Architectural Runway and future business functionality. Epics are typically large in scope and may span multiple Value Streams and Planning Intervals (PIs). For more info: [Epics in SAFe](https://share.vidyard.com/watch/T5P21eCvYgY3iiU1o6E5sd)

In DevOps, Epics include:
- **Description**: A clear statement of the Epic's intent, expected outcomes, and definition of MVP.
- **Technical Readiness**: The team's assessment of the technical understanding of the Epic.

## Epic Ownership
Epics are created by product stakeholders, who provide input on the intent, benefits, and expected outcomes. A designated Epic Owner is responsible for coordinating the Epic through the Kanban system, ensuring alignment with strategic objectives, and collaborating with various stakeholders to define and prioritize the Epic.

## Features

A Feature is a piece of solution functionality that delivers business value and fulfills stakeholder need. Features may deliver customer-centric value or support business requirements such as architecture or infrastructure.  More simply, features are collections of stories.

In DevOps, Features include:
*   **Description**: A clear statement of what the feature is and explains the value it delivers
*   **Technical Readiness**: The team's assessment of the technical understanding of the Feature

### Feature Ownership
Features are primarily owned by product managers, who ensure that the features align with the overall goals of the Epic parent and that the execute delivers value to stakeholders. Once the Feature is defined, the Agile Teams are responsible for breaking the feature down into smaller work items (like user stories) and implementing them during iterations.

### Feature Lifetime

Following SAFe, _Features_ drive a delivery within an _Agile Release Train_. Ideally, they are constrained to a single PI.  Features that extend beyond a PI is a process "smell" that indicates that the feature is not scoped correctly.  Implications includes:

* Size and Complexity - The feature may need to be split into smaller, more manageable parts that can be delivered and most importantly, demonstrated to stakeholders.
* Risk Management - Features that extend beyond a PI introduce higher project risk as it is harder to manage dependencies and deliver value incrementally.
* Priority Mismatch - Features that extend across multiple PIs spread the focus of the team, and create priority mismatches where the team is not working collaboratively to execute to a solutions, and then there becomes an increased likelihood for priority mismatches as team members work on other features.


## User Stories

A User Story is a brief, informal description of a feature or requirement from the perspective of the user. The User Story forms the "contract" between the product owner and the team members performing the work.

In DevOps, User Stories include:
*   **Description**: The story itself, always of the form "As a [user type], I want to [perform a task], so that [I can achieve a goal]"
*   **Acceptance Criteria**: Communicates what the team must deliver to complete the story
*   **Platform Decision and Rationale**: Assesses whether this user story drives creation of a platform or product specific delivery

### User Story Lifetime

Following SAFe, User Stories are expected to be completed within **a single Sprint**. User Stories are meant to deliver value incrementally, and each Story should be sized to fit within one iteration. 

## Tasks
Tasks are the smallest unit of work.  The Agile Team analyzes the requirements of a User Story and then decomposes the work into Tasks.  Each task tracks the work required to deliver some functionality by an individual.

Each Task includes a formal _Definition of Done_, which is always of the form "I know that I am done when...", and then a list of things that the Agile Team member will _demonstrate_ when the work is finished.  

In DevOps, Tasks include:
- Description
  - The description of the work that the Agile Team member needs to be perform
  - The Definition of Done 
  - The Planned, Remaining, and Completed _Story Points_ for the task


# Lifetime of Work Items

The following durations are guidelines for determining when a work item is sized correctly.  Remember, guidelines aren't rules.  The guidelines are useful to detect when an item needs to be further refined.  When a task requires more than a single sprint, it may be ill defined or have many dependencies.  The task should be split up to provide better tracking, realize progress towards completion, and allow reassignment to others to improve velocity.

|Type| Points | Duration |
|--|--|--|
| Epic | > 100 | Epics have no defined time bound and may be open throughout the whole project |
| Feature | 20 to 100 | Features should be completed within a single PI |
| User Stories | 1 to 20 | Should be completed within one sprint.* |
| Tasks | 1 to 3 | Should be completed within a Sprint |

 
# Work Item Hierarchy
The following figure shows the relationships between work items.  

![image.png](/.attachments/image-7b3ec428-9e0d-4da9-b393-da6f00786ae8.png)****

# Example of Epics, Features, and Stories

The following example illustrates a good example of describing work as an Epic/Feature/Story.  We can see that the Story is focused on delivering value to a user's perspective.  More importantly, the user story says nothing about the _many_ complications that go along with early warning scores, e.g.,: national standards, partial scores, sub-scores, configurability, custom scores, units, compute engine, and error handling.  

We should be able to envision delivering at least a proof-of-concept for the story in a sprint, and then do a demo.  This gives the _stakeholder_ a chance to review the result, and then write new stories for refinement of the existing demo or add new functions.  Importantly, it also gives the team an opportunity to deliver something completely and move on.  

Review the [Guide to Writing User Stories](/Agile-Practices/Guide-to-Writing-User-Stories)

**Epic:  Enable nurses to receive early warnings for patient deterioration**

**Feature: Early Warning Score Display**
_Description_: Display early warning score in real-time to identify patients at risk and take action.
_Story Points_: 34
_Acceptance Criteria_:
- Score is calculated and updated every minute
- Display is color-coded (green/yellow/red) based on risk-level
- Score determines the color-coding

**User Story: Display EWS on Patient Monitor**
_Story_: As a clinician at the bedside, I want to see the patient's Early Warning Score on the home screen, so that I can assess patient status without interacting with the device.
_Story Points:_ 13
_Acceptance Criteria_:
- Score is visible on the home screen
- Score updates every minute
- Score color changes based on threshold
   - Green: score 0 - 3
   - Yellow: score 4 - 6
   - Red: score 7+
- If score cannot be calculated, a "--" indicator will be shown
- Unit test coverage will be at least 80% for calculation logic

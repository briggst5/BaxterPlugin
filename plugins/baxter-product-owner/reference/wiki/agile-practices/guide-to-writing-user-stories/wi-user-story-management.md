# Agile Practices/Guide to Writing User Stories/WI - User Story Management

**Work Instruction (WI): User Story Management in Platform Backlog – Azure DevOps**

[[_TOC_]]   

# Purpose 
This work instruction provides a comprehensive guide for the creation and management of user stories in an agile backlog. It explicitly addresses the collaborative nature of the process, including the specific scenario where a team member proposes a new story.

# User Story Process Workflow
This diagram is intended to complement the process that follows, with the expressed purpose of adding clarity to the transition of work item states and owner. <br>
*[Platform User Story Management.pdf](/.attachments/Platform%20User%20Story%20Management-6985b0c3-a57c-4da3-a881-6143162ce682.pdf)*


## User Story Creation

This phase involves the initial drafting of a user story and its acceptance criteria in Azure DevOps. While the Product Owner is the primary author, any team member can propose a story.
*   **Responsibility**: The **Product Owner** is ultimately accountable for the content and clarity of all stories. The **Development Team** members are empowered and encouraged to propose stories, especially when they identify technical debt, a potential improvement, or a solution to a user problem they have uncovered.
*   **Process**:
    1.  **Draft the User Story**: A team member or the Product Owner drafts a user story following the standard format:
        *   **As a** `[user role]`: Identify who the user is.
        *   **I want to** `[goal]`: Describe the desired action.
        *   **So that** `[reason/benefit]`: Explain the value or purpose.
    2.  **Add Preliminary Details**: The author should add any initial information they have, such as:
        *   A brief description of the problem or opportunity.
        *   Any known technical considerations or dependencies.
    3.  **Define Acceptance Criteria**: In the "Acceptance Criteria" field, list the conditions that must be met for the story to be considered complete. These should be clear, testable statements. It is necessary that both the team and Product Owner align on clear, testable **Acceptance Criteria**.
    4.  **Propose the Story for Review**:
        *   **If proposed by a Team Member**: The team member creates a draft story and brings it to the Product Owner's attention, typically during a backlog refinement meeting or a scheduled one-on-one. The story is initially in a "New” or similar state.
            *   Tags: Use `tags` (e.g., `NeedsPO`) to enable visibility on proposed stories that require review and refinement.
        *   **If created by the Product Owner**: The story is created and immediately available for the team to review and refine.

## Review and Refinement

This is a collaborative phase where the team and Product Owner discuss, clarify, and flesh out the user story.
*   **Responsibility**: A collaborative effort between the **Product Owner** and the **Development Team**.
*   **Process**:
    1.  **Backlog Refinement**: The Product Owner may lead a backlog refinement session with the entire team (or targeted audience) to discuss user stories.
    2.  **Present the Story**: The author of the story (Product Owner or team member) presents the draft.
    3.  **Discussion**: The team asks questions and discusses the "what" and "why" of the story. This conversation is crucial to uncover missing details, assumptions, and potential technical challenges. This information is added to the work item, as appropriate.
    4.  **Story Owner Approval**:
        *   **For a Team-Proposed Story**: The Product Owner reviews the story, its acceptance criteria, and its value proposition. The story is **contingent on the Product Owner's review and approval**. The Product Owner's acceptance of the story confirms its value to the product backlog and enables the removal of the `NeedsPO` tag from the work item.
    5.  **Effort Estimation**: The team discusses the story's complexity and estimates the effort required to complete it (e.g., using story points or T-shirt sizes). This estimate helps with prioritization and sprint planning.
    6.  **Parenting**: If the story is part of a larger initiative, link it to a parent **Feature** or **Epic** using the "Add link" option on the work item form. This creates a clear hierarchy.
    7.  **Task Breakdown**: For larger stories, the team should create child **Tasks** directly from the story.

## Prioritization

The Product Owner orders the backlog to reflect the highest business value and urgency. Technical dependencies should be considered relative to parallel workstreams.
*   **Responsibility**: The **Product Owner** is responsible for the final prioritization of the backlog.
*   **Process**:
    1.  The Product Owner uses the effort estimations and their understanding of business value to order the stories.
    2.  **ART Alignment**: During planning or other ART-level syncs, Product Owners should review and align on priorities across teams. This ensures stories are prioritized to support the overall objectives of the Agile Release Train (ART), to include recognition of any dependencies that enable future work.
    3.  The backlog is arranged from the highest priority at the top to the lowest at the bottom.
        *   The backlog view is a prioritized list by default.
        *   **Drag-and-Drop Prioritization**: The Product Owner can simply drag and drop stories to reorder them. The highest priority items should be at the top of the list. Azure DevOps automatically updates the Stack Rank field to reflect this new order.
        *   **Priority Field**: The Priority field (1=highest, 4=lowest) can be used as a secondary method for a broader categorization of importance, but the drag-and-drop order is the primary way to prioritize work for the team.
    4.  The Product Owner considers dependencies—for example, a foundational technical story may need to be prioritized ahead of the user-facing stories that rely on it.
    5.  Stories at the top of the backlog are considered for the next sprint during Sprint Planning.

## Acceptance

Once a story is completed by the development team and is marked as ‘Resolved’, the Product Owner must confirm it meets all acceptance criteria for formal closure.
*   **Responsibility**: The **Product Owner** is responsible for the final acceptance of the user story.
*   **Process**:
    1.  When a developer completes a story, they move its state from "Active" to "Resolved" in Azure Boards.
    2.  The Product Owner reviews the completed work, testing the functionality against the defined **Acceptance Criteria**. This can also be accomplished through evidence provided in [sprint] demonstrations.
    3.  **Acceptance**:
        *   If all criteria are met, the Product Owner changes the story's status to **"Closed".** This marks the story as complete and provides a clear record that the work is finished and accepted.
        *   The Product Owner can add a comment in the **Discussion** section to confirm acceptance.
    4.  **Rejection**:
        *   If the story does not meet all criteria, the Product Owner moves the story back to an "Active" state, adds comments in the **Discussion** section explaining what needs to be fixed, and reassigns it to the responsible team member. A new story may be recommended to fix the issues. In this case, the user story would be addressed by the developer and presented for acceptance in the same way.

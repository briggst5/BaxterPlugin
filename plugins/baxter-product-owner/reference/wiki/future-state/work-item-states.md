# Future State/Work Item States

# Work Item States: Understanding Our Workflow  
  
This page defines the various states for our work items (User Stories, Bugs, Features, etc.) in Azure DevOps. Understanding these states is crucial for tracking progress, managing our backlog, and maintaining an accurate representation of our development efforts.  
  
## State Transitions Flow  
  
**Typical Workflow:** New → Active → Resolved → Closed  
**For Bugs (with a potential for re-opening):** New → Active → Resolved → Closed (can revert to Active if re-opened)  
**Special States:** Waiting, Removed  

![image.png](/.attachments/image-aee7320b-9565-4028-be31-d7dbf18b5f7b.png =600x)

---  
  
## State Definitions  
  
Here's a detailed breakdown of each state:  
  
### 1. New  
  
* **Definition:** This is the initial state for any newly created work item. The work item has been identified and entered into our system, but no work has begun on it yet.  
* **Purpose:** To capture new requirements, bugs, or feature ideas. It signifies that the item is in the backlog, awaiting prioritization and assignment.  
* **When to Use:**  
    * When a new User Story is groomed and added to the backlog.  
    * When a Bug is reported for the first time.  
    * When a new Feature idea is proposed.  
* **Transitions From:** N/A (Initial State)  
* **Transitions To:** Active, Waiting, Removed, or potentially directly to Closed if deemed not applicable without work.  
  
### 2. Waiting  *(reflects block at the task level)*
  
* **Definition:** The work item is currently blocked or paused, awaiting an external dependency or information before active development can continue. No immediate action can be taken by the assigned team member.  
* **Purpose:** To clearly indicate that a work item is not actively being worked on due to a dependency outside the team's immediate control. *This helps in identifying bottlenecks and communicating progress accurately.*  
* **When to Use:**  
    * Awaiting design mock-ups from the UX team.  
    * Waiting for API documentation from a third-party vendor.  
    * Blocked by another team's deliverable.  
    * Waiting for stakeholder approval or clarification.  
* **Transitions From:** New, Active, Resolved  
* **Transitions To:** Active (once the block is removed), Resolved (if the block resolves the item without further work), Removed, Closed.  
  
### 3. Active  
  
* **Definition:** Work on the item has officially begun. The assigned team member is actively working on the task, developing, testing, or researching as required.  
* **Purpose:** To indicate that the work item is currently in progress and is being addressed by the team.  
* **When to Use:**  
    * A developer starts coding a User Story.  
    * A tester begins investigating a Bug.  
    * A designer starts working on the UI for a Feature.  
* **Transitions From:** New, Waiting, Resolved (if a Bug is reopened).  
* **Transitions To:** Resolved, Waiting, Removed, or potentially Closed (e.g., if it was a quick fix that didn't need a separate resolved state).  
  
### 4. Resolved  
  
* **Definition:** The primary work on the item is complete. For a User Story or Feature, this typically means the code has been written and unit-tested. For a Bug, it means the fix has been implemented. *The item is now ready for verification or review.*  
* **Purpose:** To signal that the development or initial fix is done and the item is ready for the next stage, usually testing or user acceptance.  
* **When to Use:**  
    * A developer finishes writing code for a User Story.  
    * A Bug fix has been implemented and committed.  
    * Initial documentation for a Feature is complete.  
* **Transitions From:** Active, Waiting  
* **Transitions To:** Closed, Active (if a Bug is re-opened), Removed.  
  
### 5. Closed  
  
* **Definition:** The work item has been fully completed, verified, and accepted. For User Stories and Features, this means it has been deployed and validated. For Bugs, it means the fix has been confirmed, and the bug no longer exists.  
* **Purpose:** To signify that the work item is entirely done and no further action is required. This is the final state for successfully completed work.  
* **When to Use:**  
    * A User Story has been deployed to production and verified.  
    * A Bug fix has been tested and confirmed.  
    * A Feature has been released and accepted by stakeholders.  
* **Transitions From:** Resolved, New (if deemed not applicable without work), Active (for very simple tasks that don't need a resolved step).  
* **Transitions To:** N/A (Final State, though Bugs can sometimes be reactivated to "Active" if reopened).  
  
### 6. Removed  
  
* **Definition:** The work item is no longer relevant, required, or valid. It has been permanently removed from the active backlog and will not be worked on.  
* **Purpose:** To clean up the backlog and remove items that are no longer pertinent without actually completing them. This is different from "Closed" as no work was completed.  
* **When to Use:**  
    * A duplicate Bug is found.  
    * A Feature request is no longer aligned with product strategy.  
    * A User Story is deprecated by another feature.  
    * The scope of a project changes, making an item obsolete.  
* **Transitions From:** New, Waiting, Active, Resolved  
* **Transitions To:** N/A (Terminal State - typically not reverted once removed).  
  
---  
  
## Best Practices for State Management  
  
* **Be Diligent:** Always update the state of your work items promptly to reflect their current status accurately.  
* **Communicate:** If you move an item to "Waiting," add a comment explaining the blocker and who is responsible for resolving it.  
* **Define "Done":** Ensure your team has a clear "Definition of Done" for each work item type. This helps in consistently moving items to "Resolved" and "Closed."  
* **Review Regularly:** Periodically review your backlog and the states of your work items to identify any stalled or miscategorized items.

### Note(s): 
- An 'Active' work item should be assigned to the current sprint, otherwise the state should be changed and moved to the backlog for future consideration.

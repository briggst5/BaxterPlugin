# Training and Development/Standard Work/Code-for-Credit Story Standard Work/UX

It is important to make certain that the sprint you're starting is indeed ready to be a "Code for Credit", so it's best to run through a quick checklist to make sure:

## How do I know the story is ready for a CfC sprint?
* Wireframes are finished and reviewed by SW, SWT, HF and SYS from PoC sprint. There should be no ambiguity remaining, no more questions to ask about the "what" or the "how". If there are, **this needs to be a Proof of Concept sprint (PoC)**.
* ALL components needed to create the screens are already implemented
A component would be a reusable UI element, which itself contains one or more DLS atoms or molecules. If you require UI assets that do not yet exist, they must be able to be completed in ONE DAY, as the need to hand off assets early is critical to other team members. If you have any doubt to the one day rule, **raise the issue that this story should be a PoC**

## Basic rules of thumb for more specific sprint types:
*   CfC sprint - Single screen
    *   you're implementing one main screen, or updating an existing one
    *   you've addressed edge cases using 80/20 rule (SW/SWT agrees)
*   CfC sprint - Screen flow
    *   you've diagrammed the flow and reviewed with SW, SWT
    *   screens are COMPLETE from previous CfC sprint
    *   you've addressed edge cases (80/20) for the FLOW

The operational flow from here is illustrated below:
![image.png](/.attachments/image-5047a5d9-fba6-46a6-8a05-8bb38e05c302.png)

# Tasks for the sprint

### Step 1: Quick review of PoC with SW, SWT (for screens only) and CLIN, MKT as needed for flows
Quick edit cycle if necessary and re-review, this should answer all the conerns listed above for if you're ready for a CfC sprint or if you need more PoC to fill in gaps.

From here there are two threads that need to be addressed, the documentation (UIS) and the asset handoff (Figma). They can be handled by different individuals concurrently if resources exist.

### UIS
1.  Finalize entries in the UIS (for screens)
1.  Finalize screen navigation sections of UIS (for flows)
1.  Set sections to Peer Review
1.  Watch for feedback and address
1.  When all feedback addressed, set to Formal Review
        
###  Figma
1. Create screen in appropriate project (see Figma Best Practices - *)
    *   if screen belongs to new primary top-level area, create new Figma Page
    *   create Figma section

1.  ASSEMBLE screen (remember, there should be NO new components!)
    *   Create screen variations horizontally (different states, conditions)
    *   draw flow lines if helpful
1. Mark Ready for Dev / alert SW developer
   * Watch for feedback
   * Address feedback immediately, the clock is ticking...
1. Review implementation in Chromatic
The developer working on this will notify you when their implementation is ready for review. Use Chromatic / Storybook to verify that it is implemented as intended

1. Approve implementation when ready
1. Mark tasks complete in DevOps and set User Story to "Resolved"

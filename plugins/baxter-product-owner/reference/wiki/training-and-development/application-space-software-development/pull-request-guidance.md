# Training and Development/Application Space Software Development/Pull Request Guidance

[[_TOC_]]

# References
The SDP Defines [PLT1-790 - Development Practices](https://polarion.hrc.corp/polarion/redirect/project/flc.platform.01/workitem?id=PLT1-790)

# Creation
- Ensure title and description are complete 
  - The title will be the first line of the commit message, it should sum up the work
  - The description will be included in the commit message, mention what has been added, removed, fixed, or changed. 
- Ensure relevant work items are linked
  - Azure DevOps will add the work items to the commit message upon completion

# Review
- **Pull Requests are part of our formal code review process**. 
- Ensure code aligns with detailed designs and unit verification 
- See [SDS and UVR Guidance](/Training-and-Development/Standard-Work/Code%2Dfor%2DCredit-Story-Standard-Work/SW-Dev/SDS-and-UVR-Guidance) for full review checklist
- PRs require 2 SW engineers and SW Test as optional

# Completion
- When completing, select merge type "Squash commit" and delete source branch after merging
  - "Squash commit" will convert all feature branch commits into a single commit with the Pull Request title as the first line of the commit message, the description as the next lines of the message, and linked work items append at the end

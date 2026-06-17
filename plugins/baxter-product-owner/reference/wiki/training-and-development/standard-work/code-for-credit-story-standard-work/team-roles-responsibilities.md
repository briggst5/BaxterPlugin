# Training and Development/Standard Work/Code-for-Credit Story Standard Work/Team Roles & Responsibilities

# **Overview**
 
Scaled Agile is how we coordinate multiple Scrum teams under a shared direction to deliver one product. An overall team (i.e. FLC Platform Overall) maintains Feature alignment and direction while individual scrum teams (Scrum Teams 1 - N) execute and deliver User Stories

![image.png](/.attachments/image-ddbdad02-906e-42da-a315-558571d0857e.png)

#  **Roles**

As defined in [Code-For-Credit Standard Work](https://dev.azure.com/FLC-NPD/FutureState/_wiki/wikis/Platform.wiki/158/Code-for-Credit-Story-Standard-Work), the minimal cross-functional scrum team required to deliver a CfC User Story is listed below:
*   Systems
*   UX (if the User Story contains scope associated with user facing functionality)
*   Software Test
*   Software
    * Software Lead
    * Technical Lead/Architect
    * Software 

Note: The team may have additional roles aligned to SAFe (Scrum Master, Product Owner etc.).  The definition of those roles and their responsibilities is outside the scope of this Wiki page.

# **Responsibilities**
This page extends the definitions of the key cross-functional roles and explicitly outlines the responsibilities of each role at the team level.

- Systems (Sys)
- UX (UX)
- Software Test (SwT)
- Software Lead (SwL)
    - Represents SW Function to extended core team
    - Ensure team delivery aligns to SDP
    - Review team User Stories to ensure alignment to Feature delivery
    - Secondary PR approver for team PRs (Platform SW PR Approvers)
- Technical Lead (TechL)
    - Ensure team delivery aligns to architectural vision
    - Primary PR approver for team PRs (Platform SW PR Approvers)
- Software Devs (SwDev)
    - Follows established SDP to deliver software implementation aligned to scope of User Story



The table below provides more detail to expectations of each role with respect to the delivery  (creation and approval) of DHF artifacts in Polarion and software.  <br><br>

|   | **Sys  <br>Req** | **SW  <br>Req** | **UIS** | **Sw Arch** | **Sw  <br>FMEA** | **SHA** | **SW  <br>(Code)** | **SDS  <br>(Detail)** | **SDS  <br>(Test Case)** | **IDS** | **VVP** | **VVR** |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| **Sys** | P |   |   |   |   | P |   |   |   |   |   |   |
| **SwL** | A | A |   | A | A | A | A |   |   |   | A | A |
| **UX** |   |   | P |   |   |   |   |   |   |   |   |   |
| **TechL** |   | A |   |   | A | A | A | A | A | A |   |   |
| **SwT** | A |   | A | A |   |   | A | A | A  |   | P | P |
| **SwDev** |   |   |   | A | P |   | P | P | P | P |   |   |

(P) = Role is author of the content (Polarion work item or Code)
(A) = Role is an approver for the work item (Polarion Peer Review/Formal Review or PR Approver)

Notes:
1. The table represents the minimal amount of cross functional collaboration required.  Additional roles should be added as reviewers as necessary.
2. Product Quality is a required approval on all DHF artifacts except SW (Code).
3.  There is no (P) for SW Req and Sw Arch items.  To maintain a single voice in high level, directionally important documents, they will be authored by the SW Architect at the overall platform level.

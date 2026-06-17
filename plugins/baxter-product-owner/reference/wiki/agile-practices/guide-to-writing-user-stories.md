# Agile Practices/Guide to Writing User Stories

# Writing Effective User Stories in SAFe

This training module provides a practical guide for engineering teams to write high-quality user stories within the **Scaled Agile Framework (SAFe)**. It covers structure, value alignment, traceability to features and epics, and common pitfalls.

## Why User Stories Matter

- Communicate **who**, **what**, and **why**

- Clarify work for developers and stakeholders

- Enable iterative delivery and fast feedback

  
## SAFe User Story Definition

- Format: `As a [user role], I want [goal/activity] so that [benefit/business value]`

- Represents a *small, valuable piece* of functionality

- Should be INVEST-compliant

## INVEST Criteria

| Letter | Principle    | Description                             |
|--------|--------------|-----------------------------------------|
| I      | Independent  | Can be implemented separately           |
| N      | Negotiable   | Flexible and open to discussion         |
| V      | Valuable     | Delivers value to the end user          |
| E      | Estimable    | Can be sized by the team                |
| S      | Small        | Fits in one iteration (2 weeks or less) |
| T      | Testable     | Has clear acceptance criteria           |

##Acceptance Criteria
- Provide details from a testing point of view

- Created by the agile team

- Can be written in the Given - When - Then format (example below)
![image.png](/.attachments/image-c1dba21e-0abf-42fb-9f7b-d01b67c7755f.png)

## Connecting to Features and Epics
- **Epics** → large initiatives across trains

- **Features** → capabilities within a PI

- **User Stories** → team-level deliverables

**Example Traceability:**

```

Epic → Feature → Story

"Improve emergency response times" → "Mobile ECG Viewer" → "View ECG data"

```

> Every user story should map to a feature, and every feature to an epic.

  
## SAFe Roles Involved

- **Product Owner:** Defines and prioritizes stories

- **Scrum Master:** Facilitates team processes

- **Agile Team:** Estimates and delivers

- **System Architect:** Defines enablers and supports NFRs

---
## Example Story

> *As a clinician, I want to view ECG results on my tablet so I can make timely decisions.*
 

**Acceptance Criteria:**

- ECG displays in under 5 seconds

- Chronological order

- Gesture support: zoom and pan

---

## Common Pitfalls

- Writing tasks instead of user value

- No acceptance criteria

- Stories too large (should be split)

- Missing user role or benefit

  
## Enabler Stories

- Support exploration, architecture, infrastructure

- Follow same INVEST model

- Deliver indirectly visible value

---
> *As a system architect, I want to evaluate Bluetooth stacks...*

---

## How to Write a Good Story

1. Identify the **user**

2. Use the **story format**

3. Focus on **value**

4. Confirm **INVEST** principles

5. Add **acceptance criteria**

  
## From Features to Stories

Example Feature: *Mobile ECG Viewer*  

Break down into:

- View ECG data

- Add annotations

- Filter by date

- Export results

## Team Workshop Exercise

- Form small groups

- Pick a real feature

- Write 2–3 user stories

- Share and critique together

## **Key Takeaways**

✅ Write for the user  

✅ Apply INVEST  

✅ Include acceptance criteria  

✅ Map stories to features and epics  

✅ Collaborate across team roles


## 📎 Downloads

[SAFe_User_Story_Training.pptx](https://worksites.baxter.com/:p:/r/sites/FLCPlatform/Shared%20Documents/General/SAFe_User_Story_Training.pptx?d=we1ca93b69f434181a4d1000cac663005&csf=1&web=1&e=TEn5YH)

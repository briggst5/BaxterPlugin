# Agile Practices/SAFe Agile/Agile Ceremonies - SAFe

[[_TOC_]]

---

# Introduction to SAFe

The Scaled Agile Framework (SAFe) is a set of organizational and workflow patterns for implementing agile practices at enterprise scale. It promotes alignment, collaboration, and delivery across large numbers of agile teams. This wiki serves as a guide to key SAFe events and synchronization mechanisms.

# Key SAFe Cadences & Events

## PI Planning

Program Increment (PI) Planning is a cornerstone event in SAFe, typically a two-day event where all teams in an Agile Release Train (ART) gather to plan the upcoming Program Increment.

### Purpose

* To create a shared understanding of the upcoming business objectives and features.
* To identify dependencies and facilitate cross-team collaboration.
* To create a high-level plan (PI Objectives) for the entire ART.
* To identify risks and create mitigation plans.

### Planning Attendees

* **All ART Members:** Agile Teams (Scrum Masters, Product Owners, Developers)
* **Stakeholders:** Business Owners, Product Management, System Architects/Engineers, Release Train Engineer (RTE)
* **Specialists:** UX, operations, security, etc.

### Planning Agenda Overview

* **Day 1:** Business context, product vision, architecture vision, team breakouts for draft plan, dependency identification, initial risk identification.
* **Day 2:** Management review and problem-solving, final plan review, program board creation, risk resolution, confidence vote, PI planning retrospective.

### Planning Outputs

* **Committed PI Objectives:** Business-value-driven objectives for the PI.
* **ART/Program Board:** Visualizes features, dependencies, and milestones.
* **Resolved Risks:** Identified and addressed program-level risks.
* **Confidence Vote:** A measure of the ART's confidence in achieving the PI objectives.

<center>
<img src="https://framework.scaledagile.com/wp-content/uploads/2022/11/PI_F06-4.svg" style="width:50%;">
<br> 

*Source: [PI Planning - Scaled Agile Framework](https://framework.scaledagile.com/pi-planning)*
</center>

## Innovation and Planning (IP) Iteration

The Innovation and Planning (IP) Iteration is a dedicated iteration that occurs at the end of each Program Increment (PI). It serves several important purposes beyond typical sprint work.

### IP Purpose

* **Innovation:** Time for exploration, research, prototyping, and preparing for future PIs.
* **Planning:** Dedicated time for the next PI Planning event prep.
* **Continuing Education:** Allows for training and skill development.
* **Infrastructure & Tooling:** Opportunity to improve the development environment.
* **Hardening:** Time to finalize System Demos, address technical debt, and prepare for release.
* **PI Planning Prep:** Crucial for preparing content and logistics for the upcoming PI Planning.

### IP Activities

* Hackathons
* Spikes (research projects)
* Refactoring and technical debt cleanup
* Tooling upgrades
* SAFe training
* Finalizing System Demos and Solution Demos
* Preparing for the Inspect & Adapt workshop (metrics gathering)

### Benefits

* Buffers against unforeseen scope or capacity issues.
* Fosters continuous learning and improvement.
* Ensures the ART has time to prepare for future work efficiently.
* Reduces pressure on development iterations for non-feature work.

<center>
<img src="https://framework.scaledagile.com/wp-content/uploads/2023/03/IP_Iteration_F02-1.svg" style="width:50%;">
<br> 

*Source: [Innovation and Planning Iteration - Scaled Agile Framework](https://framework.scaledagile.com/innovation-and-planning-iteration)*
</center>

## Inspect & Adapt (I&A) Workshop

The Inspect & Adapt (I&A) workshop is a significant event held at the end of each PI, following the System Demo. It's where the Agile Release Train (ART) reflects on its performance and identifies systemic improvements.

### I&A Purpose

* To quantify and understand the ART's performance.
* To identify root causes of problems and impediments.
* To create actionable improvement items for the next PI.
* To continuously improve the ART's process and effectiveness.

### Components

1.  **PI System Demo:** The culmination of the entire PI, demonstrating the integrated solution to stakeholders.
2.  **Quantitative Measurement:** Reviewing objective metrics (e.g., PI predictability measure, flow metrics, burn-ups/downs).
3.  **Problem-Solving Workshop:** Facilitated root-cause analysis using techniques like Fishbone diagrams and the "5 Whys" to identify improvement backlog items.

### I&A Output

* **Improvement Items:** Specific, actionable improvements to be prioritized in the ART Backlog for the next PI.
* **Shared Understanding:** Agreement on what went well, what didn't, and how to improve.

---

## SAFe Synchronization Meetings

These are ongoing sync meetings designed to maintain alignment and address emerging issues throughout a PI.

<center>
<img src="https://framework.scaledagile.com/wp-content/uploads/2022/11/PI_F04-3.svg" style="width:40%;">
<br> 

*Source: [Planning Interval (PI) - Scaled Agile Framework](https://framework.scaledagile.com/planning-interval)*
</center>

### ART Sync (Scrum of Scrums)

The ART Sync, also known as the Scrum of Scrums, is a key meeting for the Release Train Engineer (RTE) to facilitate synchronization across all teams in the ART.

[Facilitator's Guide to SAFe - ART Sync.pdf](/.attachments/Facilitator's%20Guide%20to%20SAFe%20-%20ART%20Sync-7e0be42a-bab8-4455-b278-442d8e0b7f01.pdf)

#### ART Sync Purpose

* To identify cross-team impediments and dependencies.
* To ensure alignment on progress towards PI Objectives.
* To address risks that span multiple teams.
* To coordinate technical activities, especially around integration.

#### ART Sync Attendees

* Release Train Engineer (RTE)
* Scrum Masters from each Agile Team
* Optional: System Architect, other specialists as needed

#### ART Sync Topics

* Progress against PI Objectives
* Impediments that teams cannot resolve on their own
* Dependencies between teams
* Risks that have emerged
* Decisions regarding technical coordination or integration

### PO Sync

The PO Sync is a meeting for Product Owners (POs) and Product Management to synchronize on content and priorities throughout the PI.

#### PO Sync Purpose

* To ensure alignment on feature scope, acceptance criteria, and priority.
* To address content-related issues and dependencies.
* To prepare for future iterations and the next PI Planning.
* To coordinate customer and stakeholder feedback.

#### PO Sync Attendees

* Product Management (PM)
* Product Owners (POs) from each Agile Team
* Optional: Release Train Engineer (RTE), System Architect

#### PO Sync Topics

* Feature progress and health
* Backlog refinement at the ART and team level
* Prioritization decisions
* Dependency resolution related to features
* Upcoming customer needs or feedback
* Preparation for System Demos

### Coach Sync

The Coach Sync is a meeting for the RTE and Scrum Masters to discuss agile practices, coaching needs, and continuous improvement within the ART.

#### Coach Sync Purpose

* To foster a learning community among Scrum Masters.
* To discuss patterns, challenges, and successes in agile adoption.
* To identify common impediments and areas for coaching.
* To share best practices and help each other grow.

#### Coach Sync Attendees

* Release Train Engineer (RTE)
* Scrum Masters from each Agile Team
* Optional: Agile Coaches, other specialists as needed

#### Coach Sync Topics

* Team health and dynamics
* Challenges with agile ceremonies or practices
* Effectiveness of facilitation
* Impediments related to processes or organizational structure
* Training and coaching needs for teams or individuals
* Sharing of successful strategies or tools

*Additional information on the above syncs can be found here: [Planning Interval (PI) - Scaled Agile Framework](https://framework.scaledagile.com/planning-interval/)*


---

**Further Resources:**
* [Scaled Agile Framework Official Website]([https://www.scaledagileframework.com/](https://www.scaledagileframework.com/ "https://www.scaledagileframework.com/"))
* (Link to internal SAFe guidelines or documents, if applicable)

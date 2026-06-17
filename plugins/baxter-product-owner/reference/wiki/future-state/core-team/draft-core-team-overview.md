# Future State/Core Team/*DRAFT* Core Team Overview

# Core Team and Agile Release Trains
The Product Delivery Excellence (PDE) strategy is a lightweight, outcome-driven way of working that aligns work into a sequence of: 
::: mermaid
 graph LR;
 A[Strategy] --> B[Discovery];
 B --> C[Discovery];
 C --> D[Measurement];
:::

The heart of PDE is an articulation of clear decision rights and dependable execution.  It is a synthesis of product management, lean/agile/DevOps, engineering excellence, and governance into one operating system.

One criticism of the classic Core Team model is that it promotes a failure mode where all decisions route through the core team leader.  Our implementation adjusts to leverage the cross-functional leads of the Agile Release Train.  The following is recommended as a standing list, but the ART team leads list should be considered dynamic based upon the current needs of the project.

- Release Train Engineer (RTE)*
- Product Owner (PO)
- Business Owner (BO)
- System Architect

- Cross-Functional Delivery Team
  - Human Factors
  - Medical Affairs
  - Procurement
  - Quality Assurance
  - R&D Team Lead (often Systems Engineering)
  - Regulatory Affairs
  - Service Engineering

This adjusted Core Team should be small with clear decision rights.  Members of the Core Team must be prepared to be responsible for making decisions and driving those decisions into action.  Where decisions involve other functions, the Circle of Responsibility must be embroadened by a representative of the Core Team rather than simply adding more Core Team members.

# Governance Cadence

* Weekly Core Team Sync - cross-function decisions, risks, dependencies, unblockers
* PO Sync + ART Sync - cross-product/platform decisions, risks, dependencies, unblockers
* PI Ceremonies - PI Planning, System Demos, Inspect & Adapt

**Decision Rights** 
The following outlines the decision rights for the Core Team.
RACI - Responsible, Accountable, Consult, Inform

- Product outcomes, roadmap, capacity split (Features/Enablers): Product Manager (A)

- Technical approach, NFRs, architecture runway: System Architect/Eng (A)

- Flow, predictability, dependency mgmt: RTE (A)

- Quality strategy & release readiness evidence: Quality Lead (A for quality)

- Security/privacy risk acceptance (per policy): Security/Privacy (A), with PM/Arch consulted

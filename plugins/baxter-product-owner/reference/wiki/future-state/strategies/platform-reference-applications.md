# Future State/Strategies/Platform - Reference Applications

# Platform Reference Applications
This section is document why we are planning early effort to create a Platform reference application, especially since the Platform will never be a saleable product.

The Platform Reference Application will effectively be synonymous "Platform Application Shell" that forms the basis for all of the derived products.  Differences between the two will be driven where there are mandatory extension points in the application shell that all products must extend to function.  

## Values
1. Enables iterative development by decoupling the platform and products (see below).
1. The platform project can iterate on new features without colliding on the dependent products' schedules / requirements
1. The platform project can implement a new feature by branching off this reference and demo it independently from the derived products
1. The reference application must also serve as a "lowest common denominator" when there are questions about functions & bugs.  The idea being that as a reference, if it works in platform but not in a product, then we have a culprit.

## Enabling Incremental Development

::: mermaid
timeline
    title Reference Application
    Ideation : UX Describes Workflow
         : SW creates prototype
    Demo Idea : Branch Platform Ref App
         : SW integrates prototype
         : Accept by Platform Stakeholders

    Formal Requirements: Prototype is Evaluated
         : Systems & Test finalize formal specs and test
         : SW Implements to meet specs
         : Accept by Platform Stakeholders

    Integration : Branch Product PoC App
         : Early Validation
         : Accept by Product Stakeholders

    Integrate : Integrate into Product
         : Product V&V
         : Deliver
:::

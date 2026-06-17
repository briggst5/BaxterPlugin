# Checklist for Metrics in project's Software Development Plan

Use this checklist to review individual files or code changes aligned to quality metrics defined in the Software Development Plan

## Cohesion (PLT1-285)
- [ ] Each method has a single, clear purpose (one-sentence description)
- [ ] Classes follow the single responsibility principle
- [ ] Class methods are cohesive (they share purpose and state)
- [ ] No mixing data + behavior: Entities should be thin; behavior in separate classes
  
## Coupling & Dependency Management (PLT1-285)
- [ ] Method and class fan-out should be reasonable - don't depend on too many external entities
- [ ] No circular dependencies
- [ ] Dependencies are injected, not created
- [ ] Favor small, focused interfaces over oversized ones

## Composition and Inheritance (PLT1-285)
- [ ] Inheritance is used for "is-a" relationships
- [ ] Code follows the Liskov Substitution Principle - child substitutes for parent
- [ ] Composition is used for "has-a" relationships and behavior re-use

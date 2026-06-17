# Training and Development/Application Space Software Development/SDS and UVR Guidance

# SDS Review Checklist 

The following document is a list of items that should be checked in an SDS review. In future versions of this checklist, some items may be covered by static analysis tools such as SonarQube. 

## Traceability  

- Any SDS contents that will be verified must be in a work item (not plain text)
- SDS Design Details traced to SADS 
- All SDS Design Details must have a Unit Verification Strategy in the SDS
  - Create Software Test Cases in Given-When-Then (BDD) style documenting how we will test the design details 
  - Unit Verification work items traced to Design Details 
  - Unit Verification work items must be traced to unit tests in the code
    - Kotlin - each relevant `@Test` is annotated with `@DisplayName(<ID> - <Description>)`
    - TypeScript - each relevant test case is annotated with `describe('<ID> - <Description>', () => {`
- Developers are expected to use Test Driven Development (TDD) to unit test all code, but we are not required to document and trace every TDD test case, only the specific unit verification strategies. 

## Unit Verification Writing

- Unit Verification Sections should include the following:
  - Explanation of how we map unit verification work items to acceptance criteria 
    - Use [PLT1-1522 - Software Unit Verification](https://polarion.hrc.corp/polarion/redirect/project/flc.platform.01/workitem?id=PLT1-1522)
  - Explain how to build and run the automated unit tests 
  - Explain what unit test frameworks are used
- Unit verification test cases should:
    - Use Given-When-Then format 
    - Be created in Polarion as a Software Test Case work item
        - Verification Type = Unit
        - Verification Method = Test
        - Automation Level = Automated
        - Description = Test Scenario (Give-When-Then)
        - Other fields may be left blank for Unit verification test cases


# SDS Review/Release Process 

- Use Polarion Peer Review for updated work items or whole documents per User Story 
- At the end of the PI, hold formal Design Review of all documents (GQT-09-03-01) 
- After formal review, use Polarion Signature Review

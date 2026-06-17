# Code Review Checklist

Use this checklist for comprehensive code reviews on individual files or code changes.

## Logic & Correctness

- [ ] Code logic matches the intended functionality
- [ ] All edge cases are handled appropriately
- [ ] Error handling is present and appropriate
- [ ] No obvious logic errors or infinite loops
- [ ] Boundary conditions are properly handled
- [ ] Return values and side effects are as expected
- [ ] Assumptions about input are documented or validated

## Code Quality & Maintainability

- [ ] Code is readable and easy to understand
- [ ] Variable and function names are clear and meaningful
- [ ] Complexity is reasonable for the task (not over-engineered)
- [ ] Code follows project style guidelines and conventions
- [ ] Duplicated code is minimized (DRY principle)
- [ ] Functions are focused and reasonably sized
- [ ] Comments explain "why", not "what"
- [ ] Magic numbers/strings are extracted to named constants

## Security & Performance

- [ ] No obvious security vulnerabilities (SQL injection, XSS, etc.)
- [ ] Sensitive data is not logged or exposed
- [ ] Input validation is performed where necessary
- [ ] No hardcoded secrets, credentials, or API keys
- [ ] Performance is acceptable (no obvious inefficiencies)
- [ ] Database queries are optimized (no N+1 problems)
- [ ] External API calls are reasonable and rate-aware
- [ ] Proper use of caching where appropriate

## Testing & Error Handling

- [ ] Logic is testable (not overly coupled or complex)
- [ ] Error messages are informative
- [ ] Exceptions are handled at appropriate levels
- [ ] Null/undefined checks are present where needed
- [ ] Type safety is utilized (TypeScript, type hints, etc.)
- [ ] No silent failures or swallowed exceptions

## Dependencies & Compatibility

- [ ] New dependencies are justified and necessary
- [ ] Dependency versions are compatible with project constraints
- [ ] Breaking changes are documented and considered
- [ ] Code is compatible with supported platforms/versions
- [ ] No deprecated APIs are used without justification

## Documentation & Communication

- [ ] Code changes are clear and well-described
- [ ] Complex algorithms have explanatory comments
- [ ] Public APIs/functions are documented
- [ ] Parameters and return types are clear
- [ ] Known limitations are documented

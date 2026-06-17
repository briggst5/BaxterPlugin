---
name: architectural-runway-review
description: Reviews enabler vs feature balance and architectural runway gaps for an upcoming PI. Use for architecture sync, PI planning, and platform governance.
---

# Architectural Runway Review

Glossary: Architectural Runway — `reference/wiki/agile-practices/agile-glossary.md`
SAFe Category Enabler: `reference/wiki/agile-practices/safe-agile/safe-category.md`

## Steps

1. Query Features/Stories for PI scope (`wit_query_by_wiql`)
2. Classify each item:
   - **Business Feature** — customer value
   - **Enabler Feature/Story** — runway, infra, compliance, exploration
   - **Mixed** — flag for split
3. Compute ratio: enabler SP / total SP (guideline: ART-specific; flag if enabler <10% when heavy tech debt reported, or >40% without business outcomes)
4. List **runway gaps** — upcoming business features lacking enabler predecessors
5. Cross-check Platform Decision Matrix — platform work should align to shared runway
6. Recommend enabler stories for next IP iteration or current PI

## Output

```
## Architectural Runway Review — [PI/ART] — [date]

### Composition
| Category | Items | Story Points | % of PI |

### Runway gaps (business features at risk)
| Feature | Missing enabler | Recommendation |

### Enabler backlog candidates
| Title | Rationale | Suggested PI |

### Platform alignment notes
- ...
```

Engage System Architect role in review before committing enablers.

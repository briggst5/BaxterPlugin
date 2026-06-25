# Embedded UI → ConfigTool Web Mapping

The Nexus DLS **Embedded UI** section targets device/HMI interfaces. ConfigTool is a web admin portal built on Bootstrap 5 Razor Pages. This document records approved adaptations.

## Direct mappings (no change needed)

| Embedded pattern | Web implementation |
|------------------|-------------------|
| Primary blue `#2265c9` | Bootstrap `--bs-primary`, customer portal accent |
| Neutral dark `#212121` | Admin portal accent, `btn-secondary` |
| Text input with label + helper | `form-label`, `form-control`, `form-text` |
| Checkbox / radio controls | Bootstrap `form-check` |
| Toast / banner feedback | Bootstrap `alert` |
| Card-based grouping | Bootstrap `card` |
| Top navigation | Bootstrap `navbar` |

## Adapted mappings

| Embedded pattern | Adaptation for web |
|------------------|-------------------|
| Fixed pixel button heights (lg/md/sm/xs) | Use Bootstrap `btn` / `btn-sm`; default md; responsive rem sizing |
| Touch-target sizes (32–40px icons) | Keep default form control height; adequate click targets via padding |
| Side / bottom navigation bars | Use top `navbar` in `_Layout.cshtml`; no bottom nav on desktop web |
| Alarm lock-screen modals | Out of scope; use standard `alert-danger` for errors |
| Focus mode overlay | Out of scope for prototype |
| Segmented control / icon switch | Use `btn-group` or native selects where needed (not yet in ConfigTool) |
| Inverted toast palette | Standard Bootstrap alert colors on light page background |

## Out of scope

- GSS Remote XS-only button sizes
- HMI alarm lock-screen flows
- Device-specific unit suffix inputs (infusion pump numerics)
- Figma mode variants (light/dark file switching)

## Portal theming

ConfigTool retains **customer** (primary blue) and **admin** (neutral dark) portal differentiation via body classes:

- `portal-body-customer` — primary accent borders and hero color
- `portal-body-admin` — neutral accent borders and hero color

Both portals share Nexus foundation tokens (typography, spacing, form styling).

## Figma source (read-only)

Design reference: [Nexus Web](https://www.figma.com/design/L5MvUzq1j2M8xZj8Svkdqt/Nexus-Web) — component pages for Button, Navigation, Page Header, Controls, Table, and Notification & Feedback. Web implementation uses Bootstrap + `nexus-tokens.css` (not embedded pill buttons or device-only xs sizes).

## Authority

When in doubt, prefer `docs/design-system/tokens.md` and component pages under `docs/design-system/pages/`.

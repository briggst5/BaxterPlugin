# Nexus Design System — Component Matrix

Mapped from zeroheight Embedded UI component pages. Full specs in `pages/<slug>.md`.

## Base components

| Component | DLS page | Bootstrap / ConfigTool mapping |
|-----------|----------|--------------------------------|
| Standard Button | `standard-button` | `btn btn-primary` (regular), `btn btn-secondary` (neutral/admin), `btn btn-outline-primary`, `btn btn-outline-secondary`, `btn btn-danger` (destructive) |
| Text Input | `text-input` | `form-label` + `form-control` + optional `form-text`; required mark via `*` in label |
| Checkbox | `checkbox` | `form-check` + `form-check-input` + `form-check-label` |
| Radio | `radio` | `form-check` with `type="radio"` |
| Selection Input | `selection-input` | `form-select` |
| Search Bar | `search-bar` | `form-control` in `input-group` |
| Tag | `tag` | `badge rounded-pill nexus-tag` |
| Toast | `toast` | `alert` with severity variants |
| Modal Dialog | `modal-dialog` | Bootstrap modal (future); `alert` for inline feedback today |
| Tab Navigation | `tab-navigation` | `nav nav-tabs` |
| Side Navigation Bar | `side-navigation-bar` | `navbar` + `nav-link` in `_Layout.cshtml` |
| List item | `list-item` | `list-group-item` or table rows |
| Progress Bar | `progress-bar` | Bootstrap `progress` |

## Style variants (buttons)

| DLS variant | Web class |
|-------------|-----------|
| Regular primary | `btn btn-primary` |
| Regular secondary | `btn btn-outline-primary` |
| Neutral primary | `btn btn-secondary` (admin CTAs) |
| Neutral secondary | `btn btn-outline-secondary` |
| Destructive | `btn btn-danger`, `btn btn-outline-danger` |

## Sizes

Default web size: **md**. Use `btn-sm` for compact table actions. Avoid `btn-lg` unless hero CTAs.

## Alerts / feedback

| Intent | Class |
|--------|-------|
| Error | `alert alert-danger` |
| Success | `alert alert-success` |
| Warning / prototype | `alert alert-warning` |
| Info | `alert alert-info` |

## Cards / panels

| Pattern | Class |
|---------|-------|
| Portal card | `card portal-card` + `portal-card-customer` or `portal-card-admin` |
| Schema group | `card mb-4 nexus-card` with `card-header` / `card-body` |
| Login shell | `login-card card` inside `login-shell` |

# Gap Analysis — ConfigTool vs Nexus DLS

## Completion status

| Area | Status |
|------|--------|
| **ConfigTool.Platform** — token layer, layouts, 46-page markup batches | **Complete** |
| **ConfigTool.Platform** — `_SchemaFormField` (`nexus-form-field`, required `*`, `aria-required`) | **Complete** |
| **ConfigTool.Connex360.App** — `nexus-tokens.css`, layout, schema partials | **Complete** |
| Playwright Nexus smoke tests (`e2e/tests/nexus-design.spec.ts`) | Automated for Platform |

Stale paths from the original checklist:

- `Edit/Ews.cshtml` → [`SchemaConfiguration.cshtml`](../../src/ConfigTool.Platform/Pages/Edit/SchemaConfiguration.cshtml) (`/edit/{configurationTypeCode}`, e.g. `/edit/ews`)
- `Edit/Network.cshtml` (Connex 360) → redirect to [`Product.cshtml`](../../src/ConfigTool.Connex360.App/Pages/Edit/Product.cshtml); no page markup

---

## Direct replacements (CSS / token layer) — done

| Was | Now |
|-----|-----|
| `--customer-accent: #0d6efd` | `--nexus-color-primary` via alias in `nexus-tokens.css` |
| `--admin-accent: #212529` | `--nexus-color-neutral` via alias |
| `.btn-primary` `#1b6ec2` in `_Layout.cshtml.css` | Removed; tokens own Bootstrap bridge |
| Hardcoded `#f8f9fa` body bg | `--nexus-color-background: #F6F6F7` |
| Default system font | DM Sans (`@import` in `nexus-tokens.css`) |
| Focus ring `#258cfb` | `--nexus-color-primary-focus: #2359FB` |

Authority: [`src/ConfigTool.Platform/wwwroot/css/nexus-tokens.css`](../../src/ConfigTool.Platform/wwwroot/css/nexus-tokens.css)

---

## Partials — done

| File | Changes |
|------|---------|
| `_Layout.cshtml` | Bootstrap → `nexus-tokens.css` → `site.css`; `portal-navbar` / `portal-footer` |
| `_LoginLayout.cshtml` | `login-page`, `login-card nexus-card` |
| `_SchemaFormLayout.cshtml` | Delegates to `_SchemaFormBlock` (groups use `nexus-card`) |
| `_SchemaFormField.cshtml` | `nexus-form-field`, `nexus-required-mark`, `IsRequired` from `FieldVisibility` |

Connex 360 copies: same partial patterns under `ConfigTool.Connex360.App/Pages/Shared/`, with `nexus-tokens.css` linked from Platform (same pattern as Ews.App).

---

## Platform pages (46 files) — done

All batches below were applied: no `btn-dark` / `btn-outline-dark`, `text-nexus-muted` on secondary copy, admin CTAs use `btn-secondary`, configuration forms use `nexus-form-narrow` where narrow width was intended.

### Batch 1 — Home + auth (9)

`Index.cshtml`, `Customer/Login`, `Customer/FirstPassword`, `Customer/Logout`, `Admin/Login`, `Admin/FirstPassword`, `Admin/Logout`, `Error.cshtml`, `Privacy.cshtml`

### Batch 2 — Customer portal (10)

`Customer/Index`, `Users`, `Users/Edit`, `Account`, `Teams/Index`, `SelectTeam`, configurations/shared pages

### Batch 3 — Admin dashboard + users (5)

`Admin/Index`, `Admin/Users/*`

### Batch 4 — Admin customers (10)

`Admin/Customers/**`

### Batch 5 — Admin configuration (13)

`Admin/Configuration/**`

### Batch 6 — Schema editors (Platform)

`Edit/SchemaConfiguration.cshtml`, `Edit/ProgrammedIntervals.cshtml` — schema groups via `_SchemaFormBlock`; page-level alerts unchanged

---

## Connex 360 app — done

| Item | Notes |
|------|-------|
| `nexus-tokens.css` | Linked in `ConfigTool.Connex360.App.csproj` from Platform `wwwroot` |
| `_Layout.cshtml` | `portal-body-customer`, `portal-navbar`, stylesheet order |
| `_SchemaFormBlock` / `_SchemaFormField` | `nexus-card`, `nexus-form-field`, required indicator |
| `Index.cshtml`, `Edit/Product.cshtml` | `text-nexus-muted` |
| `Edit/Network.cshtml` | Redirect only — no markup |

---

## Not in scope

- Bootstrap modal dialogs (no modals in current UI)
- Segmented controls, icon buttons, alarm patterns
- Dark mode / inverted toast palette
- Dynamic `RequiredWhen` indicators in schema forms (static/render-time only via `FieldVisibility.IsRequired`)
- Pixel-diff against Figma

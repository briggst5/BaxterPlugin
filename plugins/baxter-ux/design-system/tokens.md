# Nexus Design System — Tokens

Extracted from [Nexus Design System (zeroheight)](https://zeroheight.com/67cfae98e/v/latest/p/43ce34-embedded-ui). See `pages/` for per-component source snapshots.

## Color

| Token | Value | Usage |
|-------|-------|-------|
| `--nexus-color-primary` | `#2265c9` | Primary actions, links, customer accent |
| `--nexus-color-primary-hover` | `#154898` | Primary hover / pressed (rgb 21,72,152) |
| `--nexus-color-primary-focus` | `#2359FB` | Focus ring |
| `--nexus-color-primary-soft` | `#F4F6FF` | Soft primary backgrounds |
| `--nexus-color-neutral` | `#212121` | Admin accent, primary text |
| `--nexus-color-neutral-secondary` | `#606774` | Secondary text |
| `--nexus-color-neutral-muted` | `#605f60` | Muted labels |
| `--nexus-color-neutral-disabled` | `#A9A9A9` | Disabled controls |
| `--nexus-color-surface` | `#FFFFFF` | Cards, navbar, inputs |
| `--nexus-color-background` | `#F6F6F7` | Page background |
| `--nexus-color-border` | `#EDEEF0` | Dividers, borders |
| `--nexus-color-border-subtle` | `#f6f6f6` | Input backgrounds |
| `--nexus-color-inverse` | `#000000` | High-contrast text |

## Typography

| Token | Value |
|-------|-------|
| `--nexus-font-family` | `"DM Sans", system-ui, -apple-system, sans-serif` |
| `--nexus-font-size-base` | `1rem` (16px desktop, 14px mobile) |
| `--nexus-font-weight-regular` | `400` |
| `--nexus-font-weight-semibold` | `600` |
| `--nexus-font-weight-bold` | `700` |

Component label styles referenced in DLS: `component-label/button/md`, `header/h4`, `body/body-md-regular`, `component-label/chip-tag/md`.

## Spacing

| Token | Web value |
|-------|-----------|
| `--nexus-space-xs` | `0.25rem` (4px) |
| `--nexus-space-sm` | `0.5rem` (8px) |
| `--nexus-space-md` | `0.75rem` (12px) |
| `--nexus-space-lg` | `1rem` (16px) |
| `--nexus-space-xl` | `1.25rem` (20px) |
| `--nexus-space-2xl` | `1.5rem` (24px) |
| `--nexus-space-3xl` | `2rem` (32px) |
| `--nexus-space-4xl` | `2.5rem` (40px) |

## Radius & elevation

| Token | Value |
|-------|-------|
| `--nexus-radius-sm` | `4px` |
| `--nexus-radius-md` | `6px` |
| `--nexus-radius-lg` | `8px` |
| `--nexus-shadow-card` | `0 0.25rem 1rem rgba(0, 0, 0, 0.06)` |
| `--nexus-shadow-navbar` | `0 1px 2px rgba(0, 0, 0, 0.04)` |

## Portal aliases

| Portal | Accent token |
|--------|--------------|
| Customer | `--nexus-color-primary` |
| Admin | `--nexus-color-neutral` |
| Customer soft bg | `--nexus-color-primary-soft` |
| Admin soft bg | `--nexus-color-border-subtle` |

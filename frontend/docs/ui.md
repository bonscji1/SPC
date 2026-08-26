# UI and typography

Visual source of truth for the Blazor app. **Sizes live as CSS variables** in `src/SPC.Web/wwwroot/css/app.css` (`:root`). Do not invent new font sizes in `.razor` files or extra stylesheets.

When adding a page or changing layout, also follow **UI preferences** in [architecture.md](./architecture.md).

## Font

| Token | Value |
|-------|--------|
| `--font-sans` | `"Segoe UI", system-ui, -apple-system, sans-serif` |

One family for everything (body, headings, buttons, inputs, the info **i**). No second typeface.

| Token | Weight | Use |
|-------|--------|-----|
| `--font-weight-regular` | 400 | Body copy, hints, tooltips body |
| `--font-weight-medium` | 500 | Tooltip text |
| `--font-weight-semibold` | 600 | Labels, nav, buttons, summary values, list names |
| `--font-weight-bold` | 700 | Brand, active profile, info **i** |

Body is `--text-body` (1rem ≈ 16px) at `--line-body` (1.45). Color `--color-text`; secondary copy `--color-muted`.

## Type scale

Use the **role**, not a one-off rem. If something does not fit, extend this table and add a token — do not skip the table.

| Role | Token | Size | Where |
|------|--------|------|--------|
| Info **i** letter | `--text-info-mark` | 0.5rem | Inside `InfoTip` only. Must stay smaller than the label beside it. |
| Caption | `--text-caption` | 0.8rem | Summary meta labels (`dt`), tooltip body |
| Hint | `--text-hint` | 0.85rem | Field hints, compact menu actions |
| Small | `--text-small` | 0.9rem | Field labels, legends, section hints, footer, list meta |
| Callout | `--text-callout` | 0.95rem | Summary `h3`, banners, leftover/detail lines |
| Body | `--text-body` | 1rem | Default paragraph, inputs, buttons |
| Large input | `--text-input-lg` | 1.05rem | Primary name fields (`.input-lg`) |
| Section title | `--text-section` | 1.1rem | Card `h2`, `.section-title`, nav brand, modal title |
| Tagline | `--text-tagline` | 1.15rem | Home hero subtitle only |
| Page title | `--text-page` | clamp(1.75–2.25rem) | `.page-header h1` |
| Hero title | `--text-hero` | clamp(2–2.75rem) | Home hero `h1` only |

## Text roles (classes)

| Kind | Markup | Notes |
|------|--------|--------|
| Page title | `.page-header h1` | One per page. Subtitle is `p.muted` at `--text-small`. |
| Section title | `h2.section-title` | Groups of fields. First section has no extra top margin (`.editor-section:first-child`). |
| Section hint | `p.section-hint.muted` | One line under the section title: what the group is for. |
| Summary card title | `.summary-card h2` | Same size as section title (e.g. Estimate, Summary). |
| Summary subsection | `.summary-section h3` | Nested group inside a card (e.g. Meal split under Estimate). |
| Field label | `.field label` / `legend` | Muted, semibold, `--text-small`. |
| Field hint | `p.field-hint.muted` | Under a control or value. Visible caveats belong here, not only in a tooltip. |
| Summary label + value | `.summary-meta dt` / `dd` | `dt` caption + uppercase; `dd` body, semibold. |
| Primary button | `.btn.btn-primary` | Body size, semibold. |
| Secondary button | `.btn.btn-secondary` | Same type, outline. |
| Empty / loading | `p.muted` | Body size, muted color. |

Do not style headings with inline `style` or a new `font-size` on a one-off class.

## Info **i** (`InfoTip`)

Component: `src/SPC.Web/Components/InfoTip.razor`.

| Token | Size | Rule |
|-------|------|------|
| `--size-info-tip` | 0.7rem | Circle diameter. **Smaller than adjacent text** (caption is 0.8rem, labels 0.9rem). |
| `--text-info-mark` | 0.5rem | Italic **i** inside the circle. |

Hover/focus shows `--text-caption` tooltip (`--line-tooltip` 1.35), dark background, normal sentence case (do not inherit uppercase from `dt`). Cursor stays the default arrow (not help/pointer).

**When to use**

- Derived or computed values (BMR, daily kcal, meal budgets, similar estimates).
- Tooltip content: what the number is, then `Formula: …` with **live** inputs in `[brackets]`.
- Do **not** put the only copy of an important caveat in the tooltip (error bars, “not medical advice”). Repeat that as a `.field-hint` under the value.

## Layout primitives

| Class | Use |
|-------|-----|
| `.card` | Surface for a form or summary. |
| `.editor-card` + `.editor-section` | Grouped fields on an edit page. |
| `.summary-card` + `.summary-section` | Live preview; subsections divided by a top border. |
| `.recipe-layout` | Two columns from 900px: editor \| summary. Use for create/edit pages. |
| `.page-stack` | Full-width stacked cards (library). Last card `.page-stack-fill` grows with the page. |
| `.list-pager` | Page size, “Page X of Y”, previous/next under a list (`ListPager`). Sizes 10 / 25 / 50. Status uses `--text-small`. |
| `.list-filters` | Name (and type on Home) above a list. |
| `.name-combobox` | Ingredient/spice name field with library picker (reuses `.instruction-picker`). |
| `.portion-inputs` | Two-column field grid (one column under 720px). |
| `.field` | Label + control + optional hint. |

## Changing the look

1. Edit the token in `:root`.
2. Update the table in this file if a role or size changed.
3. Do not patch a single page unless that page is the only consumer of a new role — then add a token first.

# Plan: Smart Pig's Cookbook — Master roadmap

**Date:** 2026-08-24  
**Updated:** 2026-08-27  
**Scope:** both (frontend-first; backend when persistence/API is needed)  
**Status:** in progress — steps 1–8, 10, and 11 done; next product step is nutrition API (step 9).

## Vision

A recipe and portion calculator for people who cook and care about calories without the tedium of traditional calorie tracking.

**Core value:** plug in a recipe and personal info → get sensible portion sizes and how many portions the dish yields.

**Stretch goals (later, not in early steps):**

- Saved recipes with easy modification
- Ingredient nutrition lookup without manual entry (external API or our own)
- AI: photo of recipe or ingredients → structured data
- Per-person portion sizing in one household

## Principles

- **Incremental delivery** — each step builds on the previous and is independently testable.
- **Refine as we go** — sub-plans are living documents; update them when design lessons emerge.
- **Frontend-first** — early steps run locally in the browser; persistence and APIs come when needed.
- **Stack** — Blazor WebAssembly on .NET 10 (`SPC.Web` + `SPC.Core`). See `notes-tech-stack-options.md` for the earlier options.

## Roadmap

| Step | Goal | Status | Sub-plan |
|------|------|--------|----------|
| 1 | Runnable local frontend shell for iteration | **done** | [step1-init-fe.md](./step1-init-fe.md) |
| 2 | Create a recipe (name, ingredients, spices, TipTap instructions) | **done** | [step2-create-recipe.md](./step2-create-recipe.md), [step-recipe-instructions.md](./step-recipe-instructions.md) |
| 3 | Compute dish totals; portions ↔ kcal/portion; optional cooked yield | **done** | [step3-compute-portions.md](./step3-compute-portions.md) |
| 4 | Derive lunch kcal from selectable person profiles (meal split per person) | **done** | [step4-human-tracking.md](./step4-human-tracking.md) |
| 5 | Persist recipes and ingredient nutrition data | **done** — localStorage; backend later | [step5-save-recipes-and-ingredients.md](./step5-save-recipes-and-ingredients.md) |
| 6 | Deployments — containerize frontend; repo-level compose | **done** | [step6-deployments.md](./step6-deployments.md) |
| 7 | Cookbook — browse and open saved recipes | **done** — Home list; no extra route | [step7-cookbook.md](./step7-cookbook.md) |
| 8 | Recipe scaling, variations, and what-if | **done** | [step8-recipe-adjustment.md](./step8-recipe-adjustment.md) |
| 9 | Automatic ingredient nutrition lookup (API) | draft | [step9-ingredient-nutrition-api.md](./step9-ingredient-nutrition-api.md) |
| 10 | Backend + database — persist per-user data; Bearer on the API | **done** | [step10-backend.md](./step10-backend.md) |
| 11 | Login user — Blazor shell against the real API | **done** | [step11-login-user.md](./step11-login-user.md) |

### Future / aspirational (not scheduled)

| Idea | Notes |
|------|-------|
| AI recipe/ingredient capture from photo | High UX value; needs vision + parsing pipeline |
| Multi-person portions in one session | Different targets per diner from one dish |
| Mobile / PWA | After web core is solid |

## Dependency graph

```
step1-init-fe
    ├── step6-deployments          # compose/images; independent of product steps after 1
    └── step2-create-recipe
            └── step3-compute-portions
                    └── step4-human-tracking
                            └── step5-save-recipes-and-ingredients
                                    ├── step7-cookbook
                                    ├── step8-recipe-adjustment
                                    │       └── step9-ingredient-nutrition-api
                                    └── step10-backend           # discuss DB first; also uses step 6 compose slots
                                            └── step11-login-user   # Blazor login + Api* repos; no fake local auth
```

Step 9 (nutrition API) is the next **product** step and does not wait on the backend. Steps 10–11 are **infrastructure** drafts: API + database first (seeded users, Bearer on the wire), then the Blazor login shell. Cookbook browsing lives on Home (step 7). Deployments (step 6) only needed a publishable frontend (step 1); compose already has room for `backend` + `db`.

## How to use these plans

1. Read this file for context and order.
2. Open the sub-plan for the step you are implementing.
3. Complete acceptance criteria before moving on.
4. Update the sub-plan (and this file if scope shifts) when we learn something new.

## Open questions (cross-cutting)

- When do we introduce a backend vs. localStorage-only persistence? **Step 5 was the localStorage prototype.** API + DB is [step 10](./step10-backend.md). Blazor login + HTTP repositories is [step 11](./step11-login-user.md) (**done**).
- Which nutrition API (if any) is viable for step 9 — licensing, coverage, Czech/EU foods?
- ~~Exact formulas for TDEE / meal calorie split~~ — decided: Mifflin–St Jeor × US activity factors; see `frontend/docs/energy-targets.md`.

# Plan: Step 8 — Ingredient nutrition API

**Date:** 2026-08-24  
**Scope:** both  
**Status:** draft  
**Depends on:** [step5-save-recipes-and-ingredients.md](./step5-save-recipes-and-ingredients.md)  
**Parent:** [thePlan.md](./thePlan.md)

## Goal

Reduce manual entry of **calories per 100 g** by looking up ingredients from an external source or a small backend we control.

## Out of scope

- Full macro tracking (protein, fat, carbs) unless cheap add-on
- AI photo recognition (separate aspirational track)

## Options (evaluate before build)

| Approach | Notes |
|----------|-------|
| Third-party API | e.g. Open Food Facts, USDA FoodData Central, commercial nutrition APIs — check license, rate limits, EU/CZ food names |
| Own backend + cached DB | Scrape/sync once, serve our API; PostgreSQL cache |
| Hybrid | API on miss, cache hits locally |

## Deliverables

- [ ] Research note in `docs/` comparing 2–3 API options
- [ ] Lookup by ingredient name (fuzzy match)
- [ ] Autofill `caloriesPer100g` in step 2 ingredient row
- [ ] Cache results in ingredient library (step 5)
- [ ] Graceful fallback: manual entry if lookup fails

## Acceptance criteria

- Common ingredients (e.g. carrot, chicken breast, rice) resolve without manual kcal entry
- API keys and rate limits handled securely (env vars, not committed)
- User can override looked-up value

## Open questions

- Czech/local food naming — multilingual search?
- Backend required vs. direct browser calls (CORS, API key exposure)?

# Plan: Step 2 — Create a recipe

**Date:** 2026-08-24  
**Updated:** 2026-08-26  
**Scope:** frontend  
**Status:** implemented — recipe editor with ingredients, spices, TipTap instructions; live portion summary  
**Depends on:** [step1-init-fe.md](./step1-init-fe.md) — complete  
**Parent:** [thePlan.md](./thePlan.md)

## Goal

Let the user define a recipe with a name and a list of ingredients, each with amount in grams and caloric value per 100 g. Data lives in memory (or simple local state) — persistence is step 5.

## Out of scope

- Portion calculations (step 3)
- Nutrition API lookup (step 9)
- Saving to database / cookbook (steps 5 and 7)

## Data model (initial)

```
Recipe
  name: string
  ingredients: Ingredient[]

Ingredient
  name: string
  grams: number          // amount used in this recipe
  caloriesPer100g: number
```

Example ingredient row: **carrot — 200 g — 41 kcal/100 g**

## Deliverables

- [x] UI to enter recipe name
- [x] UI to add, edit, and remove ingredient rows (name, grams, kcal/100g)
- [x] Basic validation (non-empty name, positive grams, non-negative calories)
- [x] Display a summary of entered ingredients (read-only list or live preview)
- [x] Recipe object available in app state for the next step (no API yet)
- [x] Ordered instruction steps with ingredient/spice chips (TipTap editor; ids, not `#name` text)

## Also delivered (beyond original step 2 scope)

- Spices section (optional grams/kcal rows)
- Instruction steps with `#` mention picker and rich text (see [step-recipe-instructions.md](./step-recipe-instructions.md))
- Recipes persist in localStorage when saved (step 5 — partial; see [step5-save-recipes-and-ingredients.md](./step5-save-recipes-and-ingredients.md))

## UX notes

- Optimise for fast data entry — tab between fields, add row with one action.
- Show units clearly: grams and kcal per 100 g.
- Optional: running subtotal of ingredient grams (helps sanity-check before step 3).

## Acceptance criteria

- User can create a recipe with at least one ingredient and see it reflected in state
- Invalid input is caught with clear messages
- Refreshing the page may clear unsaved data; saved recipes and profiles survive reload (step 5)

## Future hooks

- Ingredient name autocomplete (step 9)
- Import from photo (aspirational)
- Reuse saved ingredient nutrition from step 5

## Open questions

- Allow fractional grams (e.g. 12.5)? Recommend yes.
- Support non-gram units later (cups, pieces) — defer; document as future enhancement.

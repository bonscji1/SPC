# Plan: Step 7 — Recipe adjustment (scaling & what-if)

**Date:** 2026-08-24  
**Scope:** frontend  
**Status:** draft  
**Depends on:** [step5-save-recipes-and-ingredients.md](./step5-save-recipes-and-ingredients.md) (step 6 helpful but not strictly required)  
**Parent:** [thePlan.md](./thePlan.md)

## Goal

Adjust recipes without rebuilding from scratch:

1. **Scale portions** — e.g. “I need 8 portions instead of 6” → rescale ingredient grams
2. **What-if ingredient** — add/remove/change an ingredient and see impact on totals and portion math

## Scenarios

### Scale by portion count

Given base recipe and desired portion count `N` (vs. current implicit or explicit base `B`):

- `scaleFactor = N / B`
- Each ingredient `grams_i' = grams_i * scaleFactor`
- Re-run step 3 calculations (may need new actual dish weight estimate)

### Add / remove / modify ingredient

- Duplicate recipe as working copy
- Apply change → show delta in total weight, calories, portion size
- Option to save as new recipe or overwrite

## Deliverables

- [ ] UI: target portion count → scaled ingredient list
- [ ] UI: add/remove/edit ingredient on a copy → before/after summary
- [ ] Calculations reuse step 3 engine on modified recipe
- [ ] Clear labeling: “scaled from original” vs. saved new recipe

## Acceptance criteria

- Scaling 6 → 8 portions multiplies all ingredient grams by 8/6
- Adding a high-calorie ingredient increases kcal/portion or reduces portion count for fixed meal calories
- User can save adjusted recipe (ties into step 5)

## Open questions

- Scale actual dish weight proportionally or ask user to re-weigh?
- Base portion count: from last calculation or user-specified “recipe serves X”?

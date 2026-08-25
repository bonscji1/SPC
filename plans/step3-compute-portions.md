# Plan: Step 3 — Compute portions

**Date:** 2026-08-24  
**Updated:** 2026-08-25  
**Scope:** frontend  
**Status:** implemented — model locked (ingredient-sum + yield)  
**Depends on:** [step2-create-recipe.md](./step2-create-recipe.md)  
**Parent:** [thePlan.md](./thePlan.md)

Formulas and caveats: [frontend/docs/portion-math.md](../frontend/docs/portion-math.md).

## Goal

From a recipe (step 2), show dish totals and let the user set **kcal per portion**, **number of portions**, or **g per portion** (the other two follow). Optional cooked weight is the actual dish weight.

## Background

Cooking mostly moves water. Batch calories stay the sum of ingredients; cooked weight only changes density and grams per portion. Do **not** scale total calories with cooked weight (that would treat yield as a bigger batch).

Spices are already a first-class list (step 2). Include spice grams in weight; include spice calories when both grams and kcal/100 g are set.

## Calculations (v1)

### From ingredients (theoretical — does not change with cooked weight)

For each ingredient `i`:

- `calories_i = (grams_i / 100) * caloriesPer100g_i`

Same for a spice when both `grams` and `caloriesPer100g` are set; otherwise spice calories are 0.

- `theoreticalWeightG` = sum of ingredient grams + spice grams
- `theoreticalCalories` = sum of those calorie lines

(If theoretical weight is 0, show an error and skip portion math.)

### Cooked yield (optional)

User provides `actualDishWeightG` in the summary (optional). Empty → use `theoreticalWeightG`.

- `dishWeightG` = `actualDishWeightG` or `theoreticalWeightG`
- `kcalPer100gCooked` = `theoreticalCalories / dishWeightG * 100`

No second “adjusted calories” total.

### Desired portions (three linked inputs)

Identity:

- `theoreticalCalories = portions * kcalPerPortion`
- `gramsPerPortion = dishWeightG / portions`

All three fields are editable. **Last edit is independent**; the other two are derived.

### Servings (display)

- Full portions = `floor(portions)`, each at the chosen grams and kcal
- Leftover = remaining grams and remaining kcal (not a second copy of the fractional count)

## UI (summary column)

- Recipe / ingredients / spices (and lunch target from a profile)
- Theoretical values: weight, calories, kcal / 100 g
- Actual values: cooked weight (optional)
- Desired portions: kcal per portion, number of portions, g per portion (last-edit-wins)
- Servings: N full portions each X g and Y kcal; leftover as grams and kcal

Portion knobs (kcal / count / grams per portion) are session-only. Actual cooked weight is stored on `RecipeDto` and persisted with the recipe. Step 4 writes kcal per portion from the profile into the session knobs.

Live update as the recipe or inputs change. If the recipe is invalid, show validation errors and skip portion math.

## Deliverables

- [x] Input: actual cooked dish weight (optional, with helper text)
- [x] Inputs: kcal per portion, number of portions, and g per portion, last-edit-wins
- [x] Display: theoretical weight, theoretical calories (no adjusted-calorie total)
- [x] Display: grams per portion, kcal per 100 g of the cooked dish
- [x] Display: portion count including fractions
- [x] Pure calculation module in `SPC.Core` (`IPortionCalculator`)
- [x] Edge cases: empty recipe, zero weight, zero/empty independent field

## Acceptance criteria

- Given a known recipe and inputs, results match hand-calculated fixtures (2–3 cases in tests)
- Changing any one of kcal per portion, number of portions, or g per portion updates the other two without a feedback loop
- Changing cooked weight updates grams per portion only, not batch calories
- UI updates when the recipe or inputs change
- Model documented in `frontend/docs/portion-math.md`

## Out of scope

- Profile / meal type (step 4)
- Saving cooked weight or portion target with the recipe
- Fat drip, oil absorption, alcohol, micronutrient retention — see “Later improvements” in `portion-math.md`

## Open questions

- Default independent field on a new recipe: kcal per portion (fits step 4) vs portions — lean **kcal per portion** so step 4 can set it.

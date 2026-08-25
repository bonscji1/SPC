# Plan: Step 4 — Human tracking (daily calories → meal portions)

**Date:** 2026-08-24  
**Updated:** 2026-08-25  
**Scope:** frontend  
**Status:** implemented — Mifflin–St Jeor + EFSA PALs; profiles independent of recipes  
**Depends on:** [step3-compute-portions.md](./step3-compute-portions.md)  
**Parent:** [thePlan.md](./thePlan.md)

Formulas: [frontend/docs/energy-targets.md](../frontend/docs/energy-targets.md).

## Goal

Estimate a **lunch kcal budget** from a person profile and offer it as the step 3 kcal-per-portion target. Profiles and recipes are separate: either works without the other.

## Out of scope

- Medical-grade accuracy
- Recipe meal type (breakfast / dinner / snack) — v1 lunch only; type comes later
- Linking a profile id onto a recipe
- Deficit / surplus, children, pregnancy, body-fat %
- Multiple people eating the same dish in one sitting (aspirational)

## Flow

1. Cookbook (home) → top-right **profile menu** → **Add profile** → profile page
2. Fill name, sex, age, height, weight, activity, **meal % split** (editable, default 20/30/35/15)
3. Save. Profile is selectable from the same menu (switch without leaving the current page)
4. When cooking, if a profile is selected, show **lunch target (kcal)**. User can apply it to the portion calculator or keep typing kcal/portions by hand

## Calculations (v1)

See `energy-targets.md`. Short form:

- BMR = Mifflin–St Jeor (kg, cm, years, sex)
- TDEE = BMR × PAL (EFSA-style: 1.4 … 2.0)
- `lunchKcal = TDEE × (profile lunch % / 100)`
- That value is a **suggestion** for step 3 `kcalPerPortion`, not stored on the recipe

## Deliverables

- [x] Multiple named profiles, persisted locally, not on recipes
- [x] Top-right profile menu: switch, add, open to edit
- [x] Profile page: sex, age, height, weight, activity, meal distribution
- [x] Lunch recommendation on the recipe editor when a profile is active
- [x] Pure energy calculator in `SPC.Core` with unit tests
- [x] Recipes still work with no profile; profiles work with no recipes

## Acceptance criteria

- Switching profile updates the shown lunch target without changing saved recipes
- Custom meal percents (must sum to 100) change the lunch target
- Fixture profiles match hand-calculated Mifflin + PAL examples
- Manual kcal/portion and portion count still work (last-edit-wins)

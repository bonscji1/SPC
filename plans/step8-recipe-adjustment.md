# Plan: Step 8 — Recipe scaling, variations, and what-if

**Date:** 2026-08-24  
**Updated:** 2026-08-27  
**Scope:** frontend  
**Status:** **done** — variants live on the recipe (Default tab + siblings, rename/delete from the tab pen); Home is one row per dish; Scale this for… includes scale-for-profile and a one-field save  
**Depends on:** [step5-save-recipes-and-ingredients.md](./step5-save-recipes-and-ingredients.md)  
**Parent:** [thePlan.md](./thePlan.md)

## Goal

Adjust a dish without rebuilding it:

1. **Variations** — same recipe family, different try-outs (amounts, extra onion, different meat).
2. **Scale this for…** — pick a target batch (portion size in **g or kcal**, plus **how many portions**), preview a proportionally scaled **raw** ingredient list, then optionally save.

The editor already covers live add/remove/edit and the three *slicing* knobs (kcal / count / g per portion of the **current** batch). Those knobs do **not** change shopping-list grams. This step adds **batch scaling** and a **variant** relationship.

## Can variations be done?

Yes. Each saved row stays a full `RecipeDto` (own id, ingredients, spices, instructions). A shared **family id** groups them as one recipe with variants. Home lists the **recipe** (one family row). Opening it loads the default variant; other variants switch on the editor. Persistence stays behind `IRecipeRepository` (localStorage now, HTTP later). No backend required.

## Out of scope

- Scaling cooked / yield weight (use theoretical / raw grams only)
- Changing the step 3 slicing knobs (they stay session-only, current batch)
- Nutrition API (step 9)
- Ingredient-list search ([future-improvements.md](../frontend/docs/future-improvements.md))

## Variations

On `RecipeDto`:

- **`FamilyId`** — all variants of one dish share it. A brand-new recipe gets `FamilyId = Id`. Existing saved recipes with a missing/empty family id load as `FamilyId = Id` (no storage key bump if JSON defaults work).
- **`VariantLabel`** — short optional name (`extra onion`, `turkey`, `half batch`). Empty is the base row; UI shows it as **Default**. Do not persist the string `"Default"` for that row.

Keep ingredient, spice, and instruction **line ids** when cloning so instruction chips still resolve. Only `Recipe.Id` is new. Copy instructions and meal type; **do not** copy `ActualDishWeightG` onto a scaled result (raw list, not a weighed cook).

**Home:** one row per family (the default/primary). Paging is by **family**, not by every variant as a top-level card. Name filter still matches recipe name or variant label so the dish can be found. Delete on Home removes the **entire family**. A quiet “N variants” count on the row is fine; nested variant rows are not.

**Editor:** Excel-style **tabs** under **Edit recipe** (even when there is only Default). Switching tabs navigates to that row’s id and uses the existing unsaved-changes guard. A pen on each tab opens **Edit variant**: rename (empty / Default = unlabeled base; names unique in the family) or delete. **Save recipe** overwrites the current variant. **Save as variant** (under Scale / Save) copies the current draft into a new tab; the open variant stays as last saved.

## Scale this for…

New control on the recipe editor (e.g. **Scale this for…**). Uses the **current draft** (including unsaved edits). Does not write storage until the user picks a save action.

### Inputs (modal)

- **Number of portions** `N` (> 0)
- **Portion size** — one of:
  - grams per portion, or
  - kcal per portion
- **Scale for profile** — same kcal as summary **Use as portion target** (active profile × recipe meal type). Fills portion size as kcal; does not change `N` except defaulting empty `N` to 1. Disabled when there is no meal kcal.

Not both at once. Uniform scaling of every line means weight-based and kcal-based factors are the same identity if the user typed a size that matches this recipe’s density; we still take **one** size field so the target batch is well defined.

### Math (theoretical / raw only)

`theoreticalWeightG` and `theoreticalCalories` as in [portion-math.md](../frontend/docs/portion-math.md) (ingredients + weighed spices).

- Size in **g:** `scaleFactor = (N × gramsPerPortion) / theoreticalWeightG`
- Size in **kcal:** `scaleFactor = (N × kcalPerPortion) / theoreticalCalories`

Each ingredient gram and each **weighed** spice gram becomes `grams × scaleFactor`. Names and kcal/100 g stay put. Name-only spices stay name-only.

If theoretical weight or calories is 0 (kcal path), do not scale; show the same validation the summary already uses.

Do **not** scale `ActualDishWeightG`. The preview is a shopping list of raw inputs.

### Preview (no save)

First result is **show only**: scaled ingredient (and spice) list, plus implied batch grams/kcal. No repository write, no new id, editor draft unchanged.

One **Name** field on the preview (default: source name + “ (scaled)”). Then one click:

- **Save as recipe** — new `Id`, new `FamilyId` (= that id). Independent dish. Uses Name as the recipe name.
- **Save as variant** — new `Id`, **same** `FamilyId`. Uses Name as the variant tab (not empty / not Default). Source row left as-is.
- **Cancel** — discard the preview.

After a successful save, open that new/variant row in the editor.

## Deliverables

- [x] `FamilyId` + `VariantLabel` on `RecipeDto`; clone / equivalence / tests; load migration for old rows
- [x] Home one row per family; editor tabs for variants (Default when unlabeled)
- [x] **Scale this for…** modal (N + g-or-kcal size + scale for profile)
- [x] Core scale helper (factor × line grams); unit tests (e.g. 6→8 portions at same g/portion multiplies grams by 8/6)
- [x] Preview list with one name field; Save as variant / Save as recipe / Cancel
- [x] Rename / delete variant from a pen on each tab
- [x] **Save as variant** on the editor save panel (copy current draft to a new tab)

## Acceptance criteria

- Scaling to `N` portions of `G` g each multiplies every weighed line by `(N × G) / theoreticalWeightG`
- Preview does not persist; Cancel leaves the original recipe unchanged
- Save as variant keeps one family on Home; save as new appears as its own recipe
- Instruction chips still show amounts after save (line ids preserved)
- Home does not list variant rows; switching variants happens on the recipe
- An unlabeled base row is shown as **Default**
- Scale for profile fills the same meal kcal as Use as portion target
- A pen on each variant tab can rename it (unique in the family) or delete that variant

## Open questions

- (none)

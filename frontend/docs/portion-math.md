# Portion and calorie model

How SPC turns a recipe into dish totals and portion sizes. Implemented in `SPC.Core` (step 3). Roadmap: `../../plans/step3-compute-portions.md`.

## v1 model (locked)

**Ingredient-sum + recipe yield.** Cooking does not create or destroy calories; it mostly moves water. Batch energy stays the theoretical sum. Cooked weight only changes grams on the plate (and kcal per 100 g of the finished dish).

### Dish totals (theoretical)

For each ingredient: `calories = (grams / 100) × caloriesPer100g`.

Spices use the same formula when both grams and kcal/100 g are set; name-only spices add nothing. Spice grams always add to weight.

- `theoreticalWeightG` = ingredient grams + spice grams
- `theoreticalCalories` = sum of those calorie lines

These two numbers do **not** change when the user enters a cooked weight.

### Cooked yield (optional)

`dishWeightG` = actual cooked weight (`RecipeDto.ActualDishWeightG`) if provided, otherwise `theoreticalWeightG`. The cooked weight is saved with the recipe for later planning. Portion count / kcal / grams-per-portion knobs stay session-only.

- `theoreticalKcalPer100g` = `theoreticalCalories / theoreticalWeightG × 100` (does not change with cooked weight)
- `kcalPer100gCooked` = `theoreticalCalories / dishWeightG × 100`
- `gramsPerPortion` = `dishWeightG / portions`

### Desired portions (three linked inputs)

One identity, three knobs. Last edit is independent; the other two are derived.

`theoreticalCalories = portions × kcalPerPortion`  
`gramsPerPortion = dishWeightG / portions`

Filling grams per portion sets `portions = dishWeightG / gramsPerPortion` and kcal from the identity.

Changing cooked weight does not change batch calories. If the independent field is kcal or portion count, grams per portion updates. If the independent field is grams, portion count (and kcal) update.

### Servings display

Do not repeat the fractional count. Show:

- **Full portions:** `floor(portions)` servings, each `gramsPerPortion` and `kcalPerPortion`
- **Leftover:** remaining grams and remaining kcal (`dishWeight − full × gramsPerPortion`, `calories − full × kcalPerPortion`)

When ingredient totals change, keep the last independent field and recompute the other two.

## Pairing grams with kcal/100 g

`caloriesPer100g` must describe the **same state** as the grams you typed. That is usually automatic.

Packaging in the EU is nutrition **as sold** unless the pack also prints an “as prepared” column. Dry pasta, dry rice, raw meat, a tin of tomatoes: the 100 g on the pack is the product in the packet, not the boiled or roasted result. If you weigh that packet (or the raw meat), pack kcal/100 g is the right number.

Example: 80 g dry pasta × 350 kcal/100 g (pack, as sold) = 280 kcal. After boiling it may weigh ~200 g; the batch is still 280 kcal, just wetter. That is what the cooked-weight field is for.

**When the pairing actually breaks** (not the normal path):

- Using a database row labelled “pasta, cooked” (~130 kcal/100 g) while weighing **dry** pasta — batch calories come out ~3× too low.
- Using the pack’s **as prepared** column while weighing **dry** pasta — same undercount.
- Weighing a **cooked leftover** scoop and multiplying by **dry** pack kcal/100 g — ~3× too high. For leftovers, use this recipe’s cooked density (`kcalPer100gCooked`) or weigh the leftover as a fraction of `dishWeightG`.

v1 does not try to detect this. Step 8 (nutrition API) should prefer the database entry that matches how the user weighed the ingredient.

## Later improvements

Not in v1. Logged so we do not pretend the calorie sum is lab-accurate.

| Issue | Effect | When to consider |
|-------|--------|------------------|
| Fat left in the pan, or oil absorbed in frying | Calories leave or enter; water-only yield misses it | Optional “oil used vs discarded”; not needed for boiled/stewed dishes |
| Discarded cooking liquid (pasta water, marinade) | Some starch/salt/kcal leave the dish | User already omits discarded water as an ingredient; call out in help text if it comes up |
| Alcohol | Some ethanol (and its kcal) evaporates | If we add alcohol as a nutrient |
| Dual-column packaging (as sold vs as prepared) | Easy to copy the prepared column | Hint on the kcal field; step 9 can pick the as-sold entry |
| Leftover logging by cooked grams | Need cooked density, not dry pack kcal | Falls out of yield math; add a “I ate X g of the cooked dish” path when we log meals |
| Vitamin/mineral retention (EuroFIR / USDA) | Heat and leaching; **not** relevant to calories/macros | Only if we show micronutrients |
| Persist cooked weight / last portion target with the recipe | Convenience | After step 5, if the same dish is cooked often |
| Rounding and locale fractions (½ vs 0.5; comma decimals) | Display only | Quantity format is locked (`NumberFormat`); locale comma later |

References for the full food-composition method (yield at recipe level, retention at ingredient level): EuroFIR mixed method, FAO/INFOODS recipe guidelines, Cronometer “set cooked recipe weight” (same energy model as v1).

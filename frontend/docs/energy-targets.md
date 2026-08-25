# Energy targets (person → meal kcal)

How SPC estimates a daily calorie need and a meal budget. Implemented in `SPC.Core` (step 4). Roadmap: `../../plans/step4-human-tracking.md`.

Profiles and recipes are **independent**. A profile never stores recipe ids; a recipe never stores a profile id. The recipe editor may *read* the active profile to suggest lunch kcal.

## v1 model (locked)

**TDEE = BMR × PAL** (maintenance). Not medical advice; typical REE error is about ±10%.

### BMR — Mifflin–St Jeor (1990), kcal/day

- Male: `10 × kg + 6.25 × cm − 5 × age + 5`
- Female: `10 × kg + 6.25 × cm − 5 × age − 161`

Adults roughly 18–80. Sex is the equation coefficient, not identity.

### PAL — EFSA-style (not the 1.2 “sedentary” app factor)

| Activity | PAL |
|----------|-----|
| Sedentary | 1.4 |
| Light | 1.5 |
| Moderate | 1.6 (default) |
| Active | 1.8 |
| Very active | 2.0 |
| Custom | user-entered PAL (1.0–2.4) |

TDEE and meal kcal are rounded to the nearest 10 kcal. The profile estimate shows BMR, daily maintenance, and kcal for every meal in the split. Cooking still uses **lunch** as the portion suggestion.

### Meal split (per profile, editable)

Defaults (must sum to 100%):

| Meal | % |
|------|---|
| Breakfast | 20 |
| Lunch | 30 |
| Dinner | 35 |
| Snack | 15 |

v1 cooking UI uses **lunch** only: `lunchKcal = TDEE × lunchPercent / 100`. The profile estimate already shows kcal for breakfast, lunch, dinner, and snack. Recipe meal type comes later.

## Later improvements

| Issue | When |
|-------|------|
| Recipe meal type (breakfast/dinner/snack) | After lunch-only UX is enough |
| Goal: lose / maintain / gain | Slider off TDEE |
| Unspecified sex (midpoint intercept −78) | If requested |
| Body-fat % (Katch–McArdle) | If we collect composition |
| kJ beside kcal | Locale polish |
| Henry/Oxford REE (EFSA tables) | Only if we need EU reference-table parity |

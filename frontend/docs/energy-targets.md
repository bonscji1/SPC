# Energy targets (person → meal kcal)

How SPC estimates a daily calorie need and a meal budget. Implemented in `SPC.Core` (step 4). Roadmap: `../../plans/step4-human-tracking.md`.

Profiles and recipes are **independent**. A profile never stores recipe ids; a recipe never stores a profile id. The recipe editor reads the active profile and the recipe’s meal type to suggest meal kcal.

## v1 model (locked)

**TDEE = BMR × activity factor** (maintenance). Not medical advice; typical REE error is about ±10%.

Uses one coherent stack: **Mifflin–St Jeor** for BMR and **US activity factors** for TDEE (the pairing used by most clinical and fitness calculators). Alternative models (e.g. EFSA Henry + PAL) are listed in [future-improvements.md](./future-improvements.md).

### BMR — Mifflin–St Jeor (1990), kcal/day

- Male: `10 × kg + 6.25 × cm − 5 × age + 5`
- Female: `10 × kg + 6.25 × cm − 5 × age − 161`

Adults roughly 18–80. Sex is the equation coefficient, not identity.

### Activity factors — US (paired with Mifflin)

| Activity | Factor |
|----------|--------|
| Sedentary | 1.2 |
| Light | 1.375 |
| Moderate | 1.55 (default) |
| Active | 1.725 |
| Very active | 1.9 |
| Custom | user-entered factor (1.0–2.4) |

TDEE and meal kcal are rounded to the nearest 10 kcal. The profile estimate shows BMR, daily maintenance, and kcal for every meal in the split. Cooking uses the **recipe’s meal type** (breakfast, lunch, dinner, or snack) with the matching profile percent.

### Meal split (per profile, editable)

Defaults (must sum to 100%):

| Meal | % |
|------|---|
| Breakfast | 20 |
| Lunch | 30 |
| Dinner | 35 |
| Snack | 15 |

v1 cooking UI uses the recipe’s `MealType`: `mealKcal = TDEE × matching profile percent / 100`. Existing recipes without a stored type default to **lunch**. The recipe does not store a profile id — it only stores which meal it is; the active profile supplies the percents.

## Deferred features

See [future-improvements.md](./future-improvements.md) for goal offsets, EFSA model choice, and other planned work.

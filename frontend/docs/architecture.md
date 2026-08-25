# Frontend architecture

Blazor WebAssembly frontend for SPC. Read `../../docs/architecture.md` for monorepo context.

## Projects

| Project | Path | Role |
|---------|------|------|
| **SPC.Web** | `src/SPC.Web/` | UI — pages, components, CSS, UI-only services |
| **SPC.Core** | `src/SPC.Core/` | Shared logic — DTOs, validation, calculators, repository interfaces |
| **SPC.Core.Tests** | `src/SPC.Core.Tests/` | xUnit tests targeting Core only |

**Rule:** No business logic in `.razor` code-behind. Components bind data and call Core helpers or injected services.

## Folder layout

```
src/SPC.Core/
├── Models/           RecipeDto, IngredientDto, …
├── Validation/       RecipeValidator, ProfileValidator
├── Formatting/       NumberFormat (display quantities)
├── Services/         IPortionCalculator, IEnergyCalculator, …
└── Repositories/     IRecipeRepository, IUserProfileRepository

src/SPC.Web/
├── Pages/            Routable pages (Home, RecipeEditor, …)
├── Components/       Reusable UI (IngredientRow, RecipeSummary, …)
├── Layout/           MainLayout, NavMenu
├── Services/         RecipeDraftService (session state until persistence)
└── wwwroot/css/      Global styles (CSS variables in app.css)
```

## State management

| State | Where | Lifetime |
|-------|-------|----------|
| Recipe being edited | `RecipeDraftService` | Singleton (in-memory) |
| Portion inputs (kcal/portion or count) | `RecipeDraftService` | Session-only; not saved with the recipe |
| Actual cooked weight | `RecipeDto.ActualDishWeightG` | Saved with the recipe |
| Saved recipes | `IRecipeRepository` → `LocalStorageRecipeRepository` | Browser localStorage (`spc.recipes.v1`) |
| Person profiles | `IUserProfileRepository` → `LocalStorageUserProfileRepository` | Browser localStorage (`spc.profiles.v1`) |
| Selected profile | `ActiveProfileService` | Session + `spc.activeProfileId.v1`; not stored on recipes |
| Validation / totals | `RecipeValidator` in Core | Stateless |
| Portion math | `IPortionCalculator` in Core | Stateless; ingredient-sum + yield ([portion-math.md](./portion-math.md)) |
| Energy targets | `IEnergyCalculator` in Core | Stateless; Mifflin × PAL ([energy-targets.md](./energy-targets.md)) |

`RecipeDraftService` holds the current edit. **Save** writes through `IRecipeRepository` (not directly to localStorage).

### Unsaved changes

A **baseline snapshot** (`RecipeDto` clone) is stored when the recipe is loaded, created, or saved. `HasUnsavedChanges` compares the current draft to that baseline via `RecipeEquivalence` in Core (name, ingredients, spices, actual cooked weight — not `UpdatedAt` or ids).

Guards when dirty:

- In-app navigation: `NavigationLock` + leave confirmation modal
- Tab close / refresh: `beforeunload` via `wwwroot/js/spc.js`

### Persistence (stopgap → backend)

```
UI → IRecipeRepository → LocalStorageRecipeRepository → IBrowserLocalStorage
                      ↘ (later) ApiRecipeRepository   → HTTP + PostgreSQL
```

Swap the DI registration in `Program.cs` to move to the backend. DTOs and UI stay unchanged.

## UI patterns

- **Pages** own layout and wire services to **components**.
- **Components** receive DTOs via `[Parameter]` and report changes via `EventCallback`.
- **Validation** is computed in Core; UI displays `RecipeValidator.ValidateRecipe()` results.
- **Styling** — plain CSS with tokens in `app.css`. Fonts, sizes, and the info **i** are specified in [ui.md](./ui.md). Do not add one-off `font-size` in `.razor` or a new stylesheet.
- **Numbers** — `NumberFormat` in `SPC.Core/Formatting`. Round to 2 decimal places (away from zero). If the result is a whole number, show no decimals (`1`, `100`, `543`). Otherwise always show two digits (`4.32`, `4.30` not `4.3`). Decimal separator is `.` (invariant). Integers that are not quantities (age, counts, dates) stay as integers. PAL and meal-split **factors** in labels/tooltips use a short form (`1.4`, `0.3`).

### Product preferences (do not re-ask)

Apply these on every new page and whenever an existing screen is reworked. They come from repeated product feedback.

1. **Sections, not flat forms.** Group related fields in `.editor-section` with `h2.section-title` and a short `.section-hint` (what the group is for). Nested groups in a summary card use `.summary-section` + `h3` (example: profile Estimate = BMR/daily, then a Meal split block).
2. **Edit layout.** Create/edit pages use `.recipe-layout`: inputs in an editor card, live preview/estimate in a summary card.
3. **Explain computed numbers.** Values derived from a formula (BMR, maintenance, meal kcal, similar) get an `InfoTip` (**i**) with what it is and `Formula: …` using **live** numbers in `[brackets]`.
4. **Caveats stay visible.** Important disclaimers (typical ±10% error, not medical advice) sit as `.field-hint` under the value. Tooltips add the formula; they do not hide the caveat.
5. **Info mark is smaller than the label** it sits next to (`--size-info-tip` / `--text-info-mark` in [ui.md](./ui.md)).
6. **Reuse.** `InfoTip`, `AppModal`, `.field`, `.card`, `.section-title` — extend these instead of parallel markup.

Do **not** treat a one-off control as a global pattern. Activity PAL options with everyday examples and `[1.4]` indexes are for that field only; other selects stay plain labels unless asked.

## Testing strategy

- **Core:** unit tests for validators, calculators, mappers (primary focus). Agents run these.
- **Web:** the human evaluates interactions in the browser. Agents do not drive the UI unless asked. Component tests only if complexity warrants.

## Related

- Stack and commands: `README.md` (this folder)
- UI / typography: `ui.md`
- Portion math: `portion-math.md`
- Energy targets: `energy-targets.md`
- Roadmap: `../../plans/thePlan.md`
- Agent rules: `../AGENTS.md`

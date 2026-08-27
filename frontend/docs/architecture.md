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
├── Models/           RecipeDto, RecipeIngredientDto, IngredientDto, …
├── Validation/       RecipeValidator, ProfileValidator
├── Formatting/       NumberFormat (display quantities)
├── Services/         IPortionCalculator, IEnergyCalculator, …
└── Repositories/     IRecipeRepository, IUserProfileRepository, IIngredientRepository

src/SPC.Web/
├── Pages/            Routable pages (Home, Library, RecipeEditor, …)
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
| Recipe family / variant | `RecipeDto.FamilyId`, `VariantLabel` | Variations of one dish. Home lists **one row per family**. Empty label displays as **Default**; switch variants on the recipe editor tabs |
| Recipe notes | `RecipeDto.Notes` | One rich-text block per variant (same editor as an instruction step; no number or add/remove) |
| Saved recipes | `IRecipeRepository` → `LocalStorageRecipeRepository` | Browser localStorage (`spc.recipes.v1`); home list via `GetPageAsync` (10 / 25 / 50, newest family first, name or variant-label contains + meal-type filters) |
| Ingredient library | `IIngredientRepository` → `LocalStorageIngredientRepository` | Browser localStorage (`spc.ingredients.v1`); copy-on-use for recipe/spice rows; admin page `/library` |
| Person profiles | `IUserProfileRepository` → `LocalStorageUserProfileRepository` | Browser localStorage (`spc.profiles.v1`) |
| Selected profile | `ActiveProfileService` | Session + `spc.activeProfileId.v1`; not stored on recipes |
| Validation / totals | `RecipeValidator` in Core | Stateless |
| Portion math | `IPortionCalculator` in Core | Stateless; ingredient-sum + yield ([portion-math.md](./portion-math.md)) |
| Energy targets | `IEnergyCalculator` in Core | Stateless; Mifflin × US activity factors ([energy-targets.md](./energy-targets.md)) |

`RecipeDraftService` holds the current edit. **Save** writes through `IRecipeRepository` (not directly to localStorage).

### Identity vs calorie profile (planned — step 11)

A **login account** (username + password sent to the API, Bearer token in the session) is not `UserProfileDto`. Profiles stay the household bodies used for energy targets. After step 11, recipes, the ingredient library, and profiles load from the signed-in account via `Api*` repositories. See [plans/step11-login-user.md](../../plans/step11-login-user.md). Logout must reset `RecipeDraftService` (singleton draft), `ActiveProfileService` (including `spc.activeProfileId.v1`), and the in-memory ingredient library cache.

### Unsaved changes

A **baseline snapshot** (`RecipeDto` clone) is stored when the recipe is loaded, created, or saved. `HasUnsavedChanges` compares the current draft to that baseline via `RecipeEquivalence` in Core (name, family id, variant label, meal type, ingredients, spices, instruction steps, notes, actual cooked weight — not `UpdatedAt` or recipe id).

Guards when dirty:

- In-app navigation: `NavigationLock` + leave confirmation modal
- Tab close / refresh: `beforeunload` via `wwwroot/js/spc.js`

### Persistence (stopgap → backend)

`localStorage` is the prototype store. It is not the long-term store (quota, one browser, no sharing). Plan: `plans/step5-save-recipes-and-ingredients.md`.

```
UI → IRecipeRepository.GetPageAsync / GetByFamilyIdAsync / Save / Delete / DeleteFamilyAsync
        → LocalStorageRecipeRepository → IBrowserLocalStorage   (now)
        → ApiRecipeRepository          → HTTP + Bearer + DB     (step 11; API is step 10)
```

Swap the DI registration in `Program.cs` to move to the backend. DTOs and UI stay unchanged. Database choice is a step 10 discussion, not a frontend decision.

- **Recipe line:** `RecipeIngredientDto` (name, grams used, kcal/100 g)
- **Nutrition library:** `IngredientDto` (canonical name, kcal/100 g) in `spc.ingredients.v1`. Shared by ingredient and spice rows. Copy-on-use: picking fills the row; saving a recipe adds new foods and may ask before changing library kcal. Existing recipes are not rewritten.
- **Recipe type:** `MealType` on the recipe (breakfast / lunch / dinner / snack). Profile meal-split percents are applied at read time; no profile id on the recipe.

**Name picker.** Filter on each keystroke while the library is local (no debounce, no spinner). Omit library foods already used on other rows of the same list (ingredients vs spices). First match is highlighted so Enter selects. Tab/Escape dismisses without filling. **When search is remote** (HTTP `IIngredientRepository` or step 9 API): debounce **200–250 ms** and show a spinner only while a request is in flight. Do not ship that delay for in-memory/localStorage search.

**Delete** a saved recipe from home (the whole family) or from a variant tab’s pen (that variant only). Confirm when the recipe has data. Home lists **one row per dish**; the name filter also matches variant labels so a family still appears. Open the recipe to switch variants (Excel-style tabs under **Edit recipe**, including a **Default** tab when the base row has no label). A pen on each tab opens a modal to rename that variant (empty or Default = base tab; names unique in the family) or delete it. The save panel is **Scale this for…**, **Save recipe**, then **Save as variant** (copy the current draft to a new tab; the open variant stays as last saved). Per page / previous / next sit under the list. Page sizes are 10, 25, or 50 (`Paging.PageSizes`) and count **families**. New recipes still open `/recipe/new`. **Scale this for…** can fill kcal from the selected profile’s meal split (same value as **Use as portion target**); the control is labeled **Scale for profile {name}**. Preview first, with one name field, then **Save as variant** or **Save as recipe**.

**Library page** (`/library`). **Add ingredient** opens an `AppModal` with name + kcal; **Edit** on a row opens the same dialog for that food. Adding a name already in the library updates that food. Renaming onto another existing food is an error. Delete removes the library row only; recipes stay as they are. Name filter is a case-insensitive contains match. `SearchAsync` still caps the recipe-editor picker at 8.

## UI patterns

- **Pages** own layout and wire services to **components**.
- **Components** receive DTOs via `[Parameter]` and report changes via `EventCallback`.
- **Validation** is computed in Core; UI displays `RecipeValidator.ValidateRecipe()` results.
- **Styling** — plain CSS with tokens in `app.css`. Fonts, sizes, and the info **i** are specified in [ui.md](./ui.md). Do not add one-off `font-size` in `.razor` or a new stylesheet.
- **Numbers** — `NumberFormat` in `SPC.Core/Formatting`. Round to 2 decimal places (away from zero). If the result is a whole number, show no decimals (`1`, `100`, `543`). Otherwise always show two digits (`4.32`, `4.30` not `4.3`). Decimal separator is `.` (invariant). Integers that are not quantities (age, counts, dates) stay as integers. Activity and meal-split **factors** in labels/tooltips use a short form (`1.2`, `0.3`).

### Product preferences (do not re-ask)

Apply these on every new page and whenever an existing screen is reworked. They come from repeated product feedback.

1. **Sections, not flat forms.** Group related fields in `.editor-section` with `h2.section-title` and a short `.section-hint` (what the group is for). Nested groups in a summary card use `.summary-section` + `h3` (example: profile Estimate = BMR/daily, then a Meal split block).
2. **Edit layout.** Create/edit pages use `.recipe-layout`: inputs in an editor card, live preview/estimate in a summary card.
3. **Explain computed numbers.** Values derived from a formula (BMR, maintenance, meal kcal, similar) get an `InfoTip` (**i**) with what it is and `Formula: …` using **live** numbers in `[brackets]`.
4. **Caveats stay visible.** Important disclaimers (typical ±10% error, not medical advice) sit as `.field-hint` under the value. Tooltips add the formula; they do not hide the caveat.
5. **Info mark is smaller than the label** it sits next to (`--size-info-tip` / `--text-info-mark` in [ui.md](./ui.md)).
6. **Reuse.** `InfoTip`, `AppModal`, `.field`, `.card`, `.section-title` — extend these instead of parallel markup.
7. **Confirm before discarding entered data.** Empty rows (add-then-immediately-remove) delete at once. If a row has any name or number typed, confirm with `AppModal` first.

Do **not** treat a one-off control as a global pattern. Activity options with everyday examples and `[1.2]` factors are for that field only; other selects stay plain labels unless asked.

## Testing strategy

- **Core:** unit tests for validators, calculators, mappers (primary focus). Agents run these (`dotnet test`).
- **Web / UI:** the human tests in the browser. Agents do not start the app or drive the UI unless **explicitly** asked. Component tests only if complexity warrants.

## Related

- Stack and commands: `README.md` (this folder)
- UI / typography: `ui.md`
- Recipe instructions: `recipe-instructions.md`
- Portion math: `portion-math.md`
- Energy targets: `energy-targets.md`
- Roadmap: `../../plans/thePlan.md`
- Login accounts: `../../plans/step11-login-user.md`
- Backend + database: `../../plans/step10-backend.md`
- Agent rules: `../AGENTS.md`

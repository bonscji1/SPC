# Plan: Step 5 — Save recipes and ingredient nutrition

**Date:** 2026-08-24  
**Updated:** 2026-08-26  
**Scope:** both  
**Status:** **done** — localStorage prototype (recipes, profiles, ingredient library, library admin page). Backend is later, not this step.  
**Depends on:** [step4-human-tracking.md](./step4-human-tracking.md) — complete  
**Parent:** [thePlan.md](./thePlan.md)

## Goal

Persist recipes and reusable ingredient nutrition (name + kcal/100g) so users do not re-enter the same data every session.

## Out of scope

- Cookbook browsing UX polish — **done on Home** (step 7); no extra route. Ingredient-*list* search deferred (`frontend/docs/future-improvements.md`)
- External nutrition API (step 9)
- Auth, multi-user accounts, and device sync (backend phase below)

## Data to persist

- **Recipes** — name, meal type (breakfast / lunch / dinner / snack), ingredients (`RecipeIngredientDto`: amount + kcal/100g), spices, instructions (tokens + TipTap `editorJson`), portion fields, `updatedAt`
- **Ingredients library** — `IngredientDto`: canonical name, caloriesPer100g — `spc.ingredients.v1`
- **User profiles** — from step 4

`RecipeIngredientDto` is a **line on a recipe** (how much of this food is in this dish). `IngredientDto` is a **reusable nutrition fact**. They are not the same type: a library entry has no grams-used; a recipe line is not the canonical food record.

## Persistence strategy

Frontend-only `localStorage` is the **prototype**. It is enough to iterate UX and DTO shapes. It is **not** the long-term store for a real app.

### Why localStorage is a stopgap

- Quota (~5 MB) and a single JSON blob per collection; no indexes, no server-side search
- Data lives in one browser profile — wipe, another device, or another browser and it is gone
- No backup, sharing, or concurrent users
- Paging a full dump in the client does not scale past a modest cookbook
- A shared ingredient library should eventually be canonical (and later API-backed), not copied per browser

### Target architecture

DTO-first, storage-agnostic. UI talks to DTOs and repository interfaces only.

```
UI / components
    → services (orchestration)
        → IRecipeRepository, IIngredientRepository, IUserProfileRepository
            → LocalStorage*Repository (now — prototype)
            → Api*Repository (real app)
                → HTTP → backend → database (step 10)
```

Swap the DI registration in `SPC.Web/Program.cs`. DTOs, validation, and UI stay unchanged.

| Layer | Now | Real app |
|-------|-----|----------|
| Recipes | `spc.recipes.v1` in localStorage | `ApiRecipeRepository` → REST → database (step 11 uses API from step 10) |
| Profiles | `spc.profiles.v1` | same pattern, per authenticated user |
| Ingredient library | `spc.ingredients.v1` | per-user table (step 10); later optional shared catalog; step 9 may fill kcal |
| List reads | `GetPageAsync(page, pageSize)` slices in memory | `LIMIT`/`OFFSET` or keyset; same method signature |
| Auth | none | login endpoint + Bearer in step 10; Blazor login UI in step 11 |

**Paged reads are already on the repository** (`IRecipeRepository.GetPageAsync`, `IIngredientRepository.GetPageAsync`, page sizes **10**, **25**, and **50**) so the backend can implement paging without a UI rewrite. Allowed sizes live in `Paging.PageSizes`.

### When to introduce the backend

Stay on localStorage while we finish this step’s library UX and while a single-browser prototype is enough.

Introduce the API + database ([step 10](./step10-backend.md)) when any of these become true (Blazor login after that: [step 11](./step11-login-user.md)):

- Users need the same recipes on more than one device
- We want a shared or curated ingredient library
- Recipe count or payload size outgrows a JSON blob
- We need backup, export-to-account, or more than one person on one dataset

Suggested first backend slice: the **same DTOs** over REST, login that issues a Bearer token, and account-scoped recipe/ingredient/profile routes. **API + DB first** ([step 10](./step10-backend.md)); the Blazor login page and `Api*` repositories are [step 11](./step11-login-user.md). Optional one-time **import from localStorage** in step 11 so prototype data is not stranded. Do not start step 10 until we discuss which database and schema fit.

### Storage format / migration

- Keys are versioned (`spc.recipes.v1`). JSON property names on DTOs are the contract, not C# type names — renaming `IngredientDto` → `RecipeIngredientDto` does not require a key bump.
- When a stored shape changes incompatibly, bump the key (`v2`) and migrate `v1` → `v2` on first read, or document a wipe if still pre-release.
- Backend: treat Core DTOs as the HTTP contract; schema migrations live in the API, not in Blazor.

## Architecture (decided)

**Storage chosen for v1 (prototype):** `localStorage` in `SPC.Web/Repositories/`.

- Recipes: `spc.recipes.v1`
- Profiles: `spc.profiles.v1`
- Ingredient library: `spc.ingredients.v1` via `LocalStorageIngredientRepository`

## Ingredient library (decided)

**Copy-on-use.** The library is a nutrition lookup (`IngredientDto`: canonical name, kcal/100 g). Recipe lines (`RecipeIngredientDto` / `SpiceDto`) store their own kcal. Changing the library does **not** rewrite existing recipes. There is no foreign key from a recipe line to a library row.

**One list for ingredients and spices.** Ingredient vs spice is a role on the recipe row, not a type on the food. Butter can be a spice in one dish and an ingredient in another.

**Picker (name field, both row kinds):**

- Filter the list **on each keystroke** (in-memory / localStorage). No spinner.
- Show after one character; cap at 8 matches (prefix, word prefix, or query starts with the name so `onions` still lists `onion`).
- **First match is highlighted.** Enter selects it. Arrow keys move; click selects. Selection writes the canonical name and fills kcal. Foods already used on **other rows of the same list** (other ingredients, or other spices) are omitted; the row being edited can still match itself.
- Tab, Escape, or moving to grams **dismisses without selecting**. Typing alone does not fill kcal.
- When `SearchAsync` is served over HTTP (backend or step 9), debounce **200–250 ms** and show a spinner only if the request is still in flight. Do not add that wait while search is local. See `frontend/docs/architecture.md`.

**On recipe save:**

- Lines with a name and kcal **> 0** are considered (spices with blank kcal are skipped).
- New normalized name → add to the library (no prompt).
- Same food (normalized name, case-insensitive), same kcal → nothing.
- Same food, different kcal → one confirm listing the changes. Yes updates the library; No leaves the library. Other recipes stay as they are.
- Last row wins if the same name appears twice in one recipe.

**Library page (`/library`).** **Add ingredient** (header button, same idea as Home’s Create a recipe) opens a modal for name + kcal. **Edit** on a saved row opens that modal for the same food. Name filter above the list; per page / previous / next below it. Adding a name already in the library updates that food. Renaming onto another existing food is an error. Delete removes the library row only; recipes stay as they are. Name filter is a case-insensitive contains match. The recipe-editor picker still uses `SearchAsync` (cap 8).

## Deliverables

- [x] Save recipe (create / update) — includes instructions and spices
- [x] Load recipe into editor
- [x] Persist user profile across sessions
- [x] **Delete recipe** — home list and editor (confirm when the recipe has data)
- [x] **Paged recipe list** — 10, 25, or 50 per page, previous/next, current page; newest first; name contains + meal-type filters
- [x] **Ingredient library** — `LocalStorageIngredientRepository` implementing `IIngredientRepository`
- [x] **Library page** — add / edit / delete on `/library` (same page as the list)
- [x] **Save to library** on recipe save (add new foods; confirm before changing library kcal)
- [x] **Pick from library** on ingredient and spice name fields (Enter/click fills name + kcal)

## Suggested implementation order

1. ~~Delete recipe UI~~ **done**
2. ~~Ingredient library repository + picker + save-time sync~~ **done**
3. Backend — **not this step**; introduce when the criteria above are met
4. Library admin page — **done** (`/library`)

## Acceptance criteria

- After reload, saved recipes, profile, and library foods are still available
- User can delete a saved recipe from home without opening it, and from the editor next to Save
- Deleting a recipe that has data asks for confirmation
- Home stays usable with many recipes (page size 10/25/50, page indicator, previous/next, name + type filters)
- Ingredient library reduces duplicate manual entry: pick from the list to fill kcal; new foods accrue on save
- Library page can add, edit, and delete foods without opening a recipe; changing the library does not rewrite recipes
- Overwriting kcal on a recipe asks before changing the library; other recipes are unchanged
- Migration path documented if storage format changes (see Persistence strategy)
- Repository contracts remain valid when persistence moves to HTTP + database (steps 10–11)

## Open questions

- Backend now vs. stay on localStorage — **stay on localStorage for this step**; API in [step 10](./step10-backend.md), Blazor login in [step 11](./step11-login-user.md)
- Recipe versioning on save — `updatedAt` on `RecipeDto` exists; formal migration story TBD (key bump)
- Ingredient library: one global list vs. per-user when auth arrives — **per-user** (accounts share the database, not the library). A canonical shared catalog can come later beside that.
- Home sort: newest `updatedAt` first, then name — kept; not revisiting for a separate cookbook

# Plan: Step 5 — Save recipes and ingredient nutrition

**Date:** 2026-08-24  
**Scope:** both  
**Status:** draft  
**Depends on:** [step4-human-tracking.md](./step4-human-tracking.md)  
**Parent:** [thePlan.md](./thePlan.md)

## Goal

Persist recipes and reusable ingredient nutrition (name + kcal/100g) so users do not re-enter the same data every session.

## Out of scope

- Cookbook browsing UX polish (step 6)
- External nutrition API (step 8)
- Multi-user accounts / sync (unless trivial with chosen backend)

## Data to persist

- **Recipes** — full recipe from step 2 (name, ingredients)
- **Ingredients library** — canonical name, caloriesPer100g (and optional aliases)
- **User profile** — from step 4 (if not already local-only)

## Architecture (decided direction)

**DTO-first, storage-agnostic.** UI and business logic talk to **DTOs** and **repository interfaces** — never directly to localStorage, IndexedDB, or HTTP.

```
UI / components
    → services (orchestration)
        → IRecipeRepository, IIngredientRepository, IUserProfileRepository
            → implementation A: local (IndexedDB / localStorage)  ← likely first
            → implementation B: API client → backend + PostgreSQL  ← later
```

- **DTOs** live in a shared-friendly project/namespace from step 1 onward (e.g. `SPC.Core` or `frontend/Models/` structured for later extraction to `shared/`).
- **Mappers** between DTOs and any storage-specific shapes stay in the repository layer only.
- When backend arrives, add REST endpoints that accept/return the same DTOs; swap repository implementation, not UI.

Storage backend (**local vs. API**) is **TBD at implementation time** — the interface boundary is not.

## Storage options (decide at implementation)

| Option | Pros | Cons |
|--------|------|------|
| localStorage / IndexedDB | No backend yet | No cross-device |
| Backend + PostgreSQL | Real persistence, sharing later | More setup |

Document the chosen implementation in `docs/` when made.

## Deliverables

- [ ] Save recipe (create / update)
- [ ] Load recipe into editor
- [ ] Delete recipe
- [ ] Save ingredient to library when entering a recipe (optional prompt: “save carrot 41 kcal/100g for reuse?”)
- [ ] Pick ingredient from library when adding a row (autofill kcal/100g)
- [ ] Persist user profile across sessions

## Acceptance criteria

- After reload, saved recipes and profile are still available
- Ingredient library reduces duplicate manual entry
- Migration path documented if storage format changes

## Open questions

- Backend now vs. IndexedDB first — **deferred**; repository interface must work either way
- Recipe versioning on save (simple `updatedAt` timestamp minimum)
- Extract `SPC.Core` class library timing — step 1 can lay groundwork; step 5 may formalize

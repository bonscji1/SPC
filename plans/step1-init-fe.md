# Plan: Step 1 — Init frontend

**Date:** 2026-08-24  
**Scope:** frontend  
**Status:** draft  
**Depends on:** —  
**Parent:** [thePlan.md](./thePlan.md)

## Goal

Create a minimal frontend application that runs locally, is pleasant to iterate on, and gives us a place to hang features from step 2 onward.

## Out of scope

- Recipe logic, forms, or persistence
- Backend
- Final stack commitment (pick something practical for fast local dev; stack note lives in `notes-tech-stack-options.md`)

## Deliverables

- [x] `frontend/` project initialized with chosen tooling
- [x] Dev server runs with one command documented in `frontend/AGENTS.md`
- [x] Single landing view confirming the app loads (app name, short tagline)
- [x] Basic project structure (components, pages/routes, styles — match framework conventions)
- [x] Lint/format setup (minimal but consistent)

## Recommended stack (step 1)

**Blazor WebAssembly** — best fit given Blazor FE + possible C# backend:

| Choice | Rationale |
|--------|-----------|
| **Blazor WASM** (not Server) | Calculations run in-browser; works offline later; no latency on every keystroke |
| **.NET 10** | Current LTS; good WASM tooling |
| **Plain CSS or minimal CSS** | Skip heavy UI kits until step 2 forms prove what we need |
| **`SPC.Core` class library** | DTOs + calculation interfaces from day 1; later shared with backend |

If backend ends up **Go** instead of C#, DTOs stay in the FE as TypeScript-shaped contracts and we generate OpenAPI clients — but C# backend keeps one language end-to-end.

## Suggested folder layout

```
frontend/
├── SPC.sln
├── src/
│   ├── SPC.Core/              # DTOs, calculation logic, repository interfaces
│   │   ├── Models/            # RecipeDto, IngredientDto, …
│   │   ├── Services/          # IPortionCalculator, …
│   │   └── Repositories/      # IRecipeRepository (stubs OK in step 1)
│   ├── SPC.Web/               # Blazor WASM app
│   │   ├── Pages/
│   │   ├── Components/
│   │   └── Program.cs
│   └── SPC.Core.Tests/        # xUnit — empty scaffold; used heavily from step 3
```

## Suggested approach

1. `dotnet new blazorwasm` (or `blazorwasm-empty`) under `frontend/src/SPC.Web`.
2. Add `SPC.Core` class library; reference from Web and Tests.
3. Define placeholder DTOs (`RecipeDto`, `IngredientDto`) even if unused until step 2.
4. Add a home page with app name + tagline; optional nav shell for future routes.
5. Add `xunit` test project wired to `SPC.Core`.
6. Document commands in `frontend/AGENTS.md`.

## Acceptance criteria

- `dotnet watch run --project src/SPC.Web` starts without errors
- App opens in browser on localhost
- `dotnet test` runs (even if zero tests yet)
- `SPC.Core` exists with at least one DTO and is referenced by the Web project
- Commands documented in `frontend/AGENTS.md`

## Notes for implementers

- Keep dependencies lean; no MudBlazor/Radzen until UI complexity warrants it.
- Put **all** future business logic in `SPC.Core`, not in `.razor` code-behind.
- Repository interfaces can be empty stubs in step 1 — step 5 implements them.
- Add `global.json` or document SDK version if team needs reproducible builds.

## Open questions

- Confirm Blazor WASM with project owner before scaffold (recommended default).
- `SPC.Core` inside `frontend/` vs. top-level `shared/` — either works; `frontend/src/SPC.Core` is fine until backend exists.

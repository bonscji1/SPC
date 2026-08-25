# Architecture

High-level structure of the Smart Pig's Cookbook monorepo.

## Overview

SPC is a **monorepo** with a frontend app today and a backend added when persistence or APIs are needed (step 5+).

```
┌─────────────────────────────────────────────────────────┐
│  SPC (repo root)                                        │
│  ├── docs/           Cross-cutting documentation        │
│  ├── plans/          Implementation plans               │
│  ├── frontend/       Blazor WASM app (active)           │
│  └── backend/        API service (future)               │
└─────────────────────────────────────────────────────────┘
```

## Design principles

1. **Incremental delivery** — each plan step is independently testable.
2. **DTO-first** — UI and storage never share raw DB or file shapes; everything goes through DTOs in `SPC.Core`.
3. **Repository boundary** — persistence is behind interfaces; swap local storage for HTTP without touching UI.
4. **Logic in Core** — calculations, validation, and business rules live in `SPC.Core`, not in `.razor` files.

## Frontend (current)

```
SPC.Web (Blazor WASM)          SPC.Core (class library)
┌──────────────────────┐       ┌──────────────────────────┐
│ Pages / Components   │──────▶│ Models (DTOs)            │
│ Services (UI state)  │       │ Validation               │
│ wwwroot (CSS)        │       │ Services (interfaces)    │
└──────────────────────┘       │ Repositories (interfaces) │
                               └──────────────────────────┘
```

| Layer | Responsibility |
|-------|----------------|
| **SPC.Web** | Routing, layout, forms, in-memory session state |
| **SPC.Core** | DTOs, validation, portion math, repository contracts |
| **SPC.Core.Tests** | Unit tests for Core logic |

See `frontend/docs/architecture.md` for frontend detail.

## Backend (future)

When introduced, the backend will:

- Expose REST (or similar) endpoints accepting/returning the **same DTOs** as `SPC.Core`
- Implement repository interfaces against PostgreSQL
- Optionally share `SPC.Core` as a project reference if the backend is C#

If the backend is **Go**, DTOs in Core become the API contract; generate or hand-map clients in the Web project.

## Data flow (today)

```
User input → Blazor components → RecipeDraftService (in-memory)
                                      ↓
                               RecipeDto / IngredientDto
                                      ↓
                               RecipeValidator + PortionCalculator (SPC.Core)
```

Persistence (step 5): `RecipeDraftService` → `IRecipeRepository` → local storage or API.

## Documentation map

| Topic | Location |
|-------|----------|
| Monorepo architecture | `docs/architecture.md` (this file) |
| Frontend architecture | `frontend/docs/architecture.md` |
| Frontend UI / typography | `frontend/docs/ui.md` |
| Portion / calorie model | `frontend/docs/portion-math.md` |
| Energy targets (BMR / TDEE) | `frontend/docs/energy-targets.md` |
| Agent rules (shared) | `AGENTS.md` |
| Implementation roadmap | `plans/thePlan.md` |

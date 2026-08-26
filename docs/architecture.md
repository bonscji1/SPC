# Architecture

High-level structure of the Smart Pig's Cookbook monorepo.

## Overview

SPC is a **monorepo** with a frontend app today and a backend added when localStorage is no longer enough (multi-device, sharing, or a canonical ingredient library). See `plans/step5-save-recipes-and-ingredients.md`.

```
┌─────────────────────────────────────────────────────────┐
│  SPC (repo root)                                        │
│  ├── docker-compose.yml  Orchestrates services (step 6) │
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

## Deployment (step 6)

Repo-root **Compose** orchestrates the stack. Each subproject owns its **Dockerfile**. Run from the repo root:

```bash
docker compose up --build
```

Open http://localhost:8080 (host **8080** → nginx **80** in the frontend image).

Blazor WASM publishes to static files, so the frontend image is **nginx**, not an ASP.NET runtime. That nginx is the public entry. When a backend exists, proxy `/api` to it on the compose network (same origin, no CORS). Postgres stays internal.

See [plans/step6-deployments.md](../plans/step6-deployments.md) and the root `README.md`.

## Data flow (today)

```
User input → Blazor components → RecipeDraftService (in-memory)
                                      ↓
                               RecipeDto / RecipeIngredientDto
                                      ↓
                               RecipeValidator + PortionCalculator (SPC.Core)
```

Persistence (step 5 prototype): `RecipeDraftService` → `IRecipeRepository` → localStorage. Nutrition library: `IIngredientRepository` → `spc.ingredients.v1`. List UI uses `GetPageAsync` (10/25/50). Real app: swap in `Api*Repository` → HTTP + PostgreSQL; same DTOs. `IngredientDto` is the nutrition library (not a recipe line).

## Documentation map

| Topic | Location |
|-------|----------|
| Monorepo architecture | `docs/architecture.md` (this file) |
| Frontend architecture | `frontend/docs/architecture.md` |
| Frontend UI / typography | `frontend/docs/ui.md` |
| Recipe instructions | `frontend/docs/recipe-instructions.md` |
| Portion / calorie model | `frontend/docs/portion-math.md` |
| Energy targets (BMR / TDEE) | `frontend/docs/energy-targets.md` |
| Agent rules (shared) | `AGENTS.md` |
| Implementation roadmap | `plans/thePlan.md` |
| Run the stack | Root `README.md` (`docker compose up --build`) |
| Deployment (compose) | `plans/step6-deployments.md` |

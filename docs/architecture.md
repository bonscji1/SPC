# Architecture

High-level structure of the Smart Pig's Cookbook monorepo.

## Overview

SPC is a **monorepo** with a frontend app today. Persistence is browser localStorage (step 5). **Login accounts** (step 10, dummy user until the API exists) prepare per-user recipes and ingredient lists. A **backend + database** (step 11) is added when we need data off the browser; engine and schema are chosen in that step’s discussion, not here. See `plans/thePlan.md`.

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

## Identity (planned — step 10)

**Account** (login user) is not the same as a step 4 **person profile** (BMR / TDEE). The account owns recipes, the ingredient library, and profiles. Until the API exists, one **dummy default user** is seeded; passwords are stored as salt + hash and compared on login. The session holds a bearer token so HTTP can attach `Authorization: Bearer …` later.

See [plans/step10-login-user.md](../plans/step10-login-user.md).

## Backend (planned — step 11)

**Discussion first:** which database(s) and what structure fit this use case. PostgreSQL is a strong candidate from earlier notes; it is not locked. Do not add Compose `backend` / `db` services until that discussion lands.

When introduced, the backend will:

- Issue the login token; validate Bearer on every API request; scope rows by account id
- Expose REST (or similar) endpoints accepting/returning the **same DTOs** as `SPC.Core`
- Implement repository interfaces against the chosen store
- Optionally share `SPC.Core` as a project reference if the backend is C#

If the backend is **Go**, DTOs in Core become the API contract; generate or hand-map clients in the Web project.

Users **share the database**, not each other’s recipes or libraries.

See [plans/step11-backend.md](../plans/step11-backend.md).

## Deployment (step 6)

Repo-root **Compose** orchestrates the stack. Each subproject owns its **Dockerfile**. Run from the repo root:

```bash
docker compose up --build
```

Open http://localhost:8080 (host **8080** → nginx **80** in the frontend image).

Blazor WASM publishes to static files, so the frontend image is **nginx**, not an ASP.NET runtime. That nginx is the public entry. When a backend exists, proxy `/api` to it on the compose network (same origin, no CORS). The database stays internal (engine chosen in step 11).

See [plans/step6-deployments.md](../plans/step6-deployments.md) and the root `README.md`.

## Data flow (today)

```
User input → Blazor components → RecipeDraftService (in-memory)
                                      ↓
                               RecipeDto / RecipeIngredientDto
                                      ↓
                               RecipeValidator + PortionCalculator (SPC.Core)
```

Persistence (step 5 prototype): `RecipeDraftService` → `IRecipeRepository` → localStorage. Nutrition library: `IIngredientRepository` → `spc.ingredients.v1`. Home lists **one row per recipe family** via `GetPageAsync` (10/25/50); variants switch on the recipe editor. Step 10 scopes those stores to the signed-in account. Real app: swap in `Api*Repository` → HTTP + Bearer + database (step 11); same DTOs. `IngredientDto` is the nutrition library (not a recipe line).

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
| Login / accounts | `plans/step10-login-user.md` |
| Backend + database | `plans/step11-backend.md` |
| Run the stack | Root `README.md` (`docker compose up --build`) |
| Deployment (compose) | `plans/step6-deployments.md` |

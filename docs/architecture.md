# Architecture

High-level structure of the Smart Pig's Cookbook monorepo.

## Overview

SPC is a **monorepo** with a frontend app today. Persistence is browser localStorage (step 5). A **backend + database** (step 10) is added when we need data off the browser; engine and schema are chosen in that step’s discussion. **Login** (step 11) is the Blazor shell that calls that API with a Bearer token — no dummy local auth. See `plans/thePlan.md`.

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

## Backend (planned — step 10)

**Decided:** C# Minimal APIs, one PostgreSQL, JWT, one baked-in default user (`spc` / `spc`) until a later accounts step. See [plans/step10-backend.md](../plans/step10-backend.md).

When introduced, the backend will:

- Seed accounts; `POST /api/auth/login` issues a Bearer token (server-side salt + hash)
- Validate Bearer on every other API request; scope rows by account id
- Expose REST (or similar) endpoints accepting/returning the **same DTOs** as `SPC.Core`
- Implement persistence in PostgreSQL (`account_id` on user-owned rows; JSONB for nested recipe parts)
- Share `SPC.Core` as a project reference (C# API)

Go is not in scope.

Users **share the database**, not each other’s recipes or libraries. The Blazor app stays on localStorage until step 11.

See [plans/step10-backend.md](../plans/step10-backend.md).

## Identity (planned — step 11)

**Account** (login user) is not the same as a step 4 **person profile** (BMR / TDEE). The account owns recipes, the ingredient library, and profiles.

The UI logs in against the step 10 API, holds the issued Bearer token, and swaps `LocalStorage*` repositories for `Api*`. The ingredient library is loaded once into memory so the name picker stays local. No client-side hashing, dummy tokens, or per-account localStorage keys. Logout must clear `RecipeDraftService`, `ActiveProfileService`, and the ingredient cache.

See [plans/step11-login-user.md](../plans/step11-login-user.md).

## Deployment (step 6)

Repo-root **Compose** orchestrates the stack. Each subproject owns its **Dockerfile**. Run from the repo root:

```bash
docker compose up --build
```

Open http://localhost:8080 (host **8080** → nginx **80** in the frontend image).

Blazor WASM publishes to static files, so the frontend image is **nginx**, not an ASP.NET runtime. That nginx is the public entry. When a backend exists, proxy `/api` to it on the compose network (same origin, no CORS). **`dotnet watch` is a different origin** (frontend :5180, API elsewhere) — CORS or a local proxy is required; see step 10. The database stays internal (engine chosen in step 10).

See [plans/step6-deployments.md](../plans/step6-deployments.md) and the root `README.md`.

## Data flow (today)

```
User input → Blazor components → RecipeDraftService (in-memory)
                                      ↓
                               RecipeDto / RecipeIngredientDto
                                      ↓
                               RecipeValidator + PortionCalculator (SPC.Core)
```

Persistence (step 5 prototype): `RecipeDraftService` → `IRecipeRepository` → localStorage. Nutrition library: `IIngredientRepository` → `spc.ingredients.v1`. Home lists **one row per recipe family** via `GetPageAsync` (10/25/50); variants switch on the recipe editor. Real app: step 10 is the API + DB; step 11 swaps in `Api*Repository` → HTTP + Bearer. Same DTOs. `IngredientDto` is the nutrition library (not a recipe line).

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
| Login / accounts | `plans/step11-login-user.md` |
| Backend + database | `plans/step10-backend.md` |
| Run the stack | Root `README.md` (`docker compose up --build`) |
| Deployment (compose) | `plans/step6-deployments.md` |

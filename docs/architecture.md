# Architecture

High-level structure of the Smart Pig's Cookbook monorepo.

## Overview

SPC is a **monorepo**. A **C# API + PostgreSQL** persist accounts, recipes, libraries, and profiles. The Blazor app logs in and talks to that API with a JWT. See `plans/thePlan.md`.

```
┌─────────────────────────────────────────────────────────┐
│  SPC (repo root)                                        │
│  ├── docker-compose.yml  Orchestrates services (step 6) │
│  ├── docs/           Cross-cutting documentation        │
│  ├── plans/          Implementation plans               │
│  ├── frontend/       Blazor WASM app (active)           │
│  └── backend/        C# API + PostgreSQL (step 10)      │
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

## Backend (step 10)

**In place:** C# Minimal APIs, PostgreSQL, JWT, self-serve sign-up. See [plans/step10-backend.md](../plans/step10-backend.md) and [plans/step12-signup.md](../plans/step12-signup.md).

The API:

- Accepts sign-up and login; `POST /api/auth/signup` stores a salted hash; `POST /api/auth/login` compares hashes and issues a Bearer token
- Validates Bearer on every other API request; scopes rows by account id
- Exposes REST endpoints accepting/returning the **same DTOs** as `SPC.Core`
- Persists in PostgreSQL (`account_id` on user-owned rows; JSONB for nested recipe parts)
- Shares `SPC.Core` as a project reference

Go is not in scope.

Users **share the database**, not each other’s recipes or libraries.

See [plans/step10-backend.md](../plans/step10-backend.md).

## Identity (steps 11–12)

**Account** (login user) is not the same as a step 4 **person profile** (BMR / TDEE). The account owns recipes, the ingredient library, and profiles. **Extra users** are extra login accounts (sign-up), not extra calorie profiles.

The UI logs in or signs up against the API, holds the issued Bearer token in `sessionStorage`, and uses `Api*` / cached ingredient repositories. The ingredient library is loaded once into memory so the name picker stays local. No client-side hashing. Logout clears `RecipeDraftService`, `ActiveProfileService`, and the ingredient cache.

See [plans/step11-login-user.md](../plans/step11-login-user.md) and [plans/step12-signup.md](../plans/step12-signup.md).

## Deployment (step 6)

Repo-root **Compose** orchestrates the stack. Each subproject owns its **Dockerfile**. Run from the repo root:

```bash
docker compose up --build
```

Open http://localhost:8080 (host **8080** → nginx **80** in the frontend image).

Blazor WASM publishes to static files, so the frontend image is **nginx**, not an ASP.NET runtime. That nginx is the public entry and proxies `/api` to the backend on the compose network (same origin, no CORS). **`dotnet watch` is a different origin** (frontend :5180, API :5100) — the API CORS policy allows the Blazor origin. Postgres stays internal.

See [plans/step6-deployments.md](../plans/step6-deployments.md) and the root `README.md`.

## Data flow (today)

```
User input → Blazor components → RecipeDraftService (in-memory)
                                      ↓
                               RecipeDto / RecipeIngredientDto
                                      ↓
                               RecipeValidator + PortionCalculator (SPC.Core)
```

Persistence: `RecipeDraftService` → `IRecipeRepository` → HTTP + Bearer → API → PostgreSQL. Nutrition library: `IIngredientRepository` → in-memory cache hydrated from the API. Home lists **one row per recipe family** via `GetPageAsync` (10/25/50); variants switch on the recipe editor. Same DTOs. `IngredientDto` is the nutrition library (not a recipe line). The login token is in `sessionStorage` (`spc.auth.v1`), not localStorage.

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
| Login / accounts | `plans/step11-login-user.md`, `plans/step12-signup.md` |
| Backend + database | `plans/step10-backend.md` |
| Run the stack | Root `README.md` (`docker compose up --build`) |
| Deployment (compose) | `plans/step6-deployments.md` |

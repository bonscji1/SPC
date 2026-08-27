# Backend documentation

ASP.NET Core Minimal API for SPC. Also read `../../docs/README.md`.

## Stack

- .NET 10, Minimal APIs, EF Core, PostgreSQL
- JWT Bearer (8 hours). Default user **`spc` / `spc`** (placeholder until a later accounts step)
- Project reference to `frontend/src/SPC.Core`

## Run

**Compose (repo root):** `docker compose up --build` → http://localhost:8080 (nginx proxies `/api` to the backend). Copy `.env.example` to `.env` to set `POSTGRES_PASSWORD` and `JWT_KEY` (JWT key at least 32 characters).

**SDK:** Postgres on localhost (user/db `spc`, password from appsettings). Then from `backend/`:

```bash
dotnet watch run --project src/SPC.Api/SPC.Api.csproj
```

http://localhost:5100 — CORS allows the Blazor `dotnet watch` origin http://localhost:5180. The Blazor app sends that origin’s `ApiBaseUrl` (see `frontend/src/SPC.Web/wwwroot/appsettings.Development.json`).

## Auth

`POST /api/auth/login` `{ "username": "spc", "password": "spc" }` → `{ "accessToken", "account" }`.

Other `/api/*` routes (except `/api/health`) require `Authorization: Bearer <token>`.

## Data routes

All JSON camelCase, same Core DTOs as the frontend.

- `GET/PUT /api/recipes`, `GET /api/recipes/{id}`, `GET /api/recipes/families/{familyId}`, `DELETE /api/recipes/{id}`, `DELETE /api/recipes/families/{familyId}`
- `GET /api/ingredients` (full library for hydrate), `GET /api/ingredients/search`, `GET /api/ingredients/page`, `PUT /api/ingredients`, `DELETE /api/ingredients/{id}`
- `GET/PUT /api/profiles`, `GET /api/profiles/{id}`, `DELETE /api/profiles/{id}`

Deletes are hard deletes. Rows are scoped by the account in the JWT.

## Migrations

From `backend/`: `dotnet tool restore` then `dotnet tool run dotnet-ef migrations add <Name> --project src/SPC.Api --startup-project src/SPC.Api`. The API applies pending migrations on startup.

## Tests

`dotnet test` in `backend/` (Docker required for Testcontainers Postgres).

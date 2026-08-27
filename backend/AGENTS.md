# SPC — Backend Agent Instructions

Instructions for agents working in the **backend** subproject.

## Required reading

1. `../AGENTS.md` — shared monorepo rules
2. `docs/README.md` — backend-specific documentation (this directory)
3. `../docs/README.md` — cross-cutting project documentation
4. `../plans/step10-backend.md` — API + Postgres decisions

Do not read `../frontend/docs/` unless the task involves frontend integration.

## Stack

- **ASP.NET Core** Minimal APIs on **.NET 10**
- **PostgreSQL** via EF Core + Npgsql (JSONB for nested recipe parts)
- **JWT** (HS256, 8h); default login `spc` / `spc` until a later accounts step
- Shares **`SPC.Core`** (`frontend/src/SPC.Core`) as a project reference

## Commands

Run from `backend/`:

| Action | Command |
|--------|---------|
| Restore | `dotnet restore SPC.Api.slnx` |
| Run API | `dotnet watch run --project src/SPC.Api/SPC.Api.csproj` → http://localhost:5100 |
| Test | `dotnet test SPC.Api.slnx` (integration tests need Docker for Testcontainers) |
| Migrations | `dotnet tool restore` then `dotnet tool run dotnet-ef migrations add <Name> --project src/SPC.Api --startup-project src/SPC.Api` |

Needs a local Postgres matching `ConnectionStrings:Default` in `src/SPC.Api/appsettings.json` (or Compose `db`) for `dotnet watch`. Tests spin up their own Postgres.

Published stack from repo root: `docker compose up --build` → UI http://localhost:8080, API via `/api`.

## Conventions

- DTOs live in `SPC.Core`; the API maps EF entities to those types
- Scope every query by `account_id` from the JWT
- Hard deletes
- Do not hash passwords in the frontend; `PasswordHasher` on the server
- JWT signing key and DB password from the environment in Compose (`.env`); dummy `spc` / `spc` is documented on purpose

## Documentation

| Topic | Location |
|-------|----------|
| Backend docs | `docs/README.md` |
| Shared docs | `../docs/README.md` |
| Step 10 plan | `../plans/step10-backend.md` |

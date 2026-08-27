# SPC

Smart Pig's Cookbook — a recipe and portion calculator.

## Run

From the repository root (Docker required):

```bash
cp .env.example .env   # optional; defaults exist in compose
docker compose up --build
```

Open http://localhost:8080

Nginx serves the Blazor app and proxies `/api` to the backend. Postgres stays internal. Log in as **`spc` / `spc`** (placeholder account until a later accounts step).

## Local SDK

- Frontend: `dotnet watch run --project src/SPC.Web/SPC.Web.csproj` from `frontend/` → http://localhost:5180 (talks to the API at http://localhost:5100)
- Backend: Postgres on localhost, then `dotnet watch run --project src/SPC.Api/SPC.Api.csproj` from `backend/` → http://localhost:5100 (CORS for the frontend origin above)

# Plan: Step 6 — Deployments (containerize)

**Date:** 2026-08-26  
**Updated:** 2026-08-26  
**Scope:** both (repo-level compose; frontend image now; backend/DB slots later)  
**Status:** implemented — `docker compose up --build` serves the frontend at http://localhost:8080  
**Depends on:** [step1-init-fe.md](./step1-init-fe.md) (runnable frontend). Independent of cookbook/scaling.  
**Parent:** [thePlan.md](./thePlan.md)

## Goal

Run the published stack with one command from the repo root, for local testing and for deploying the whole app. The compose layout can grow into backend + databases without a redesign.

**Today:** `docker compose up --build` serves the Blazor WASM frontend at http://localhost:8080 (host **8080** → nginx **80** in the container).  
**Later:** the same file gains `backend` and `db` services; the browser still hits one origin.

## Out of scope

- TLS / custom domains / Traefik
- Image registry, CI publish, Kubernetes
- Running Postgres or a backend container in this step
- Changing persistence (localStorage still lives in the browser)
- Stub services or commented-out compose/Dockerfile placeholders

## Why this shape

Blazor WebAssembly **publishes to static files**. The frontend image is therefore **nginx serving `wwwroot`**, not an ASP.NET runtime. That is the right long-term edge too: nginx can later reverse-proxy `/api` to a backend on the compose network so the WASM app talks same-origin. No CORS, no per-environment API URL baked into the image.

Each subproject owns its Dockerfile (`frontend/Dockerfile`, later `backend/Dockerfile`). Root compose only orchestrates.

```
┌─ host :8080 → container :80 ─────────────────────────┐
│  frontend (nginx)                                    │
│    /           → static WASM                         │
│    /api/*      → backend          (not yet)          │
│                                                      │
│  backend (future) ──► db (future, internal only)     │
└──────────────────────────────────────────────────────┘
```

Do **not** add an nginx `/api` `proxy_pass` until the `backend` service exists. Nginx resolves upstream hostnames at start by default; a missing `backend` name can prevent the frontend container from starting.

## Layout

```
SPC/
├── docker-compose.yml
├── frontend/
│   ├── Dockerfile
│   ├── nginx.conf
│   └── .dockerignore
└── backend/                    # future: Dockerfile
```

### Frontend image (multi-stage)

1. **Node** — `npm ci` + `npm run build:instruction-editor` (TipTap bundle is not assumed committed).
2. **.NET 10 SDK** — `dotnet publish` `SPC.Web` (`-c Release`). Final stage is **not** `aspnet`; WASM has no server runtime.
3. **nginx:alpine** — listen **80**; SPA fallback; WASM MIME; `gzip_static` for the `.gz` files `dotnet publish` already emits. Skip brotli in v1 (needs an extra nginx module).

`.dockerignore`: `bin/`, `obj/`, `node_modules/`, tests.

### Compose (today)

Host port **8080** (http://localhost:8080) maps to nginx **80** in the image.

When a backend is added: no public backend port — nginx proxies `/api`. Postgres stays internal, credentials via `.env` (gitignored), named volume for data. Do not add those services until they exist.

## Implementation steps

1. `frontend/.dockerignore`, `frontend/nginx.conf` (SPA `try_files`, `application/wasm`, gzip_static, listen 80).
2. `frontend/Dockerfile` as above.
3. Root `docker-compose.yml` with `frontend` only (`8080:80`).
4. Document in root `README.md`: `docker compose up --build` → http://localhost:8080.

## Acceptance criteria

- From repo root, `docker compose up --build` serves the app at http://localhost:8080
- Blazor client routes (`/recipe/new`, `/library`, …) load via nginx fallback (not only `/`)
- Instruction editor JS is present (built in the image, not missing from wwwroot)
- Adding a future `backend` / `db` service does not require moving the frontend Dockerfile or changing the public port

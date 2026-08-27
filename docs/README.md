# SPC — Documentation

Cross-cutting documentation for the Smart Pig's Cookbook monorepo.

Agents and contributors working in a single subproject should read this folder **plus** that subproject's `docs/` folder — not the other subproject's docs unless the task requires it.

## Contents

<!-- Add documents as the project grows. Suggested topics: -->

| Document | Status | Description |
|----------|--------|-------------|
| [Architecture overview](./architecture.md) | done | Monorepo layout, DTO/repository principles |
| _Development setup_ | done | Root `README.md`: `docker compose up --build` → http://localhost:8080 |
| _API contract_ | done | Login + recipe/ingredient/profile JSON in [backend/docs](../backend/docs/README.md); Blazor uses that API ([step 11](../plans/step11-login-user.md)) |
| _Deployment_ | done | Repo-root Compose + frontend image; see [step 6](../plans/step6-deployments.md) |

## Subproject documentation

- **Frontend:** `frontend/docs/`
- **Backend:** `backend/docs/`

## Agent instructions

- Shared: `../AGENTS.md`
- Frontend: `../frontend/AGENTS.md`
- Backend: `../backend/AGENTS.md`

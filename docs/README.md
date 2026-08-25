# SPC — Documentation

Cross-cutting documentation for the Smart Pig's Cookbook monorepo.

Agents and contributors working in a single subproject should read this folder **plus** that subproject's `docs/` folder — not the other subproject's docs unless the task requires it.

## Contents

<!-- Add documents as the project grows. Suggested topics: -->

| Document | Status | Description |
|----------|--------|-------------|
| [Architecture overview](./architecture.md) | done | Monorepo layout, DTO/repository principles |
| _Development setup_ | partial | See frontend `README.md` below |
| _API contract_ | TBD | How frontend and backend communicate |
| _Deployment_ | TBD | Environments, release process |

## Subproject documentation

- **Frontend:** `frontend/docs/`
- **Backend:** `backend/docs/`

## Agent instructions

- Shared: `../AGENTS.md`
- Frontend: `../frontend/AGENTS.md`
- Backend: `../backend/AGENTS.md`

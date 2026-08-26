# SPC — Agent Instructions

Smart Pig's Cookbook (SPC) is a monorepo with separate **frontend** and **backend** subprojects.

## Repository layout

```
SPC/
├── AGENTS.md          # Shared agent rules (this file)
├── CLAUDE.md          # Claude-specific entry point
├── docker-compose.yml # Stack: frontend now; backend/db later
├── docs/              # Cross-cutting documentation
├── plans/             # Implementation plans (created before larger work)
├── frontend/          # Frontend application
│   ├── AGENTS.md      # Frontend-specific agent rules
│   └── docs/          # Frontend-specific documentation
└── backend/           # Backend application
    ├── AGENTS.md      # Backend-specific agent rules
    └── docs/          # Backend-specific documentation
```

## Scope

- **Working anywhere in the repo:** read this file first.
- **Working in `frontend/`:** also read `frontend/AGENTS.md` and `frontend/docs/`.
- **Working in `backend/`:** also read `backend/AGENTS.md` and `backend/docs/`.
- Do not read the other subproject's `docs/` unless the task explicitly requires cross-cutting integration.

## Shared rules

<!-- Fill in as the project grows -->

- Keep changes focused and minimal; match existing conventions in the area you are editing.
- Prefer extending existing code over introducing parallel patterns.
- Add or update documentation in `docs/` when behavior or architecture changes.
- Save implementation plans for non-trivial work in `plans/` before starting.
- **Frontend persistence:** never read/write storage directly from UI. Use a repository interface in `SPC.Core` with the implementation in `SPC.Web` (localStorage now, HTTP API later). UI and components depend only on the interface and DTOs.
- **Verification:** agents run unit tests (`dotnet test` in the relevant subproject). Do not drive the browser or exercise the UI unless the user asks. The human evaluates user interactions.

## Documentation map

| Topic | Location |
|-------|----------|
| Architecture (monorepo) | `docs/architecture.md` |
| Project overview and cross-cutting concerns | `docs/README.md` |
| Frontend architecture | `frontend/docs/architecture.md` |
| Frontend UI / typography | `frontend/docs/ui.md` |
| Frontend conventions | `frontend/docs/README.md` |
| Backend conventions and architecture | `backend/docs/README.md` |
| Implementation plans | `plans/` |

## Build and test

From the repository root:

| Action | Command |
|--------|---------|
| Run the stack | `docker compose up --build` → http://localhost:8080 |
| Frontend tests | `dotnet test` in `frontend/` (`frontend/AGENTS.md`) |

## Plans

Before substantial features or refactors, create a plan in `plans/` with:

- Goal and scope
- Affected areas (frontend, backend, or both)
- Implementation steps
- Open questions

## Security

<!-- Add shared security boundaries (auth, secrets, data handling) -->

_TBD_

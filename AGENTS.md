# SPC — Agent Instructions

Smart Pig's Cookbook (SPC) is a monorepo with separate **frontend** and **backend** subprojects.

## Repository layout

```
SPC/
├── AGENTS.md          # Shared agent rules (this file)
├── CLAUDE.md          # Claude-specific entry point
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

## Documentation map

| Topic | Location |
|-------|----------|
| Project overview and cross-cutting concerns | `docs/README.md` |
| Frontend conventions and architecture | `frontend/docs/README.md` |
| Backend conventions and architecture | `backend/docs/README.md` |
| Implementation plans | `plans/` |

## Build and test

<!-- Add shared commands once tooling is chosen (e.g. make, task runners, CI scripts) -->

_TBD — document root-level build, test, and lint commands here._

## Plans

Before substantial features or refactors, create a plan in `plans/` with:

- Goal and scope
- Affected areas (frontend, backend, or both)
- Implementation steps
- Open questions

## Security

<!-- Add shared security boundaries (auth, secrets, data handling) -->

_TBD_

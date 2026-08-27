# Note: Tech stack options (not decided)

**Date:** 2026-08-24  
**Status:** draft — for later, after application design  
**Scope:** both

## Context

Stack selection should follow application design. These are candidate options only — not commitments.

## Candidates

| Layer | Options under consideration |
|-------|----------------------------|
| Frontend | Blazor |
| Backend | Go **or** C# |
| SQL database | PostgreSQL |
| NoSQL / cache | Redis (maybe) |

## Next step

Backend language and database are **not** chosen here. When we implement [step 11](./step11-backend.md), discuss which store and schema fit per-user recipes and libraries (shared DB, isolated rows) before scaffolding. Frontend is Blazor WASM (.NET 10), already in use.

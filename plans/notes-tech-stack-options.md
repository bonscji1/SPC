# Note: Tech stack options

**Date:** 2026-08-24  
**Updated:** 2026-08-27  
**Status:** draft — C# API + PostgreSQL + Minimal APIs decided in [step 10](./step10-backend.md)  
**Scope:** both

## Context

Stack selection follows application design. Frontend is already Blazor WASM on .NET 10.

## Recommendations

- **Database:** PostgreSQL only for v1 — accounts, recipes (JSONB for nested lines), per-user ingredient library, profiles. See [step10-backend.md](./step10-backend.md). Redis is not the library store; optional later as a cache (step 9).
- **Ingredient picker speed:** Postgres is the source of truth. Step 11 loads that account’s library into the browser on login/start and searches in memory (write-through on save).
- **Backend language: C# (decided).** Project-reference `SPC.Core`. Go is not in scope.

## Still to confirm when implementing step 10

See the locked decisions in [step10-backend.md](./step10-backend.md).

# Plan: Step 11 — Backend and database

**Date:** 2026-08-27  
**Scope:** both  
**Status:** draft — **do not implement until we discuss database choice and schema**  
**Depends on:** [step10-login-user.md](./step10-login-user.md) (account + Bearer session), [step6-deployments.md](./step6-deployments.md) (compose layout), [step5-save-recipes-and-ingredients.md](./step5-save-recipes-and-ingredients.md) (DTO / repository contracts)  
**Parent:** [thePlan.md](./thePlan.md)

## Goal

Stand up an API and a database so accounts, recipes, and ingredient lists persist off the browser. Multiple people can use the same deployment: they **share the database**, not each other’s data. The frontend authenticates, then calls the API with `Authorization: Bearer <token>`. The backend issues that token and scopes every query to the account in it.

This step is the real store. Step 5’s localStorage stays a prototype until we swap DI to `Api*Repository`.

## Discussion gate (required before coding)

When we decide to implement this step, **stop and discuss** before writing services or migrations:

1. **Which database(s)** fit this use case (see candidates below).
2. **What structure** — tables vs documents, how recipe lines and TipTap instruction JSON are stored, how `user_id` isolation is enforced.
3. **Backend language** — C# (can reference `SPC.Core`) vs Go (Core DTOs are the HTTP contract; map in Web). See [notes-tech-stack-options.md](./notes-tech-stack-options.md). Existing architecture text assumed PostgreSQL; that is a **candidate**, not a lock.

Record the decisions in `backend/docs/` and update `docs/architecture.md` before implementation.

## Out of scope (until we explicitly add them)

- Picking a database in this draft
- Public internet TLS / custom domains (compose stays local; see step 6)
- Nutrition lookup API (step 9) — may later call out from the backend so keys stay off the client
- Sharing recipes between accounts
- Admin UI for user management (seed or a simple create-user path is enough at first)

## Why we need this

localStorage is one browser, ~5 MB, no backup, no second device. Step 10 gives us identity and per-account keys, but the data still dies with the profile. A backend is what makes “many users, one app” true.

## Auth with the API

```
POST /api/auth/login  { username, password }
    → look up account
    → hash(password, stored salt), compare to stored hash
    → return { accessToken, account }

All other /api/* requests
    → Authorization: Bearer <accessToken>
    → validate token, take account id, scope queries
```

- Passwords: unique salt per account, slow hash, never store plaintext. Hashing happens **on the server**.
- Token format (JWT vs opaque server session, expiry, refresh) is part of the discussion, not decided here.
- Frontend: `ApiAuthService` replaces the dummy service; `HttpClient` attaches the Bearer token. Login page from step 10 stays.

Creating further users: seed, a one-off admin path, or a simple register endpoint — decide at implementation. Dummy-only login goes away once real accounts live in the DB.

## Data ownership

One shared database. Every user-owned row carries the account id. A request must never read or write another account’s recipes, library, or profiles.

- **Accounts** — username, password hash, salt, ids
- **Recipes** — per account (families / variants stay as in step 8)
- **Ingredient library** — per account (not a global shared catalog in v1)
- **Person profiles** — per account

A later canonical / curated food catalog can sit beside per-user libraries; do not assume that for the first schema.

Optional one-time **import from localStorage** so prototype cookbooks are not stranded (already suggested in step 5).

## Database discussion (agenda, not a decision)

**What we store:** accounts with credentials; recipes that nest ingredient lines, spices, and instruction steps (including TipTap JSON); a per-user nutrition library (name + kcal/100 g); calorie profiles. Reads are paged (10 / 25 / 50), filtered by name and meal type, grouped by recipe family. Write volume is modest (home cooking, not a social feed). First deploy is Compose on one host; scale is “several people,” then more.

**Questions to answer in the discussion**

- One database vs more than one (e.g. primary store + cache).
- Relational tables vs document collections vs a hybrid (relational metadata + JSON for instructions).
- Isolation: `user_id` on every row (likely enough) vs schema-per-tenant (probably overkill at this size).
- Recipe lines: normalized child tables vs JSON/JSONB on the recipe row. Instructions already have structured tokens plus editor JSON.
- How paging and family grouping map to indexes (`updatedAt`, `familyId`, name).
- Migrations and backups (named volume in Compose at minimum).
- Where secrets live (`.env`, never committed) — DB credentials, token signing key.

**Candidates to compare (add or drop during the discussion)**

- **PostgreSQL** — relational + JSONB; strong paging and `user_id` indexes; already mentioned in older architecture notes. Fits “shared DB, isolated rows” well.
- **SQLite** — simplest operations, one file; fine for a tiny single-server prototype; weaker concurrent writes as user count grows.
- **MySQL / MariaDB** — similar role to Postgres; compare ops familiarity, JSON support, Compose image.
- **Document store (e.g. MongoDB)** — a recipe maps cleanly to one document; auth + later “shared catalog + per-user overlay” joins are clumsier. Worth arguing, not assuming.

Redis or similar is **not** required for v1 unless we have a concrete session/cache need.

Do not add a `db` service to Compose until this discussion has a winner.

## Target architecture (after discussion)

```
Browser (Blazor WASM)
    → nginx :80  (existing frontend image)
        /        → static WASM
        /api/*   → backend (same origin, no CORS)
            → database (internal Compose network only)
```

- Same Core DTOs on the wire as today.
- `ApiRecipeRepository` / `ApiIngredientRepository` / `ApiUserProfileRepository` / `ApiAuthService` in Web; swap DI in `Program.cs`.
- Backend implements those contracts against the chosen store.
- Step 6 already reserved this layout: no public backend port; Postgres-or-other stays internal; do not add nginx `proxy_pass` until the `backend` hostname exists.

## Suggested implementation order (after the gate)

1. Write down DB + schema + language decisions in `backend/docs/`.
2. Scaffold the backend project and Dockerfile; add `backend` + `db` to root Compose; proxy `/api`.
3. Accounts table + login endpoint (salted hash, Bearer token).
4. Recipe / ingredient / profile endpoints, scoped by account, same paging signatures as Core.
5. Swap frontend DI; keep localStorage implementations in the tree until import is done.
6. Tests: hash verify, token rejected when missing/invalid, cannot read another account’s recipe.

## Deliverables

- [ ] Discussion notes: database choice, schema, auth token shape, backend language
- [ ] Runnable backend + database via `docker compose up --build`
- [ ] Login issues a Bearer token; API rejects unauthenticated and cross-account access
- [ ] Recipes, ingredient library, and profiles persist per account
- [ ] Frontend uses `Api*` implementations; dummy auth no longer the source of truth
- [ ] `backend/AGENTS.md` and `backend/docs/` filled in (stack, commands, migrations)

## Acceptance criteria

- Two accounts in one DB see different cookbooks and libraries
- Login with the stored username + password works; wrong password does not issue a token
- Reloading the app (new browser session with a valid token) still loads that account’s data from the API, not only from localStorage
- Compose still serves the UI at http://localhost:8080; `/api` is not a second public origin
- No passwords in git, images, or logs

## Open questions (resolve in the discussion gate)

- Database and schema (the point of this draft)
- C# vs Go for the API
- JWT vs opaque sessions; token lifetime
- Register vs seed-only users in v1
- Import tool from localStorage
- Whether step 9 nutrition calls should go through this API (keys, CORS)

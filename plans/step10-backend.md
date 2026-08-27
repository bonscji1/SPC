# Plan: Step 10 — Backend and database

**Date:** 2026-08-27  
**Scope:** both (API + DB now; Blazor still on localStorage until step 11)  
**Status:** draft — **decisions locked** (see below). Ready to implement when we start this step.  
**Depends on:** [step6-deployments.md](./step6-deployments.md) (compose layout), [step5-save-recipes-and-ingredients.md](./step5-save-recipes-and-ingredients.md) (DTO / repository contracts)  
**Parent:** [thePlan.md](./thePlan.md)  
**Unlocks:** [step11-login-user.md](./step11-login-user.md)

## Goal

Stand up an API and a database so accounts, recipes, and ingredient lists can persist off the browser. Multiple people share **one database**, not each other’s rows.

The backend **issues** a Bearer token on login and **validates** it on every other request. Hashing is server-side (unique salt per account, compare hashes). The Blazor login page and `Api*` repositories are **step 11**. This step is testable with HTTP against **one default account** baked into API seed data (same credentials later in the frontend). A second user and isolation tests wait for a later accounts step.

## Decisions (locked)

- **Language:** C# (ASP.NET Core, .NET 10), project-reference `frontend/src/SPC.Core`
- **Database:** PostgreSQL only; `account_id` on user-owned rows
- **Recipe lines / instructions / notes:** JSONB on the recipe row
- **API style:** **Minimal APIs**, prefix `/api` (no `/v1`), JSON camelCase
- **Tokens:** JWT (HS256, secret in `.env`), 8 hour expiry, no refresh tokens, no session table
- **Password hash:** ASP.NET Core `PasswordHasher` (PBKDF2; salt in the stored payload)
- **Data access:** EF Core + Npgsql, EF migrations
- **Deletes:** hard delete
- **Accounts in this step:** **one default user**, same username/password baked into backend seed **and** frontend (constants or config) until a later accounts step. Not a production secret. Schema still uses `account_id` so extra users later are an add, not a rewrite.
- **Default credentials:** username `spc`, password `spc` (document in `backend/docs/`; change when real users land)
- **Dev listen:** API http://localhost:5100; Compose public port 8080 only; CORS for http://localhost:5180
- **Step 9 nutrition:** not this step

## Out of scope

- Redis, Mongo, or a second database in the first Compose file
- A global shared ingredient catalog (per-user library only; a catalog table can come later)
- Blazor login page, route gate, `AuthenticationStateProvider` — [step 11](./step11-login-user.md)
- Client-side password hashing, dummy tokens, or per-account localStorage keys (do not build those)
- Public internet TLS / custom domains (compose stays local; see step 6)
- Nutrition lookup API (step 9) — may later call out from this API so keys stay off the client
- Sharing recipes between accounts
- Public sign-up and a second user (one baked-in default until a later accounts step)

## Why the frontend stays on localStorage

The UI already works against repository interfaces. Wiring HTTP + a login gate at the same time as scaffolding the API mixes two failure modes. Step 10 proves the server: login issues a JWT, scoped CRUD works for the default account. Step 11 points the browser at that API.

Keep `LocalStorage*Repository` registered in `Program.cs` until step 11.

## Auth on the API (real, not a frontend stub)

```
POST /api/auth/login  { username, password }
    → look up account
    → hash(password, stored salt), compare to stored hash
    → return { accessToken, account }

All other /api/* requests
    → Authorization: Bearer <accessToken>
    → validate token, take account id, scope queries
    → 401 if missing/invalid; never return another account’s rows
```

- Passwords: unique salt per account, slow hash (Argon2id preferred), never store or log plaintext. Hashing happens **only on the server**.
- The client (later) sends username + password over HTTPS. It must not send a precomputed hash as the credential.

### Seeded user (one default)

Seed **one** account: username `spc`, password `spc`. Hash it with `PasswordHasher` in the database. Put the same plaintext pair in frontend config/constants in step 11 (documented dummy, not a production secret). A second user, sign-up, and cross-account isolation tests are a **later step**.

Still scope every query by `account_id` from the JWT so adding users later does not rewrite the data layer.

## Data ownership

Every user-owned row carries the account id. A request must never read or write another account’s recipes, library, or profiles.

- **Accounts** — one default login; `password_hash` is PasswordHasher output (salt inside that value)
- **Recipes** — per account (families / variants stay as in step 8)
- **Ingredient library** — per account (not a global shared catalog in v1). Postgres is the source of truth. Step 11 loads that account’s list into the browser on login/start so the name picker stays in-memory-fast; writes go back to the API.
- **Person profiles** — per account

A later canonical food catalog can sit beside per-user libraries; do not assume that for the first schema.

Optional **import from localStorage** can wait for step 11 (when the UI can attach a token). Document the expected JSON shape here if useful.

## How frontend data maps (why one Postgres)

Today each collection is one JSON blob in localStorage. The API should keep the **same DTOs** and the same query shapes, not invent a second model.

**Recipes** (`spc.recipes.v1` → `RecipeDto`) — one saved row is already a document: name, `FamilyId`, `VariantLabel`, `MealType`, `UpdatedAt`, `ActualDishWeightG`, plus nested lists (`Ingredients`, `Spices`, `Instructions`) and `Notes`. Instruction steps carry TipTap `EditorJson`. Recipe lines are **copy-on-use**: they store name + grams + kcal, with **no foreign key** to the library. Chips in instructions point at **line ids**, so those ids must stay stable.

Home does not page raw recipes. It pages **families**: group by `FamilyId`, newest family activity first, name/variant-label contains, meal type if **any** member matches. Page sizes 10 / 25 / 50. Load one family with `GetByFamilyIdAsync`. Save/delete one variant or delete the whole family.

**Ingredient library** (`spc.ingredients.v1` → `IngredientDto`) — tiny durable rows: `Id`, `Name`, `CaloriesPer100g`. Per account. `SearchAsync` is typeahead (cap 8); `GetPageAsync` is contains + name order. Adding a name that already exists **updates** that food. Delete does not rewrite recipes.

**Person profiles** (`spc.profiles.v1` → `UserProfileDto`) — a handful of small records per account (body stats + meal-split percents). Not a login identity.

**Accounts** — one default login (`spc` / `spc`) seeded in the API. Frontend uses the same pair until a later accounts step. Table still keyed so more users can be added later.

Write volume is household-scale. The hot path is “open this recipe” and “type three letters in the name picker,” not a social feed.

## Store recommendation (v1): PostgreSQL only

**Use one PostgreSQL database for accounts, recipes, the ingredient library, and profiles.** Users share that instance; every user-owned row has `account_id`.

Postgres fits both shapes at once:

- **Query columns** where we filter and page (`account_id`, `family_id`, `name`, `variant_label`, `meal_type`, `updated_at`).
- **JSONB** where the frontend already stores a nested blob (ingredient/spice lines, instruction tokens + `editorJson`, notes, optional meal-split).

That is the honest mapping of localStorage: one recipe save is still one row write. `pg_trgm` (or `ILIKE`) covers library typeahead and recipe name contains. Unique `(account_id, lower(name))` on the library matches “same name updates that food.” Family paging is a SQL query (distinct `family_id` ordered by `max(updated_at)`, then fetch members) instead of loading every recipe into memory.

C# or Go both talk to Postgres fine. Language stays a separate confirm.

### Why not Redis for the ingredient library

The library feels like a “lookup cache,” but it is a **system of record**: per-user rows that must survive restart, unique names, contains search, paging, and (on recipe save) an optional update in the same user action. Redis is a cache and ephemeral KV store. Using it as the library would mean:

- Persistence and backup are a second story (AOF/RDB), worse than Postgres for this data.
- Search is SCAN or RediSearch — heavier than `ILIKE` / trigram on a few hundred foods per user.
- No shared transaction with “save recipe and maybe update library.”
- Restart, failover, and Compose ops for a dataset that is smaller than a single Postgres table.

A per-account library of name + kcal is **not** a cache. Cache it later if a **global** catalog (step 9) is huge; do not put the user’s own foods only in Redis.

### Why not a key-value NoSQL store “for speed”

KV stores (Redis, DynamoDB, etc.) are fast at **GET exact key**. The ingredient picker is not that. `SearchAsync` ranks by normalized name: exact, prefix, “query starts with the food name” (`onions` → `onion`), word prefix; omits names already on the row list; returns 8. The library page is **contains** + sort by name + page. That is search over `account_id` + name, not `GET ingredients:{id}`.

To do that in a KV store you either scan every food for the user or add secondary indexes — at which point you have built a weaker database.

At this size the engines are in the same league. A per-user library is hundreds to a few thousand rows. Postgres `WHERE account_id = $1 AND name ILIKE … LIMIT 8` with a trigram (or even a btree on `lower(name)`) is typically **under a few milliseconds**. After the API exists, **HTTP + debounce (200–250 ms)** dominate. A second store cannot restore today’s in-memory localStorage speed; no network round trip can.

If the picker must stay instant after login, keep a **small in-memory (or local) copy of that account’s library in the browser**, hydrate once, search locally, write-through to Postgres. That is a client cache, not another database.

Do not add Dynamo/Cassandra/Mongo just for this list.

### When Redis *would* be worth adding (not v1)

- Cache of external nutrition API responses (step 9), keyed by search string, with TTL.
- Optional cache in front of a large **shared** catalog — still with Postgres (or similar) as the source of truth.
- Token blacklist / short-lived session helper if we pick opaque tokens and need fast revoke.

Do not add a Redis Compose service in step 10.

### Other stores (weaker for v1)

- **SQLite** — simplest single-file ops; weaker concurrent writes once several people share one deploy. Fine for a laptop spike, not the Compose target.
- **MySQL / MariaDB** — same job as Postgres; JSON and `ILIKE`/trigram are more natural in Postgres given nested instructions.
- **MongoDB** — a `RecipeDto` maps to one document, but accounts, unique library names, and family paging are ordinary relational work. A second database without a second problem.
- **Postgres + Redis split** — two systems, two backups, no cross-store transaction, for a lookup table Postgres already does well.

## Suggested Postgres schema (confirm at implementation)

Isolation: `account_id` on every user-owned row (not schema-per-tenant).

```
accounts
  id, username (unique, case-insensitive), password_hash, created_at
  -- password_hash is ASP.NET PasswordHasher output (algorithm + salt + hash in one value)

recipes
  id, account_id, family_id, variant_label, name, meal_type, updated_at,
  actual_dish_weight_g,
  ingredients jsonb,   -- RecipeIngredientDto[]
  spices jsonb,        -- SpiceDto[]
  instructions jsonb,  -- InstructionStepDto[] (tokens + editorJson)
  notes jsonb          -- InstructionStepDto
  indexes: (account_id, family_id), (account_id, updated_at desc),
           trigram or ilike on name + variant_label

ingredients          -- library, not recipe lines
  id, account_id, name, calories_per_100g
  unique (account_id, lower(name))
  trigram or ilike on name

profiles
  id, account_id, name, sex, weight_kg, height_cm, age_years,
  activity_level, custom_pal, meal_split jsonb (or four percent columns)
```

**JSONB for recipe lines, not child tables, in v1.** The editor always loads a full `RecipeDto`. Lines are not referenced by other recipes. Instruction JSON is already JSON. Normalized `recipe_ingredients` tables help later “find dishes I can cook from this list of foods”; that search is still in `frontend/docs/future-improvements.md`. Extract columns if that feature lands.

**Family list query:** page `family_id` values for this account (filter name/variant/meal on members, order by `max(updated_at)`), then `SELECT` every recipe in those families. Do not `LIMIT` individual recipe rows or a family will split across pages.

**Recipe list:** do not expose `GetAllAsync` as an unbounded dump. `GetPageAsync` / `GetByIdAsync` / `GetByFamilyIdAsync` are enough.

**Ingredient library:** `GetAllAsync` for that account **is** the hydrate endpoint for step 11 (payload is small: id, name, kcal). Also keep `SearchAsync` / `GetPageAsync` on the API for clients that do not cache and for tests. The typeahead in Blazor should not hit the network after hydrate.

## Backend language (C#)

`SPC.Core` is the contract. The API project-references `frontend/src/SPC.Core`. Same .NET 10 SDK as the frontend. Minimal APIs + EF Core + JWT.

## Target architecture (after confirmation)

```
Browser (Blazor WASM) — still localStorage this step
    → nginx :80  (existing frontend image)
        /        → static WASM
        /api/*   → backend (same origin when using Compose)
            → PostgreSQL (internal Compose network only)

Host SDK (dotnet watch :5180) — API is a different origin
    → backend must allow local CORS, or document a local proxy
    → do not assume “no CORS” except behind nginx
```

- Same Core DTOs on the wire as today (C# backend can reference `SPC.Core`).
- Backend implements recipe / ingredient / profile / auth as **Minimal APIs** against PostgreSQL.
- Step 6 already reserved this layout: no public backend port in Compose; Postgres stays internal; do not add nginx `proxy_pass` until the `backend` hostname exists.
- **Local `dotnet watch`:** publish the API base URL (and CORS for `http://localhost:5180`) in `backend/docs/`. Compose users hit `/api` on http://localhost:8080.

## Suggested implementation order (after the gate)

1. Scaffold the backend project (Minimal APIs) and Dockerfile; add `backend` + `db` (Postgres) to root Compose; proxy `/api`.
2. Accounts table + `POST /api/auth/login` (PasswordHasher, JWT). Seed default user `spc` / `spc`.
3. Recipe / ingredient / profile endpoints, scoped by `account_id` from the JWT; hard deletes.
4. Tests: login succeeds for the default user, fails for a wrong password, 401 without token. (Two-user isolation is a later step.)
5. Document Compose vs `dotnet watch`. Frontend DI stays on localStorage.

## Deliverables

- [ ] Decisions recorded in `backend/docs/` (C#, Postgres, Minimal APIs, JWT, one default user)
- [ ] Runnable backend + database via `docker compose up --build`
- [ ] Login issues a JWT for `spc` / `spc`; other routes reject missing/invalid tokens
- [ ] Recipes, ingredient library, and profiles persist for that account (hard delete)
- [ ] CORS / base-URL notes for SDK development
- [ ] `backend/AGENTS.md` and `backend/docs/` filled in (stack, commands, migrations)

## Acceptance criteria

- Login with `spc` / `spc` returns a JWT; wrong password does not
- Unauthenticated requests to data routes are 401
- Data for the default account survives restart (Postgres volume)
- Compose still serves the UI at http://localhost:8080; `/api` is not a second public origin
- The Blazor app still runs on localStorage (step 11 switches it)
- JWT signing key and DB password are not committed; the dummy `spc` / `spc` pair is documented on purpose
- Two-user isolation is **not** required in this step

## Open questions

- None blocking implementation. A later step replaces the baked-in `spc` user with real accounts.

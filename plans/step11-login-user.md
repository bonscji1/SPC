# Plan: Step 11 — Login user (frontend identity)

**Date:** 2026-08-27  
**Scope:** frontend (calls the step 10 API; no fake local auth)  
**Status:** **done**  
**Depends on:** [step10-backend.md](./step10-backend.md)  
**Parent:** [thePlan.md](./thePlan.md)

## Goal

Wire the Blazor app to the real API: the user logs in with username + password, the **backend** returns a Bearer token, and every repository call sends `Authorization: Bearer <token>`. Recipes, ingredient library, and person profiles then come from that account’s rows in the shared database.

This step is the identity **shell** in the UI. It does not invent a second auth system in the browser.

## Out of scope (do not build)

- Client-side password hashing or salt storage
- Dummy / local opaque tokens
- Per-account localStorage keys (`spc.recipes.v1.{accountId}` and similar)
- A local `DummyAuthService` that “looks like” the API
- Public sign-up (one baked-in default user until a later accounts step)
- OAuth, email verification, password reset, refresh-token rotation
- Sharing a recipe or library across accounts
- Changing calorie math or cookbook UX
- Import from the old localStorage prototype (those stores were removed)

## Account vs person profile (do not mix)

**Account (login user)** — who is signed in. Username + password checked by the API. Owns recipes, ingredient library, and calorie profiles.

**Person profile (step 4)** — a body in the household used for BMR / TDEE / meal kcal. One account can have several. Not a login. Already exists as `UserProfileDto` / `IUserProfileRepository`.

Do **not** reuse `UserProfileDto` as the login identity. Keep a separate account/session type.

## Auth model (API is the source of truth)

```
Login page
    → IAuthService.Login(username, password)
        → POST /api/auth/login  { username, password }   // plaintext over HTTPS; server hashes
        → AuthSession { Account, AccessToken }
    → HttpClient sends Authorization: Bearer <AccessToken> on all /api calls
    → Api*Repository / CachedIngredientRepository implement existing interfaces
```

On logout: drop the token (and `sessionStorage` key `spc.auth.v1`), notify Blazor auth state, and **clear in-memory UI state**.

### Session types

- `AccountDto` — public identity (`Id`, `Username`); no secrets
- `AuthSession` — current account + access token (what HttpClient reads)
- `IAuthService` — `LoginAsync`, `LogoutAsync`, `RestoreAsync`; does not own recipe/profile data

Keep session separate from the HTTP handler so repositories do not depend on the full login service (avoids DI cycles).

## Blazor gate

- `AuthorizeRouteView` plus `[Authorize]` on pages (via `Pages/_Imports.razor`); `/login` is `[AllowAnonymous]`.
- Unauthenticated visits redirect to `/login` and return to the requested page after login when the URL is a safe relative path
- Token lives in **sessionStorage** (`spc.auth.v1`) so a tab refresh stays signed in until the tab closes or the JWT expires (8h)

Nav: show username + **Log out**. Keep the step 4 **profile** menu as calorie profiles (different concept).

## API base URL (Compose vs `dotnet watch`)

- **Compose** (http://localhost:8080): empty `ApiBaseUrl` → same origin; nginx proxies `/api` to the backend
- **SDK:** `wwwroot/appsettings.Development.json` sets `ApiBaseUrl` to `http://localhost:5100/`. CORS on the API allows http://localhost:5180

## Logout must not leak state

- **`RecipeDraftService` is a singleton.** Reset on login and logout.
- **`ActiveProfileService`** is in-memory only (no `spc.activeProfileId.v1`). Clear on logout; refresh from the API on login / restore.
- **Ingredient library cache.** Clear on logout. Hydrate from `GET /api/ingredients` on login / restore. Search stays in-memory.

## Persistence swap

```
UI → IRecipeRepository / IIngredientRepository / IUserProfileRepository
        → ApiRecipeRepository / CachedIngredientRepository / ApiUserProfileRepository
            → HTTP + Bearer → step 10 API → PostgreSQL
```

LocalStorage recipe, ingredient, and profile repositories were **removed**. The API is the store.

Pages keep talking to the same repository interfaces. The token is attached in one `HttpClient` handler.

### Ingredient library: Postgres truth, in-memory picker

After login (and when a stored session is restored), `GET` that account’s library once. `SearchAsync` / the library page filter run on that cache with existing `IngredientLibrary` / `IngredientList` helpers. Save/delete write through to the API, then update the cache.

Recipes stay paged from the API; do not download the whole cookbook on start.

## Deliverables

- [x] Account/session types; `IAuthService` that calls `POST /api/auth/login` only
- [x] `AuthenticationStateProvider` + `AuthorizeRouteView`; `/login` page (username + password, existing field/card styles)
- [x] `HttpClient` with Bearer token and a configurable API base URL
- [x] Ingredient library hydrated into memory on login/start; picker stays local; writes go to the API; cache cleared on logout
- [x] Logout clears token, draft, active profile, and the ingredient cache
- [x] Architecture docs: account vs profile; Compose vs `dotnet watch`
- [x] Removed localStorage repositories for recipes, ingredients, and profiles (no import)

## Acceptance criteria

- Seeded default user `spc` / `spc` logs in (same pair as the API seed)
- Wrong password does not sign in; no extra accounts in this step
- Reloading a tab with a still-valid token stays signed in (`sessionStorage`); a new tab starts signed out
- Unauthenticated visits to `/`, `/library`, `/recipe/…` end on `/login`
- `dotnet watch` can log in against a locally running API using `ApiBaseUrl`; Compose still works at http://localhost:8080
- Name picker after login does not wait on the network per keystroke (library already in memory); a new food saved on the server appears in the cache after write-through
- Logout still clears draft, profiles, and the ingredient cache

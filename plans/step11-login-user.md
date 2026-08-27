# Plan: Step 11 — Login user (frontend identity)

**Date:** 2026-08-27  
**Scope:** frontend (calls the step 10 API; no fake local auth)  
**Status:** draft  
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

## Account vs person profile (do not mix)

**Account (login user)** — who is signed in. Username + password checked by the API. Owns recipes, ingredient library, and calorie profiles.

**Person profile (step 4)** — a body in the household used for BMR / TDEE / meal kcal. One account can have several. Not a login. Already exists as `UserProfileDto` / `IUserProfileRepository`.

Do **not** reuse `UserProfileDto` as the login identity. Keep a separate account/session type.

## Auth model (API is the source of truth)

```
Login page
    → IAuthService.Login(username, password)
        → POST /api/auth/login  { username, password }   // plaintext over HTTPS; server hashes
        → AuthSession { AccountId, Username, AccessToken }
    → HttpClient sends Authorization: Bearer <AccessToken> on all /api calls
    → Api*Repository implements existing IRecipeRepository / IIngredientRepository / IUserProfileRepository
```

On logout: drop the token, notify Blazor auth state, and **clear in-memory UI state** (see below).

### Session types (names can shift)

- `AccountDto` — public identity (`Id`, `Username`); no secrets
- `AuthSession` / `IAuthSession` — current account + access token (what HttpClient reads)
- `IAuthService` — `LoginAsync`, `LogoutAsync`; does not own recipe/profile data

Keep session separate from the HTTP handler so repositories do not depend on the full login service (avoids DI cycles).

## Blazor gate (use the framework)

Today `App.razor` is a plain `RouteView` with no auth. Do **not** roll a one-off “if not logged in, hide the app” beside the router.

- Implement `AuthenticationStateProvider` from `IAuthSession`
- Use `AuthorizeRouteView` (or equivalent) so `/login` is anonymous and the rest of the app requires an authenticated user
- Redirect unauthenticated users to `/login`; after login, return to the page they wanted if that is cheap

Nav: show username + **Log out**. Keep the step 4 **profile** menu as calorie profiles (different concept).

## API base URL (Compose vs `dotnet watch`)

Step 10’s “same origin, no CORS” is true **only** behind nginx at http://localhost:8080.

`dotnet watch` serves WASM at http://localhost:5180. The API is another origin. Step 11 must:

- Configure `HttpClient` `BaseAddress` from configuration (empty/relative `/api` in Compose; explicit API URL in SDK dev)
- Use the CORS policy documented in step 10, or a local proxy — do not hard-code “always same origin”

## Logout must not leak state

These live across a login today and would show the previous person’s data if ignored:

- **`RecipeDraftService` is a singleton.** On logout (and on login), reset the in-memory recipe and portion session. Otherwise the next account sees the last draft.
- **`ActiveProfileService`** holds `All` / `Active` in memory and writes `spc.activeProfileId.v1` with **no account scope**. On logout: clear active profile, `All`, and that localStorage key (or stop persisting it once profiles come from the API). On login: `RefreshAsync` from `IUserProfileRepository` so the menu lists this account’s profiles only.
- **Ingredient library cache.** On logout: drop it. On login / restored session: hydrate from the API once.

Do not leave a previous token on `HttpClient` after logout.

## Persistence swap

```
UI → IRecipeRepository / IIngredientRepository / IUserProfileRepository
        → LocalStorage*Repository     (until this step)
        → Api*Repository              (this step: HTTP + Bearer → step 10 API)
```

Swap DI in `Program.cs`. Keep localStorage implementations in the tree until a one-time **import** (optional) has copied prototype recipes/library/profiles into the signed-in account. Do not rename localStorage keys per user — the API is the store.

Pages keep talking to the same repository interfaces. Attach the token in one `HttpClient` handler, not in each `.razor` file.

### Ingredient library: Postgres truth, in-memory picker

Do **not** debounce or round-trip on every keystroke. After login (and when a stored session is restored), `GET` that account’s library once (`GetAllAsync` — id, name, kcal; small). Keep it in a Web memory cache. `SearchAsync` / the library page filter run on that cache with existing `IngredientLibrary` / `IngredientList` helpers (same as today’s localStorage path).

Save/delete (library page or recipe-save sync) **write through** to the API, then update the cache. Logout **clears** the cache with the draft and profiles.

Recipes stay paged from the API; do not download the whole cookbook on start.

## Deliverables

- [ ] Account/session types; `IAuthService` that calls `POST /api/auth/login` only
- [ ] `AuthenticationStateProvider` + `AuthorizeRouteView`; `/login` page (username + password, existing field/card styles)
- [ ] `HttpClient` with Bearer token and a configurable API base URL
- [ ] Ingredient library hydrated into memory on login/start; picker stays local; writes go to the API; cache cleared on logout
- [ ] Logout clears token, draft, active profile, `spc.activeProfileId.v1`, and the ingredient cache
- [ ] Optional import from existing localStorage into the logged-in account
- [ ] Architecture docs: account vs profile; Compose vs `dotnet watch`

## Acceptance criteria

- Seeded default user `spc` / `spc` logs in (same pair as the API seed)
- Wrong password does not sign in; no extra accounts in this step
- Reloading a tab with a still-valid token stays signed in (or signed out if the token is gone — pick one store and document it; prefer `sessionStorage` for the token)
- Unauthenticated visits to `/`, `/library`, `/recipe/…` end on `/login`
- `dotnet watch` can log in against a locally running API using the documented base URL; Compose still works at http://localhost:8080
- Name picker after login does not wait on the network per keystroke (library already in memory); a new food saved on the server appears in the cache after write-through
- Logout still clears draft, profiles, and the ingredient cache (needed when the default user is replaced later)

## Open questions

- Token persistence: `sessionStorage` (tab) vs `localStorage` (remember me) — prefer sessionStorage unless we explicitly want remember-me
- After login, land on Home vs the originally requested URL
- Whether import from localStorage is in this step or a small follow-up

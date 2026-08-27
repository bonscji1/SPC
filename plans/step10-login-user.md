# Plan: Step 10 — Login user (account identity)

**Date:** 2026-08-27  
**Scope:** frontend (dummy account now; same contracts for the backend later)  
**Status:** draft  
**Depends on:** [step5-save-recipes-and-ingredients.md](./step5-save-recipes-and-ingredients.md)  
**Parent:** [thePlan.md](./thePlan.md)  
**Unlocks:** [step11-backend.md](./step11-backend.md)

## Goal

Introduce a **login account** as the owner of data, so the app can grow to many people sharing one deployment without mixing cookbooks.

Until the backend exists, there is **one dummy default user**. The UI, session, password check, and per-user storage shape should already match what the API will do: log in with username + password, hold a bearer token, and read/write only that account’s recipes and ingredient list.

## Why this before the backend

Today every recipe, library food, and calorie profile lives in unscoped localStorage (`spc.recipes.v1`, `spc.ingredients.v1`, `spc.profiles.v1`). That is one anonymous blob per browser.

When the API arrives we will authenticate, send `Authorization: Bearer <token>`, and the server will scope every query to the account in the token. If the frontend still treats data as global, we would have to retrofit identity, storage keys, and repository calls at the same time as HTTP.

This step puts the identity boundary in place while storage is still local.

## Account vs person profile (do not mix)

**Account (login user)** — who is signed in. Username + password. Owns recipes, ingredient library, and calorie profiles. **This step.**

**Person profile (step 4)** — a body in the household used for BMR / TDEE / meal kcal. One account can have several. Not a login. Already exists as `UserProfileDto` / `IUserProfileRepository`.

Do **not** reuse `UserProfileDto` as the login identity. Keep a separate account/session type.

## Out of scope

- Backend, database, real JWT issuance — [step 11](./step11-backend.md)
- Public sign-up / “create account” in the UI (dummy only until the API can store users)
- OAuth, email verification, password reset, refresh-token rotation
- Sharing a recipe or library across accounts
- Changing calorie math or cookbook UX

## Dummy default user

Seed **one** well-known account used until step 11:

- Documented username and password (in the plan / local README, not a secret)
- Password stored only as **salt + hash**; login compares hashes
- On first run, migrate existing unscoped localStorage into this account’s buckets so prototype data is not stranded

Optional convenience (decide at implementation): auto-establish a session as the dummy user after a successful hash check vs requiring the login page every visit. Either way the login path must work.

## Auth model

```
Login page
    → IAuthService.Login(username, password)
        → look up account
        → hash(password, stored salt) and compare to stored hash
        → on match: AuthSession { AccountId, Username, AccessToken }
            → repositories read/write only that AccountId
```

After login, every API-bound call (later) sends:

```
Authorization: Bearer <AccessToken>
```

Until step 11 the token is a **local opaque value** minted by the dummy auth service (not a real signed JWT). Repositories still do not talk HTTP. The session object already has a token field so `Api*Repository` can attach it later without changing UI.

### Passwords

- Unique **salt per account**
- Slow hash (Argon2id preferred; PBKDF2 is acceptable in .NET / WASM if Argon2 is awkward)
- Never store or log plaintext
- UI and DTOs that leave Core must not include salt or hash

**Where hashing runs**

- **This step (dummy, local):** `SPC.Core` hasher, used by the local auth implementation — so tests can cover salt + compare.
- **Step 11 (real API):** **server only.** The browser sends username + password over HTTPS. The client must not send a precomputed hash as the credential (that would make the hash the password).

WASM hashing is **not** a security boundary (the client is fully visible). It exists so the stored shape and compare path are real before the API exists.

## Per-user data

Users share the **database** later, not each other’s rows.

- **Recipes** (and families / variants) — per account
- **Ingredient library** — per account
- **Person profiles** (step 4) — per account (personal calorie data; same scoping even though this step is “login”)

LocalStorage keys become account-scoped, e.g. `spc.recipes.v1.{accountId}` (same idea for ingredients and profiles). Repositories take the current account from the session; UI still talks only to `IRecipeRepository` / `IIngredientRepository` / `IUserProfileRepository`.

Migration: if unscoped `spc.recipes.v1` (etc.) exists and the dummy account has no data yet, copy into the dummy keys and leave a note in frontend architecture. Do not silently merge two accounts later.

## Architecture (this step)

```
UI (login gate + existing pages)
    → IAuthService / AuthSession
        → DummyAuthService (seeded account, local token)
    → IRecipeRepository, IIngredientRepository, IUserProfileRepository
        → LocalStorage*Repository keyed by AuthSession.AccountId
```

Step 11 swaps `DummyAuthService` → `ApiAuthService` (login returns a server token) and `LocalStorage*Repository` → `Api*Repository` (HTTP + Bearer). Same DTOs and pages.

Suggested Core types (names can shift at implementation):

- `AccountDto` — public identity (`Id`, `Username`); no secrets
- `AuthSession` — `AccountId`, `Username`, `AccessToken`
- `IPasswordHasher` — generate salt, hash, verify
- `IAuthService` — `LoginAsync`, `Logout`, current session

Web:

- `/login` page (username + password, existing field/card styles)
- Route gate: signed-out users only see login; signed-in users see the current app
- Nav: show username + log out (keep the step 4 **profile** menu as calorie profiles)

## Deliverables

- [ ] Account/session types and `IAuthService` in Core; dummy implementation in Web
- [ ] Salted password hash + verify in Core, with unit tests
- [ ] Seeded dummy account; documented credentials
- [ ] Login page and logout; session survives refresh (e.g. sessionStorage for the token, not the password)
- [ ] Repositories scoped to `AccountId`; migrate unscoped localStorage into the dummy account
- [ ] Architecture docs: account vs profile; Bearer as the future API header

## Acceptance criteria

- A wrong password does not sign in; the dummy username + password does
- After login, Home / Library / profiles show only that account’s data
- Reloading the tab keeps the session until logout (or until the tab session ends, if that is the chosen store)
- Logout clears the session; the next visitor does not see the previous cookbook
- No plaintext password in localStorage, sessionStorage, or Core DTOs used by UI
- Existing prototype recipes/library/profiles still appear under the dummy account after migration
- UI and repository **interfaces** do not assume a single global user; adding a second account later is a storage/API change, not a rewrite of pages

## Open questions

- Require the login page every visit vs auto-session as the dummy user during the frontend-only phase
- Token persistence: `sessionStorage` (tab) vs `localStorage` (remember me) — prefer sessionStorage until step 11
- Display name separate from username — not needed for dummy
- Sign-up UI — wait for step 11 unless we want a second local dummy for testing isolation

# Plan: Step 12 — Sign up (self-serve accounts)

**Date:** 2026-08-27  
**Scope:** both (API + Blazor; schema already has `account_id`)  
**Status:** **done**  
**Depends on:** [step11-login-user.md](./step11-login-user.md)  
**Parent:** [thePlan.md](./thePlan.md)

## Goal

People create their own login. There is **no baked-in `spc` / `spc` account**. Sign-up writes an `accounts` row with a **salted password hash**. Login looks up the username and **compares hashes** (never stores or sends a hash from the browser). Each account’s recipes, ingredient library, and calorie profiles are scoped by `account_id` on the JWT.

## What “extra users” means

Not extra calorie **profiles** (step 4). An extra **user** is another login identity: another row in `accounts`, with its own cookbook data.

## Out of scope

- Email, OAuth, password reset, email verification
- Client-side password hashing
- Admin / invite-only registration
- Changing JWT expiry or adding refresh tokens
- Deleting or renaming accounts

## Auth model

```
Sign-up page (anonymous)
    → IAuthService.SignUpAsync(username, password)
        → POST /api/auth/signup  { username, password }   // plaintext over HTTPS
        → server: PasswordHasher (PBKDF2 + unique salt inside the stored hash)
        → 200 { accessToken, account }
        → 409 if the username is taken (case-insensitive)
        → 400 if username/password are invalid

Login
    → POST /api/auth/login  { username, password }
        → server: load hash, VerifyHashedPassword (salt + hash compare)
        → 200 { accessToken, account } or 401
```

Username: trim, 1–128 characters, unique on `NormalizedUsername` (lowercased). Password: non-empty; not trimmed.

## UI

- `/signup` `[AllowAnonymous]`, same card/field styles as login, `LoginLayout`
- Confirm password in the browser only
- After a successful sign-up, land in the app (auto-login)
- Login page links to sign-up; sign-up links to login

## Deliverables

- [x] `POST /api/auth/signup`; conflict on duplicate username; hash+salt on the server
- [x] `/signup` page + `IAuthService.SignUpAsync`
- [x] No seeded default account
- [x] Isolation test: one signed-up account does not see another’s recipes
- [x] Docs

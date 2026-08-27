# Plan: Step 7 — Cookbook

**Date:** 2026-08-24  
**Updated:** 2026-08-27  
**Scope:** frontend  
**Status:** **done** — Home is the cookbook (list, name/type filters, open editor, delete). No extra route or read-only page.  
**Depends on:** [step5-save-recipes-and-ingredients.md](./step5-save-recipes-and-ingredients.md)  
**Parent:** [thePlan.md](./thePlan.md)

## Goal

Browse saved recipes, filter them, open one to keep editing or run portion math.

Shipped on **Home** (`/`) during step 5; this step closes the leftover plan items without a second surface.

## What shipped

- [x] Saved-recipe list: name, meal type, ingredient (and spice) count, last updated
- [x] Filter by recipe name (contains) and meal type; paging 10 / 25 / 50
- [x] Empty states (none saved / no matches)
- [x] Open a row → recipe editor (`/recipe/{id}`); portion summary from steps 3–4 is already there
- [x] Delete with confirm (home and editor)
- [x] Home is the default route for returning users

## Dropped (not building)

- Dedicated `/cookbook` route — Home is enough
- Read-only recipe detail page — the editor form is good enough
- Tags / extra sort modes — keep newest `updatedAt` first, then name

## Deferred

Search by a **list of ingredients** (not a single name contains): see `frontend/docs/future-improvements.md`.

## Acceptance criteria

- User can find and open any saved recipe in ≤ 2 clicks from Home
- Portion flow from steps 3–4 works from a Home-selected recipe without re-entry

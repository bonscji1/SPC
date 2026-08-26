# Plan: Step 7 — Cookbook

**Date:** 2026-08-24  
**Updated:** 2026-08-26  
**Scope:** frontend (+ backend if step 5 uses one)  
**Status:** draft  
**Depends on:** [step5-save-recipes-and-ingredients.md](./step5-save-recipes-and-ingredients.md)  
**Parent:** [thePlan.md](./thePlan.md)

## Goal

A **cookbook** view: list saved recipes, search/filter, open one to view details or jump into portion calculation.

## Out of scope

- Recipe scaling / what-if (step 8)
- Sharing recipes with other users
- Print/export (nice-to-have later)

## Deliverables

- [ ] Cookbook page listing all saved recipes (name, ingredient count, optional last used) — home already has a paged list + delete; this step adds search, filters, and a dedicated view
- [ ] Search by recipe or ingredient name
- [ ] Open recipe → detail view (read-only summary)
- [ ] Actions: edit, calculate portions, delete (with confirm)
- [ ] Empty state when no recipes saved

## UX notes

- This is the “home” for returning users — consider making it the default route after step 7.
- Quick path: cookbook → select recipe → meal/profile → see portions.

## Acceptance criteria

- User can find and open any saved recipe in ≤ 2 clicks from cookbook
- Portion flow from step 3–4 works from a cookbook-selected recipe without re-entry

## Open questions

- Sort order: alphabetical, recently used, recently created?
- Tags / categories (e.g. soup, batch prep) — defer unless cheap?

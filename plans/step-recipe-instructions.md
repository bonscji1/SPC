# Plan: Recipe instructions with ingredient chips

**Date:** 2026-08-26  
**Updated:** 2026-08-26  
**Scope:** frontend  
**Status:** implemented — TipTap editor, chip links bound to ingredient/spice ids  
**Depends on:** [step2-create-recipe.md](./step2-create-recipe.md)  
**Parent:** [thePlan.md](./thePlan.md)

## Goal

Ordered cooking steps on a recipe. Mentions of ingredients/spices are **chips bound to row ids** (not `#name` text). Hover on a chip shows the amount on the recipe list.

## Model

Each step stores:

- **Tokens** — plain text, or a link `{ kind: Ingredient | Spice, itemId }` (synced from editor for persistence and chip resolution).
- **EditorJson** — TipTap document JSON per step (formatting + mention nodes).

Display name and grams for chips always come from the current ingredient/spice row, so renaming updates labels.

## UI (current)

- **Instructions** — full-width card below the recipe/summary grid (not in the narrow left column).
- Each step: TipTap rich-text editor (bold, italic, bullet lists) via JS interop (`wwwroot/js/instruction-editor/`).
- Type `#` → picker of current ingredients and spices; arrow keys, Enter, and mouse all work.
- **+ Add step**; remove confirms if the step has content.
- **Summary panel** does **not** show instructions (portion/calorie focus only).

## Persistence

Saved with the recipe in localStorage (`spc.recipes.v1`) as part of `RecipeDto.Instructions`. Rebuild JS after editing sources: `npm run build:instruction-editor` in `frontend/`.

## Out of scope (for now)

- Instructions in the summary readout
- Parsing free-typed `#flour` without the picker
- Linking to amounts other than the recipe list (e.g. “300 ml” vs listed grams)
- Lexical editor (evaluated and dropped; TipTap chosen)

## Docs

`frontend/docs/recipe-instructions.md`

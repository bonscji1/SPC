# Recipe instructions

Cooking steps on a recipe. Linked mentions are **chips bound to ingredient/spice ids**, not `#name` text. Rename a row and the chip label and hover amount update.

Stored on `RecipeDto.Instructions` as ordered steps of text + link tokens (`SPC.Core` `InstructionEditor`).

## Editor

- **Instructions** section below the recipe grid (full width). **+ Add step**; remove confirms if the step has content.
- Each step uses a TipTap rich-text editor with bold, italic, and bullet lists.
- Type `#` to open a picker of current ingredients and spices. Arrow keys and Enter work; mouse click also works.
- Hover a chip to see the amount on the recipe (grams). Hover shows the **list** amount, not extra units typed in the sentence (e.g. “300 ml” vs 200 g of water).
- If the linked row is deleted, the chip stays as missing until removed in the editor.

## Persistence

Saved with the recipe in localStorage (`spc.recipes.v1`). Each step stores TipTap JSON (`editorJson`) plus token chips. Older recipes without `instructions` load as no steps.

Instructions are **not** shown in the recipe summary panel (by design).

## Rebuild JS

After editing `wwwroot/js/instruction-editor/`: `npm run build:instruction-editor` in `frontend/`.

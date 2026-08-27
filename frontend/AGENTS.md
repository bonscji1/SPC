# SPC — Frontend Agent Instructions

Instructions for agents working in the **frontend** subproject.

## Required reading

1. `../AGENTS.md` — shared monorepo rules
2. `docs/README.md` — frontend-specific documentation (this directory)
3. `../docs/README.md` — cross-cutting project documentation
4. **UI work** (pages, layout, CSS, copy structure): also `docs/ui.md` and the **Product preferences** section in `docs/architecture.md`

Do not read `../backend/docs/` unless the task involves backend integration.

## Stack

- **Blazor WebAssembly** on **.NET 10**
- **`SPC.Core`** — DTOs, services, repository interfaces (shared-ready)
- **`SPC.Web`** — Blazor UI only; keep business logic out of `.razor` files

## Commands

Run from `frontend/`:

| Action | Command |
|--------|---------|
| Restore | `dotnet restore SPC.sln` |
| Dev server | `dotnet watch run --project src/SPC.Web/SPC.Web.csproj` |
| Build | `dotnet build SPC.sln` |
| Test | `dotnet test SPC.sln` |

Dev URL (default): http://localhost:5180

To run the **published** stack (repo root): `docker compose up --build` → http://localhost:8080. See the root `README.md`.

Requires [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (see `global.json`) for the commands above.

## Project layout

```
frontend/
├── Dockerfile
├── nginx.conf
├── SPC.sln
├── src/
│   ├── SPC.Core/           # Models, Services, Repositories
│   ├── SPC.Web/          # Blazor WASM app
│   └── SPC.Core.Tests/   # xUnit tests
```

## Conventions

- DTOs and calculation logic live in `SPC.Core`
- UI components bind and call services; **no direct `localStorage`, IndexedDB, or `fetch` in components**
- Repository interfaces in `SPC.Core/Repositories/`; implementations in `SPC.Web/Repositories/`
- To add persistence: define or use an `I*Repository` in Core, implement once in Web (e.g. `LocalStorageRecipeRepository`), register in `Program.cs`. Later add `ApiRecipeRepository` without changing UI.
- Tests target `SPC.Core` first; add UI tests only when valuable
- Display quantities with `NumberFormat` (whole numbers bare; otherwise exactly two decimals). See `docs/architecture.md`
- **UI:** follow `docs/ui.md` (type scale, InfoTip) and **Product preferences** in `docs/architecture.md` (sections, visible caveats). Do not invent new font sizes.
- **Verification:** agents run `dotnet test SPC.sln` only. **The human tests UI functionality.** Do **not** start the dev server, open a browser, take screenshots, or click through the UI unless the user **explicitly** asks. Do not treat UI work as requiring browser verification by default.

## Documentation

| Topic | Location |
|-------|----------|
| Frontend docs | `docs/README.md` |
| UI / typography | `docs/ui.md` |
| Portion math | `docs/portion-math.md` |
| Energy targets | `docs/energy-targets.md` |
| Future improvements | `docs/future-improvements.md` |
| Recipe instructions | `docs/recipe-instructions.md` |
| Shared docs | `../docs/README.md` |
| Implementation plans | `../plans/thePlan.md` (backend: step 10; login: step 11) |

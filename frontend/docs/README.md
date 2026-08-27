# Frontend documentation

Documentation for the SPC frontend subproject.

Also read `../../docs/README.md` for cross-cutting concerns.

## Stack

| Piece | Path | Notes |
|-------|------|-------|
| Blazor WASM app | `src/SPC.Web/` | UI, routing, layout |
| Core library | `src/SPC.Core/` | DTOs, services, repository interfaces |
| Tests | `src/SPC.Core.Tests/` | xUnit; Core logic (portions, energy targets) |

## Run the published app

From the **repository root** (see the root `README.md`):

```bash
docker compose up --build
```

Open http://localhost:8080

## Local development (SDK)

```bash
cd frontend
dotnet restore SPC.sln
dotnet watch run --project src/SPC.Web/SPC.Web.csproj
```

Open http://localhost:5180

## Contents

| Document | Status | Description |
|----------|--------|-------------|
| [Architecture](./architecture.md) | done | Projects, state, UI patterns and product preferences |
| [UI and typography](./ui.md) | done | Font, type scale, InfoTip, layout classes |
| [Portion and calorie model](./portion-math.md) | done | Ingredient-sum + yield; pairing grams with pack kcal/100 g |
| [Energy targets](./energy-targets.md) | done | Mifflin–St Jeor, US activity factors, meal split; profiles independent of recipes |
| [Future improvements](./future-improvements.md) | living | Deferred frontend features (e.g. EFSA energy model choice) |
| [Recipe instructions](./recipe-instructions.md) | done | Ordered steps; chips linked to ingredient/spice ids |
| _Stack and tooling_ | done | Blazor WASM, .NET 10, SPC.Core |
| _Project structure_ | done | See architecture doc |
| _Components and UI_ | done | See [ui.md](./ui.md) and architecture product preferences |
| _State and data fetching_ | partial | Recipe, profile, and ingredient-library repos (localStorage); API in step 10, login shell in step 11 |
| _Testing_ | partial | Core unit tests including portion math and energy targets |

## Agent instructions

- Shared: `../../AGENTS.md`
- Frontend: `../AGENTS.md`

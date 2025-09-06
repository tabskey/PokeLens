# PokeLens

Personal project to help new pathfinders on Pokemon!

Still needing a lot of tests and features 🎉️

## Tests and coverage

Run tests with coverage and generate HTML report:

```
dotnet test --collect:"XPlat Code Coverage"
dotnet reportgenerator -reports:"PokeLens.Tests/TestResults/*/coverage.cobertura.xml" -targetdir:"coverage-report" -reporttypes:"HtmlInline_AzurePipelines;TextSummary"
```

Or use the unified script (Windows PowerShell):

```
./tools/test-with-coverage.ps1
```

## Generation IV (HeartGold/SoulSilver) test criteria

- Endpoints
  - `GET /api/pokemon/{pokemonName}/pokelocation/generation-iv?isHeartGold=true|false`
    - Uses `generation-iv` and toggles between `heartgold` and `soulsilver` context.
  - `GET /api/pokemon/{pokemonName}/pokelocation/game/{gameName}`
    - Accepts `heartgold` or `soulsilver` and resolves generation via mapper.

- Behavior validated in tests
  - Filtering by generation includes only encounters from `heartgold`/`soulsilver`.
  - Availability text for exclusives:
    - HeartGold-exclusive (e.g., `growlithe`): "Available (HeartGold Exclusive)" when `isHeartGold=true`; otherwise "Unavailable (HeartGold Exclusive)".
    - SoulSilver-exclusive (e.g., `vulpix`): "Available (SoulSilver Exclusive)" when `isHeartGold=false`; otherwise "Unavailable (SoulSilver Exclusive)".
  - Non-exclusives show: "Available in both games" for both HG and SS.
  - NotFound payload for missing Pokémon contains: `Error`, `Pokemon`, `Generation = "generation-iv"`.

- Mapper expectations
  - `heartgold` and `soulsilver` map to `generation-iv`.
  - `GetGamesByGeneration("generation-iv")` includes both `heartgold` and `soulsilver`.
  - Exclusivity helpers cover HG/SS lists and availability string mapping.

### cURL examples

Replace <BASE_URL> with your server base URL (e.g., `http://localhost:5134`).

- Generation IV endpoint (HeartGold)
```bash
curl -s "<BASE_URL>/api/pokemon/pikachu/pokelocation/generation-iv?isHeartGold=true"
```

- Generation IV endpoint (SoulSilver)
```bash
curl -s "<BASE_URL>/api/pokemon/pikachu/pokelocation/generation-iv?isHeartGold=false"
```

- By game: HeartGold
```bash
curl -s "<BASE_URL>/api/pokemon/pikachu/pokelocation/game/heartgold"
```

- By game: SoulSilver
```bash
curl -s "<BASE_URL>/api/pokemon/pikachu/pokelocation/game/soulsilver"
```
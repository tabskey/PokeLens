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
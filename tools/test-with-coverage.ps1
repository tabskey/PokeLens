$ErrorActionPreference = 'Stop'

Write-Host 'Running tests with coverage (XPlat Code Coverage)...'
dotnet test --collect:"XPlat Code Coverage" -v minimal

$covFile = Get-ChildItem -Path "PokeLens.Tests/TestResults" -Recurse -Filter coverage.cobertura.xml | Select-Object -First 1
if (-not $covFile) {
	throw 'coverage.cobertura.xml not found. Make sure tests ran with coverage.'
}

Write-Host "Generating coverage report from: $($covFile.FullName)"
dotnet reportgenerator -reports:"$($covFile.FullName)" -targetdir:"coverage-report" -reporttypes:"HtmlInline_AzurePipelines;TextSummary"

Write-Host 'Coverage report generated at coverage-report\index.html'


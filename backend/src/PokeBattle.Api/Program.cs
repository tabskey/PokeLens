using System.Text.Json.Serialization;

using PokeBattle.Api.Endpoints;
using PokeBattle.Application.Battles.PredictBattleOutcome;
using PokeBattle.Application.GameData;
using PokeBattle.Infrastructure.GameData;

var builder = WebApplication.CreateBuilder(args);

var runningInContainer = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";
builder.Logging.ClearProviders();
builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
if (runningInContainer)
{
    builder.Logging.AddJsonConsole();
}
else
{
    builder.Logging.AddConsole();
}

builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks();
builder.Services.AddSingleton(GamePackageLoader.Load(Path.Combine(AppContext.BaseDirectory, "data", "gen3-frlg")));
builder.Services.AddSingleton<PredictBattleOutcomeUseCase>();
builder.Services.AddSingleton<GameOptionsQuery>();
builder.Services.ConfigureHttpJsonOptions(options => options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler();
}

app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => false,
});
app.MapHealthChecks("/health/ready");

app.MapPingEndpoints();
app.MapBattleEndpoints();

app.Run();

// Exposed so WebApplicationFactory<Program> can bootstrap this API in integration tests.
public partial class Program;
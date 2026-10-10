using PokeBattle.Api.Endpoints;

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

app.Run();

// Exposed so WebApplicationFactory<Program> can bootstrap this API in integration tests.
public partial class Program;
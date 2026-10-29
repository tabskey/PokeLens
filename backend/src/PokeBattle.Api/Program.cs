using System.Text.Json.Serialization;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;

using PokeBattle.Api.Auth;
using PokeBattle.Api.Endpoints;
using PokeBattle.Application.Battles.ListTargets;
using PokeBattle.Application.Battles.PredictBattleOutcome;
using PokeBattle.Application.GameData;
using PokeBattle.Infrastructure.GameData;
using PokeBattle.Infrastructure.Persistence;

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

// Secrets only from the environment (ConnectionStrings__Default, Jwt__SigningKey); missing ones stop the start-up.
var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("Set ConnectionStrings__Default to the Postgres connection string.");
var jwt = JwtOptions.Load(builder.Configuration);

builder.Services.AddProblemDetails();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddPersistence(connectionString);
builder.Services.AddHealthChecks().AddDbContextCheck<PokeBattleDbContext>();

builder.Services
    .AddIdentityCore<IdentityUser>(options =>
    {
        options.User.RequireUniqueEmail = true;
        // Length over composition rules (NIST SP 800-63B).
        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<PokeBattleDbContext>()
    .AddSignInManager()
    .AddErrorDescriber<PortugueseIdentityErrorDescriber>();

builder.Services.AddSingleton(jwt);
builder.Services.AddSingleton(AdminEmails.Load(builder.Configuration));
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = jwt.ValidationParameters();
    });
builder.Services.AddAuthorizationBuilder()
    .AddPolicy(AdminEndpoints.Policy, policy => policy.RequireRole(PokeBattleDbContext.AdminRole));
builder.Services.AddSingleton(GamePackageLoader.Load(Path.Combine(AppContext.BaseDirectory, "data", "gen3-frlg")));
builder.Services.AddSingleton<PredictBattleOutcomeUseCase>();
builder.Services.AddSingleton<ListTargetsUseCase>();
builder.Services.ConfigureHttpJsonOptions(options => options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => false,
});
app.MapHealthChecks("/health/ready");

app.MapPingEndpoints();
app.MapBattleEndpoints();
app.MapAuthEndpoints();
app.MapAdminEndpoints();

app.Run();

// Exposed so WebApplicationFactory<Program> can bootstrap this API in integration tests.
public partial class Program;
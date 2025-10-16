using Microsoft.OpenApi.Models;
using PokeLens.Services;
using PokeLens.Swagger.Filters;
using Microsoft.EntityFrameworkCore;
using PokeLens.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.  <3
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


// Swagger configuration
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "PokeLens", Version = "v1" });
    c.SchemaFilter<BooleanToGameVersionSchemaFilter>();
    c.ParameterFilter<HeartGoldSoulSilverParameterFilter>();
    c.OperationFilter<OperationTidyingFilter>();
});

// Base URL constant for reuse
var pokeApiBaseUrl = new Uri("https://pokeapi.co/api/v2/");

// HttpClient configuration
builder.Services.AddHttpClient<IPokeApiService, PokeApiService>(client =>
{
    client.BaseAddress = pokeApiBaseUrl;
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.DefaultRequestHeaders.Add("User-Agent", "PokeLens");
});

builder.Services.AddHttpClient<IPokeAPILocationService, PokeLocationService>(client =>
{
    client.BaseAddress = pokeApiBaseUrl;
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.DefaultRequestHeaders.Add("User-Agent", "PokeLens");
});

// No need to register these services again, AddHttpClient already does this, just backup
// builder.Services.AddScoped<IPokeApiService, PokeApiService>();
// builder.Services.AddScoped<IPokeAPILocationService, PokeLocationService>();

builder.Services.AddLogging();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "PokeLens V1"); });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
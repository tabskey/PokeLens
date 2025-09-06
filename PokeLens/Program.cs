using Microsoft.OpenApi.Models;
using PokeLens.Services;
using PokeLens.Swagger.Filters;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "PokeLens", Version = "v1" });
    c.SchemaFilter<BooleanToGameVersionSchemaFilter>();
    c.ParameterFilter<HeartGoldSoulSilverParameterFilter>();
    c.OperationFilter<OperationTidyingFilter>();

});
builder.Services.AddHttpClient<IPokeApiService, PokeApiService>(client =>
{
    client.BaseAddress = new Uri("https://pokelens.com/api/");
    builder.Services.AddHttpClient<IPokeAPILocationService, PokeLocationService>(client =>
    {
        client.BaseAddress = new Uri("https://pokeapi.co/api/v2/");
    });
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.DefaultRequestHeaders.Add("User-Agent", "PokeLens");
});
builder.Services.AddScoped<IPokeApiService, PokeApiService>();
builder.Services.AddScoped<IPokeAPILocationService, PokeLocationService>();
builder.Services.AddSwaggerGen(c =>
{
    c.SchemaFilter<BooleanToGameVersionSchemaFilter>();
    c.ParameterFilter<HeartGoldSoulSilverParameterFilter>();
    c.OperationFilter<OperationTidyingFilter>();
});
builder.Services.AddHttpClient<PokeLocationService>();

builder.Services.AddLogging();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "PokeLens V1");
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

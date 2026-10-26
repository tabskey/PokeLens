namespace PokeBattle.Api.Endpoints;

public static class PingEndpoints
{
    public static IEndpointRouteBuilder MapPingEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/ping", () => Results.Ok(new { status = "ok" }));
        return app;
    }
}
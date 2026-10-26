using PokeBattle.Application.Battles.PredictBattleOutcome;
using PokeBattle.Application.GameData;

namespace PokeBattle.Api.Endpoints;

public static class BattleEndpoints
{
    public static IEndpointRouteBuilder MapBattleEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/battles/predict", (PredictBattleOutcomeRequest request, PredictBattleOutcomeUseCase useCase) =>
        {
            var errors = useCase.Validate(request);
            return errors.Count > 0
                ? Results.ValidationProblem(errors)
                : Results.Ok(useCase.Execute(request));
        });

        app.MapGet("/api/v1/game-data/options", (string version, GameOptionsQuery query) =>
            PredictBattleOutcomeUseCase.Versions.Contains(version)
                ? Results.Ok(query.Execute(version))
                : Results.ValidationProblem(new Dictionary<string, string[]> { ["version"] = ["Versão deve ser 'firered' ou 'leafgreen'."] }));
        return app;
    }
}
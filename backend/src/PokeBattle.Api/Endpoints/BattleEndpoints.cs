using PokeBattle.Application.Battles.ListTargets;
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

        app.MapGet("/api/v1/targets", (string? version, string? milestone, ListTargetsUseCase useCase) =>
        {
            var errors = useCase.Validate(version, milestone);
            return errors.Count > 0
                ? Results.ValidationProblem(errors)
                : Results.Ok(useCase.Execute(version!, milestone!));
        });

        app.MapGet("/api/v1/game-data/options", (GamePackage package) => new GameOptions(package.Progression.Milestones));
        return app;
    }
}

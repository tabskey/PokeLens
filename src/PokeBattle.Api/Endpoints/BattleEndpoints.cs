using PokeBattle.Application.Battles.PredictBattleOutcome;

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
        return app;
    }
}
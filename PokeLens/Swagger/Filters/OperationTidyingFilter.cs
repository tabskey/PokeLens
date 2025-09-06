using System;
using System.Collections.Generic;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace PokeLens.Swagger.Filters;

public class OperationTidyingFilter : IOperationFilter
{
	public void Apply(OpenApiOperation operation, OperationFilterContext context)
	{
		var path = context.ApiDescription.RelativePath ?? string.Empty;

		if (path.Contains("PokeLocation/generation-iv", StringComparison.OrdinalIgnoreCase))
		{
			operation.Summary = "Gen IV – HeartGold/SoulSilver locations";
			operation.Description = "Use the Version parameter to choose HeartGold or SoulSilver.";
			operation.Tags = new List<OpenApiTag> { new OpenApiTag { Name = "Gen IV (HG/SS)" } };
			return;
		}

		if (path.Contains("PokeLocation/game/", StringComparison.OrdinalIgnoreCase))
		{
			operation.Summary = "Locations by game";
			operation.Description = "Provide the game name (e.g., heartgold or soulsilver).";
			operation.Tags = new List<OpenApiTag> { new OpenApiTag { Name = "By game" } };
		}
	}
}



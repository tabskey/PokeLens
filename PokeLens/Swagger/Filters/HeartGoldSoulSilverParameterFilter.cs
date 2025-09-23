using System;
using System.Collections.Generic;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using PokeLens.Services.mappers;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace PokeLens.Swagger.Filters;

public class HeartGoldSoulSilverParameterFilter : IParameterFilter
{
	public void Apply(OpenApiParameter parameter, ParameterFilterContext context)
	{
		if (parameter.In != ParameterLocation.Query)
			return;

		// Parâmetro isHeartGold
		if (parameter.Name.Equals("version", StringComparison.OrdinalIgnoreCase))
		{
			parameter.Name = "Version";
			parameter.Description = "Generation IV version: HeartGold or SoulSilver";
			parameter.Schema = new OpenApiSchema
			{
				Type = "string",
				Enum = new List<IOpenApiAny>
				{
					new OpenApiString("HeartGold"),
					new OpenApiString("SoulSilver")
				},
				Default = new OpenApiString("HeartGold")
			};
		}

		// Parâmetro progressDisplayName
		if (parameter.Name.Equals("progressDisplayName", StringComparison.OrdinalIgnoreCase))
		{
			parameter.Description = "Selecione a rota até onde o usuário progrediu";
			parameter.Schema = new OpenApiSchema
			{
				Type = "string",
				Enum = HeartGoldSoulSilverMapper.Locations
					.OrderBy(x => x.Key) // Opcional: para mostrar em ordem
					.Select(x => new OpenApiString(x.Value.DisplayName))
					.Cast<IOpenApiAny>()
					.ToList()
			};
		}
	}

}




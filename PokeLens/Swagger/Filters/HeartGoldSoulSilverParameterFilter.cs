using System;
using System.Collections.Generic;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace PokeLens.Swagger.Filters;

public class HeartGoldSoulSilverParameterFilter : IParameterFilter
{
	public void Apply(OpenApiParameter parameter, ParameterFilterContext context)
	{
		if (parameter.In != ParameterLocation.Query)
		{
			return;
		}

		if (!parameter.Name.Equals("isHeartGold", StringComparison.OrdinalIgnoreCase))
		{
			return;
		}

		// Doc-only transformation: present as a string enum named "Version" with HG/SS options
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
}




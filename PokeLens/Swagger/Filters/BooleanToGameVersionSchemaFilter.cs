using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace PokeLens.Swagger.Filters;

public class BooleanToGameVersionSchemaFilter : ISchemaFilter
{
	public void Apply(OpenApiSchema schema, SchemaFilterContext context)
	{
		// Keep schema as boolean; just add helpful description for docs when applicable
		if (schema.Type == "boolean" && context?.MemberInfo?.Name?.Equals("isHeartGold", StringComparison.OrdinalIgnoreCase) == true)
		{
			schema.Description = "Gen IV version selector: HeartGold or SoulSilver";
		}
	}
}
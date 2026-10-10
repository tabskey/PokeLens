using System.Reflection;

using PokeBattle.Core;

namespace PokeBattle.Core.Tests;

public class ArchitectureTests
{
    private static readonly string[] ForbiddenAssemblyPrefixes =
    [
        "Microsoft.AspNetCore",
        "Microsoft.EntityFrameworkCore",
        "System.Net.Http",
    ];

    [Fact]
    public void Core_DoesNotReferenceInfrastructureFrameworks()
    {
        var coreAssembly = typeof(CoreAssemblyMarker).Assembly;

        var forbiddenReferences = coreAssembly.GetReferencedAssemblies()
            .Where(referenced => ForbiddenAssemblyPrefixes.Any(prefix =>
                referenced.Name?.StartsWith(prefix, StringComparison.Ordinal) == true))
            .Select(referenced => referenced.Name)
            .ToList();

        Assert.True(forbiddenReferences.Count == 0,
            $"PokeBattle.Core must stay free of infrastructure dependencies, but references: {string.Join(", ", forbiddenReferences)}");
    }
}
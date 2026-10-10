FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Build.props Directory.Packages.props ./
COPY src/PokeBattle.Core/PokeBattle.Core.csproj src/PokeBattle.Core/
COPY src/PokeBattle.Application/PokeBattle.Application.csproj src/PokeBattle.Application/
COPY src/PokeBattle.Infrastructure/PokeBattle.Infrastructure.csproj src/PokeBattle.Infrastructure/
COPY src/PokeBattle.Api/PokeBattle.Api.csproj src/PokeBattle.Api/
RUN dotnet restore src/PokeBattle.Api/PokeBattle.Api.csproj

COPY src/ src/
RUN dotnet publish src/PokeBattle.Api/PokeBattle.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/* \
    && useradd --uid 10001 --create-home --shell /usr/sbin/nologin appuser

WORKDIR /app
COPY --from=build /app/publish .
USER appuser

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
    CMD curl -f http://localhost:8080/health/live || exit 1

ENTRYPOINT ["dotnet", "PokeBattle.Api.dll"]

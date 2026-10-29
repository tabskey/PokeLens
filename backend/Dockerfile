FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Build.props Directory.Packages.props ./
COPY src/PokeBattle.Core/PokeBattle.Core.csproj src/PokeBattle.Core/
COPY src/PokeBattle.Application/PokeBattle.Application.csproj src/PokeBattle.Application/
COPY src/PokeBattle.Infrastructure/PokeBattle.Infrastructure.csproj src/PokeBattle.Infrastructure/
COPY src/PokeBattle.Api/PokeBattle.Api.csproj src/PokeBattle.Api/
RUN dotnet restore src/PokeBattle.Api/PokeBattle.Api.csproj

COPY src/ src/
COPY data/gen3-frlg/ data/gen3-frlg/
RUN dotnet publish src/PokeBattle.Api/PokeBattle.Api.csproj -c Release -o /app/publish --no-restore

# Migrations bundle: applied by the `migrate` service as an explicit step, never at API start-up.
FROM build AS bundle
COPY .config/ .config/
# shortcut: the bundle targets linux-x64 only; pass the runtime from TARGETARCH if arm64 images are needed.
RUN dotnet tool restore \
    && dotnet ef migrations bundle \
        --project src/PokeBattle.Infrastructure \
        --startup-project src/PokeBattle.Infrastructure \
        --self-contained -r linux-x64 -o /app/efbundle

FROM mcr.microsoft.com/dotnet/runtime-deps:10.0 AS migrate
RUN useradd --uid 10001 --no-create-home --shell /usr/sbin/nologin appuser
WORKDIR /app
COPY --from=bundle /app/efbundle .
USER appuser
# One-shot job: it exits after applying the migrations, so there is nothing to probe.
HEALTHCHECK NONE
ENTRYPOINT ["./efbundle"]

# Last stage, so a plain `docker build` still produces the API image.
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

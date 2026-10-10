# PokeBattle Predictor

Recomenda até 5 Pokémon para **derrotar** ou **capturar** um Pokémon-alvo em **Pokémon FireRed / LeafGreen (Geração III)**, considerando apenas o que o jogador consegue obter naquele ponto da progressão.

Leia `AGENTS.md` antes de contribuir. Arquitetura em `docs/arquitetura.md`, fases em `docs/fases.md`, roteiro de execução em `docs/PLANO.md`.

## Estrutura

```
src/PokeBattle.Core            Regras de domínio (batalha, dano, stats, tipos, captura)
src/PokeBattle.Application     Orquestração dos casos de uso
src/PokeBattle.Infrastructure   Dados do jogo, persistência, auditoria
src/PokeBattle.Api              Endpoints HTTP
tests/                          Core.Tests, Application.Tests, IntegrationTests
```

## Comandos

```bash
dotnet build PokeBattle.slnx
dotnet test PokeBattle.slnx
dotnet run --project src/PokeBattle.Api
docker compose up --build
```

`GET /api/v1/ping`, `GET /health/live` e `GET /health/ready` validam o pipeline da Api.

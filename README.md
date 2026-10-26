# PokeBattle Predictor

Recomenda até 5 Pokémon para **derrotar** ou **capturar** um Pokémon-alvo em **Pokémon FireRed / LeafGreen (Geração III)**, considerando apenas o que o jogador consegue obter naquele ponto da progressão.

Leia `AGENTS.md` antes de contribuir. Arquitetura em `docs/arquitetura.md`, fases em `docs/fases.md`, roteiro de execução em `docs/PLANO.md`.

## Estrutura

```
backend/src/PokeBattle.Core            Regras de domínio (batalha, dano, stats, tipos, captura)
backend/src/PokeBattle.Application     Orquestração dos casos de uso
backend/src/PokeBattle.Infrastructure  Dados do jogo, persistência, auditoria
backend/src/PokeBattle.Api             Endpoints HTTP
backend/tests/                         Core, Application, DataImporter e Integration tests
backend/tools/PokeBattle.DataImporter  Gera o pacote de dados a partir da PokeAPI
backend/data/                          Progressão curada e pacote Gen III gerado
frontend/                              React + Vite + Tailwind (MVVM, Atomic Design)
```

## Comandos

```bash
cd backend
dotnet build PokeBattle.slnx
dotnet test PokeBattle.slnx
dotnet run --project src/PokeBattle.Api

cd frontend
npm install
npm run dev        # http://localhost:5173, com proxy de /api para o backend em :5000
npm test

# na raiz: API em :8080 e web em :8081
docker compose up --build
```

`GET /api/v1/ping`, `GET /health/live` e `GET /health/ready` validam o pipeline da Api.

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
dotnet test PokeBattle.slnx            # testes de integração sobem Postgres via Docker (Testcontainers)

# a API exige Postgres e segredos por ambiente (nada em appsettings)
docker compose up -d db                # na raiz, com .env preenchido
dotnet ef database update --project src/PokeBattle.Infrastructure --startup-project src/PokeBattle.Infrastructure \
  --connection "Host=localhost;Database=pokebattle;Username=pokebattle;Password=<POSTGRES_PASSWORD>"
ConnectionStrings__Default="Host=localhost;Database=pokebattle;Username=pokebattle;Password=<POSTGRES_PASSWORD>" \
Jwt__SigningKey="<32+ caracteres>" \
dotnet run --project src/PokeBattle.Api

cd frontend
npm install
npm run dev        # http://localhost:5173, com proxy de /api para o backend em :5000
npm test

# na raiz: copie .env.example para .env e preencha; sobe db, aplica migrations (serviço migrate), API em :8080 e web em :8081
docker compose up --build
```

`GET /api/v1/ping`, `GET /health/live` e `GET /health/ready` (inclui o banco) validam o pipeline da Api.

Contas são opcionais: `POST /api/v1/auth/register` e `POST /api/v1/auth/login` (JWT). Com o token em `Authorization: Bearer`, as previsões ficam registradas no `AuditLog` com o usuário; sem ele, como anônimas.

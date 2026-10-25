# AGENTS.md — Instruções para agentes de IA neste projeto

## Visão geral

PokeBattle Predictor recomenda até 5 Pokémon para **derrotar** ou **capturar** um Pokémon-alvo em **FireRed / LeafGreen (Geração III)**, considerando apenas o que o jogador consegue obter naquele ponto da progressão. Antes de implementar qualquer coisa, leia `docs/arquitetura.md`, `docs/fases.md` e `docs/roadmap.md`. O roteiro de execução, etapa por etapa, está em `docs/PLANO.md`.

## Stack

- Backend: .NET 10, C#
- Frontend: React + Vite
- Persistência: PostgreSQL com EF Core (Npgsql), só para usuários e auditoria. Não usar SQL Server.

## Idioma

- Todo código, nomes de classes, métodos, testes e comentários em **inglês**
- Conteúdo voltado ao usuário (UI, mensagens, documentação) em **português**, salvo pedido em contrário

## Princípios inegociáveis

- Clean Code, SOLID, DRY, KISS, YAGNI
- `PokeBattle.Core` **não pode** depender de EF Core, HTTP client, ASP.NET, PokeAPI ou qualquer infraestrutura. É C# puro e testável sem mocks pesados.
- Regras de batalha e de captura vivem exclusivamente em `Core`. `Application` orquestra, `Infrastructure` busca e persiste dados, `Api` expõe endpoints.
- Soluções simples antes de soluções elaboradas: não adicionar camada, abstração ou serviço sem necessidade real.

## Estrutura esperada

```
/src
  /PokeBattle.Core            Battles/, Damage/, Stats/, Types/, Capture/
  /PokeBattle.Application     Battles/PredictBattleOutcome/, DTOs
  /PokeBattle.Infrastructure  PokeApi/, Persistence/, Routes/, Encounters/, Moves/, Auditing/
  /PokeBattle.Api             Endpoints/, Contracts/
/tests
  /PokeBattle.Core.Tests
  /PokeBattle.Application.Tests
  /PokeBattle.IntegrationTests
/frontend                     React + Vite
/docs                         arquitetura.md, fases.md, roadmap.md, PLANO.md, validacao-dano.md
```

## Regras de domínio (não alterar sem confirmação explícita do usuário)

1. **Gen III apenas.** Categoria física/especial é definida pelo **tipo** do golpe (tabela fixa), nunca pelo metadado do golpe na PokeAPI. Sem Fairy, sem hidden ability, crítico = 2x.
2. **Cenário e RNG são conceitos separados.** `BattleScenario` descreve condições assumidas (Nature, IV, EV, nível); `DamageRollPolicy` e `CriticalHitPolicy` descrevem como avaliar a variância. Não embutir um valor de RNG no cenário.
3. **Três cenários** (pessimista, neutro, otimista) são a espinha dorsal. Não substituir por um único valor médio.
4. **Progressão é filtro de validade.** Pokémon ou golpe inacessível é descartado antes da simulação; nunca recomendar por matchup excelente.
5. **Captura é domínio próprio.** Usar a fórmula real da Gen III (X → Y → `(Y/65536)^4`) com **aritmética inteira e truncamento a cada etapa**. Nunca usar `X/255` como probabilidade.
6. **Sem precisão falsa.** Mostrar faixa (`79%–81%`) em vez de ponto (`80.17%`) quando o valor não é uma probabilidade calculada. Faixa de dano **não** é probabilidade de vitória. Chance por bola e chance acumulada são métricas distintas.
7. **A API não chama a PokeAPI em runtime.** Dados do jogo vêm do pacote versionado em `data/gen3-frlg/`, gerado por `tools/PokeBattle.DataImporter`. Encontros e níveis por slot vêm da PokeAPI via importador; progressão, presentes/trocas e obtenção de TM/HM vêm de `data/curated/progression.json`. Nunca editar `data/gen3-frlg/` à mão.
8. **Normalização Gen III** no importador: ignorar `damage_class`, aplicar `past_values`, `past_types` e `past_abilities`, descartar hidden abilities, corrigir base stats alterados na Gen VI.
9. **Ordem das operações e truncamentos** das fórmulas de stat e dano seguem a Gen III; não "simplificar" arredondamentos.
10. Natureza, IV e habilidade só são eixo de incerteza para Pokémon selvagens/estimados. Dados `Known` ou `UserProvided` não variam entre cenários.

## Testes

- `Core` exige testes unitários (xUnit) com RNG controlado pela política: dano `Minimum` e `FullRange`, cada fator de cenário isolado, captura (`FinalCaptureRate`, `ShakeThreshold`, `ChancePerBall`), bolas para 50/90/95/99%.
- Testes de progressão (Pokémon só disponível em rota futura não entra) e de disponibilidade de golpes.
- Valores esperados devem vir de cálculo manual ou de fonte da Gen III, **não** dos exemplos ilustrativos da documentação.

## Docker e esteira

- Toda mudança que adiciona um serviço, porta, variável de ambiente ou dependência de runtime atualiza o `Dockerfile` e o `docker-compose.yml` no mesmo PR.
- Imagens multi-stage, usuário não-root, `HEALTHCHECK`, tags de base fixas (nunca `latest`).
- Todo novo projeto ou pacote entra na esteira (build, testes, lint) no mesmo PR.
- Não desligar testes, regras de analyzer, gate de cobertura ou scans para fazer a esteira passar.
- Segredos só por variável de ambiente ou secrets do GitHub; nada em `appsettings` commitado.
- Migrations aplicadas por passo explícito, nunca no startup da API.

## Comandos

- Backend: `dotnet build`, `dotnet test`, `dotnet run --project src/PokeBattle.Api`
- Frontend: `npm install`, `npm run dev`, `npm run build`
- Ambiente completo: `docker compose up --build`
- Dados: `dotnet run --project tools/PokeBattle.DataImporter`

## Fora de escopo da V1 (não implementar sem pedido explícito)

- Pokédex pessoal, movesets customizados, EV/IV informados pelo usuário (V2)
- Treinadores e Gym Leaders (V3)
- Turno a turno, troca de Pokémon, múltiplos Pokémon, strategies de captura (V4)
- Outras gerações (V5)
- Burn, Screen, Weather, Flash Fire, Stockpile, DoubleDmg, Charge, Helping Hand e Double Battles
- Microsserviço de cálculo (o isolamento do `Core` já permite extrair depois)

## Convenções de commit / PR

- Fornecer o texto da mensagem, não commitar, o usuário que fará.

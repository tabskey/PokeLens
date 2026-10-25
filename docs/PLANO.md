# Plano de execução — PokeLens → PokeBattle Predictor

Plano para executar no Claude Code a partir do repositório `tabskey/PokeLens`. A análise considera a `main` (`12b5043`) e a branch `X005DatabaseInfra` (`39339d2`), que tem 3 commits ainda não mesclados na `main` (ver seção 2). Ler junto com `docs/arquitetura.md`, `docs/fases.md`, `docs/roadmap.md` e `AGENTS.md`.

**Como usar:** cada etapa tem um prompt pronto para colar no Claude Code, critérios de aceite e o que não fazer. Uma etapa por branch/PR, na ordem. A esteira tem que estar verde para fazer merge.

---

## 1. Resumo executivo

- O repositório atual é um protótipo de **consulta de localização para HeartGold/SoulSilver**. Não tem nenhuma regra de batalha. Aproveita-se infraestrutura (cliente HTTP, utilitários de teste), não domínio.
- Há um **erro de DI que quebra em runtime** e código que parece funcionar mas não faz nada (toggle HG/SS, filtros do Swagger). Detalhes na seção 2.
- **Correção de premissa:** a PokeAPI **tem** níveis de encontro por slot para FireRed/LeafGreen. Isso reduz bastante o trabalho manual de dados (seção 3).
- **Recomendação principal de arquitetura:** os dados do jogo são estáticos. Em vez de chamar a PokeAPI em runtime, gerar um **pacote de dados Gen III versionado no repo** por uma ferramenta de importação. A API fica determinística, testável e sem dependência externa em produção; banco só para usuários e auditoria (seção 4).
- **Docker e esteira entram na Etapa 1**, e cada etapa seguinte amplia os dois (seção 5).
- Maior risco do projeto: **fidelidade numérica à Gen III**. Por isso o `Core` vem antes de dados, API e frontend, com oráculo de testes externo.

---

## 2. Diagnóstico do repositório atual

### Inventário

| Área | Conteúdo | Situação |
|---|---|---|
| Projeto | Um único Web API (`PokeLens`), **net9.0** | Vira 4 projetos em .NET 10 |
| Endpoints | `GET api/poke/species/{name}`; `GET api/pokemon/{name}/pokelocation/generation-iv`; `.../game/{game}` | Focados em HG/SS (Gen IV) |
| Serviços | `PokeApiService`, `PokeLocationService` (typed `HttpClient`) | Padrão aproveitável |
| Mappers | `GameGenerationMapper`; `HeartGoldSoulSilverMapper` (121 áreas numeradas) | Específicos de Gen IV |
| Modelos | `PokeSpecies`, `PokeLocation`, `VersionDetail`, `EncounterDetail` | Ignoram `min_level` / `max_level` |
| Testes | xUnit + Moq + FluentAssertions, `StubHttpMessageHandler`, ~675 linhas | Utilitários aproveitáveis |
| Infra | Sem Dockerfile, sem CI, sem health check | Tudo a criar |

### Problemas encontrados

| # | Problema | Evidência | Impacto |
|---|---|---|---|
| 1 | Erro de DI em runtime | `PokeController` injeta a classe concreta `PokeLocationService`; `Program.cs` só registra `AddHttpClient<IPokeAPILocationService, PokeLocationService>` | O endpoint de species lança `InvalidOperationException` ao resolver o controller. Testes não pegam porque instanciam na mão. A dependência nem é usada |
| 2 | Toggle HG/SS não filtra | Em `GetPokemonLocationsGenerationIV`, `gameName` é calculado e descartado; a busca usa `"generation-iv"`. `exclusiveTo` também não é usado | Resposta diz "HeartGold" mas traz locais dos dois jogos |
| 3 | Filtros do Swagger mortos | Procuram parâmetros `version` e `progressDisplayName`, que não existem (o real é `isHeartGold`) | `HeartGoldSoulSilverMapper` só é usado por código que nunca roda |
| 4 | Filtro por geração, não por versão | `GetGamesByGeneration("generation-iii")` inclui Ruby, Sapphire, Emerald, Colosseum e XD | Inútil para FRLG, que precisa filtrar por `firered` / `leafgreen` |
| 5 | Exceções como controle de fluxo | `catch (Exception)` → 404; detecção de 404 por `ex.Message.Contains("404")` | Falha de rede vira "não encontrado" |
| 6 | Configuração duplicada | `BaseAddress` em `Program.cs` e no construtor do serviço | Fonte de divergência |
| 7 | Dependência inconsistente | `System.Text.Json 10.0.0-preview` em projeto net9 | Build frágil |
| 8 | Artefato versionado | `TestResults/.../coverage.cobertura.xml` no git | Ruído em diff |
| 9 | Nomes inconsistentes | `pokeController.cs`, pasta `mappers/`, `IPokeAPIBaseService.cs` contendo `IPokeApiService` | Menor |

### Trabalho fora da `main`

`X005DatabaseInfra` (16/10/2025) contém 3 commits que não estão na `main`, incluindo 2 feitos em `X004Calculus` depois do merge do PR #5.

- **Corrige** os itens 2, 3 e 6: o parâmetro passa a ser `version` (casa com o filtro do Swagger), existe `progressDisplayName`, o filtro passa a ser por versão do jogo e o `BaseAddress` duplicado sai.
- **Não corrige** o item 1 (DI do `PokeController`).
- **Introduz:**
  - EF Core com **SQL Server** e `Trusted_Connection=true` no `appsettings.json` (autenticação do Windows: não funciona em container Linux);
  - `DbContext` sem nenhuma entidade;
  - remoção do `coverlet.collector` (some a cobertura de testes);
  - uma chamada extra à PokeAPI por local para buscar o nome de exibição (N+1);
  - EF Core referenciado no projeto de testes sem uso.

Como o plano apaga o código de HG/SS na Etapa 1, mesclar essa branch traz pouco valor de código. O motivo para mesclar é deixar a `main` como fonte da verdade antes de reestruturar. Ver Etapa 0.

### Reaproveitar

- Typed `HttpClient` da PokeAPI → ferramenta de importação (seção 4)
- Modelos de encontro → estender com `min_level`, `max_level`, `condition_values`
- `StubHttpMessageHandler` e `HttpClientFactory` → testes da importação
- A ideia de "área numerada em ordem" → ordem de progressão de FRLG (conteúdo novo)

### Remover

Endpoints, exclusivos, filtros de Swagger e mapper de HG/SS. Ficam no histórico do git. **Recomendação: apagar**, não mover para `legacy/`. Código morto dentro do repo atrai manutenção e confunde o agente.

---

## 3. Correção sobre a PokeAPI e armadilhas da Gen III

Os documentos anteriores diziam que a PokeAPI não fornece níveis de encontro por rota. **Estava errado.** Em `location-area/kanto-route-1-area`, para `firered` e `leafgreen`, cada slot traz `method`, `min_level`, `max_level` e `chance` (ex.: Pidgey, `walk`, nível 2–5).

Dado próprio fica restrito a:
- ordem de progressão das áreas
- requisitos de progressão (insígnias, HMs, Silph Scope, Poké Flute)
- Pokémon de presente, estáticos e trocas in-game
- ponto de obtenção de cada TM/HM

### Armadilhas (bloqueantes para o `Core`)

| Dado da PokeAPI | Problema | Tratamento |
|---|---|---|
| `move.damage_class` | Classificação moderna. `bite` vem **Physical**; na Gen III golpes Dark são **especiais** | Ignorar; tabela tipo → categoria da Gen III |
| `move.power` / `type` | Valores atuais | Aplicar `past_values` pelo version group (`bite` tem entrada em `gold-silver` com tipo Normal) |
| `pokemon.types` | Tipos atuais (Fairy) | Aplicar `past_types` |
| Base stats | Alguns mudaram na Gen VI (ex.: Butterfree). Não confirmei se existe `past_stats` | Verificar; se não houver, tabela de overrides Gen III |
| `pokemon.abilities` | Inclui hidden abilities | Descartar `is_hidden` e aplicar `past_abilities` |
| Learnset | Todos os jogos | Filtrar `version_group = firered-leafgreen` |

Regra: **todo dado da PokeAPI passa por um normalizador Gen III antes de chegar ao `Core`.**

---

## 4. Decisões de arquitetura (visão sênior)

### 4.1 Dados do jogo como pacote estático, não chamada em runtime

| | PokeAPI em runtime + cache | Pacote de dados gerado (recomendado) |
|---|---|---|
| Determinismo | Depende de API externa | Total; o dado está no commit |
| Testes | Precisam de stub em todo lugar | Testes leem o pacote real |
| Produção | Latência, rate limit, indisponibilidade | Zero dependência externa |
| Fair use da PokeAPI | Chamadas por usuário | Uma importação por mudança de dado |
| Auditoria de dado | Difícil saber o que mudou | Diff no PR |
| Custo | Cache, retry, circuit breaker | Uma ferramenta CLI |

Como fica:
- `tools/PokeBattle.DataImporter` (console .NET): baixa da PokeAPI, normaliza para Gen III, junta com o JSON curado de progressão e grava `data/gen3-frlg/*.json`.
- `PokeBattle.Infrastructure` lê esses JSONs (embedded resource ou arquivo copiado na imagem) e expõe repositórios em memória.
- A esteira tem um job que roda o importador e **falha se o resultado diferir do que está commitado** (dado nunca muda sem PR).
- Banco relacional só entra na Etapa 9 (usuários e auditoria).

Isso remove do V1: cache distribuído, retry/circuit breaker para PokeAPI e tabelas de dados do jogo. É o mesmo resultado com menos peças.

### 4.2 Outras decisões

- **Mesmo repositório**, soluções `PokeBattle.*` (ou `PokeLens.*`, decidir na Etapa 1 e manter).
- **.NET 10** com `Directory.Build.props` (nullable, `TreatWarningsAsErrors`, analyzers) e **Central Package Management** (`Directory.Packages.props`).
- **API versionada** (`/api/v1/...`), erros com **ProblemDetails**, **health checks** (`/health/live`, `/health/ready`).
- **Logging estruturado** com o logger nativo em JSON no container. OpenTelemetry fica para quando houver onde enviar.
- **Oráculo de testes:** fórmula da Bulbapedia calculada à mão e conferida numa calculadora de dano Gen III (ex.: Pokémon Showdown no modo Gen 3). Nunca os exemplos ilustrativos dos docs.
- **Core antes de dados:** Etapas 2 a 5 usam Pokémon montados à mão nos testes.
- **Sem microsserviço.** Um container de API, um de frontend, um de banco (a partir da Etapa 9).

---

## 5. Docker e esteira

### 5.1 Containers

| Imagem | Base | Observações |
|---|---|---|
| `api` | build: `mcr.microsoft.com/dotnet/sdk:10.0`; runtime: `mcr.microsoft.com/dotnet/aspnet:10.0` (ou variante `-noble-chiseled`) | Multi-stage, usuário não-root, `HEALTHCHECK`, pacote de dados Gen III copiado na imagem |
| `web` | build: `node` LTS; runtime: `nginx` alpine | Multi-stage, SPA servida estática, proxy `/api` para o container da API |
| `db` | Postgres | Só a partir da Etapa 9 |

`docker-compose.yml` sobe o ambiente completo local; `docker-compose.override.yml` para desenvolvimento (hot reload). Um `.dockerignore` por contexto. Tags de imagem fixas por versão, nunca `latest` em base.

### 5.2 Esteiras (GitHub Actions)

```
PR (pull_request)
 ├─ backend      restore → format check → build (warnings = erro) → testes → cobertura (gate no Core)
 ├─ frontend     install → lint → typecheck → testes → build          (a partir da Etapa 8)
 ├─ data-check   roda o DataImporter e falha se diferir do commitado   (a partir da Etapa 6)
 ├─ docker       build das imagens (sem push) + scan de vulnerabilidades
 └─ security     dependency review + CodeQL

main (push)
 ├─ tudo do PR
 └─ release      build + push das imagens no GHCR com tag do SHA e semver

tag v*
 └─ deploy       (destino a decidir — ver pendências)
```

Complementos:
- **Branch protection** em `main`: PR obrigatório, checks verdes, sem push direto.
- **Dependabot** para NuGet, npm, Docker e Actions.
- **Conventional Commits** + template de PR com checklist (testes, docs, `AGENTS.md` respeitado).
- **Gate de cobertura** só no `PokeBattle.Core` (ex.: 90% de linha). Cobertura de controller não diz nada sobre fidelidade numérica.
- Actions fixadas por versão major (ou SHA) e `permissions:` mínimas por workflow.

---

## 6. Etapas

Tamanho relativo: **P** (algumas horas), **M** (1–2 dias), **G** (vários dias).

### Etapa 0 — Preparação (manual) · P

- Atualizar a `main`: mesclar `X005DatabaseInfra` (que já inclui os commits pendentes de `X004Calculus`) e apagar as branches antigas já mescladas. Alternativa: descartar a branch, já que a Etapa 1 remove esse código; o importante é não começar a reestruturação com trabalho solto fora da `main`
- Copiar o conteúdo da pasta `pokelens-docs` para a raiz do repositório (`AGENTS.md`, `CLAUDE.md` e `docs/`)
- Instalar .NET 10 SDK e Docker
- Configurar branch protection em `main`

---

### Etapa 1 — Reestruturação, Docker e esteira (`X005Restructure`) · M

**Prompt:**

```
Leia AGENTS.md e docs/arquitetura.md. Reestruture o repositório em .NET 10:

Estrutura
1. src/PokeBattle.Core, src/PokeBattle.Application, src/PokeBattle.Infrastructure, src/PokeBattle.Api,
   tests/PokeBattle.Core.Tests, tests/PokeBattle.Application.Tests, tests/PokeBattle.IntegrationTests.
   Referências: Api -> Application, Infrastructure; Application -> Core; Infrastructure -> Application, Core.
   Core não referencia nenhum pacote além do SDK.
2. Directory.Build.props (Nullable enable, TreatWarningsAsErrors, analyzers) e Directory.Packages.props
   (Central Package Management). .editorconfig.
3. Apague todo o código específico de HeartGold/SoulSilver (controllers, exclusivos, mapper, filtros do
   Swagger). Mova StubHttpMessageHandler e HttpClientFactory para tests/PokeBattle.IntegrationTests/TestUtils.
4. Remova System.Text.Json preview e a pasta TestResults do git; ajuste o .gitignore. Remova o
   ApplicationDbContext, os pacotes Microsoft.EntityFrameworkCore.* (SQL Server) e a connection string
   do appsettings.json; o banco (Postgres) volta só na Etapa 9. Restaure o coverlet.collector nos testes.
5. Teste de arquitetura que falha se PokeBattle.Core referenciar Microsoft.AspNetCore,
   Microsoft.EntityFrameworkCore ou System.Net.Http.
6. Api com versionamento /api/v1, ProblemDetails, health checks /health/live e /health/ready,
   logging JSON quando em container. Um endpoint GET /api/v1/ping para validar o pipeline.

Docker
7. Dockerfile multi-stage para a Api (sdk:10.0 -> aspnet:10.0), usuário não-root, HEALTHCHECK.
   .dockerignore. docker-compose.yml subindo a Api.

Esteira (GitHub Actions)
8. ci.yml em pull_request e push na main: restore, dotnet format --verify-no-changes, build, test com
   cobertura, build da imagem Docker (sem push), scan da imagem. permissions mínimas.
9. release.yml em push na main: build e push da imagem no GHCR com tag do SHA.
10. Dependabot (nuget, docker, github-actions) e template de PR.

Não implemente regras de batalha nesta etapa.
```

**Aceite:** esteira verde no PR; `docker compose up` sobe a API e `/health/ready` responde 200; teste de arquitetura passa; nenhuma referência a HG/SS.

---

### Etapa 2 — Tipos e stats (`X006Stats`) · M

**Prompt:**

```
Implemente em PokeBattle.Core, com testes em PokeBattle.Core.Tests:

1. Enum PokemonType com os 17 tipos da Gen III (sem Fairy).
2. TypeEffectivenessService com a tabela da Gen III; defensor de tipo duplo multiplica.
3. MoveCategoryResolver: categoria pelo TIPO do golpe.
   Físicos: Normal, Fighting, Flying, Poison, Ground, Rock, Bug, Ghost, Steel.
   Especiais: Fire, Water, Grass, Electric, Psychic, Ice, Dragon, Dark.
4. Nature (25 naturezas) e StatCalculator conforme docs/arquitetura.md seção 11, só inteiros e
   truncamento por etapa. HP não sofre Nature.

Testes: casos 0x, 0.5x, 2x; tipo duplo 0.25x e 4x; Ghost em Normal = 0; Bite é especial;
stats de 3+ Pokémon com IV/EV/Nature/nível conhecidos, com o cálculo manual comentado.
Adicione o gate de cobertura do Core na esteira (90% de linha).
```

**Aceite:** testes verdes, gate de cobertura ativo; nenhum `double` no `StatCalculator`.

---

### Etapa 3 — Dano (`X007Damage`) · M

**Prompt:**

```
Implemente DamageCalculator, DamageRollPolicy e CriticalHitPolicy em PokeBattle.Core/Damage, conforme
docs/arquitetura.md seções 10 e 13 e docs/fases.md Fase 1.

- Modificadores ativos: Critical (2x), STAB (1.5), Type, Random (inteiro 85..100, dividido por 100).
- Ordem das operações e truncamentos da Gen III.
- DamageRollPolicy: Minimum, Maximum, FullRange (16 valores), Average (média dos 16 resultados inteiros).
- CriticalHitPolicy: Never, Guaranteed, Normal, EvaluateProbability (chance base da Gen III).
- DamageResult com Min, Max e, em FullRange, os 16 valores.
- Dano mínimo 1 quando acerta e efetividade não é 0.

Testes: 5+ pares atacante/defensor/golpe com valores conferidos numa calculadora de dano Gen III,
fonte comentada no teste. Não implemente Burn, Screen, Weather nem outros modificadores.
```

**Aceite:** os pares batem com a referência em mínimo e máximo.

---

### Etapa 4 — Cenários (`X008Scenarios`) · P

**Prompt:**

```
Implemente BattleScenario e os perfis (NatureProfile, IvProfile, EvProfile, LevelProfile) em
PokeBattle.Core/Battles, conforme docs/arquitetura.md seção 10 e a tabela da Fase 2 de docs/fases.md.

- Três instâncias fixas: Pessimistic, Neutral, Optimistic, com os dois lados em direções opostas.
- O cenário NÃO contém RNG.
- Valores concretos num único ScenarioCatalog, documentado.
- DataSource (Known, UserProvided, Estimated) por atributo; só Estimated varia entre cenários.

Antes de implementar, me mostre os valores propostos de IV e nível por cenário e espere confirmação.
```

**Aceite:** stats diferentes nos 3 cenários para atributo `Estimated`, idênticos para `Known`.

---

### Etapa 5 — Simulação e captura (`X009Simulation`, `X010Capture`) · M + M

Decida antes o **critério de "vencer"**. Sugestão para a V1: comparar turnos para KO de cada lado com o melhor golpe disponível, sem Speed (Speed entra na V4).

**Prompt (simulação):**

```
Implemente BattleSimulationService e BattleOutcomeEvaluator em PokeBattle.Core/Battles.
Critério de vitória da V1: [critério decidido].
Saída por cenário: DamageRange, RemainingHp (faixa), TurnsToKo (mín. e máx.), Outcome.
A classificação é derivada dos números e não os substitui.
```

**Prompt (captura):**

```
Implemente CaptureCalculator e CaptureProbabilityCalculator em PokeBattle.Core/Capture, conforme
docs/arquitetura.md seção 15.

- Aritmética inteira com truncamento por etapa; ball bonus em ponto fixo.
- X >= 255 => captura garantida, ChancePerBall = 1.
- Y pela fórmula real, não X/255. ChancePerBall = (Y / 65536)^4.
- BallsForConfidence(q) = ceil(ln(1 - q) / ln(1 - p)), com p = 1 tratado à parte.

Testes: X e Y para 4+ combinações com referência documentada; para p = 0.19752,
BallsForConfidence deve dar 4, 11, 14 e 21 para 50/90/95/99%.
```

---

### Etapa 6 — Pacote de dados FRLG (`X011Data`) · G

**Prompt:**

```
Crie tools/PokeBattle.DataImporter (console .NET) e a leitura dos dados na Infrastructure:

1. Gen3Normalizer: aplica past_values (golpes), past_types e past_abilities; descarta hidden abilities;
   filtra learnset por version group firered-leafgreen. Verifique se a PokeAPI expõe base stats antigos;
   se não, crie overrides de base stats da Gen III e liste as espécies afetadas no PR.
2. Importa encontros de location-area para firered e leafgreen (área, Pokémon, método, min_level,
   max_level, chance, versão).
3. data/curated/progression.json (editado à mão): ordem das áreas de Kanto e Sevii, requisito de cada
   área (insígnia, HM, item), presentes/estáticos/trocas, ponto de obtenção de cada TM/HM.
4. Saída determinística (ordenação estável, JSON formatado) em data/gen3-frlg/.
5. Infrastructure lê data/gen3-frlg como repositórios em memória. O Dockerfile copia a pasta para a imagem.
6. Job data-check na esteira: roda o importador e falha se houver diff com o commitado.
   Esse job roda só em PRs que tocam data/ ou tools/ e em agendamento semanal.

Testes: Bite resolve como especial e tipo Dark; Pokémon com hidden ability não a recebe;
Route 1 em firered traz níveis 2 a 5.
```

**Aceite:** os três testes passam; `data-check` verde; progressão cobre ao menos até a 2ª insígnia (completar depois).

> O `progression.json` é o maior trabalho manual. Comece até Cerulean, valide o fluxo de ponta a ponta na Etapa 7, depois complete.

> Com o pacote de dados pronto, entra o **teste de confronto em massa** do dano contra o `@smogon/calc` (`docs/validacao-dano.md` §7). Pode ser um PR próprio logo depois desta etapa.

---

### Etapa 7 — Caso de uso, ranking e API (`X012Predict`) · M

**Prompt:**

```
Implemente PredictBattleOutcomeUseCase (Application) e BattleRankingService (Core), conforme
docs/arquitetura.md seções 6, 7 e 14.

- Entrada: alvo, objetivo (Defeat | Catch), versão (firered | leafgreen), ponto de progressão.
- Filtra candidatos e golpes inacessíveis ANTES de simular.
- 3 cenários por candidato, ranking pela seção 14, até 5 candidatos.
- POST /api/v1/battles/predict com validação e ProblemDetails.
- Testes de integração com WebApplicationFactory.

Testes: Pokémon disponível só depois da progressão não aparece; golpe de TM não obtido fica fora;
exclusivo de leafgreen não aparece em firered.
```

---

### Etapa 8 — Frontend (`X013Frontend`) · G

**Prompt:**

```
Crie frontend/ com React + Vite + TypeScript consumindo POST /api/v1/battles/predict.
Telas: versão, progressão, alvo e objetivo; até 5 candidatos com os 3 cenários.
Exibição (docs/arquitetura.md seção 18): categoria em destaque, faixa de dano como detalhe, nunca
percentual que pareça chance de vitória; para Catch, chance por bola e bolas para 50/90/95/99% separadas.
Textos em português.

Docker: Dockerfile multi-stage (node -> nginx) servindo a SPA com proxy /api para a Api; serviço web no
docker-compose. Esteira: job frontend (lint, typecheck, testes, build) e build da imagem web; push no GHCR
no release.yml. Dependabot para npm.
```

**Aceite:** `docker compose up` sobe API + web e o fluxo funciona pelo navegador; job frontend verde.

---

### Etapa 9 — Login e auditoria (`X014Auth`) · M

**Prompt:**

```
Adicione autenticação (ASP.NET Identity + JWT) e auditoria:
BattlePredictionRequested e BattlePredictionCompleted gravados em AuditLog pela Application, com
usuário, alvo, objetivo, progressão, candidatos e timestamp. O Core não conhece auditoria.

EF Core com Postgres. Serviço db no docker-compose com volume e healthcheck; a Api depende dele.
Migrations geradas como bundle e aplicadas por um passo explícito (não no startup da Api).
/health/ready passa a checar o banco. Testes de integração com Testcontainers.
Segredos só por variável de ambiente; nada em appsettings commitado.
Não implemente Pokédex pessoal (é V2).
```

---

## 7. Riscos

| Risco | Probabilidade | Mitigação |
|---|---|---|
| Números divergentes do jogo (truncamento, ordem de operações, dado moderno) | Alta | Core primeiro, oráculo externo, normalizador Gen III, testes com fonte comentada |
| `progression.json` incompleto ou errado | Alta | Começar até Cerulean; revisar contra walkthrough; teste que valida que toda área referenciada existe |
| Escopo crescer antes da V1 (ginásio, Pokédex, turno a turno) | Média | `AGENTS.md` lista o fora de escopo; roadmap separado |
| PokeAPI mudar formato ou dado | Baixa | Dado é gerado e commitado; `data-check` semanal avisa |
| Agente "simplificando" fórmulas | Média | Regras explícitas no `AGENTS.md` e revisão do plano dele antes de implementar |

---

## 8. Pendências

- [x] Prefixo: **`PokeBattle.*`** (decidido na Etapa 1)
- [ ] Valores dos perfis de IV (Etapa 4): em revisão com jogadores, provisório 8 / 15 / 23 (`docs/decisoes-cenarios.md` §2). Nível decidido: média truncada
- [x] Onde entra a habilidade: **fora do cenário** (`docs/decisoes-cenarios.md` §6)
- [ ] Critério de "vencer" (Etapa 5): em revisão com jogadores, provisório = faixas de turnos sem sobreposição, sem Speed (`docs/decisoes-simulacao.md` §2). Golpe do oponente (sempre o melhor) e precisão (fora da V1) decididos
- [x] Banco: **Postgres** (decidido). O SQL Server da `X005DatabaseInfra` sai na Etapa 1, junto com os pacotes `Microsoft.EntityFrameworkCore.SqlServer` e a connection string com `Trusted_Connection`
- [ ] Teste de confronto em massa do dano: tamanho da amostra e local do script gerador (`docs/validacao-dano.md` §7)
- [ ] Destino de deploy: nenhum por ora, VPS com compose, ou um serviço gerenciado de containers (define o job `deploy`)

## 9. Uso no Claude Code

- Rode cada etapa primeiro em modo de plano, revise e só então deixe implementar.
- Exija esteira verde antes do merge; não aceite PR que desliga teste ou regra de analyzer.
- Se ele sugerir usar `damage_class` da PokeAPI, chamar a PokeAPI em runtime ou "simplificar" truncamentos, a resposta é não: está no `AGENTS.md`.
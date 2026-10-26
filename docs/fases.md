# Fases de implementação — V1

Plano de execução da **V1** descrita em `arquitetura.md` (FireRed / LeafGreen, selvagens, Defeat e Catch, top 5). Evoluções posteriores estão em `roadmap.md`.

Ordem pensada para validar o núcleo de cálculo primeiro: se os números do `Core` estiverem errados, nada acima deles importa.

---

## Fase 0 — Fundação

- Solution .NET 10 com `Api`, `Application`, `Core`, `Infrastructure` e os três projetos de teste
- Regra de dependência aplicada (`Core` sem referências a infraestrutura)
- Dockerfile multi-stage da API, `docker-compose.yml`, health checks
- Esteira GitHub Actions: format, build com warnings como erro, testes, build e scan da imagem; publicação no GHCR na `main`

**Pronto quando:** esteira verde, `docker compose up` sobe a API saudável e o `Core` não referencia nenhum pacote de infra.

---

## Fase 1 — Núcleo de dano (`Core`)

- `StatCalculator` (HP e demais stats, Nature, IV, EV, truncamentos Gen III)
- `TypeEffectivenessService` (tabela Gen III, sem Fairy)
- Tabela tipo → categoria física/especial da Gen III
- `DamageCalculator` com `DamageRollPolicy` e `CriticalHitPolicy`

`CriticalHitPolicy`:
- `Never`: ignora crítico.
- `Guaranteed`: todo golpe é crítico.
- `Normal`: o crítico é possível; a faixa vai do menor roll sem crítico ao maior roll com crítico.
- `EvaluateProbability`: faixas sem e com crítico separadas, com a chance de crítico (1/16, ou 1/8 em golpes de crítico alto); a média é ponderada por essa chance.

Modificadores ativos nesta fase: Critical, STAB, Type, Random. Os demais da fórmula (Burn, Screen, Weather, Flash Fire, Stockpile, DoubleDmg, Charge, Helping Hand, Targets) ficam **desligados** e entram só se houver necessidade real.

**Pronto quando:** dano mínimo, máximo e faixa completa batem com valores calculados à mão para pares conhecidos.

---

## Fase 2 — Cenários

- `BattleScenario` com perfis de Nature, IV, EV e nível
- Valores concretos de cada perfil por cenário (pessimista / neutro / otimista), documentados
- Habilidade: **fora do cenário** (decidido; ver `decisoes-cenarios.md` §6)

Perfis (decisões e opções em `decisoes-cenarios.md`):

| Fator | Pessimista | Neutro | Otimista |
|---|---|---|---|
| Nature do candidato | desfavorece o stat ofensivo usado | neutra | favorece o stat ofensivo |
| Nature do oponente | favorece o ataque dele | neutra | desfavorece o ataque dele |
| IV do candidato | Low | Average | High |
| IV do oponente | High | Average | Low |
| EV | 0 (selvagens) | 0 | 0 |
| Nível | extremo desfavorável da faixa da rota | média truncada `floor((min+max)/2)` | extremo favorável da faixa |

IV: **em revisão**, provisório Low 8 / Average 15 / High 23 (alternativa 0 / 15 / 31). Nível, Nature (só no stat ofensivo), EV e habilidade decididos.

Os dois lados variam em direções opostas dentro de cada cenário: o que é ruim para o candidato é bom para o oponente. Crítico e roll **não** fazem parte do cenário; vêm das políticas.

**Pronto quando:** os três cenários são instâncias fixas, testadas isoladamente.

---

## Fase 3 — Simulação e avaliação (`Core`)

- `BattleSimulationService` (dano, faixa, HP restante, turnos para KO)
- `BattleOutcomeEvaluator`
- Critério de "vencer": **provisório** (turnos para KO de cada lado com o melhor golpe, faixas sem sobreposição, sem Speed; empate = Balanced), em revisão em `decisoes-simulacao.md` §2. Oponente usa sempre o melhor golpe; todo golpe acerta (decidido)

**Pronto quando:** um par de Pokémon produz resultado coerente nos três cenários.

---

## Fase 4 — Captura (`Core`)

- `CaptureCalculator` (X, Y, aritmética inteira com truncamento por etapa, na ordem do jogo: bola → HP → status)
- `CaptureProbabilityCalculator` (`P_ball`, probabilidade acumulada, bolas para 50 / 90 / 95 / 99%)
- `KoRiskCalculator`: chance de um golpe derrubar o alvo (rolls que derrubam / 16, ponderada pelo crítico)
- Bolas da V1: Poké, Great, Ultra e Master. Net, Nest, Repeat, Timer, Luxury/Premier e Safari ficam pendentes até a Etapa 6 confirmar onde se obtêm

**Pronto quando:** `FinalCaptureRate`, `ShakeThreshold` e `ChancePerBall` batem com valores derivados da implementação Gen III.

---

## Fase 5 — Dados próprios

- `backend/tools/PokeBattle.DataImporter`: importa da PokeAPI espécies, golpes, learnsets e encontros (a PokeAPI traz `min_level`, `max_level` e `chance` por slot e versão), aplica a normalização Gen III e grava `data/gen3-frlg/`
- `backend/data/curated/progression.json` (manual): ordem das áreas, requisitos, presentes/estáticos/trocas, obtenção de TM/HM
- Infrastructure lê o pacote em memória; a API não chama a PokeAPI em runtime
- Ordem das rotas e modelo de progressão do jogador
- Exclusão de Pokémon obtidos por troca (modelados como `Gift` ou fora da V1)

**Pronto quando:** dado um ponto de progressão, o sistema lista os Pokémon acessíveis e os golpes possíveis.

---

## Fase 6 — Orquestração e ranking

- `PredictBattleOutcomeUseCase`
- Filtro de acessibilidade antes de simular
- `BattleRankingService` para Defeat e Catch
- Endpoint `POST /battles/predict`

**Pronto quando:** a chamada devolve até 5 candidatos ranqueados, com os três cenários.

---

## Fase 7 — Frontend

- Seleção de alvo, objetivo e progressão
- Exibição dos 3 cenários com categoria textual em destaque e faixa de dano como detalhe
- Para Catch: chance por bola e bolas para cada nível de confiança, mostradas como métricas distintas
- Sem percentuais que pareçam probabilidade de vitória

---

## Fase 8 — Login e auditoria

- Autenticação de usuário
- Eventos `BattlePredictionRequested` e `BattlePredictionCompleted` em `AuditLog`
- Objetivo: entender o comportamento dos usuários e identificar problemas

A Pokédex pessoal **não** entra aqui; está na V2 (`roadmap.md`).

---

## Em aberto

- Critério de "vencer" (Fase 3)
- Onde entra a habilidade no modelo de cenário (Fase 2)
- Curadoria do `progression.json` (Fase 5)
- Destino de deploy

Docker e esteira de CI/CD começam na Fase 0 e crescem a cada fase. O detalhamento e os prompts de cada etapa estão em `PLANO.md`.
- Valores concretos dos perfis de IV e do nível por cenário (Fase 2)

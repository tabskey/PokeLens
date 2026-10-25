<!--
Preenchido automaticamente pelo GitHub em todo PR novo.
Comentários como este não aparecem no PR renderizado: podem ficar ou ser apagados.
Seções que não se aplicam: escreva "Não se aplica" em vez de apagar.
-->

## Etapa

<!-- Ex.: Etapa 3 — Dano (`X007Damage`). Ver docs/PLANO.md. Se não for uma etapa, diga o tipo: fix, chore, docs, deps. -->

## O que mudou e por quê

<!-- O problema ou objetivo, em 2–4 frases. Depois, as mudanças principais em tópicos. -->

-

## Decisões tomadas

<!--
Escolhas feitas durante a implementação e quem confirmou.
Qualquer mudança em regra de domínio do AGENTS.md precisa de confirmação explícita registrada aqui.
-->

-

## Validação numérica

<!--
Obrigatório quando a mudança toca fórmula do Core (stats, dano, captura, cenários, simulação).
- Fonte de referência (ex.: pret/pokefirered, função X) e oráculo usado (ex.: @smogon/calc 0.12.0, Gen 3)
- Quantos casos bateram e quantos divergiram; explicação de cada divergência
- Atualizar docs/validacao-dano.md (ou o documento equivalente) se for o caso
-->

## Como foi testado

<!-- Comandos rodados e resultado. Ex.: "dotnet test: 70 passed no Core, 3 na integração; cobertura do Core 99,65%". -->

- [ ] `dotnet format PokeBattle.slnx --verify-no-changes`
- [ ] `dotnet build -c Release` sem warnings
- [ ] `dotnet test` verde
- [ ] Cobertura do `PokeBattle.Core` ≥ 90%
- [ ] `docker compose up --build` sobe e `/health/ready` responde 200 (se mexeu em runtime, Docker ou config)

## Fora de escopo / pendências

<!-- O que ficou de fora de propósito e onde ficou registrado (docs/arquitetura.md §23, docs/PLANO.md §8, issue). -->

-

## Checklist

- [ ] `AGENTS.md` respeitado (sem regra de domínio alterada sem confirmação)
- [ ] `PokeBattle.Core` sem dependência de infraestrutura (teste de arquitetura passa)
- [ ] Fórmulas da Gen III em inteiros, com a ordem e os truncamentos do jogo, sem `double` no caminho do cálculo
- [ ] Valores esperados dos testes vêm de cálculo manual ou fonte da Gen III, nunca dos exemplos ilustrativos dos docs
- [ ] `data/gen3-frlg/` não foi editado à mão (só pelo `DataImporter`)
- [ ] Docs atualizados se aplicável (`arquitetura.md`, `fases.md`, `roadmap.md`, `PLANO.md`, `validacao-dano.md`)
- [ ] `Dockerfile` e `docker-compose.yml` atualizados se a mudança adiciona serviço, porta, env var ou dependência de runtime
- [ ] Projeto ou pacote novo entrou na esteira (build, testes, lint)
- [ ] Nenhum segredo em `appsettings` ou no código; migrations só por passo explícito
- [ ] Esteira verde (backend, docker, security)

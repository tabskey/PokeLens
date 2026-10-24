# Projeções — análises para versões futuras

Complementa o `roadmap.md`. O roadmap diz **o que** entra em cada versão; este documento registra **análises técnicas** já feitas para essas versões e o gatilho que faz cada uma sair do papel.

**Nada aqui altera a V1** nem as regras de domínio do `AGENTS.md`. Quando um item daqui for implementado, as regras afetadas precisam de confirmação explícita antes.

---

## 1. Cenários na V1 (o que já está decidido)

Conforme `fases.md` (Fase 2) e `arquitetura.md` (seção 10):

- Três cenários fixos (pessimista, neutro, otimista) num `ScenarioCatalog`, com os dois lados em direções opostas.
- O cenário descreve **condições** (Nature, IV, EV, nível). O RNG fica nas políticas (`DamageRollPolicy`, `CriticalHitPolicy`).
- Só atributos `Estimated` variam entre cenários.

Na V1, todo candidato é um Pokémon selvagem a capturar, então Nature e IVs são `Estimated` **dos dois lados**. A simetria do modelo vale sem esforço extra.

### Direção de produto
Os **três cenários são mostrados ao usuário**, não só um resumo. Cada cenário é um conjunto nomeado de condições, então ele mesmo explica o porquê do resultado (*"nature desfavorece seu Attack, IVs baixos"*). A decisão de risco fica com o jogador: ele pode preferir um candidato que só vai bem no cenário otimista, por exemplo, porque já tem esse Pokémon guardado.

---

## 2. Probabilidade de vitória e Monte Carlo (V4)

### Por que não entra na V1
O critério de vitória da V1 compara **turnos para KO** com o melhor golpe, sem Speed. O resultado é uma faixa (`DamageRange`, `TurnsToKo` mín./máx.), e a regra 6 proíbe apresentar faixa de dano como probabilidade de vitória. Não existe uma sequência de turnos a amostrar.

### Gatilho
A simulação **turno a turno** da V4. Com roll de dano (85–100%, 16 valores) e crítico (1/16) sorteados a cada golpe dos dois lados, e lutas de duração variável, listar todas as sequências possíveis deixa de ser viável.

### Método
**Monte Carlo:** simular a luta N vezes, sorteando roll e crítico a cada golpe, e contar os resultados. A probabilidade de vitória vira uma **probabilidade calculada**, o que é compatível com a regra 6.

Abordagem híbrida:
- **Cálculo exato** onde a enumeração é pequena: chance de nocaute em um golpe, dano mínimo e máximo. Isso já é a V1.
- **Monte Carlo** só para a luta completa.

### Como encaixa nas regras atuais
O Monte Carlo **não substitui os cenários**. Ele entra como uma nova forma de avaliar o RNG, no mesmo lugar onde as políticas já ficam:

```
BattleScenario (condições, 3 fixos) + política amostrada (RNG) → Simulação → P(vitória) por cenário
```

Assim a regra 2 (cenário e RNG separados) e a regra 3 (três cenários como espinha dorsal) continuam valendo. Cada cenário passa a ter uma probabilidade de vitória, além das faixas. A `CriticalHitPolicy.EvaluateProbability` já prevê esse tipo de avaliação para o crítico.

Alternativa considerada e descartada por ora: definir os cenários como percentis (p10/p50/p90) de uma distribuição única, sorteando também Nature e IV. Ela troca o `ScenarioCatalog` fixo por um resultado estatístico e quebra a leitura direta do porquê de cada cenário.

### Precisão
O erro é mensurável:

```
erro ≈ √( p × (1 − p) / N )
```

| N | Erro padrão (p = 50%) | Margem 95% |
|---|---|---|
| 100 | 5% | ±10% |
| 1.000 | 1,6% | ±3% |
| 10.000 | 0,5% | ±1% |

Exibição sugerida, coerente com a regra 6: faixa em vez de ponto (`72%–74%`), com um adendo curto:

> Probabilidades estimadas a partir de 10.000 batalhas simuladas (método de Monte Carlo).

### Requisitos de arquitetura
- O gerador aleatório é **injetado** no simulador, nunca criado dentro dele. O `Core` continua puro.
- **Seed fixa nos testes**: mesma entrada, mesmo resultado. Sem testes instáveis na esteira.
- **N é configuração**, não constante no código. No container, vem por variável de ambiente.

### Desempenho
Custo = N × número de candidatos × três cenários. Se a latência ficar perceptível:
- As simulações são independentes e paralelizam sem coordenação.
- Cache por entrada (espécies, níveis, cenário, N, seed) é seguro, porque o resultado é determinístico.
- Um benchmark com N fixo na esteira detecta regressões antes do deploy.

---

## 3. Métodos de encontro e progressão (insumo para a Etapa 6)

Alcançar uma área **não** garante acesso a todos os encontros dela. O jogador passa por várias rotas com água antes de ter vara ou Surf, e depois pode voltar a elas.

| Método | Onde se obtém (FRLG) |
|---|---|
| Old Rod | Vermilion City |
| Good Rod | Fuchsia City |
| Super Rod | Route 12 |
| Surf | Fuchsia City (HM03 no Safari Zone + insígnia do Koga) |

Consequência para o `progression.json`: além do requisito de cada **área**, ele precisa do ponto de desbloqueio de cada **método de encontro**. O filtro de validade passa a ter duas condições: a área está no ponto de progresso ou antes dele, e o método do encontro já está desbloqueado nesse ponto. Voltar a uma rota antiga com Surf passa a funcionar automaticamente.

**Premissa (V1):** progressão linear. Quem chegou a Fuchsia pegou a Good Rod. Old Rod e Good Rod são opcionais (dadas por NPCs), então essa premissa pode falhar. Se usuários reclamarem, uma opção "tenho a vara X" resolve.

Os pontos de obtenção da tabela devem ser revisados contra uma fonte da Gen III antes de entrar no `progression.json`.

---

## 4. Exclusivos de versão

- Não manter lista de exclusivos à mão. Com os encontros importados **por versão**, filtrar pela versão do jogador já remove os exclusivos da outra. "Exclusivo" vira um dado derivado, útil só para exibir um aviso.
- A versão entra como **enum** (`FireRed`, `LeafGreen`) desde o primeiro contrato da API, nunca como `bool`. Trocar o contrato depois, com clientes usando, custa mais.
- **V2:** os candidatos passam a ser *encontros da versão ∪ Pokémon da Pokédex do usuário*. Os da Pokédex entram independentemente da versão, porque podem ter vindo de troca.

---

## 5. Itens fora do roadmap atual

| Item | Versão | Observação |
|---|---|---|
| Synchronize | V2 ou depois | No FRLG, com esse Pokémon na frente, o selvagem tem 50% de chance de vir com a mesma Nature. Mudaria a distribuição da Nature do oponente |
| Hidden abilities | V5 (Gen V+) | Mais estados possíveis de ability em `Estimated` |
| Chance base de crítico 1/24 | V5 (Gen VII+) | Nova `CriticalHitPolicy` por geração |
| Mints | V5 (Gen VIII+) | A Nature efetiva pode diferir da Nature real |

---

## 6. Pontos em aberto

- [ ] Qual cenário alimenta o ranking Top 5 da V1, ou se o usuário escolhe ("mais seguro", "equilibrado", "maior potencial")
- [ ] Exibir ou não a margem de erro na V4 para o público comum
- [ ] Confirmar a premissa de progressão linear para as varas na curadoria do `progression.json`

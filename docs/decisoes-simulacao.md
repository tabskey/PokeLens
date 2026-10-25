# Decisões da simulação de batalha

Documento para revisar com outros jogadores as escolhas da Etapa 5 (simulação, `X009Simulation`). Mesmo formato de `decisoes-cenarios.md`: o que foi decidido, o que está **em aberto** e o motivo de cada opção. Ler junto com `arquitetura.md` §14 e `fases.md` Fase 3.

**Status geral:** golpe do oponente e precisão decididos (a precisão fica registrada para discussão futura). **Critério de vitória em aberto** (valor provisório no código).

---

## 1. O que a simulação faz

Para cada um dos três cenários (`decisoes-cenarios.md`), a simulação:
1. escolhe o **melhor golpe** de cada lado (seção 5);
2. calcula a faixa de dano desse golpe e o **HP que sobra** depois de um golpe;
3. calcula os **turnos para KO** de cada lado, em faixa;
4. classifica o resultado: **Favorable**, **Balanced** ou **Unfavorable**, mais um rótulo de KO.

A classificação vem dos números e **não os substitui**: a tela mostra os turnos e as faixas, e o rótulo só resume.

### Turnos para KO
- **Mínimo** = `ceil(HP / dano máximo)`: todos os golpes com o melhor roll.
- **Máximo** = `ceil(HP / dano mínimo)`: todos os golpes com o pior roll.
- Golpe que não causa dano (imunidade) não tem turnos para KO.

### Rótulo de KO (do candidato)
| Rótulo | Regra |
|---|---|
| Sem dano | o golpe não causa dano |
| KO garantido | 1 golpe em qualquer roll (máximo = 1) |
| KO possível | 1 golpe só com os melhores rolls (mínimo = 1) |
| 2 golpes | mínimo = 2 |
| 3 golpes | mínimo = 3 |
| 4 ou mais | mínimo ≥ 4 |

---

## 2. Critério de vitória — **EM ABERTO**

A V1 não considera Speed (`PLANO.md` sugeria deixar para a V4). A pergunta é como decidir quem leva vantagem só com os turnos para KO.

### Opção A — faixas sem sobreposição, empate = Balanced (**provisório no código**)
- **Favorable:** o pior caso do candidato derruba **antes** do melhor caso do oponente (`candidato.máx < oponente.mín`). Quem precisa de menos turnos vence **independente de quem bate primeiro**, então a conclusão não depende da Speed.
- **Unfavorable:** o inverso (`candidato.mín > oponente.máx`).
- **Balanced:** as faixas se sobrepõem ou empatam. Aí o resultado depende dos rolls ou de quem é mais rápido.
- Candidato sem dano → Unfavorable. Oponente sem dano → Favorable.

### Opção B — Speed no desempate
- Mesma regra, mas num empate quem é **mais rápido** bate primeiro e vence. Se o candidato é mais rápido, basta `candidato.máx ≤ oponente.mín` para ser Favorable.
- A sobreposição de faixas continua Balanced, porque a Speed não resolve rolls diferentes.
- Prioridade (Quick Attack) e empate de Speed (no jogo, sorteio) continuam de fora.
- **O que muda no código:** Speed entra no `Combatant` e no `ScenarioResolver` (o stat já é calculável) e na regra do `BattleOutcomeEvaluator`. Mudança pequena e localizada.

### Exemplo: Rattata (Tackle) contra Pidgey (Tackle), Route 1

| Cenário | Candidato derruba em | Oponente derruba em | Opção A | Opção B |
|---|---|---|---|---|
| Pessimista (Rattata L2 contra Pidgey L5) | 5–7 turnos | 2 turnos | Unfavorable | Unfavorable |
| Neutro (L3 contra L3) | 3 turnos | 3 turnos | **Balanced** | **Favorable** (Rattata Speed 9 > Pidgey 8) |
| Otimista (L5 contra L2) | 2 turnos | 5–7 turnos | Favorable | Favorable |

A diferença aparece **só nos empates exatos**, e no começo do jogo eles são comuns (HP e dano pequenos).

### Perguntas para os jogadores
1. Num empate de turnos, você prefere ver **"equilibrado"** (A, honesto sobre a incerteza) ou o app **decidir pela Speed** (B, mais decisivo)?
2. Em B, a Speed do selvagem também é estimada. Ela deve variar por cenário como o resto (IV e Nature), o que pode inverter o desempate entre cenários?
3. Vale incluir a prioridade (Quick Attack, Extreme Speed) já na V1, ou fica para a V4?

### Decisão
- [ ] Opção escolhida: ___
- Data / quem participou: ___
- Motivo: ___

---

## 3. Golpe do oponente — decidido

- O oponente usa **sempre o melhor golpe dele**, com o mesmo critério do candidato (seção 5).
- **Motivo:** é conservador. No jogo, a IA de selvagem na Gen III escolhe golpes **ao acaso**, então o oponente real é igual ou mais fraco que o simulado. Um candidato Favorable aqui continua Favorable no jogo.
- **Alternativa registrada:** média dos golpes do oponente (mais fiel à IA aleatória). Descartada na V1 porque os turnos deixam de ser uma faixa exata e viram valor esperado, o que esbarra na regra de não mostrar precisão falsa. Entra naturalmente com o Monte Carlo da V4 (`projecoes.md` §2).

## 4. Precisão dos golpes — decidido fora da V1 (registrado para discussão futura)

- A V1 assume que **todo golpe acerta**. O resultado precisa deixar isso claro para o jogador.
- **Prós de ficar de fora:** turnos para KO seguem exatos e explicáveis; nenhuma probabilidade "inventada"; menos dado a importar agora.
- **Contras:** golpes imprecisos fortes (Hydro Pump 80%, Fire Blast 85%, Thunder 70%) parecem melhores do que são; um "KO garantido" com golpe de 90% falha 1 vez em 10.
- **Opções para discutir depois:**
  - desempate na escolha do golpe (entre dois com os mesmos turnos, preferir o mais preciso). Exige `Accuracy` no `Move`, que a PokeAPI já fornece;
  - mostrar a precisão do golpe escolhido ao lado do resultado, sem mudar o cálculo;
  - probabilidade real de vitória com acertos e erros (V4, Monte Carlo).

## 5. Escolha do melhor golpe (definição)

Para cada lado e cada cenário, o golpe com:
1. **menos turnos para KO no pior caso**;
2. desempate: **menos turnos no melhor caso**;
3. desempate: **maior dano máximo**;
4. empate total: a **ordem da lista** de golpes.

A Nature estimada depende da categoria do golpe (físico usa Attack, especial usa Sp. Atk, `decisoes-cenarios.md` §4), então cada golpe é avaliado com o atacante resolvido para a categoria dele.

## 6. Premissas da V1

- Todo golpe acerta (seção 4).
- Uma troca de golpes, sem simulação turno a turno: sem status, sem troca de Pokémon, sem PP, sem prioridade, sem Speed (seção 2).
- Sem habilidades e itens (`decisoes-cenarios.md` §6).
- Roll e crítico seguem as políticas escolhidas pelo caso de uso (`DamageRollPolicy`, `CriticalHitPolicy`).

---

## Histórico
- 2026-10-25: documento criado na Etapa 5 (simulação). Golpe do oponente e precisão decididos; critério de vitória em aberto, provisório = opção A.

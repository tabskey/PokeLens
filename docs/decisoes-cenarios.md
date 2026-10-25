# Decisões dos cenários de batalha

Documento para revisar com outros jogadores as escolhas da Etapa 4 (`X008Scenarios`). Registra o que já foi decidido, o que está **em aberto** e o motivo de cada opção. Ler junto com `arquitetura.md` §10–11 e `fases.md` Fase 2.

**Status geral:** nível, habilidade, EV e Nature decididos. **IV em aberto** (valor provisório no código).

---

## 1. O que é um cenário

Um selvagem tem Nature, IV e nível desconhecidos. Em vez de inventar um valor único, o sistema calcula **três cenários fixos**, e cada um assume um conjunto de condições:

| | Pessimista | Neutro | Otimista |
|---|---|---|---|
| **Candidato** (o Pokémon recomendado) | Nature desfavorece o stat ofensivo, IV baixo, menor nível da faixa | Nature neutra, IV médio, nível médio | Nature favorece o stat ofensivo, IV alto, maior nível da faixa |
| **Oponente** (o alvo) | Nature favorece o stat ofensivo dele, IV alto, maior nível | Nature neutra, IV médio, nível médio | Nature desfavorece o stat ofensivo dele, IV baixo, menor nível |
| **EV** | 0 | 0 | 0 |

- Os dois lados andam em **direções opostas**: o que é ruim para o candidato é bom para o oponente.
- O cenário descreve **condições**, não sorte. Roll de dano e crítico são avaliados à parte (`DamageRollPolicy`, `CriticalHitPolicy`).
- Só atributos **desconhecidos** (`Estimated`) variam. Um dado conhecido (`Known`) ou informado pelo usuário (`UserProvided`, V2) é igual nos três cenários.
- Os três cenários são **mostrados ao jogador** (`projecoes.md` §1). Cada um explica o próprio resultado, e a decisão de risco fica com ele.

---

## 2. IV — **EM ABERTO**

IV de selvagem na Gen III é sorteado de **0 a 31**, uniforme e independente por stat. A pergunta é que valor representa "IV baixo", "médio" e "alto".

| Perfil | Opção A — quartis | Opção B — extremos |
|---|---|---|
| Low | **8** | **0** |
| Average | **15** | **15** |
| High | **23** | **31** |
| Maximum (reservado, fora dos 3 cenários) | 31 | 31 |

**Valor provisório no código: opção A.** Fica num único lugar (`ScenarioCatalog.IndividualValueFor`), e trocar é mudar três números e os comentários dos testes. O PR da Etapa 4 só é mesclado depois desta decisão.

### O que cada opção significa

| | A — quartis 8 / 15 / 23 | B — extremos 0 / 15 / 31 |
|---|---|---|
| Leitura | "Um selvagem **típico** ruim / médio / bom" | "O **pior** e o **melhor** selvagem possível" |
| Chance de um stat cair no perfil Low ou pior | IV ≤ 8: 9/32 ≈ **28%** | IV = 0: 1/32 ≈ **3%** |
| Pessimista | Realista: acontece com frequência | Garantia: nenhum selvagem é pior que isso |
| "KO garantido" no pessimista | Vale para a maioria dos selvagens, **não** para todos | Vale para **qualquer** selvagem daquela faixa |
| Largura das faixas | Mais estreitas, mais úteis para comparar candidatos | Mais largas; o pessimista combina vários extremos raros ao mesmo tempo |

### Exemplos (Rattata usa Tackle contra Pidgey, EV 0)

**Níveis baixos (Route 1, 2–5):** o IV quase não aparece; quem decide é o nível.

| Cenário | A — quartis | B — extremos |
|---|---|---|
| Pessimista (Rattata L2 contra Pidgey L5) | 3–4 de dano em 20 de HP | 3–4 em 20 |
| Neutro (L3 contra L3) | 5–6 em 15 | 5–6 em 15 |
| Otimista (L5 contra L2) | 8–10 em 13 | 10–12 em 13 |

Pidgey L5 com Atk base 45: IV 0 → 9, IV 8 → 9, IV 15 → 10, IV 23 → 10, IV 31 → 11.

**Nível alto (os dois em L50, só IV e Nature desconhecidos):** aqui a escolha pesa.

| Cenário | A — quartis | B — extremos |
|---|---|---|
| Pessimista | 21–25 (18%–22% do HP) | 18–22 (15%–19%) |
| Neutro | 28–33 (26%–30%) | 28–33 (26%–30%) |
| Otimista | 33–39 (31%–37%) | 38–45 (38%–45%) |

Rattata L50 com Atk base 56: IV 0 → 61, IV 8 → 65, IV 15 → 68, IV 23 → 72, IV 31 → 76.

### Perguntas para os jogadores
1. Quando o app disser "pessimista", você espera **"o pior que costuma acontecer"** (A) ou **"o pior possível"** (B)?
2. Você prefere um pessimista que às vezes falha (A, mais útil para comparar) ou um que nunca falha, mesmo que seja exagerado na maioria das vezes (B)?
3. Vale a pena expor a escolha na tela ("modo cauteloso" = B, "modo realista" = A)? Isso conversa com o ponto em aberto de `projecoes.md` §6, sobre qual cenário alimenta o ranking.

### Decisão
- [ ] Opção escolhida: ___
- Data / quem participou: ___
- Motivo: ___

---

## 3. Nível — decidido

- Faixa de níveis do encontro (ex.: Route 1 = 2–5).
- **Desfavorável:** menor nível para o candidato, maior para o oponente.
- **Neutro:** média **truncada**, `floor((min + max) / 2)`. Route 1 → **3** (não 4). Inteiro e determinístico, como o resto do cálculo.
- Nível conhecido (`Known`) é uma faixa de um nível só e não varia.

## 4. Nature — decidido

- Nature desconhecida não vira uma Nature concreta. O perfil aplica **×0.9 / ×1.0 / ×1.1 só no stat ofensivo** que aquele lado usa: Attack para golpe físico, Sp. Atk para especial (pela regra de tipo da Gen III). Os outros stats ficam neutros.
- Motivo: uma Nature real que desfavorece Attack sempre favorece outro stat, e escolher qual seria arbitrário. Só o stat ofensivo usado muda o resultado na V1.
- Nature conhecida usa o efeito real em todos os stats.

## 5. EV — decidido

- **0** nos três cenários: selvagem recém-encontrado não tem EV. EV real entra com a Pokédex pessoal (V2).

## 6. Habilidade — decidido

- **Fora do cenário.** Habilidade é atributo da espécie, não eixo de incerteza na V1. O cálculo de dano da V1 ainda não modela nenhuma habilidade.
- **Pendência registrada** (`arquitetura.md` §23): habilidades de **imunidade** mudam a recomendação e precisam entrar no filtro de golpes na Etapa 5/7. Exemplos: Levitate (Gastly e Koffing imunes a Ground), Volt Absorb, Water Absorb, Flash Fire, Wonder Guard. Espécie com duas habilidades possíveis: usar a pior para o candidato quando isso importar.

---

## Histórico
- 2026-10-25: documento criado na Etapa 4. Nível, Nature, EV e habilidade decididos; IV em aberto, provisório = opção A.
- 2026-10-25: corrigido um valor mostrado na primeira proposta (Pidgey L5 com IV 23 tem Atk **10**, não 11).

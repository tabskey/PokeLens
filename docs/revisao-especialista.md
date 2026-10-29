# Revisão por especialista — critérios de cálculo, desempates e projeções

Documento para uma pessoa que **conhece bem Pokémon FireRed / LeafGreen**, mas não é desenvolvedora, revisar as escolhas do PokeBattle Predictor. Não é preciso ler código: cada regra está descrita em português, com exemplos de jogo.

**O que pedimos de você:** em cada seção marcada com ✅ / ❌ / 💬, diga se concorda, se discorda (e por quê) ou se tem um comentário. As perguntas abertas estão reunidas na seção 10. No fim (seção 11) há um resumo curto, sem termos técnicos.

**Base de tudo:** as regras foram tiradas da **decompilação do FireRed** (o código do jogo reconstruído a partir da ROM, projeto `pret/pokefirered`). Quando uma fonte da internet (Bulbapedia, Smogon, Showdown) discorda do código do jogo, seguimos o jogo e explicamos a diferença.

---

## Sumário

1. O que o app faz
2. Vocabulário usado neste documento
3. Stats: Nature, IV, EV e nível
4. Fórmula de dano
5. Cenários (pessimista, neutro, otimista)
6. Simulação da batalha e critérios de desempate
7. Projeções de dano: o que a tela mostra
8. Captura: fórmula e projeções
9. O que está fora da V1 (e por quê)
10. Perguntas para você
11. Resumo leigo

---

## 1. O que o app faz

O jogador informa:
- um **Pokémon-alvo** (ex.: um Abra na Route 24);
- o **objetivo**: **derrotar** ou **capturar**;
- **até onde chegou no jogo** (quais rotas, insígnias e itens já tem).

O app devolve **até 5 Pokémon recomendados** ("candidatos") para enfrentar aquele alvo. Só entram Pokémon e golpes que o jogador **já consegue obter** naquele ponto do jogo. Um matchup perfeito com um Pokémon que só aparece depois da Victory Road nunca é recomendado.

Na V1 (primeira versão), todo candidato é um Pokémon selvagem que o jogador pode capturar. Por isso não sabemos a Nature nem os IVs dele, assim como não sabemos os do alvo. É daí que vêm os "cenários" (seção 5).

---

## 2. Vocabulário usado neste documento

| Termo | Significado aqui |
|---|---|
| **Candidato** | O Pokémon que o app está avaliando para recomendar |
| **Oponente / alvo** | O Pokémon que o jogador quer derrotar ou capturar |
| **Roll** | O fator aleatório do dano. No jogo, todo golpe multiplica o dano por um número inteiro sorteado entre 85 e 100 (e divide por 100). São **16 valores possíveis**, todos com a mesma chance |
| **Faixa de dano** | Do menor ao maior dano possível. "17–20" quer dizer que o golpe causa 17, 18, 19 ou 20 |
| **Truncar** | Jogar fora a parte decimal (arredondar sempre para baixo). O jogo faz isso **a cada passo** da conta, não só no fim |
| **Turnos para KO** | Quantos golpes iguais um lado precisa para derrubar o outro |
| **Cenário** | Um conjunto de suposições sobre o que não sabemos (Nature, IV, nível). Seção 5 |
| **Probabilidade calculada** | Um número exato, derivado das regras do jogo (ex.: chance de captura por bola). Pode ser mostrado como "6,25%" |
| **Estimativa** | Algo que depende de suposições nossas. Mostramos como faixa ("79%–81%"), nunca com casas decimais que passem falsa precisão |

---

## 3. Stats: Nature, IV, EV e nível

### 3.1 Fórmulas (Gen III)

```
HP    = trunc( (2 × Base + IV + trunc(EV / 4)) × Nível / 100 ) + Nível + 10
Stat  = trunc( ( trunc( (2 × Base + IV + trunc(EV / 4)) × Nível / 100 ) + 5 ) × Nature )
```

- Nature: **×1,1** no stat que ela favorece, **×0,9** no que ela desfavorece, ×1,0 nos outros. **Nunca afeta HP.**
- As 5 Natures neutras (Hardy, Docile, Serious, Bashful, Quirky) não mudam nenhum stat.
- A ordem das contas e os truncamentos seguem o jogo. Não "simplificamos" arredondamentos.

✅ / ❌ / 💬 — As fórmulas e a tabela das 25 Natures conferem com o que você conhece da Gen III?

### 3.2 Base stats

Usamos os **base stats da Gen III**. Alguns Pokémon tiveram base stats alterados na Gen VI (ex.: Pidgeot, Butterfree, Beedrill, Raichu, Alakazam ganharam pontos). O importador de dados desfaz essas mudanças e usa o valor original.

O mesmo vale para **tipos** que mudaram depois (Clefairy e Clefable são Normal; Jigglypuff é Normal; Mr. Mime é só Psychic: nada de Fairy) e para **habilidades**: hidden abilities não existem na Gen III e são descartadas.

✅ / ❌ / 💬 — Algum Pokémon do Kanto/Sevii que você lembra que mudou e que valha conferir na mão?

---

## 4. Fórmula de dano

### 4.1 Físico ou especial é definido pelo **tipo do golpe**

Na Gen III não existe a divisão física/especial por golpe (isso só chegou na Gen IV). Vale a tabela por tipo:

| Físico (usa Attack × Defense) | Especial (usa Sp. Atk × Sp. Def) |
|---|---|
| Normal, Fighting, Flying, Poison, Ground, Rock, Bug, Ghost, Steel | Fire, Water, Grass, Electric, Psychic, Ice, Dragon, Dark |

Consequências que surpreendem quem joga gerações novas:
- **Bite, Crunch** (Dark) são **especiais**. Um Raticate usando Bite usa o Sp. Atk baixo dele.
- **Shadow Ball** (Ghost) é **física**.
- **Fire Punch, Thunder Punch, Ice Punch** são **especiais**.

✅ / ❌ / 💬

### 4.2 Ordem da conta

Conferida no código do jogo:

```
1. Dano base = trunc( trunc( trunc(Atk × Poder × (trunc(2 × Nível / 5) + 2)) / Def ) / 50 )
   (só golpes físicos: se der 0, vira 1)
   + 2
2. Crítico: × 2
3. STAB: × 1,5 (truncando)
4. Efetividade de tipo: aplicada entrada por entrada da tabela do jogo, truncando a cada uma
5. Roll: × (85 a 100) / 100, truncando; nunca abaixo de 1 se o golpe acerta e não é imune
```

Detalhes que conferimos e que você pode validar:
- **Crítico é ×2** na Gen III (não ×1,5 como a partir da Gen VI).
- **Chance de crítico:** 1/16 normalmente; **1/8** para golpes de crítico alto (Slash, Razor Leaf, Karate Chop, Crabhammer…).
- **STAB = ×1,5** quando o tipo do golpe é um dos tipos do atacante.
- O piso "dano base no mínimo 1" só existe no ramo **físico** do código do jogo; no especial não. Na prática, um golpe especial muito fraco termina com base 2 e um físico com base 3.

✅ / ❌ / 💬

### 4.3 Tabela de tipos da Gen III

- Sem o tipo Fairy.
- **Steel resiste a Ghost e Dark** (isso só mudou na Gen VI).
- A tabela foi copiada do jogo **na mesma ordem**. A ordem importa: contra um defensor de dois tipos com uma fraqueza e uma resistência, o jogo aplica uma de cada vez e trunca no meio. Às vezes 2× seguido de 0,5× não dá exatamente 1×.

**Exemplo real:** Lapras L26 usando Ice Beam em Moltres L50 (Fire/Flying). Na tabela do jogo, a linha de Ice traz Flying (2×) **antes** de Fire (0,5×):
- Jogo: 19 × 2 = 38 → 38 × 0,5 = **19** → faixa final **16–19**.
- Showdown (que ordena pelo tipo do defensor): 19 × 0,5 = 9 → 9 × 2 = **18** → faixa **15–18**.

Seguimos o jogo. Isso é um detalhe pequeno, mas mostra que o cálculo reproduz o FireRed, não uma aproximação.

**Bug que achamos e corrigimos:** a primeira versão da nossa tabela não tinha **Ghost → Psychic (2×)** nem **Ice → Grass (2×)**. Uma comparação automática com o código do jogo pegou as duas falhas; as outras 108 entradas batiam.

✅ / ❌ / 💬 — Algum matchup de tipo da Gen III que você gostaria de ver testado explicitamente?

### 4.4 Como o cálculo de dano foi conferido

Comparamos com a calculadora do **Pokémon Showdown** (modo Gen 3), nos 16 rolls, com e sem crítico:

| Confronto | Cobre | Resultado (normal / crítico) |
|---|---|---|
| Rattata L5 Tackle → Pidgey L5 | Físico, STAB, neutro | Igual: 5–7 / 12–15 |
| Charmander L10 Ember → Bulbasaur L10 | Especial, STAB, 2× | Igual: 17–20 / 35–42 |
| Squirtle L14 Water Gun → Geodude L14 | Especial, STAB, 4× | Igual: 51–60 / 102–120 |
| Raticate L20 Bite → Gastly L20 | Bite especial, sem STAB, 2× | Igual: 27–32 / 54–64 |
| Bulbasaur L15 Vine Whip → Charmander L15 | Especial, STAB, 0,5× | Igual: 5–6 / 10–12 |
| Mankey L12 Karate Chop → Pidgey L12 | Físico, STAB, 2× e 0,5× | Igual: 16–19 / 33–39 |
| Lapras L26 Ice Beam → Moltres L50 | Ordem da tabela de tipos | Diverge por limitação do Showdown (seção 4.3) |

**6 de 6 iguais** valor por valor nos casos em que o Showdown segue o jogo. Não houve teste em emulador.

---

## 5. Cenários (pessimista, neutro, otimista)

### 5.1 A ideia

Um Pokémon selvagem tem Nature, IVs e nível (dentro da faixa da rota) desconhecidos. Em vez de inventar um único valor "médio", o app calcula **três cenários** e **mostra os três**:

| | Pessimista | Neutro | Otimista |
|---|---|---|---|
| **Candidato** | Nature desfavorece o stat de ataque que ele usa, IV baixo, **menor** nível da faixa | Nature neutra, IV médio, nível médio | Nature favorece o stat de ataque, IV alto, **maior** nível |
| **Oponente** | Nature favorece o ataque dele, IV alto, **maior** nível | Nature neutra, IV médio, nível médio | Nature desfavorece o ataque dele, IV baixo, **menor** nível |
| **EV** | 0 | 0 | 0 |

- Os dois lados andam em **direções opostas**: o pessimista é ruim para o candidato **e** bom para o oponente ao mesmo tempo.
- O cenário descreve **condições**, não sorte. Roll de dano e crítico são tratados à parte (seção 7).
- O que for **conhecido** não varia entre cenários. Exemplo: o nível de um encontro fixo (Snorlax L30, aves lendárias L50) é o mesmo nos três.

✅ / ❌ / 💬 — Faz sentido para você ver três resultados em vez de um só?

### 5.2 Nível — decidido

- Faixa de nível do encontro, como o jogo define (ex.: Route 1 = L2 a L5).
- Pessimista: menor nível para o candidato, maior para o oponente. Otimista: o contrário.
- **Neutro = média arredondada para baixo.** Route 1 (2–5) → **L3**, não L4.

✅ / ❌ / 💬 — Deveria ser o nível **mais comum** do encontro em vez da média? (Os slots de encontro do jogo têm pesos diferentes: 20%, 20%, 10%, 10%, 10%, 10%, 5%, 5%, 4%, 4%, 1%, 1%.)

### 5.3 Nature — decidido

- Nature desconhecida **não** vira uma Nature específica. O cenário aplica ×0,9 / ×1,0 / ×1,1 **só no stat de ataque que aquele lado usa**: Attack para golpe físico, Sp. Atk para especial (pela tabela de tipo da seção 4.1). Os outros stats ficam neutros.
- **Por quê:** uma Nature real que baixa Attack sempre sobe outro stat (Modest sobe Sp. Atk, Timid sobe Speed, Bold sobe Defense…). Escolher qual seria arbitrário, e na V1 só o stat de ataque muda o resultado.
- Nature conhecida usa o efeito real em todos os stats.

✅ / ❌ / 💬 — No pessimista, o **oponente** recebe Nature favorável no ataque, mas a Defense/Sp. Def dele fica neutra. Isso parece razoável?

### 5.4 EV — decidido

- **0 nos três cenários.** Selvagem recém-capturado não tem EV. EVs reais entram quando o jogador puder cadastrar os próprios Pokémon (V2).

✅ / ❌ / 💬 — Isso subestima o candidato do jogador, que provavelmente já treinou um pouco. Aceitável para a V1?

### 5.5 IV — **EM ABERTO** (precisamos da sua opinião)

IV de selvagem na Gen III é sorteado de 0 a 31, cada stat independente. Que valor representa "IV baixo", "médio" e "alto"?

| Perfil | Opção A — "típico" (quartis) | Opção B — "extremos" |
|---|---|---|
| Baixo | **8** | **0** |
| Médio | **15** | **15** |
| Alto | **23** | **31** |

| | A — 8 / 15 / 23 | B — 0 / 15 / 31 |
|---|---|---|
| Leitura | "Um selvagem **típico** ruim / médio / bom" | "O **pior** e o **melhor** selvagem possível" |
| Chance de um stat ser igual ou pior que o "baixo" | ≈ 28% (IV ≤ 8) | ≈ 3% (IV = 0) |
| Pessimista | Realista: acontece com frequência | Garantia: nenhum selvagem é pior |
| "KO garantido" no pessimista | Vale para a maioria dos selvagens, **não** todos | Vale para **qualquer** selvagem |
| Faixas | Mais estreitas, melhores para comparar | Mais largas, mais exageradas |

**Valor provisório no código: opção A.**

**Exemplo 1 — níveis baixos** (Rattata usando Tackle em Pidgey, Route 1): quase não muda nada, o nível domina.

| Cenário | A | B |
|---|---|---|
| Pessimista (Rattata L2 vs Pidgey L5) | 3–4 de dano em 20 de HP | 3–4 em 20 |
| Neutro (L3 vs L3) | 5–6 em 15 | 5–6 em 15 |
| Otimista (L5 vs L2) | 8–10 em 13 | 10–12 em 13 |

**Exemplo 2 — nível alto** (os dois em L50, só IV e Nature desconhecidos): aqui a escolha pesa.

| Cenário | A | B |
|---|---|---|
| Pessimista | 21–25 (18%–22% do HP) | 18–22 (15%–19%) |
| Neutro | 28–33 (26%–30%) | 28–33 (26%–30%) |
| Otimista | 33–39 (31%–37%) | 38–45 (38%–45%) |

💬 Ver perguntas 1 e 2 na seção 10.

### 5.6 Habilidade — decidido (fora da V1, com uma pendência)

- O cálculo de dano da V1 **não modela habilidades** (Intimidate, Guts, Thick Fat, Blaze, Torrent etc.).
- **Pendência registrada:** habilidades de **imunidade** mudam a recomendação e vão entrar no filtro de golpes: Levitate (Gastly, Haunter, Gengar, Koffing, Weezing imunes a Ground), Volt Absorb, Water Absorb, Flash Fire, Wonder Guard. Quando a espécie tem duas habilidades possíveis, usar a pior para o candidato.

✅ / ❌ / 💬 — Há outras habilidades que você considera indispensáveis já na V1 para Kanto? (Ex.: Intimidate de Growlithe/Arcanine/Ekans/Gyarados, Sturdy, Static, Swift Swim.)

---

## 6. Simulação da batalha e critérios de desempate

Esta é a parte com mais **escolhas nossas**, e onde sua revisão é mais valiosa.

### 6.1 O que a simulação faz

Para cada cenário:
1. escolhe o **melhor golpe** de cada lado (6.3);
2. calcula a faixa de dano e o **HP que sobra** depois de um golpe;
3. calcula **turnos para KO** de cada lado, em faixa;
4. classifica: **Favorável**, **Equilibrado** ou **Desfavorável**, mais um rótulo de KO.

O rótulo **resume** os números e **não os substitui**: a tela mostra turnos e faixas.

### 6.2 Turnos para KO

- **Mínimo** = HP ÷ dano máximo, arredondado para cima (todos os golpes com o melhor roll).
- **Máximo** = HP ÷ dano mínimo, arredondado para cima (todos com o pior roll).
- Golpe sem efeito (imunidade) não tem turnos para KO.

Exemplo: alvo com 15 de HP e golpe de 5–6 → mínimo 15 ÷ 6 = 2,5 → **3**; máximo 15 ÷ 5 = **3**. Resultado: sempre 3 golpes.

**Rótulo de KO do candidato:**

| Rótulo | Regra |
|---|---|
| Sem dano | o golpe não afeta o alvo |
| KO garantido | 1 golpe com **qualquer** roll |
| KO possível | 1 golpe, mas só com os rolls mais altos |
| 2 golpes | melhor caso = 2 |
| 3 golpes | melhor caso = 3 |
| 4 ou mais | melhor caso ≥ 4 |

✅ / ❌ / 💬 — "KO possível" deveria mostrar **quantos dos 16 rolls** derrubam (ex.: "KO em 6 de 16 rolls")? Isso já é calculável.

### 6.3 Escolha do melhor golpe — critério de desempate

Para cada lado, em cada cenário, o golpe escolhido é o que tem, nesta ordem:

1. **Menos turnos para KO no pior caso** (o golpe mais confiável);
2. desempate: **menos turnos no melhor caso** (o que tem mais chance de terminar antes);
3. desempate: **maior dano máximo**;
4. empate total: a ordem em que os golpes aparecem na lista.

Por que o **pior caso primeiro**: preferimos o golpe que **garante** o resultado ao que às vezes termina antes e às vezes não.

**Exemplo ilustrativo** (números inventados para mostrar a regra): alvo com 40 de HP.
- Golpe X: 18–21 de dano. Melhor caso 40 ÷ 21 = 1,9 → **2**; pior caso 40 ÷ 18 = 2,2 → **3**. Turnos: 2–3.
- Golpe Y: 20–24 de dano. Melhor caso 40 ÷ 24 = 1,7 → **2**; pior caso 40 ÷ 20 = **2**. Turnos: sempre 2.
- Escolhido: **Y**, porque o pior caso dele é 2 e o de X é 3.

O que o desempate **não** considera hoje:
- **Precisão** (todo golpe acerta na V1; 6.6);
- **PP**;
- **efeitos secundários** (chance de paralisar, queimar, baixar stat, flinch);
- **recuo/recarga** (Double-Edge, Take Down, Hyper Beam);
- **prioridade** (Quick Attack).

✅ / ❌ / 💬 — A ordem 1 → 2 → 3 faz sentido? Algum dos itens da lista acima deveria entrar como desempate já na V1? Sugestão nossa para discutir: entre dois golpes empatados nos critérios 1 e 2, preferir o **mais preciso** antes do mais forte.

### 6.4 Golpe do oponente — decidido

- O oponente usa **sempre o melhor golpe dele**, com o mesmo critério do candidato.
- **Por quê:** é a suposição conservadora. A IA de selvagem na Gen III escolhe golpes **ao acaso**, então o oponente real é igual ou mais fraco que o simulado. Se o app diz "favorável", continua favorável no jogo.
- **Alternativa descartada na V1:** média dos golpes do oponente (mais fiel à IA aleatória). Ela transforma os turnos em "valor esperado" em vez de faixa exata, o que contraria a regra de não mostrar precisão falsa. Volta a ser considerada na V4 (7.4).

✅ / ❌ / 💬 — Confirma que selvagens na Gen III escolhem golpes ao acaso (sem preferir o super efetivo)? Isso sustenta a decisão.

### 6.5 Critério de vitória (quem leva vantagem) — **EM ABERTO**

A V1 não usa **Speed**. A pergunta é como decidir a vantagem só com os turnos para KO.

**Opção A — faixas que não se cruzam; empate = Equilibrado (provisório no código)**
- **Favorável:** o pior caso do candidato derruba **antes** do melhor caso do oponente. Quem precisa de menos turnos vence **independente de quem ataca primeiro**, então não depende da Speed.
- **Desfavorável:** o inverso.
- **Equilibrado:** as faixas se cruzam ou empatam. O resultado depende dos rolls ou de quem é mais rápido.
- Candidato que não causa dano → Desfavorável. Oponente que não causa dano → Favorável.

**Opção B — Speed como desempate**
- Mesma regra, mas num **empate exato** quem é mais rápido ataca primeiro e vence.
- Faixas que se cruzam continuam Equilibrado (Speed não resolve rolls diferentes).
- Prioridade (Quick Attack) e empate de Speed (no jogo, sorteio) continuam de fora.

**Exemplo: Rattata (Tackle) vs Pidgey (Tackle), Route 1**

| Cenário | Candidato derruba em | Oponente derruba em | Opção A | Opção B |
|---|---|---|---|---|
| Pessimista (Rattata L2 vs Pidgey L5) | 5–7 turnos | 2 turnos | Desfavorável | Desfavorável |
| Neutro (L3 vs L3) | 3 turnos | 3 turnos | **Equilibrado** | **Favorável** (Speed 9 vs 8) |
| Otimista (L5 vs L2) | 2 turnos | 5–7 turnos | Favorável | Favorável |

A diferença aparece **só nos empates exatos**, e eles são comuns no começo do jogo (HP e dano pequenos).

💬 Ver perguntas 4, 5 e 6 na seção 10.

### 6.6 Precisão — decidido fora da V1 (registrado para discussão)

- A V1 assume que **todo golpe acerta**. A tela precisa deixar isso claro.
- **A favor:** turnos seguem exatos e explicáveis; nenhuma probabilidade "inventada".
- **Contra:** golpes fortes e imprecisos parecem melhores do que são (Hydro Pump 80%, Fire Blast 85%, Thunder 70%, Sing 55%…). Um "KO garantido" com golpe de 90% falha 1 vez em 10.
- **Opções para depois:** desempate por precisão (6.3); mostrar a precisão ao lado do resultado sem mudar a conta; probabilidade real de vitória com acertos e erros (V4).

✅ / ❌ / 💬

### 6.7 Ranking final (implementado na Etapa 7)

Ordem de critérios prevista para escolher os até 5 candidatos:

**Objetivo derrotar:**
```
Acessível na progressão → Matchup → Dano → Sobrevivência → Turnos para KO → Consistência
```

**Objetivo capturar:**
```
Acessível na progressão → Consegue baixar o HP com segurança → Tem golpe de status disponível
→ Chance de captura → Risco de KO → Bolas necessárias
```

**Decidido:** o **jogador escolhe** o cenário que ordena o ranking (pessimista = "mais seguro", neutro = "equilibrado", otimista = "maior potencial"). Os três cenários continuam aparecendo para cada candidato.

Na V1, o critério "tem golpe de status disponível" fica de fora: o pacote de dados ainda não traz golpes de status. "Bolas necessárias" acompanha a chance por bola, então não muda a ordem.

💬 Ver perguntas 7 e 8 na seção 10.

### 6.8 Quem entra como candidato e com quais golpes (Etapa 7)

Escolhas nossas ao montar a lista de candidatos:

- **Só entra quem o jogador já consegue pegar:** áreas e métodos de encontro liberados no ponto de progressão informado, e só da versão escolhida (um exclusivo de LeafGreen nunca aparece em FireRed).
- **Um encontro por espécie:** se a espécie aparece em várias áreas liberadas, usamos a de **nível mais alto** (o exemplar mais forte que dá para pegar).
- **Golpes do candidato:** os que ele aprende por nível até o **menor nível** do encontro, mais as **TMs/HMs que o jogador já obteve**. Golpes de ovo e de tutor ficam de fora.
- **Golpes do alvo selvagem:** todos os que ele aprende por nível até o **maior nível** do encontro (conservador). No jogo, um selvagem só tem os **4 últimos** golpes aprendidos; ainda não aplicamos essa regra porque o pacote não traz golpes de status, que ocupam esses espaços.
- **Condições de encontro ignoradas:** os três iniciais aparecem como candidatos (no jogo você escolhe só um), assim como Pokémon de troca ou que dependem de evento da história. A progressão curada é que decide.
- **Golpes de vários acertos removidos** (Double Kick, Fury Attack, Bullet Seed, Pin Missile, Beat Up…): a fórmula da V1 só calcula um acerto, e tratá-los como um acerto escondia o risco de KO na captura. Eles simplesmente não aparecem como opção.

✅ / ❌ / 💬 — Ver perguntas 11 a 13 na seção 10.

### 6.9 Escolha do alvo (Etapa 7b)

O jogador busca o **Pokémon** (ou a rota) num campo só. Se o alvo aparece em mais de um lugar liberado, ele escolhe vendo método, nível e chance (ex.: `Route 24 · Grama · nv. 20–22`). O nível do alvo nos cenários vem desse par (área, método), não de todos os encontros da área.

- **Itens de encontro não são perguntados:** varas e Surf liberam no marco em que o item é obtido na história. Depois disso, o encontro aparece com a tag de requisito (ex.: `Requer Super Vara`), mesmo que o jogador possa não ter pego o item.
- **Só selvagens na V1:** presente, estático e troca não aparecem como alvo.

✅ / ❌ / 💬 — Ver perguntas 18 e 19 na seção 10.

### 6.10 Marcos de progressão (Etapa 8.1)

A pergunta "até onde você chegou?" usa 17 marcos, em ordem: Início, Pokédex, Insígnias Boulder e Cascade, S.S. Anne (Cut), Insígnias Thunder e Rainbow, Silph Scope, Poké Flute, Insígnia Soul (com Surf), Insígnias Marsh e Volcano, Tri-Pass, Insígnia Earth, Elite Four, Rainbow Pass e Pokédex Nacional. Cada área, método de encontro e TM/HM fica disponível a partir de um marco. Locais das TMs/HMs conferidos na Serebii.

Escolhas nossas que precisam de conferência:

- **A ordem é uma só.** O jogo permite variar (Koga antes ou depois de Sabrina, por exemplo); usamos a ordem mais comum.
- **Tudo que dá para alcançar conta, mesmo opcional:** Celadon (Mart, Game Corner) libera logo depois do Cut, pelo Rock Tunnel. As TMs do Game Corner (Ice Beam, Thunderbolt...) entram nesse marco, mesmo custando muitas fichas.
- **Varas:** Old Rod com a Insígnia Cascade (Vermilion). Good Rod e Super Rod com a Poké Flute: Fuchsia e a casa da Route 12 ficam depois do Snorlax.
- **Surf** junto com a Insígnia Soul: o HM03 está no Safari Zone, aberto antes de Koga, e só se usa fora de batalha com essa insígnia. Como golpe em batalha, o HM03 conta desde a Poké Flute. Não criamos marco separado para Strength: o HM04 vem do mesmo Safari Zone e a Insígnia Rainbow, que libera o uso, vem antes.
- **Rock Smash** com o Tri-Pass (HM06 em One Island).
- **Saffron** (Dojo, Silph Co., TM29 Psychic) só com a Insígnia Marsh. Dá para entrar antes, com o chá de Celadon; escolhemos o marco mais tarde por segurança.
- **Ilhas Sevii:** Ilhas 1–3 com o Tri-Pass; Ilhas 4–7 e o Ruby Path do Mt. Ember com o Rainbow Pass. A Cerulean Cave e as câmaras de Tanoby (Unown) ficam na Pokédex Nacional.
- **Fora da lista:** Altering Cave B–I (mudam só com evento), Birth Island, Navel Rock e lendários errantes.

✅ / ❌ / 💬 — Ver perguntas 20 e 21 na seção 10.

---

## 7. Projeções de dano: o que a tela mostra

### 7.1 Formas de olhar o roll

O cenário diz **quais condições** assumimos. A variação de sorte é tratada separadamente, por um destes modos:

| Modo | Mostra |
|---|---|
| Mínimo | o menor dano possível (roll 85) |
| Máximo | o maior dano possível (roll 100) |
| Média | a média dos 16 rolls, respeitando a distribuição real (que não é uniforme depois dos truncamentos) |
| Faixa completa | mínimo, máximo **e** os 16 valores |

Assim dá para combinar, por exemplo, "cenário neutro + faixa completa" ou "cenário pessimista + dano mínimo" sem misturar as duas coisas.

### 7.2 Formas de olhar o crítico

| Modo | Comportamento |
|---|---|
| Nunca | ignora crítico |
| Normal | a faixa vai do menor dano sem crítico até o maior dano **com** crítico |
| Sempre | todo golpe é crítico |
| Avaliar probabilidade | mostra a faixa sem crítico e a faixa com crítico separadas, mais a chance (1/16 ou 1/8). A média pondera as duas |

Exemplo (Charmander L10 Ember → Bulbasaur L10): sem crítico **17–20**, com crítico **35–42**, chance de crítico 1/16.

✅ / ❌ / 💬 — Para o jogador comum, qual modo de crítico deveria ser o padrão da tela? Nossa inclinação: "Avaliar probabilidade", mostrando o crítico como informação extra e não misturado na faixa principal.

### 7.3 Faixa de dano **não é** chance de vitória

"79–81 de 100 HP" significa que o golpe tira 79, 80 ou 81. Mostramos também como **79%–81% do HP**. Isso **não** é "80% de chance de vencer". A chance de vencer depende da batalha inteira (quem ataca primeiro, erros, críticos, troca de golpes). Na V1, o app não mostra chance de vitória.

### 7.3.1 Como a tela mostra cada número (Etapa 8)

Cada candidato aparece num card com os **três cenários lado a lado**. O 1º da lista (a ordem vem do ranking) ganha borda verde e o selo **★ Melhor escolha**. Em cada cenário:
- **em destaque:** o resultado (**Favorável**, **Equilibrado** ou **Desfavorável**), o rótulo de KO e, em número grande, **quantos golpes seus derrubam o alvo** ("3–4 golpes para KO"). Quanto menor, melhor. A cor do número segue o resultado (verde, preto ou vermelho);
- **como detalhe**, escondido até o jogador clicar em "Karate Chop · detalhes", uma linha para cada lado: o golpe, o dano e, entre parênteses, a fatia do HP. Exemplo: "Você: Karate Chop 8–10 de 25 HP (32%–40% do HP do alvo)" e "Oponente: Tackle 3–4 de 20 HP (15%–20% do seu HP) · 5–7 golpes para KO". O texto sempre diz **de quem** é o HP. O "HP que sobra" saiu da tela porque era a mesma conta escrita de outro jeito;
- o percentual no objetivo derrotar é só essa **fatia do HP**, sempre em faixa. A barra embaixo do número grande mostra a fatia do HP do alvo: cheia até o roll mais fraco, mais clara até o mais forte, e limitada a 100%. **Não** é chance de vencer, ao contrário do mockup antigo (48% / 76% / 94%), que tinha números ilustrativos. Rodapé fixo: "Faixa de dano não é chance de vitória. A V1 considera que todo golpe acerta e não olha Speed".
- **Capturar:** o número grande vira **golpes seguros**: quantas vezes o golpe de preparo pode ser usado sem nunca derrubar o alvo, **crítico incluído** ("2 golpes seguros"; "Nenhum golpe seguro" quando um golpe já pode derrubar). A barra e os detalhes passam a mostrar o golpe de preparo. Abaixo, fechado em "Preparação e Poké Balls · 62,4% por bola" (o título já traz a chance da Poké Ball) até o jogador clicar: o golpe de preparo com quantas vezes usar ("2× Tackle"), o risco de KO (%), o HP do alvo e uma tabela com a **chance por bola** numa coluna e **quantas bolas para 50/90/95/99%** em outras.

O jogador escolhe a ordem com três botões: **Mais seguro** (cenário pessimista), **Equilibrado** (neutro) e **Maior potencial** (otimista).

✅ / ❌ / 💬 — Ver perguntas 14, 15 e 16 na seção 10.

### 7.4 Projeção para o futuro (V4): chance de vitória por simulação

Quando a simulação passar a ser **turno a turno** (com Speed, precisão, crítico e roll sorteados a cada golpe), listar todas as sequências possíveis fica inviável. O plano é o **método de Monte Carlo**: simular a mesma luta milhares de vezes, sorteando tudo, e contar quantas o candidato vence.

- Continua havendo **três cenários**; cada um ganha uma chance de vitória, além das faixas.
- O erro é mensurável. Com 10.000 lutas simuladas, a margem fica em cerca de ±1 ponto percentual.

| Lutas simuladas | Margem (95% de confiança) |
|---|---|
| 100 | ±10% |
| 1.000 | ±3% |
| 10.000 | ±1% |

- Exibição sugerida: faixa (`72%–74%`) e uma nota curta, "estimado a partir de 10.000 batalhas simuladas".

✅ / ❌ / 💬

---

## 8. Captura: fórmula e projeções

Captura é tratada como um problema **separado** do dano. A conta é a do jogo, com inteiros e truncamento a cada passo.

### 8.1 Fórmula (Gen III, conferida no código do FireRed)

```
1. X = Catch rate × Bola / 10                  Poké Ball 10, Great 15, Ultra 20
2. X = X × (3 × HP máx − 2 × HP atual) / (3 × HP máx)
3. Status:  Sleep ou Freeze → X × 2
            Paralysis, Poison (inclui Toxic) ou Burn → X × 1,5
4. Se X > 254 → captura garantida (nem balança)
   Master Ball → sempre captura (o jogo pula a fórmula)
5. Y = 1048560 / raiz( raiz( 16711680 / X ) )   (raiz inteira, truncada)
6. O jogo sorteia 4 números de 0 a 65535; se os 4 forem menores que Y, captura.
   Chance por bola = (Y / 65536)⁴
```

Pontos que conferimos e que gostaríamos que você validasse:
- **Ordem dos truncamentos:** primeiro a bola, depois o HP, depois o status. Exemplo: catch rate 45, Great Ball, alvo com 1/10 do HP. Jogo: 45 × 15 / 10 = **67**; 67 × 28 / 30 = **62**. Uma fórmula "tudo junto" daria 63.
- **Status Gen III:** Sleep e Freeze ×2; Paralysis, Poison e Burn ×1,5 (na Gen V+ Sleep virou ×2,5; aqui não).
- **Usamos a fórmula real das 4 verificações**, não a aproximação "X / 255" que aparece em alguns guias. As duas dão números bem diferentes.

**Passo a passo de um caso:** Bulbasaur (catch rate 45), Poké Ball, HP cheio 30/30, sem status.
- X = 45 × 10 / 10 = 45 → 45 × (90 − 60) / 90 = **15**
- 16711680 / 15 = 1114112 → raiz = 1055 → raiz = 32 → Y = 1048560 / 32 = **32767**
- Chance por bola = (32767 / 65536)⁴ ≈ **6,25%** (cerca de 1 em 16)

✅ / ❌ / 💬

### 8.2 Duas métricas diferentes: por bola e acumulada

- **Chance por bola:** a chance de **cada** arremesso. É fixa enquanto HP e status não mudam.
- **Chance acumulada:** a chance de ter capturado **até** a bola N. Fórmula: 1 − (1 − p)ᴺ.
- O app mostra **quantas bolas** são necessárias para chegar a 50%, 90%, 95% e 99% de chance acumulada.

"6,25% por bola" **não** é a mesma coisa que "95% em 47 bolas". As duas aparecem, com nomes diferentes.

### 8.3 Casos testados

| Caso | X | Chance por bola | Bolas para 50% / 90% / 95% / 99% |
|---|---|---|---|
| Bulbasaur (45), Poké Ball, HP cheio | 15 | 6,25% | 11 / 36 / 47 / 72 |
| Pikachu (190), Great Ball, 30/35 HP, paralisado | 183 | 78,46% | 1 / 2 / 2 / 3 |
| Snorlax (25), Ultra Ball, metade do HP (95/190), dormindo | 66 | 27,97% | 3 / 8 / 10 / 15 |
| Mewtwo (3), Ultra Ball, 1 HP (1/250) | 5 | 2,11% | 33 / 109 / 141 / 217 |
| Pikachu (190), Ultra Ball, 7/35 HP, dormindo | 658 | garantida | 1 / 1 / 1 / 1 |

Também testados: cada status, a Master Ball, o limite exato da captura garantida (X = 254 ainda balança; X = 255 não) e a menor chance possível (≈ 0,42% por bola).

✅ / ❌ / 💬 — Esses números batem com a sua experiência? Em especial o Mewtwo: ~2% por Ultra Ball a 1 HP sem status, ~217 bolas para 99%.

### 8.4 Risco de KO (para o objetivo capturar)

No objetivo **capturar**, bater forte demais é ruim: derrubar o alvo perde a captura. Por isso calculamos o **risco de KO**: a chance de um golpe derrubar o alvo em vez de só enfraquecer.

- Conta: quantos dos 16 rolls derrubam ÷ 16, ponderado pela chance de crítico.
- É uma **probabilidade calculada** (os 16 rolls têm a mesma chance), então pode ser mostrada como número.

**Exemplo:** Charmander L10 usando Ember em Bulbasaur L10 (rolls 17 a 20). Se o Bulbasaur está com 19 de HP, 6 dos 16 rolls sem crítico chegam a 19 ou mais: risco **37,5%** (sem contar crítico; com crítico, que sempre derruba aqui, sobe um pouco).

Consequência para o ranking: um candidato que deixa o alvo com 19–21 de HP e arrisca KO no próximo golpe pode ser **pior** para capturar do que outro que causa menos dano, mesmo sendo pior para derrotar.

**Decidido para a V1** (Etapa 7, revisto na Etapa 8): o candidato usa o golpe de preparo **todas as vezes que for seguro** e depois começa a jogar bola. "Seguro" quer dizer que nem o maior dano possível, **crítico incluído**, somado em todos os golpes, derruba o alvo: são (HP − 1) ÷ maior dano golpes. Entre os golpes que têm pelo menos 1 uso seguro, fica o que deixa **menos HP** no alvo. Se nenhum é seguro nem uma vez, o candidato dá **um golpe** com o de menor risco. Exemplo: Tackle tira 3–4 (7–9 com crítico) de um Pidgey com 20 HP: 19 ÷ 9 = **2 golpes seguros**, e o alvo fica com no máximo 20 − 2×3 = 14 HP. A captura é calculada com o HP que sobra no **pior roll** (o que tira menos HP), sem status, para Poké, Great e Ultra Ball. O ranking usa a Poké Ball.

✅ / ❌ / 💬 — Ver pergunta 9 na seção 10.

### 8.5 Limites da captura na V1

- Bolas: **só Poké, Great, Ultra e Master.** Net, Nest, Repeat, Timer, Luxury, Premier e Dive ficam pendentes até confirmarmos onde se obtêm no FRLG (os multiplicadores já foram tirados do código do jogo).
- **Safari Zone fora:** lá o catch rate muda com isca e pedra, é outra mecânica.
- Golpes como False Swipe ainda não são tratados de forma especial: entram como golpe comum, com risco de KO calculado pela fórmula normal.
- **Estratégia de captura da V1** (8.4): todos os golpes seguros (crítico incluído) do golpe que deixa menos HP; se nenhum é seguro, um golpe com o menor risco; captura no HP do pior roll, sem status. Sem golpes de status, porque o pacote de dados ainda não os traz.

---

## 9. O que está fora da V1 (e por quê)

| Fora da V1 | Por quê |
|---|---|
| Speed | Em decisão (6.5). Sem ela, empates ficam "Equilibrado" |
| Precisão | Todo golpe acerta (6.6) |
| Habilidades (exceto imunidades, pendentes) | Complexidade; 5.6 |
| Itens segurados | V1 é sobre selvagens recém-capturados |
| Burn reduzindo Attack, Reflect/Light Screen, clima, Flash Fire | Fora do escopo da V1 |
| Mudança de stat no meio da luta (Growl, Swords Dance, Leer…) | Não há simulação turno a turno na V1 |
| Golpes de dano fixo (Seismic Toss, Night Shade, Dragon Rage, Sonic Boom, Super Fang) | Não usam a fórmula comum; removidos do pacote de dados |
| Golpes de poder variável ou múltiplos acertos (Low Kick, Return, Double Kick, Fury Attack…) | Idem; removidos do pacote de dados (6.8) |
| Golpes de status (Sleep Powder, Thunder Wave, Growl…) | Ainda não importados; por isso a captura não considera status |
| Bônus de insígnia do FRLG (+10% em Attack com a Boulder, Defense com a Soul, Sp. Atk/Sp. Def com a Volcano) | Depende das insígnias do jogador; decidir quando a progressão estiver pronta |
| Synchronize (50% de chance de o selvagem vir com a mesma Nature) | V2 ou depois |
| Treinadores e líderes de ginásio | V3 |
| Troca de Pokémon, vários Pokémon, turno a turno | V4 |

✅ / ❌ / 💬 — Algum desses itens muda tanto o resultado no FRLG que você considera **obrigatório** já na V1? O bônus de insígnia e o Intimidate são os que mais nos preocupam.

---

## 10. Perguntas para você

**Cenários**
1. Quando o app diz "pessimista", você espera **"o pior que costuma acontecer"** (IV 8, opção A) ou **"o pior possível"** (IV 0, opção B)?
2. Você prefere um pessimista que às vezes falha, mas é útil para comparar (A), ou um que nunca falha, mesmo que exagere na maioria das vezes (B)?
3. O nível do cenário neutro deveria ser a média da faixa (atual) ou o nível mais frequente do encontro?

**Simulação e desempates**
4. Num empate de turnos, você prefere ver **"Equilibrado"** (honesto sobre a incerteza) ou o app **decidir pela Speed**?
5. Se a Speed entrar, a Speed do selvagem também deve variar por cenário (IV e Nature), mesmo que isso inverta o desempate entre cenários?
6. Vale incluir prioridade (Quick Attack, Extreme Speed) já na V1?
7. ~~Qual cenário deveria ordenar o Top 5?~~ **Decidido:** o jogador escolhe (6.7).
8. Na escolha do melhor golpe, a precisão deveria desempatar antes do dano máximo? Algum outro critério (PP, efeito secundário, recuo) faz falta?

**Captura**
9. Para o objetivo capturar, o que é mais importante no ranking: **menos bolas** ou **menos risco de derrubar o alvo**? Hoje o app põe na frente quem **nunca derruba** o alvo e só depois olha a chance por bola. Um Pokémon com golpe de Sleep (Sleep Powder, Hypnosis, Sing) deveria passar na frente de um que só bate?
10. Faz falta tratar False Swipe já na V1 (golpe que nunca derruba, sempre deixa 1 HP)?

**Candidatos e golpes (6.8)**
11. Faz sentido usar, para cada espécie, o encontro de **nível mais alto** já liberado? Ou o jogador costuma usar o que pegou primeiro?
12. O candidato deveria contar com golpes que aprende **subindo de nível** depois de capturado, ou só os que já sabe ao ser pego (como hoje)?
13. Os três iniciais deveriam aparecer juntos como candidatos, ou o jogador informa qual escolheu?

**Tela (7.3.1)**
14. Os nomes **Mais seguro / Equilibrado / Maior potencial** deixam claro o que cada ordem faz? E **Pessimista / Neutro / Otimista** nos cards?
15. O rótulo "Equilibrado" aparece em dois lugares com sentidos diferentes: como **resultado** de um cenário (faixas de turnos que se cruzam) e como **ordem** das sugestões (cenário neutro). Isso confunde? Sugestão nossa: renomear a ordem para "Cenário médio".

16. O número grande de cada cenário é **quantos golpes você precisa para derrubar o alvo**. É isso que você olharia primeiro para saber se a escolha vale a pena, ou seria outro número (ex.: quantos golpes o oponente precisa para te derrubar)?

17. ✅ **Decidido:** na captura, a chance das bolas agora é calculada depois de **todos os golpes seguros**, não só de 1. O custo é passar mais turnos apanhando do oponente, o que o app ainda não pesa. Você concorda, ou o jogador prefere menos turnos mesmo com bolas piores?

**Escolha do alvo (6.9)**
18. Mostrar o encontro de vara assim que a vara fica obtenível (com a tag "Requer"), mesmo para quem pulou o item, é melhor do que esconder? Em que marco você considera cada vara e o Surf obtidos em FR/LG?
19. Faz falta poder escolher Pokémon de presente, estático (Snorlax, lendários) ou troca como alvo já na V1?

**Marcos de progressão (6.10)**
20. A ordem dos marcos bate com o jeito que você joga? Algum marco faz falta ou sobra (ex.: um marco só para Strength)?
21. Liberar o Game Corner e o Celadon Mart logo depois do Cut está certo, ou o jogador comum só chega lá depois da Insígnia Thunder?

**Geral**
22. Algum caso de jogo que você conhece de cor (ex.: "Charmander L5 contra Brock") e que gostaria de ver o app calcular, para conferir?

---

## 11. Resumo leigo

**O que o app faz:** você diz qual Pokémon quer enfrentar (para derrotar ou capturar) e até onde chegou no jogo. O app sugere até 5 Pokémon que você já consegue pegar naquele ponto, que se saem bem nessa luta.

**Como ele faz as contas de dano:** copiamos as regras do próprio FireRed, incluindo os arredondamentos esquisitos que o jogo faz no meio da conta. Conferimos com a calculadora do Showdown em vários confrontos e os números bateram exatamente.

**Por que três resultados em vez de um:** um Pokémon selvagem pode ter vindo "bom" ou "ruim" (Nature, IVs, nível). Em vez de chutar, o app mostra três situações: **azar** (seu Pokémon veio fraco e o oponente forte), **normal** e **sorte**. Você decide quanto risco quer correr.

**Como ele escolhe o golpe:** para cada lado, o golpe que **garante** derrubar o oponente com menos ataques, mesmo com azar no dano. Se empatar, o que pode terminar mais rápido com sorte; se empatar de novo, o mais forte. O oponente também usa o melhor golpe dele, o que deixa o app do lado da cautela (no jogo, selvagens escolhem golpes ao acaso).

**Como ele decide quem leva vantagem:** se você derruba o oponente em menos ataques do que ele precisa para te derrubar, mesmo no seu pior caso e no melhor dele, é **Favorável**. O contrário é **Desfavorável**. Se depende de sorte ou de quem é mais rápido, é **Equilibrado**. Por enquanto o app não olha a Speed nem considera que golpes podem errar.

**Captura:** usamos a fórmula de verdade do jogo (as 4 "balançadas"), não a versão simplificada que aparece em alguns guias. O app mostra duas coisas diferentes: a chance de **cada** bola e **quantas bolas** você precisa para ter 50%, 90%, 95% ou 99% de chance de já ter capturado. Também avisa o **risco de derrubar** o Pokémon sem querer quando você bate nele para enfraquecer.

**O que ainda falta decidir com você:**
- se o cenário de azar deve ser "azar comum" ou "o pior azar possível";
- se a Speed deve desempatar lutas;
- para captura, se vale mais gastar menos bolas ou arriscar menos derrubar o alvo (hoje: arriscar menos vem primeiro);
- se o candidato conta só com os golpes que já sabe ao ser pego, e se os três iniciais aparecem juntos;
- se os nomes na tela ("Mais seguro", "Equilibrado", "Maior potencial") estão claros;
- se "golpes para derrubar o alvo" é o melhor número para ficar em destaque em cada cenário.

**Já decidido:** você escolhe qual dos três cenários (azar, normal, sorte) ordena a lista. Na captura, o app bate todas as vezes que for seguro (nem crítico derruba) antes de jogar bola.

**O que fica para depois:** habilidades, itens, golpes que erram, Speed, troca de Pokémon, treinadores e ginásios. A ideia é começar com o que dá para calcular com certeza e ir adicionando.

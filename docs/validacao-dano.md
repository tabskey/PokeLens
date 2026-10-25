# Validação do cálculo de dano

Registro de como o `DamageCalculator` (Etapa 3, branch `X007Damage`) foi conferido, do que foi encontrado e do que ainda falta. Ler junto com `arquitetura.md` §13 (fórmula) e `fases.md` Fase 1 (políticas).

---

## 1. Fontes de referência

| Fonte | O que é | Como foi usada | Peso |
|---|---|---|---|
| **Decompilação do FireRed** (`pret/pokefirered`, GitHub) | O código do jogo reconstruído a partir da ROM | Leitura de `CalculateBaseDamage` (`src/pokemon.c`), `Cmd_critcalc`, `Cmd_damagecalc`, `Cmd_typecalc`, `ModulateDmgByType` e `ApplyRandomDmgMultiplier` (`src/battle_script_commands.c`) e da tabela `gTypeEffectiveness` (`src/battle_main.c`) | **Referência final.** Em caso de conflito, vale o jogo |
| **`@smogon/calc` 0.12.0** (npm) | O motor da calculadora de dano do Pokémon Showdown | Instalado localmente num diretório temporário fora do repo e rodado com um script (seção 6), modo Gen 3 | Segunda opinião independente. Tem aproximações conhecidas (seção 3) |

O pacote **não** foi adicionado ao repositório: não há dependência npm no backend nem na esteira.

## 2. O que a fonte do jogo definiu

- **Ordem das operações:** base → crítico ×2 → STAB ×15/10 → tipo (entrada por entrada) → random 85..100. A fórmula antiga do `arquitetura.md` tinha outra ordem e foi corrigida.
- **Piso de 1 no dano base só no ramo físico.** O ramo especial não tem essa linha. Um golpe especial muito fraco tem base 2, um físico tem base 3.
- **Tabela de tipos aplicada na ordem do jogo.** Para cada entrada de `gTypeEffectiveness` que casa com um tipo do defensor, faz `d = d × mult / 10` e trunca. Contra tipo duplo com 2x e 0.5x, a ordem muda o resultado.
- **Chance de crítico:** tabela `{16, 8, 4, 3, 2}`. Estágio 0 = 1/16 e golpe de crítico alto = 1/8.
- **Bônus de insígnia** (Boulder, Soul e Volcano, +10% para o lado do jogador): existe no FRLG e ficou registrado como pendência (`arquitetura.md` §23).

### Bug encontrado na Etapa 2

Comparei automaticamente o `TypeEffectivenessService` com `gTypeEffectiveness`. **Faltavam Ghost→Psychic (2x) e Ice→Grass (2x).** As outras 108 entradas batiam. Corrigido em `X007Damage`, com teste para as duas entradas. A tabela foi regenerada a partir da fonte, na mesma ordem.

## 3. Resultado da comparação com o oráculo

Stats derivados pelo próprio `@smogon/calc` (EV 0, sem item, IV e natureza indicados) e usados como entrada do `DamageCalculator`. Comparação feita nos **16 rolls**, sem crítico e com crítico.

| # | Confronto | Cobre | Resultado |
|---|---|---|---|
| 1 | Rattata L5 Tackle → Pidgey L5 | Físico, STAB, neutro | Igual (5–7; crítico 12–15) |
| 2 | Charmander L10 Ember → Bulbasaur L10 | Especial, STAB, 2x | Igual (17–20; crítico 35–42) |
| 3 | Squirtle L14 Water Gun → Geodude L14 | Especial, STAB, 4x | Igual (51–60; crítico 102–120) |
| 4 | Raticate L20 Bite → Gastly L20 | Bite é Dark, portanto **especial**; sem STAB, 2x | Igual (27–32; crítico 54–64) |
| 5 | Bulbasaur L15 Vine Whip → Charmander L15 | Especial, STAB, 0.5x | Igual (5–6; crítico 10–12) |
| 6 | Mankey L12 Karate Chop → Pidgey L12 | Físico, STAB, 2x e 0.5x no tipo duplo | Igual (16–19; crítico 33–39) |
| 7 | Lapras L26 Ice Beam → Moltres L50 | Ordem da tabela de tipos | **Diverge**: jogo 16–19, Showdown 15–18 |

**6 de 6 confrontos iguais, valor por valor**, nos casos em que o oráculo segue o jogo.

**O caso 7 é limitação do oráculo, não erro nosso.** O `@smogon/calc` ordena a efetividade pelo **tipo do defensor**, numa lista fixa (Fire antes de Flying). O jogo percorre a tabela **por tipo atacante**, e na linha de Ice a entrada Flying vem antes de Fire:
- Jogo: 19 × 20/10 = 38 → 38 × 5/10 = **19** → rolls 16–19.
- Showdown: 19 × 0.5 = 9 → 9 × 2 = **18** → rolls 15–18.

O teste desse caso usa o cálculo manual, citando a fonte do jogo.

## 4. Erros encontrados durante a validação

Registrados por transparência. Nenhum estava no código de produção:
- **Script de conferência:** foi aplicado um multiplicador de tipo errado no caso 1 (Tackle). O resultado não bateu com o oráculo e a investigação mostrou que o erro era do script, não do calculador.
- **Valor esperado escrito à mão:** a soma dos 16 rolls do caso 2 foi anotada como 293 em vez de 290. O teste falharia, e o valor foi corrigido antes de a suíte rodar. O código estava certo; o número digitado no teste estava errado.

É o motivo de os valores esperados virem de cálculo conferido em duas fontes, e não só de conta à mão.

## 5. O que os números significam (e o que não significam)

- **"6 de 6 iguais"** quer dizer igualdade exata nos 16 rolls de cada confronto, não "erro pequeno".
- **99,65% é cobertura de testes do Core** (linhas executadas pelos testes), não taxa de acerto.
- **Seis confrontos são uma amostra.** Cobrem os ramos principais (físico, especial, STAB, 0x, 0.5x, 2x, 4x, misto, crítico, crítico alto, pisos de 1), mas não provam todos os casos.
- **Não houve teste em emulador.** A referência final é a decompilação e o Showdown é a segunda opinião.

## 6. Como reproduzir

Num diretório temporário, **fora do repositório**:

```sh
npm init -y
npm i --ignore-scripts @smogon/calc@0.12.0
node run.js
```

`run.js`:

```js
const {calculate, Generations, Pokemon, Move} = require('@smogon/calc');
const gen = Generations.get(3);
const zero = {hp: 0, atk: 0, def: 0, spa: 0, spd: 0, spe: 0};
const iv = v => ({hp: v, atk: v, def: v, spa: v, spd: v, spe: v});
const mon = (name, level, ivs, nature) => new Pokemon(gen, name, {level, ivs: iv(ivs), evs: zero, nature});

// [atacante, nível, IV, natureza, defensor, nível, IV, natureza, golpe]
const cases = [
  ['Rattata', 5, 20, 'Hardy', 'Pidgey', 5, 10, 'Hardy', 'Tackle'],
  ['Charmander', 10, 31, 'Modest', 'Bulbasaur', 10, 15, 'Hardy', 'Ember'],
  ['Squirtle', 14, 25, 'Hardy', 'Geodude', 14, 10, 'Hardy', 'Water Gun'],
  ['Raticate', 20, 15, 'Hardy', 'Gastly', 20, 20, 'Hardy', 'Bite'],
  ['Bulbasaur', 15, 31, 'Hardy', 'Charmander', 15, 31, 'Hardy', 'Vine Whip'],
  ['Mankey', 12, 18, 'Adamant', 'Pidgey', 12, 5, 'Hardy', 'Karate Chop'],
  ['Lapras', 26, 10, 'Hardy', 'Moltres', 50, 31, 'Hardy', 'Ice Beam'],
];

for (const [a, al, ai, an, d, dl, di, dn, m] of cases) {
  const A = mon(a, al, ai, an), D = mon(d, dl, di, dn);
  const r = calculate(gen, A, D, new Move(gen, m));
  const rc = calculate(gen, A, D, new Move(gen, m, {isCrit: true}));
  console.log(`${a} ${JSON.stringify(A.stats)} vs ${d} ${JSON.stringify(D.stats)} — ${m}`);
  console.log(`  normal: ${r.damage}`);
  console.log(`  crit:   ${rc.damage}`);
}
```

Os stats impressos são os usados como `Combatant` em `tests/PokeBattle.Core.Tests/Damage/DamageCalculatorTests.cs`.

---

## 7. Próximo passo: teste de confronto em massa (diferencial)

Objetivo: sair da amostra de 6 casos para **centenas ou milhares de confrontos**, comparados automaticamente com o `@smogon/calc`.

### Quando
Depois da **Etapa 6** (pacote de dados FRLG), para usar espécies, base stats e golpes reais e normalizados para a Gen III. Antes disso daria para usar só stats aleatórios, mas o teste com dado real também valida o normalizador.

### Como
1. Um gerador (no próprio teste .NET, com seed fixa) sorteia confrontos: espécies e golpes de dano do pacote, nível 2–100, IV 0–31, natureza, crítico sim/não.
2. Um script Node com `@smogon/calc` versionado calcula os mesmos confrontos e grava `expected.json`. O arquivo é gerado offline e **commitado**, como o pacote de dados; a esteira não roda npm.
3. Um teste xUnit lê `expected.json` e compara os 16 rolls de cada confronto com o `DamageCalculator`.
4. **Exclusões conhecidas**, filtradas no gerador e listadas no teste:
   - golpe com 2x e 0.5x num defensor de tipo duplo em que a ordem da tabela do jogo difere da ordem por defensor do Showdown (caso 7). Esses casos ficam cobertos por testes manuais;
   - golpes de dano fixo ou poder variável (fora do `DamageCalculator` na V1);
   - habilidades e itens que o Showdown aplicaria e a V1 não modela (forçar habilidade neutra e nenhum item).
5. Qualquer divergência fora das exclusões **falha o teste** e precisa ser explicada (bug nosso, bug do oráculo ou nova exclusão documentada aqui).

### Decisões pendentes
- [ ] Tamanho da amostra (sugestão: 2.000 confrontos, seed fixa)
- [ ] Onde fica o script gerador (sugestão: `tools/damage-oracle/`, fora da solution .NET, com versão do `@smogon/calc` fixa no `package.json` e Dependabot desligado para ele, porque mudar o oráculo exige regenerar `expected.json` num PR próprio)
- [ ] Se o mesmo método vale para a captura (Etapa 5); o Showdown não calcula captura, então ali o oráculo seria a decompilação mais cálculo manual

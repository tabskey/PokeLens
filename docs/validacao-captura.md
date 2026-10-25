# Validação do cálculo de captura

Registro de como `CaptureCalculator`, `CaptureProbabilityCalculator` e `KoRiskCalculator` (Etapa 5, branch `X010Capture`) foram conferidos. Mesmo espírito de `validacao-dano.md`. Ler junto com `arquitetura.md` §15.

---

## 1. Fontes

| Fonte | Como foi usada |
|---|---|
| **Decompilação do FireRed** (`pret/pokefirered`) | `Cmd_handleballthrow` e `sBallCatchBonuses` (`src/battle_script_commands.c`); `BALL_3_SHAKES_SUCCESS = 4` (`include/battle_controllers.h`); `Sqrt` é a chamada do BIOS do GBA (`include/gba/syscall.h`), que devolve a raiz inteira truncada |
| **Implementação independente em Python** | Reescrita da mesma fonte, fora do repositório, usada para conferir cada valor esperado dos testes antes de rodar o C# |

**Não há oráculo externo** como o Showdown no dano: o `@smogon/calc` não calcula captura. A validação se apoia no código do jogo, mais duas implementações independentes (C# e Python) que precisam concordar.

## 2. O que a fonte do jogo definiu

```
X = catchRate × bola / 10                   bola em décimos: Poké 10, Great 15, Ultra 20
X = X × (3M − 2H) / (3M)
Sleep/Freeze ×2;  Poison/Burn/Paralysis/Toxic ×15/10
X > 254 → captura garantida;  Master Ball → sempre captura
Y = 1048560 / Sqrt(Sqrt(16711680 / X))
4 verificações: Random() < Y  →  P = (Y / 65536)^4
```

### Diferenças encontradas no `arquitetura.md` antigo (corrigidas)
- **Ordem do truncamento.** O doc usava uma expressão única, `((3M − 2H) × C × B / 3M) × S`. O jogo trunca primeiro a bola, depois a fração de HP, depois o status. Exemplo: catch rate 45, Great Ball, 1/10 HP: o jogo dá **62** (`45×15/10 = 67`; `67×28/30 = 62`), e a expressão única daria **63**.
- **Piso "X < 1 → 1"**: não existe no jogo e não faz falta. Com catch rate ≥ 3 e bola ≥ ×1, X ≥ 1 sempre.
- **Status** multiplica depois da fração de HP, não junto com ela.

## 3. Casos testados

| Caso | X | Y | Chance por bola | Bolas 50 / 90 / 95 / 99% |
|---|---|---|---|---|
| Bulbasaur (45), Poké Ball, HP cheio 30/30 | 15 | 32767 | 6,25% | 11 / 36 / 47 / 72 |
| Pikachu (190), Great Ball, 30/35, paralisado | 183 | 61680 | 78,46% | 1 / 2 / 2 / 3 |
| Snorlax (25), Ultra Ball, 95/190, dormindo | 66 | 47661 | 27,97% | 3 / 8 / 10 / 15 |
| Mewtwo (3), Ultra Ball, 1/250 | 5 | 24965 | 2,11% | 33 / 109 / 141 / 217 |
| Pikachu (190), Ultra Ball, 7/35, dormindo | 658 | — | garantida | 1 / 1 / 1 / 1 |

Também cobertos:
- os limites: X = 254 dá Y = 65535 (ainda há verificações) e X = 255 é garantida; X = 1 dá Y = 16643, a menor chance possível, cerca de 0,42% por bola;
- cada status;
- a Master Ball;
- a raiz inteira em quadrados perfeitos e vizinhos;
- as bolas para p = 19,752% (4 / 11 / 14 / 21);
- o caso exato p = 50%, q = 75%, que precisa dar 2 bolas, sem uma a mais por erro de ponto flutuante.

**Risco de KO:** conferido com os rolls do caso 2 do dano (Charmander usando Ember contra Bulbasaur), com e sem crítico. Exemplo: com 19 de HP, 6 dos 16 rolls derrubam, então o risco é 37,5%.

## 4. O que os números significam

- **Chance por bola** e **chance acumulada** são métricas diferentes (regra 6 do `AGENTS.md`): "6,25% por bola" não é "95% em 47 bolas".
- As duas são **probabilidades calculadas** (as 4 verificações do jogo são independentes), e o risco de KO também (16 rolls equiprováveis). Podem ser mostradas como número.
- O exemplo do Pikachu 7/35 com Ultra Ball e Sleep, citado como ilustração nos docs, é **captura garantida** no jogo. Não serve como exemplo de "63,8% por bola".

## 5. Limites

- Só Poké, Great, Ultra e Master (bolas condicionais pendentes; `arquitetura.md` §23).
- Sem teste em emulador; a referência é a decompilação.
- Safari Zone fora (catch rate muda com isca e pedra).

## 6. Como reproduzir

Implementação de referência em Python, seguindo a fonte do jogo:

```python
from math import isqrt
BALL = {'poke': 10, 'great': 15, 'ultra': 20}

def final_capture_rate(catch_rate, max_hp, hp, ball, status):
    x = catch_rate * BALL[ball] // 10
    x = x * (3 * max_hp - 2 * hp) // (3 * max_hp)
    if status in ('sleep', 'freeze'):
        x *= 2
    if status in ('poison', 'burn', 'paralysis'):
        x = x * 15 // 10
    return x

def shake_threshold(x):
    return None if x > 254 else 1048560 // isqrt(isqrt(16711680 // x))

x = final_capture_rate(45, 30, 30, 'poke', 'none')   # 15
y = shake_threshold(x)                               # 32767
print(x, y, (y / 65536) ** 4)                        # 0.0624...
```

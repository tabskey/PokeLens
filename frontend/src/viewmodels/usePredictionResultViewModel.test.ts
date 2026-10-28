import { describe, expect, it } from 'vitest'

import { response } from '../test/fixtures'
import { range, toResultView } from './usePredictionResultViewModel'

describe('toResultView', () => {
  it('translates labels and shows numbers as ranges', () => {
    const view = toResultView(response())

    expect(view.target).toBe('Geodude')
    expect(view.targetSprite).toBe('https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/versions/generation-iii/firered-leafgreen/74.png')
    expect(view.targetLevels).toBe('Nv. 7–9')
    expect(view.rankedBy).toBe('Equilibrado')
    const [candidate] = view.candidates
    expect(candidate?.where).toBe('Route 22 · Nv. 3–5 · grama/caverna')
    expect(candidate?.scenarios.map((s) => [s.title, s.outcome, s.tone])).toEqual([
      ['Pessimista', 'Equilibrado', 'neutral'],
      ['Neutro', 'Favorável', 'good'],
      ['Otimista', 'Favorável', 'good'],
    ])
    expect(candidate?.scenarios[0]?.attack).toEqual({
      move: 'Karate Chop',
      damage: '8–10 de 25 HP',
      hpShare: '32%–40% do HP do alvo',
      bar: { min: 32, max: 40 },
      hits: '3–4 golpes',
      turns: '3–4 golpes para KO',
    })
    expect(candidate?.scenarios[0]?.opponent?.hpShare).toBe('15%–20% do seu HP')
  })

  it('only uses percentages as shares of HP in defeat results, never as a win chance', () => {
    const view = toResultView(response())

    const percentages = JSON.stringify(view).match(/[^"]*%[^"]*/g) ?? []
    expect(percentages.length).toBeGreaterThan(0)
    expect(percentages.every((text) => /% (do HP do alvo|do seu HP)$/.test(text))).toBe(true)
    expect(view.candidates[0]?.scenarios[0]?.catch).toBeNull()
  })

  it('caps the bar at 100% when one hit takes more than the whole HP', () => {
    const attack = { ...response().candidates[0]!.scenarios[0]!.candidateAttack!, minDamage: 34, maxDamage: 40, targetMaxHp: 28 }
    const base = response()
    base.candidates[0]!.scenarios[0]!.candidateAttack = attack

    const view = toResultView(base).candidates[0]?.scenarios[0]?.attack

    expect(view?.hpShare).toBe('121%–142% do HP do alvo')
    expect(view?.bar).toEqual({ min: 100, max: 100 })
  })

  it('formats capture odds as calculated probabilities, separate from balls needed', () => {
    const plan = toResultView(response('Catch')).candidates[0]?.scenarios[0]?.catch

    expect(plan?.hit?.move).toBe('Karate Chop')
    expect(plan?.hits).toBe('1×')
    expect(plan?.safeHits).toBe('Nenhum golpe seguro')
    expect(plan?.koRisk).toBe('6,3%')
    expect(plan?.balls[0]?.chancePerBall).toBe('62,4%')
    expect(plan?.pokeBallChance).toBe('62,4%')
    expect(plan?.balls[0]?.ballsFor).toEqual([
      { confidence: '50%', balls: 1 },
      { confidence: '90%', balls: 3 },
      { confidence: '95%', balls: 3 },
      { confidence: '99%', balls: 5 },
    ])
  })

  it('has no headline hits when the move does no damage', () => {
    const base = response()
    base.candidates[0]!.scenarios[0]!.candidateAttack = { ...base.candidates[0]!.scenarios[0]!.candidateAttack!, turnsToKoMin: null, turnsToKoMax: null }

    const view = toResultView(base).candidates[0]?.scenarios[0]?.attack

    expect(view?.hits).toBeNull()
    expect(view?.turns).toBe('não causa dano')
  })

  it('counts the safe hits when the plan has no KO risk', () => {
    const base = response('Catch')
    base.candidates[0]!.scenarios[0]!.catch = { ...base.candidates[0]!.scenarios[0]!.catch!, hits: 2, koRisk: 0 }

    const plan = toResultView(base).candidates[0]?.scenarios[0]?.catch

    expect(plan?.safeHits).toBe('2 golpes seguros')
    expect(plan?.hits).toBe('2×')
  })

  it('collapses equal bounds', () => {
    expect(range(2, 2)).toBe('2')
    expect(range(2, 3)).toBe('2–3')
  })
})

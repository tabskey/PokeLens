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
      remainingHp: '15–17 HP',
      turns: '3–4 golpes para KO',
    })
  })

  it('keeps defeat results free of percentages', () => {
    const view = toResultView(response())

    expect(JSON.stringify(view)).not.toContain('%')
    expect(view.candidates[0]?.scenarios[0]?.catch).toBeNull()
  })

  it('formats capture odds as calculated probabilities, separate from balls needed', () => {
    const plan = toResultView(response('Catch')).candidates[0]?.scenarios[0]?.catch

    expect(plan?.koRisk).toBe('6,3%')
    expect(plan?.balls[0]?.chancePerBall).toBe('62,4%')
    expect(plan?.balls[0]?.ballsFor).toEqual([
      { confidence: '50%', balls: 1 },
      { confidence: '90%', balls: 3 },
      { confidence: '95%', balls: 3 },
      { confidence: '99%', balls: 5 },
    ])
  })

  it('collapses equal bounds', () => {
    expect(range(2, 2)).toBe('2')
    expect(range(2, 3)).toBe('2–3')
  })
})

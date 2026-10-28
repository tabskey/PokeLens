import type { AttackResult, CandidateResult, GameOptions, PredictResponse, ScenarioKind, ScenarioResult } from '../models/battle'

export const options: GameOptions = {
  milestones: ['start', 'pokedex', 'boulder-badge'],
  areas: [
    { area: 'pallet-town-area', milestone: 'start', pokemon: ['bulbasaur'] },
    { area: 'kanto-route-1-area', milestone: 'pokedex', pokemon: ['pidgey', 'rattata'] },
    { area: 'mt-moon-1f', milestone: 'boulder-badge', pokemon: ['geodude', 'zubat'] },
    { area: 'seafoam-islands-1f', milestone: null, pokemon: ['seel'] },
  ],
}

const attack = (move: string, min: number, max: number, hp: number, turns: [number, number] | null): AttackResult => ({
  move,
  minDamage: min,
  maxDamage: max,
  targetMaxHp: hp,
  remainingHpMin: Math.max(0, hp - max),
  remainingHpMax: Math.max(0, hp - min),
  turnsToKoMin: turns?.[0] ?? null,
  turnsToKoMax: turns?.[1] ?? null,
})

const scenario = (kind: ScenarioKind, withCatch: boolean): ScenarioResult => ({
  scenario: kind,
  candidateAttack: attack('karate-chop', 8, 10, 25, [3, 4]),
  opponentAttack: attack('tackle', 3, 4, 20, [5, 7]),
  candidateKo: 'ThreeHitKo',
  outcome: kind === 'Pessimistic' ? 'Balanced' : 'Favorable',
  catch: withCatch
    ? {
        hit: attack('karate-chop', 8, 10, 25, [3, 4]),
        hits: 1,
        koRisk: 0.0625,
        targetHp: 17,
        balls: [{ ball: 'PokeBall', chancePerBall: 0.6241, ballsFor50Percent: 1, ballsFor90Percent: 3, ballsFor95Percent: 3, ballsFor99Percent: 5 }],
      }
    : null,
})

export const candidate = (withCatch = false): CandidateResult => ({
  id: 56,
  pokemon: 'mankey',
  types: ['Fighting'],
  area: 'kanto-route-22-area',
  method: 'walk',
  minLevel: 3,
  maxLevel: 5,
  scenarios: (['Pessimistic', 'Neutral', 'Optimistic'] as const).map((kind) => scenario(kind, withCatch)),
})

export const response = (objective: 'Defeat' | 'Catch' = 'Defeat'): PredictResponse => ({
  targetId: 74,
  target: 'geodude',
  targetTypes: ['Rock', 'Ground'],
  targetMinLevel: 7,
  targetMaxLevel: 9,
  objective,
  rankedBy: 'Neutral',
  candidates: [candidate(objective === 'Catch')],
})

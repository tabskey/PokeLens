import { useMemo } from 'react'

import type { AttackResult, BattleOutcome, CatchResult, PokemonType, PredictResponse, ScenarioKind } from '../models/battle'
import {
  areaName,
  ballLabels,
  koLabels,
  methodLabel,
  moveName,
  objectiveLabels,
  outcomeLabels,
  pokemonName,
  rankingLabels,
  scenarioLabels,
  spriteUrl,
} from '../models/labels'

export type Tone = 'good' | 'neutral' | 'bad'

export interface AttackView {
  move: string
  /** "8–10 de 25 HP": a range, never a win chance. */
  damage: string
  remainingHp: string
  turns: string
}

export interface BallView {
  ball: string
  /** Calculated probability of each throw. */
  chancePerBall: string
  /** Throws needed for 50/90/95/99% cumulative chance; a different metric from chancePerBall. */
  ballsFor: { confidence: string; balls: number }[]
}

export interface CatchView {
  move: string | null
  koRisk: string
  targetHp: string
  balls: BallView[]
}

export interface ScenarioView {
  kind: ScenarioKind
  title: string
  outcome: string
  tone: Tone
  ko: string
  attack: AttackView | null
  opponent: AttackView | null
  catch: CatchView | null
}

export interface CandidateView {
  name: string
  sprite: string
  types: PokemonType[]
  where: string
  scenarios: ScenarioView[]
}

export interface ResultView {
  target: string
  targetSprite: string
  targetTypes: PokemonType[]
  targetLevels: string
  objective: string
  rankedBy: string
  isCatch: boolean
  candidates: CandidateView[]
}

const tones: Record<BattleOutcome, Tone> = { Favorable: 'good', Balanced: 'neutral', Unfavorable: 'bad' }

const percent = new Intl.NumberFormat('pt-BR', { style: 'percent', maximumFractionDigits: 1 })

export const range = (min: number, max: number): string => (min === max ? `${min}` : `${min}–${max}`)

function toAttackView(attack: AttackResult | null): AttackView | null {
  if (attack === null) {
    return null
  }

  const turns = attack.turnsToKoMin === null || attack.turnsToKoMax === null
    ? 'não causa dano'
    : `${range(attack.turnsToKoMin, attack.turnsToKoMax)} ${attack.turnsToKoMax === 1 ? 'golpe' : 'golpes'} para KO`

  return {
    move: moveName(attack.move),
    damage: `${range(attack.minDamage, attack.maxDamage)} de ${attack.targetMaxHp} HP`,
    remainingHp: `${range(attack.remainingHpMin, attack.remainingHpMax)} HP`,
    turns,
  }
}

function toCatchView(plan: CatchResult): CatchView {
  return {
    move: plan.hit === null ? null : moveName(plan.hit.move),
    koRisk: percent.format(plan.koRisk),
    targetHp: `${plan.targetHp} HP`,
    balls: plan.balls.map((ball) => ({
      ball: ballLabels[ball.ball],
      chancePerBall: percent.format(ball.chancePerBall),
      ballsFor: [
        { confidence: '50%', balls: ball.ballsFor50Percent },
        { confidence: '90%', balls: ball.ballsFor90Percent },
        { confidence: '95%', balls: ball.ballsFor95Percent },
        { confidence: '99%', balls: ball.ballsFor99Percent },
      ],
    })),
  }
}

/** Turns the API response into display-ready text. Pure, so it is tested without React. */
export function toResultView(response: PredictResponse): ResultView {
  return {
    target: pokemonName(response.target),
    targetSprite: spriteUrl(response.targetId),
    targetTypes: response.targetTypes,
    targetLevels: `Nv. ${range(response.targetMinLevel, response.targetMaxLevel)}`,
    objective: objectiveLabels[response.objective],
    rankedBy: rankingLabels[response.rankedBy],
    isCatch: response.objective === 'Catch',
    candidates: response.candidates.map((candidate) => ({
      name: pokemonName(candidate.pokemon),
      sprite: spriteUrl(candidate.id),
      types: candidate.types,
      where: `${areaName(candidate.area)} · Nv. ${range(candidate.minLevel, candidate.maxLevel)} · ${methodLabel(candidate.method)}`,
      scenarios: candidate.scenarios.map((scenario) => ({
        kind: scenario.scenario,
        title: scenarioLabels[scenario.scenario],
        outcome: outcomeLabels[scenario.outcome],
        tone: tones[scenario.outcome],
        ko: koLabels[scenario.candidateKo],
        attack: toAttackView(scenario.candidateAttack),
        opponent: toAttackView(scenario.opponentAttack),
        catch: scenario.catch === null ? null : toCatchView(scenario.catch),
      })),
    })),
  }
}

export function usePredictionResultViewModel(response: PredictResponse | null): ResultView | null {
  return useMemo(() => (response === null ? null : toResultView(response)), [response])
}
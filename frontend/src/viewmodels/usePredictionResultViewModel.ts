import { useContext, useMemo } from 'react'

import type { AttackResult, BattleOutcome, CatchResult, PokemonType, PredictResponse, ScenarioKind } from '../models/battle'
import { LanguageContext } from '../i18n/LanguageContext'
import type { Translator } from '../i18n/translator'
import { areaName, ballLabels, methodLabel, moveName, pokemonName, spriteUrl } from '../models/labels'

export type Tone = 'good' | 'neutral' | 'bad'

export interface AttackView {
  move: string
  /** "8–10 de 25 HP": a range, never a win chance. */
  damage: string
  /** The same range as a share of whose HP it hits ("32%–40% do HP do alvo"); can exceed 100%. */
  hpShare: string
  /** Bar widths in percent (0–100): weakest and strongest roll. */
  bar: { min: number; max: number }
  /** "3–4 golpes", the headline number; null when the move does no damage. */
  hits: string | null
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
  /** The weakening move from the plan; null when the candidate has no damaging move. */
  hit: AttackView | null
  /** "2 golpes seguros": hits that never knock the target out, critical hits included. */
  safeHits: string
  /** "2×": how many times the plan uses the hit. */
  hits: string
  koRisk: string
  targetHp: string
  /** Poké Ball chance per throw for the collapsed summary; null when no ball can be thrown. */
  pokeBallChance: string | null
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

export const range = (min: number, max: number): string => (min === max ? `${min}` : `${min}–${max}`)

function toAttackView(attack: AttackResult | null, hpOf: string, { t }: Translator): AttackView | null {
  if (attack === null) {
    return null
  }

  const hits = attack.turnsToKoMin === null || attack.turnsToKoMax === null
    ? null
    : t('attack.hits', { range: range(attack.turnsToKoMin, attack.turnsToKoMax), count: attack.turnsToKoMax })

  // Truncated to whole percents: more digits would be false precision.
  const shareMin = Math.floor((attack.minDamage * 100) / attack.targetMaxHp)
  const shareMax = Math.floor((attack.maxDamage * 100) / attack.targetMaxHp)

  return {
    move: moveName(attack.move),
    damage: t('attack.damage', { range: range(attack.minDamage, attack.maxDamage), hp: attack.targetMaxHp }),
    hpShare: `${shareMin === shareMax ? `${shareMin}%` : `${shareMin}%–${shareMax}%`} ${hpOf}`,
    bar: { min: Math.min(shareMin, 100), max: Math.min(shareMax, 100) },
    hits,
    turns: hits === null ? t('attack.doesNoDamage') : t('attack.toKo', { hits }),
  }
}

function safeHits(plan: CatchResult, { t }: Translator): string {
  if (plan.hit === null) {
    return t('attack.noMove')
  }

  // The planner uses every safe hit, so zero risk means all of them are safe.
  return plan.koRisk > 0 ? t('catch.noSafeHit') : t('catch.safeHits', { count: plan.hits })
}

function toCatchView(plan: CatchResult, i18n: Translator): CatchView {
  const { t, percent } = i18n
  const pokeBall = plan.balls.find((ball) => ball.ball === 'PokeBall')

  return {
    hit: toAttackView(plan.hit, t('attack.targetHp'), i18n),
    safeHits: safeHits(plan, i18n),
    hits: `${plan.hits}×`,
    koRisk: percent(plan.koRisk),
    targetHp: `${plan.targetHp} HP`,
    pokeBallChance: pokeBall === undefined ? null : percent(pokeBall.chancePerBall),
    balls: plan.balls.map((ball) => ({
      ball: ballLabels[ball.ball],
      chancePerBall: percent(ball.chancePerBall),
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
export function toResultView(response: PredictResponse, i18n: Translator): ResultView {
  const { t } = i18n

  return {
    target: pokemonName(response.target),
    targetSprite: spriteUrl(response.targetId),
    targetTypes: response.targetTypes,
    targetLevels: t('results.level', { levels: range(response.targetMinLevel, response.targetMaxLevel) }),
    objective: t(`objective.${response.objective}`),
    rankedBy: t(`ranking.${response.rankedBy}`),
    isCatch: response.objective === 'Catch',
    candidates: response.candidates.map((candidate) => ({
      name: pokemonName(candidate.pokemon),
      sprite: spriteUrl(candidate.id),
      types: candidate.types,
      where: `${areaName(candidate.area)} · ${t('results.level', { levels: range(candidate.minLevel, candidate.maxLevel) })} · ${methodLabel(candidate.method, t)}`,
      scenarios: candidate.scenarios.map((scenario) => ({
        kind: scenario.scenario,
        title: t(`scenario.${scenario.scenario}`),
        outcome: t(`outcome.${scenario.outcome}`),
        tone: tones[scenario.outcome],
        ko: t(`ko.${scenario.candidateKo}`),
        attack: toAttackView(scenario.candidateAttack, t('attack.targetHp'), i18n),
        opponent: toAttackView(scenario.opponentAttack, t('attack.yourHp'), i18n),
        catch: scenario.catch === null ? null : toCatchView(scenario.catch, i18n),
      })),
    })),
  }
}

export function usePredictionResultViewModel(response: PredictResponse | null): ResultView | null {
  const { translator } = useContext(LanguageContext)
  return useMemo(() => (response === null ? null : toResultView(response, translator)), [response, translator])
}
// Mirrors the backend contracts in PokeBattle.Application (PredictBattleOutcomeContracts, GameOptionsQuery).

export type Version = 'firered' | 'leafgreen'
export type Objective = 'Defeat' | 'Catch'
export type ScenarioKind = 'Pessimistic' | 'Neutral' | 'Optimistic'
export type BattleOutcome = 'Favorable' | 'Balanced' | 'Unfavorable'
export type KoClassification = 'NoDamage' | 'GuaranteedKo' | 'PossibleKo' | 'TwoHitKo' | 'ThreeHitKo' | 'FourOrMoreHits'
export type PokeBall = 'PokeBall' | 'GreatBall' | 'UltraBall' | 'MasterBall'
export type PokemonType =
  | 'Normal' | 'Fighting' | 'Flying' | 'Poison' | 'Ground' | 'Rock' | 'Bug' | 'Ghost' | 'Steel'
  | 'Fire' | 'Water' | 'Grass' | 'Electric' | 'Psychic' | 'Ice' | 'Dragon' | 'Dark'

export interface PredictRequest {
  target: string
  area: string
  version: Version
  milestone: string
  objective: Objective
  scenario: ScenarioKind
}

/** Damage and remaining HP are ranges, never probabilities. */
export interface AttackResult {
  move: string
  minDamage: number
  maxDamage: number
  targetMaxHp: number
  remainingHpMin: number
  remainingHpMax: number
  turnsToKoMin: number | null
  turnsToKoMax: number | null
}

export interface BallResult {
  ball: PokeBall
  chancePerBall: number
  ballsFor50Percent: number
  ballsFor90Percent: number
  ballsFor95Percent: number
  ballsFor99Percent: number
}

export interface CatchResult {
  hit: AttackResult | null
  koRisk: number
  targetHp: number
  balls: BallResult[]
}

export interface ScenarioResult {
  scenario: ScenarioKind
  candidateAttack: AttackResult | null
  opponentAttack: AttackResult | null
  candidateKo: KoClassification
  outcome: BattleOutcome
  catch: CatchResult | null
}

export interface CandidateResult {
  /** National Pokédex number. */
  id: number
  pokemon: string
  types: PokemonType[]
  area: string
  method: string
  minLevel: number
  maxLevel: number
  scenarios: ScenarioResult[]
}

export interface PredictResponse {
  targetId: number
  target: string
  targetTypes: PokemonType[]
  targetMinLevel: number
  targetMaxLevel: number
  objective: Objective
  rankedBy: ScenarioKind
  candidates: CandidateResult[]
}

export interface AreaOption {
  area: string
  /** Null when the area is not in the curated progression yet. */
  milestone: string | null
  pokemon: string[]
}

export interface GameOptions {
  milestones: string[]
  areas: AreaOption[]
}
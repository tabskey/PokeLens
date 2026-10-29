// Mirrors the backend contracts in PokeBattle.Application (PredictBattleOutcomeContracts, ListTargetsContracts, GameOptions).

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
  /** Wild encounter method in that area (walk, surf, old-rod...); the target's levels come from the pair. */
  method: string
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
  /** Times the hit is used: every safe hit, or 1 when none is safe; 0 without a hit. */
  hits: number
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

export interface GameOptions {
  milestones: string[]
}

/** One way to meet a target: chance is the species' encounter rate there, summed over its slots. */
export interface TargetEncounter {
  area: string
  displayName: string
  method: string
  minLevel: number
  maxLevel: number
  chance: number
}

/** A Pokémon the player can meet at the milestone. Id is the National Pokédex number. */
export interface TargetOption {
  pokemon: string
  id: number
  encounters: TargetEncounter[]
}
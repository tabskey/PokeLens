import type { BattleOutcome, KoClassification, Objective, PokeBall, ScenarioKind, Version } from './battle'

export const versionLabels: Record<Version, string> = { firered: 'FireRed', leafgreen: 'LeafGreen' }

export const objectiveLabels: Record<Objective, string> = { Defeat: 'Derrotar', Catch: 'Capturar' }

/** Scenario names as the player sees them when choosing how to rank. */
export const rankingLabels: Record<ScenarioKind, string> = {
  Pessimistic: 'Mais seguro',
  Neutral: 'Equilibrado',
  Optimistic: 'Maior potencial',
}

export const scenarioLabels: Record<ScenarioKind, string> = {
  Pessimistic: 'Pessimista',
  Neutral: 'Neutro',
  Optimistic: 'Otimista',
}

export const outcomeLabels: Record<BattleOutcome, string> = {
  Favorable: 'Favorável',
  Balanced: 'Equilibrado',
  Unfavorable: 'Desfavorável',
}

export const koLabels: Record<KoClassification, string> = {
  NoDamage: 'Sem dano',
  GuaranteedKo: 'KO garantido',
  PossibleKo: 'KO possível',
  TwoHitKo: '2 golpes',
  ThreeHitKo: '3 golpes',
  FourOrMoreHits: '4 golpes ou mais',
}

export const ballLabels: Record<PokeBall, string> = {
  PokeBall: 'Poké Ball',
  GreatBall: 'Great Ball',
  UltraBall: 'Ultra Ball',
  MasterBall: 'Master Ball',
}

const milestoneLabels: Record<string, string> = {
  start: 'Início (Pallet Town)',
  pokedex: 'Recebeu a Pokédex',
  'boulder-badge': 'Insígnia Boulder (Brock)',
  'cascade-badge': 'Insígnia Cascade (Misty)',
}

const methodLabels: Record<string, string> = {
  walk: 'grama/caverna',
  gift: 'presente',
  surf: 'Surf',
  'old-rod': 'Old Rod',
  'good-rod': 'Good Rod',
  'super-rod': 'Super Rod',
  'rock-smash': 'Rock Smash',
}

const pokemonNames: Record<string, string> = {
  'nidoran-f': 'Nidoran♀',
  'nidoran-m': 'Nidoran♂',
  'mr-mime': 'Mr. Mime',
  farfetchd: "Farfetch'd",
  'ho-oh': 'Ho-Oh',
}

function titleCase(words: string[]): string {
  return words.map((word) => (/^b?\d+f$/.test(word) ? word.toUpperCase() : word.charAt(0).toUpperCase() + word.slice(1))).join(' ')
}

export const milestoneLabel = (milestone: string): string => milestoneLabels[milestone] ?? titleCase(milestone.split('-'))

export const methodLabel = (method: string): string => methodLabels[method] ?? method

export const pokemonName = (slug: string): string => pokemonNames[slug] ?? titleCase(slug.split('-'))

/** "kanto-route-1-area" → "Route 1", "mt-moon-b1f" → "Mt Moon B1F". */
export const areaName = (slug: string): string =>
  titleCase(slug.replace(/^kanto-/, '').replace(/-area$/, '').split('-'))

export const moveName = (slug: string): string => titleCase(slug.split('-'))

/**
 * FireRed/LeafGreen front sprite from the PokeAPI sprites repository. Loaded by the browser; the
 * backend never calls PokeAPI at runtime.
 */
export const spriteUrl = (id: number): string =>
  `https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/versions/generation-iii/firered-leafgreen/${id}.png`

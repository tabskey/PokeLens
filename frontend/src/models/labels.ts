import type { Translator } from '../i18n/translator'
import type { PokeBall, Version } from './battle'

// Game names (versions, balls, Pokémon, areas, moves) stay as in the English games; UI words are in locales/.

export const versionLabels: Record<Version, string> = { firered: 'FireRed', leafgreen: 'LeafGreen' }

export const ballLabels: Record<PokeBall, string> = {
  PokeBall: 'Poké Ball',
  GreatBall: 'Great Ball',
  UltraBall: 'Ultra Ball',
  MasterBall: 'Master Ball',
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

export const milestoneLabel = (milestone: string, t: Translator['t']): string =>
  t(`milestone.${milestone}`, { defaultValue: titleCase(milestone.split('-')) })

// Rods, Surf and Rock Smash are item and move names, so only walk and gift are translated.
export const methodLabel = (method: string, t: Translator['t']): string => t(`method.${method}`, { defaultValue: titleCase(method.split('-')) })

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

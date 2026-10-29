import { useCallback, useEffect, useMemo, useState } from 'react'

import { ApiError, getOptions, getTargets, postPredict } from '../models/api'
import type { GameOptions, Objective, PredictRequest, PredictResponse, ScenarioKind, TargetEncounter, TargetOption, Version } from '../models/battle'
import { predictionFromHash } from '../routing'
import { useTranslation } from '../i18n/useTranslation'
import { methodLabel, milestoneLabel, pokemonName, versionLabels } from '../models/labels'
import { range } from './usePredictionResultViewModel'

export interface Option<T extends string = string> {
  value: T
  label: string
  activeClassName?: string
}

/** A search suggestion: `id` is "pokemon:<slug>" or "area:<slug>". */
export interface Suggestion {
  id: string
  label: string
  /** Where a Pokémon shows up, e.g. "Route 1, Route 2 (South)". */
  detail?: string
}

export interface SuggestionGroup {
  label: string
  options: Suggestion[]
}

/** One way to meet the picked target; the player chooses when there is more than one. */
export interface EncounterChoice {
  id: string
  area: string
  method: string
  /** "Requer Super Vara" for methods that need an item; null for walking. */
  requirement: string | null
  levels: string
  chance: string
}

type FieldErrors = Record<string, string[]>

// Methods that need an item or HM; walking needs nothing (docs/arquitetura.md §7.1).
const itemMethods = new Set(['surf', 'old-rod', 'good-rod', 'super-rod', 'rock-smash'])

// Accents and punctuation are ignored: "mt moon" finds "Mt. Moon 1F", "pokemon" finds "Pokémon".
const normalize = (text: string) =>
  text.normalize('NFD').replace(/\p{Diacritic}/gu, '').replace(/[^\p{L}\p{N}]+/gu, ' ').toLowerCase().trim()
const encounterId = (e: TargetEncounter) => `${e.area}|${e.method}`

const versions: Option<Version>[] = [
  { value: 'firered', label: versionLabels.firered.toUpperCase(), activeClassName: 'bg-firered' },
  { value: 'leafgreen', label: versionLabels.leafgreen.toUpperCase(), activeClassName: 'bg-leafgreen' },
]

/**
 * State and actions of the prediction form. The target is found with a single search over GET /targets
 * (docs/arquitetura.md §7.1), which only lists what is unlocked at the chosen milestone. A target with
 * one encounter is ready at once; with several, the player picks the area and method.
 * A prediction in the URL (an opened favorite) fills the form and runs right away.
 */
export function usePredictionFormViewModel() {
  const { t } = useTranslation()
  const [opened] = useState(predictionFromHash)
  const [version, setVersionState] = useState<Version>(opened?.version ?? 'firered')
  const [options, setOptions] = useState<GameOptions | null>(null)
  const [targetList, setTargetList] = useState<TargetOption[]>([])
  const [optionsError, setOptionsError] = useState<string | null>(null)
  const [milestone, setMilestone] = useState(opened?.milestone ?? '')
  const [query, setQueryState] = useState(opened ? pokemonName(opened.target) : '')
  const [picked, setPicked] = useState(opened?.target ?? '')
  // Area the player searched before picking the Pokémon; narrows its encounters to that route.
  const [route, setRoute] = useState(opened?.area ?? '')
  const [chosenEncounter, setChosenEncounter] = useState(opened ? `${opened.area}|${opened.method}` : '')
  const [objective, setObjective] = useState<Objective>(opened?.objective ?? 'Defeat')
  const [scenario, setScenario] = useState<ScenarioKind>(opened?.scenario ?? 'Neutral')
  const [loading, setLoading] = useState(opened !== null)
  const [result, setResult] = useState<PredictResponse | null>(null)
  // The question behind the result on screen, e.g. to save it as a favorite.
  const [prediction, setPrediction] = useState<PredictRequest | null>(null)
  const [error, setError] = useState<string | null>(null)
  const [fieldErrors, setFieldErrors] = useState<FieldErrors>({})

  useEffect(() => {
    let cancelled = false
    getOptions()
      .then((loaded) => {
        if (cancelled) {
          return
        }
        setOptions(loaded)
        setOptionsError(null)
        setMilestone((current) => current || (loaded.milestones[0] ?? ''))
      })
      .catch((reason: unknown) => {
        if (!cancelled) {
          setOptionsError(reason instanceof ApiError ? reason.message : 'errors.options')
        }
      })
    return () => {
      cancelled = true
    }
  }, [])

  useEffect(() => {
    if (!milestone) {
      return
    }
    let cancelled = false
    getTargets(version, milestone)
      .then((loaded) => {
        if (cancelled) {
          return
        }
        setTargetList(loaded)
        setOptionsError(null)
      })
      .catch((reason: unknown) => {
        if (!cancelled) {
          setOptionsError(reason instanceof ApiError ? reason.message : 'errors.options')
        }
      })
    return () => {
      cancelled = true
    }
  }, [version, milestone])

  const milestones = useMemo(() => options?.milestones ?? [], [options])

  const areaNames = useMemo(
    () => new Map(targetList.flatMap((option) => option.encounters.map((e): [string, string] => [e.area, e.displayName]))),
    [targetList],
  )

  const groups = useMemo((): SuggestionGroup[] => {
    const needle = normalize(query)
    if (!needle || picked) {
      return []
    }
    const matches = (text: string) => normalize(text).includes(needle)
    // A Pokémon also matches by the routes it shows up in, so searching a route lists its Pokémon.
    const pokemon = targetList
      .filter((option) => matches(pokemonName(option.pokemon)) || option.encounters.some((e) => matches(e.displayName)))
      .map((option): Suggestion => ({
        id: `pokemon:${option.pokemon}`,
        label: pokemonName(option.pokemon),
        detail: [...new Set(option.encounters.map((e) => e.displayName))].join(', '),
      }))
    const routes = [...areaNames]
      .filter(([, name]) => matches(name))
      .sort(([, a], [, b]) => a.localeCompare(b))
      .map(([area, name]): Suggestion => ({ id: `area:${area}`, label: name }))
    return [
      { label: t('search.pokemon'), options: pokemon },
      { label: t('search.routes'), options: routes },
    ].filter((group) => group.options.length > 0)
  }, [query, picked, targetList, areaNames, t])

  // Going back in progression (or switching version) drops a pick that is no longer open.
  const target = targetList.find((option) => option.pokemon === picked)
  const encounters = useMemo(() => {
    const all = target?.encounters ?? []
    const onRoute = all.filter((e) => e.area === route)
    return onRoute.length > 0 ? onRoute : all
  }, [target, route])
  const selected = encounters.length === 1 ? encounters[0] : encounters.find((e) => encounterId(e) === chosenEncounter)

  const setQuery = (next: string) => {
    setQueryState(next)
    setPicked('')
    setRoute('')
    setChosenEncounter('')
  }

  const pick = (id: string) => {
    const [kind, slug = ''] = id.split(':')
    if (kind === 'area') {
      setQuery(areaNames.get(slug) ?? '')
      return
    }
    const searched = [...areaNames].find(([, name]) => normalize(name) === normalize(query))
    setQueryState(pokemonName(slug))
    setPicked(slug)
    setRoute(searched?.[0] ?? '')
    setChosenEncounter('')
  }

  const setVersion = (next: Version) => {
    setVersionState(next)
    setResult(null)
  }

  // Only state setters inside, so it is stable and the opened prediction can use it.
  const fail = useCallback((reason: unknown) => {
    setResult(null)
    setPrediction(null)
    if (reason instanceof ApiError) {
      setError(reason.message)
      setFieldErrors(reason.fieldErrors)
    } else {
      setError('errors.unexpected')
    }
  }, [])

  useEffect(() => {
    if (!opened) {
      return
    }
    let cancelled = false
    postPredict(opened)
      .then((response) => {
        if (cancelled) return
        setResult(response)
        setPrediction(opened)
      })
      .catch((reason: unknown) => {
        if (!cancelled) fail(reason)
      })
      .finally(() => {
        if (!cancelled) setLoading(false)
      })
    return () => {
      cancelled = true
    }
  }, [opened, fail])

  const submit = async () => {
    const missing: FieldErrors = {}
    if (!milestone) missing.milestone = ['errors.milestoneRequired']
    if (!target) missing.target = ['errors.targetRequired']
    else if (!selected) missing.target = ['errors.encounterRequired']
    setFieldErrors(missing)
    setError(null)
    if (Object.keys(missing).length > 0) {
      return
    }

    const request: PredictRequest = { target: target!.pokemon, area: selected!.area, method: selected!.method, version, milestone, objective, scenario }
    setLoading(true)
    try {
      setResult(await postPredict(request))
      setPrediction(request)
    } catch (reason) {
      fail(reason)
    } finally {
      setLoading(false)
    }
  }

  const translate = (messages: string[] = []) => messages.map((message) => t(message))

  return {
    version,
    milestone,
    objective,
    scenario,
    versions,
    objectives: (['Defeat', 'Catch'] as const).map((value): Option<Objective> => ({ value, label: t(`objective.${value}`) })),
    scenarios: (['Pessimistic', 'Neutral', 'Optimistic'] as const).map((value): Option<ScenarioKind> => ({ value, label: t(`ranking.${value}`) })),
    milestoneOptions: milestones.map((value): Option => ({ value, label: milestoneLabel(value, t) })),
    search: {
      query,
      groups,
      noMatches: normalize(query) !== '' && !picked && groups.length === 0,
      choices: encounters.length > 1
        ? encounters.map((e): EncounterChoice => ({
            id: encounterId(e),
            area: e.displayName,
            method: methodLabel(e.method, t),
            requirement: itemMethods.has(e.method) ? t('search.requires', { item: methodLabel(e.method, t) }) : null,
            levels: t('results.level', { levels: range(e.minLevel, e.maxLevel) }),
            chance: t('search.chance', { chance: e.chance }),
          }))
        : [],
      chosen: selected ? encounterId(selected) : '',
      // Backend errors about area or method are about this field too.
      errors: [...translate(fieldErrors.target), ...translate(fieldErrors.area), ...translate(fieldErrors.method)],
      setQuery,
      pick,
      choose: setChosenEncounter,
    },
    optionsLoaded: options !== null,
    // Errors are kept as keys so they follow a language switch; backend messages pass through as-is.
    optionsError: optionsError && t(optionsError),
    loading,
    result,
    prediction,
    error: error && t(error),
    fieldErrors: Object.fromEntries(Object.entries(fieldErrors).map(([field, messages]) => [field, translate(messages)])),
    setVersion,
    setMilestone,
    setObjective,
    setScenario,
    submit,
  }
}

export type PredictionFormViewModel = ReturnType<typeof usePredictionFormViewModel>
export type TargetSearchViewModel = PredictionFormViewModel['search']
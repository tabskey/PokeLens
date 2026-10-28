import { useEffect, useMemo, useState } from 'react'

import { ApiError, getOptions, getTargets, postPredict } from '../models/api'
import type { GameOptions, Objective, PredictResponse, ScenarioKind, TargetOption, Version } from '../models/battle'
import { useTranslation } from '../i18n/useTranslation'
import { milestoneLabel, pokemonName, versionLabels } from '../models/labels'

export interface Option<T extends string = string> {
  value: T
  label: string
  activeClassName?: string
}

type FieldErrors = Record<string, string[]>

const versions: Option<Version>[] = [
  { value: 'firered', label: versionLabels.firered.toUpperCase(), activeClassName: 'bg-firered' },
  { value: 'leafgreen', label: versionLabels.leafgreen.toUpperCase(), activeClassName: 'bg-leafgreen' },
]

/**
 * State and actions of the prediction form. Areas and targets come from GET /targets, which only lists
 * what is unlocked at the chosen milestone, mirroring the progression filter applied to candidates.
 */
export function usePredictionFormViewModel() {
  const { t } = useTranslation()
  const [version, setVersionState] = useState<Version>('firered')
  const [options, setOptions] = useState<GameOptions | null>(null)
  const [targetList, setTargetList] = useState<TargetOption[]>([])
  const [optionsError, setOptionsError] = useState<string | null>(null)
  const [milestone, setMilestone] = useState('')
  const [chosenArea, setAreaState] = useState('')
  const [chosenTarget, setTarget] = useState('')
  const [objective, setObjective] = useState<Objective>('Defeat')
  const [scenario, setScenario] = useState<ScenarioKind>('Neutral')
  const [loading, setLoading] = useState(false)
  const [result, setResult] = useState<PredictResponse | null>(null)
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
        setMilestone(loaded.milestones[0] ?? '')
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

  const areas = useMemo(() => {
    const names = new Map(targetList.flatMap((option) => option.encounters.map((e): [string, string] => [e.area, e.displayName])))
    return [...names].sort(([, a], [, b]) => a.localeCompare(b))
  }, [targetList])

  // Going back in progression (or switching version) hides a choice that is no longer open.
  const area = areas.some(([value]) => value === chosenArea) ? chosenArea : ''
  const targets = useMemo(() => targetList.filter((option) => option.encounters.some((e) => e.area === area)), [targetList, area])
  const target = targets.some((option) => option.pokemon === chosenTarget) ? chosenTarget : ''

  const setVersion = (next: Version) => {
    setVersionState(next)
    setResult(null)
  }

  const setArea = (next: string) => {
    setAreaState(next)
    setTarget('')
  }

  const submit = async () => {
    const missing: FieldErrors = {}
    if (!milestone) missing.milestone = ['errors.milestoneRequired']
    if (!area) missing.area = ['errors.areaRequired']
    if (!target) missing.target = ['errors.targetRequired']
    setFieldErrors(missing)
    setError(null)
    if (Object.keys(missing).length > 0) {
      return
    }

    setLoading(true)
    try {
      // shortcut: takes the first method of the target in the area; the target search (Etapa 8b) lets the player pick it.
      const method = targets.find((option) => option.pokemon === target)?.encounters.find((e) => e.area === area)?.method ?? ''
      setResult(await postPredict({ target, area, method, version, milestone, objective, scenario }))
    } catch (reason) {
      setResult(null)
      if (reason instanceof ApiError) {
        setError(reason.message)
        setFieldErrors(reason.fieldErrors)
      } else {
        setError('errors.unexpected')
      }
    } finally {
      setLoading(false)
    }
  }

  return {
    version,
    milestone,
    area,
    target,
    objective,
    scenario,
    versions,
    objectives: (['Defeat', 'Catch'] as const).map((value): Option<Objective> => ({ value, label: t(`objective.${value}`) })),
    scenarios: (['Pessimistic', 'Neutral', 'Optimistic'] as const).map((value): Option<ScenarioKind> => ({ value, label: t(`ranking.${value}`) })),
    milestoneOptions: milestones.map((value): Option => ({ value, label: milestoneLabel(value, t) })),
    areaOptions: areas.map(([value, label]): Option => ({ value, label })),
    targetOptions: targets.map((option): Option => ({ value: option.pokemon, label: pokemonName(option.pokemon) })),
    optionsLoaded: options !== null,
    // Errors are kept as keys so they follow a language switch; backend messages pass through as-is.
    optionsError: optionsError && t(optionsError),
    loading,
    result,
    error: error && t(error),
    fieldErrors: Object.fromEntries(Object.entries(fieldErrors).map(([field, messages]) => [field, messages.map((message) => t(message))])),
    setVersion,
    setMilestone,
    setArea,
    setTarget,
    setObjective,
    setScenario,
    submit,
  }
}

export type PredictionFormViewModel = ReturnType<typeof usePredictionFormViewModel>
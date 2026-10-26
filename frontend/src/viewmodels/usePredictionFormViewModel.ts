import { useEffect, useMemo, useState } from 'react'

import { ApiError, getOptions, postPredict } from '../models/api'
import type { GameOptions, Objective, PredictResponse, ScenarioKind, Version } from '../models/battle'
import { areaName, milestoneLabel, objectiveLabels, pokemonName, rankingLabels, versionLabels } from '../models/labels'

export interface Option<T extends string = string> {
  value: T
  label: string
  activeClassName?: string
}

type FieldErrors = Record<string, string[]>

const objectives: Option<Objective>[] = (['Defeat', 'Catch'] as const).map((value) => ({ value, label: objectiveLabels[value] }))
const scenarios: Option<ScenarioKind>[] = (['Pessimistic', 'Neutral', 'Optimistic'] as const).map((value) => ({ value, label: rankingLabels[value] }))
const versions: Option<Version>[] = [
  { value: 'firered', label: versionLabels.firered.toUpperCase(), activeClassName: 'bg-firered' },
  { value: 'leafgreen', label: versionLabels.leafgreen.toUpperCase(), activeClassName: 'bg-leafgreen' },
]

/**
 * State and actions of the prediction form. Only areas unlocked at the chosen milestone are offered,
 * mirroring the progression filter the backend applies to candidates.
 */
export function usePredictionFormViewModel() {
  const [version, setVersionState] = useState<Version>('firered')
  const [options, setOptions] = useState<GameOptions | null>(null)
  const [optionsError, setOptionsError] = useState<string | null>(null)
  const [milestone, setMilestoneState] = useState('')
  const [area, setAreaState] = useState('')
  const [target, setTarget] = useState('')
  const [objective, setObjective] = useState<Objective>('Defeat')
  const [scenario, setScenario] = useState<ScenarioKind>('Neutral')
  const [loading, setLoading] = useState(false)
  const [result, setResult] = useState<PredictResponse | null>(null)
  const [error, setError] = useState<string | null>(null)
  const [fieldErrors, setFieldErrors] = useState<FieldErrors>({})

  useEffect(() => {
    let cancelled = false
    getOptions(version)
      .then((loaded) => {
        if (cancelled) {
          return
        }
        setOptions(loaded)
        setOptionsError(null)
        setMilestoneState((current) => (loaded.milestones.includes(current) ? current : (loaded.milestones[0] ?? '')))
      })
      .catch((reason: unknown) => {
        if (!cancelled) {
          setOptionsError(reason instanceof ApiError ? reason.message : 'Não foi possível carregar as opções.')
        }
      })
    return () => {
      cancelled = true
    }
  }, [version])

  const milestones = useMemo(() => options?.milestones ?? [], [options])

  const areas = useMemo(() => {
    const reached = milestones.indexOf(milestone)
    return (options?.areas ?? [])
      .filter((option) => option.milestone !== null && milestones.indexOf(option.milestone) <= reached)
      .sort((a, b) => milestones.indexOf(a.milestone!) - milestones.indexOf(b.milestone!) || a.area.localeCompare(b.area))
  }, [options, milestones, milestone])

  const targets = useMemo(() => areas.find((option) => option.area === area)?.pokemon ?? [], [areas, area])

  const setVersion = (next: Version) => {
    setVersionState(next)
    setAreaState('')
    setTarget('')
    setResult(null)
  }

  const setMilestone = (next: string) => {
    setMilestoneState(next)
    const reached = milestones.indexOf(next)
    const stillOpen = options?.areas.some((option) => option.area === area && option.milestone !== null && milestones.indexOf(option.milestone) <= reached)
    if (!stillOpen) {
      setAreaState('')
      setTarget('')
    }
  }

  const setArea = (next: string) => {
    setAreaState(next)
    setTarget('')
  }

  const submit = async () => {
    const missing: FieldErrors = {}
    if (!milestone) missing.milestone = ['Escolha até onde você chegou.']
    if (!area) missing.area = ['Escolha a área.']
    if (!target) missing.target = ['Escolha o Pokémon-alvo.']
    setFieldErrors(missing)
    setError(null)
    if (Object.keys(missing).length > 0) {
      return
    }

    setLoading(true)
    try {
      setResult(await postPredict({ target, area, version, milestone, objective, scenario }))
    } catch (reason) {
      setResult(null)
      if (reason instanceof ApiError) {
        setError(reason.message)
        setFieldErrors(reason.fieldErrors)
      } else {
        setError('Erro inesperado.')
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
    objectives,
    scenarios,
    milestoneOptions: milestones.map((value): Option => ({ value, label: milestoneLabel(value) })),
    areaOptions: areas.map((option): Option => ({ value: option.area, label: areaName(option.area) })),
    targetOptions: targets.map((value): Option => ({ value, label: pokemonName(value) })),
    optionsLoaded: options !== null,
    optionsError,
    loading,
    result,
    error,
    fieldErrors,
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
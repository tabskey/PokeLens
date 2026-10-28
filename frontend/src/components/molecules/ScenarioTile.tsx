import type { ScenarioView } from '../../viewmodels/usePredictionResultViewModel'
import { Tag } from '../atoms/Tag'
import { CatchOdds } from './CatchOdds'
import { DamageRange } from './DamageRange'

const borders = { good: 'border-t-good', neutral: 'border-t-neutral', bad: 'border-t-bad' }
// Neutral yellow is too light for text, so the neutral headline stays ink.
const headlineColors = { good: 'text-good', neutral: 'text-ink', bad: 'text-bad' }

/**
 * The outcome label and the hits to KO (safe hits when catching) are the headline; the rest is detail.
 * Outside the catch odds, the only percentages are HP shares, never a win chance.
 */
export function ScenarioTile({ scenario }: { scenario: ScenarioView }) {
  const { attack } = scenario
  // When catching, the hit that matters is the weakening move, and knocking out is what to avoid.
  const featured = scenario.catch ? scenario.catch.hit : attack

  return (
    <section aria-label={`Cenário ${scenario.title}`} className={`rounded-lg border border-line border-t-4 bg-white p-3 ${borders[scenario.tone]}`}>
      <h5 className="font-pixel text-[9px] text-muted">{scenario.title.toUpperCase()}</h5>
      <div className="mt-2 flex flex-wrap items-center gap-1.5">
        <Tag tone={scenario.tone}>{scenario.outcome}</Tag>
        <Tag>{scenario.ko}</Tag>
      </div>
      {featured !== null && (
        <>
          <p className={`mt-2 text-2xl font-bold leading-tight ${headlineColors[scenario.tone]}`}>
            {scenario.catch ? (
              scenario.catch.safeHits
            ) : (
              <>
                {featured.hits ?? 'Sem dano'}
                {featured.hits !== null && <span className="ml-1 text-xs font-normal text-muted">para KO</span>}
              </>
            )}
          </p>
          {/* Solid up to the weakest roll, lighter up to the strongest: HP taken by one hit, not a chance. */}
          <div className="relative mt-1 h-1.5 overflow-hidden rounded bg-line" title="Dano de 1 golpe no HP do alvo (roll mínimo–máximo)" aria-hidden="true">
            <div className="absolute inset-y-0 left-0 bg-ink/30" style={{ width: `${featured.bar.max}%` }} />
            <div className="absolute inset-y-0 left-0 bg-ink" style={{ width: `${featured.bar.min}%` }} />
          </div>
        </>
      )}
      <details className="mt-2">
        <summary className="cursor-pointer text-xs font-semibold text-ink">{featured?.move ?? 'Sem golpe de dano'} · detalhes</summary>
        <div className="mt-1 space-y-1">
          <DamageRange who="Você" attack={featured} />
          <DamageRange who="Oponente" attack={scenario.opponent} withTurns />
        </div>
      </details>
      {scenario.catch && <CatchOdds plan={scenario.catch} />}
    </section>
  )
}

import type { ScenarioView } from '../../viewmodels/usePredictionResultViewModel'
import { Tag } from '../atoms/Tag'
import { CatchOdds } from './CatchOdds'
import { DamageRange } from './DamageRange'

const borders = { good: 'border-t-good', neutral: 'border-t-neutral', bad: 'border-t-bad' }

/** The outcome label is the headline; numbers are supporting detail (no win percentage). */
export function ScenarioTile({ scenario }: { scenario: ScenarioView }) {
  return (
    <section aria-label={`Cenário ${scenario.title}`} className={`rounded-lg border border-line border-t-4 bg-white p-3 ${borders[scenario.tone]}`}>
      <h5 className="font-pixel text-[9px] text-muted">{scenario.title.toUpperCase()}</h5>
      <div className="mt-2 flex flex-wrap items-center gap-1.5">
        <Tag tone={scenario.tone}>{scenario.outcome}</Tag>
        <Tag>{scenario.ko}</Tag>
      </div>
      <div className="mt-2 space-y-1">
        <DamageRange who="Você" attack={scenario.attack} />
        <DamageRange who="Oponente" attack={scenario.opponent} />
      </div>
      {scenario.catch && <CatchOdds plan={scenario.catch} />}
    </section>
  )
}

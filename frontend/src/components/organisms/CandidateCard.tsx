import type { CandidateView } from '../../viewmodels/usePredictionResultViewModel'
import { Avatar } from '../atoms/Avatar'
import { TypeBadge } from '../atoms/TypeBadge'
import { ScenarioTile } from '../molecules/ScenarioTile'

export function CandidateCard({ rank, candidate }: { rank: number; candidate: CandidateView }) {
  return (
    <article className="rounded-[10px] border border-line bg-panel p-[18px]" aria-label={`${rank}º ${candidate.name}`}>
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div className="flex items-center gap-3">
          <Avatar name={candidate.name} />
          <div>
            <h4 className="text-base font-bold">
              <span className="text-muted">{rank}.</span> {candidate.name}
            </h4>
            <p className="text-xs text-muted">{candidate.where}</p>
          </div>
        </div>
        <div className="flex flex-wrap gap-1">
          {candidate.types.map((type) => (
            <TypeBadge key={type} type={type} />
          ))}
        </div>
      </div>
      <div className="mt-3 grid gap-2 md:grid-cols-3">
        {candidate.scenarios.map((scenario) => (
          <ScenarioTile key={scenario.kind} scenario={scenario} />
        ))}
      </div>
    </article>
  )
}

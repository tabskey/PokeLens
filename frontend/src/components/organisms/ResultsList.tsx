import type { ResultView } from '../../viewmodels/usePredictionResultViewModel'
import { CandidateCard } from './CandidateCard'
import { TargetSummary } from './TargetSummary'

export function ResultsList({ result }: { result: ResultView }) {
  return (
    <div className="space-y-4">
      <TargetSummary result={result} />
      <div className="flex flex-wrap items-baseline justify-between gap-2">
        <h2 className="font-pixel text-[10px] text-accent">MELHORES OPÇÕES</h2>
        <span className="text-[13px] text-muted">
          {result.candidates.length} {result.candidates.length === 1 ? 'sugestão' : 'sugestões'} · ordenadas por: {result.rankedBy}
        </span>
      </div>
      {result.candidates.length === 0 ? (
        <p className="text-muted">Nenhum Pokémon disponível até aqui. Avance na progressão e tente de novo.</p>
      ) : (
        result.candidates.map((candidate, index) => <CandidateCard key={candidate.name} rank={index + 1} candidate={candidate} />)
      )}
      <p className="text-xs text-muted">
        Faixa de dano não é chance de vitória. A V1 considera que todo golpe acerta e não olha Speed.
        {result.isCatch && ' Chance por bola é a chance de cada arremesso; bolas para 50/90/95/99% é quantas jogar para chegar a essa chance acumulada.'}
      </p>
    </div>
  )
}

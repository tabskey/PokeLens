import type { CatchView } from '../../viewmodels/usePredictionResultViewModel'
import { BallOddsRow } from './BallOddsRow'

export function CatchOdds({ plan }: { plan: CatchView }) {
  return (
    <div className="mt-2 text-xs">
      <p className="text-muted">
        <span className="font-semibold text-ink">Preparar:</span>{' '}
        {plan.move === null ? 'sem golpe de dano; bolas com HP cheio' : `1× ${plan.move}`} · risco de KO {plan.koRisk} · alvo com {plan.targetHp}
      </p>
      {plan.balls.length === 0 ? (
        <p className="mt-1 text-bad">O golpe derruba o alvo em qualquer roll: não dá para capturar assim.</p>
      ) : (
        <table className="mt-1 w-full">
          <thead className="text-muted">
            <tr>
              <th scope="col" className="text-left font-normal">Bola</th>
              <th scope="col" className="text-right font-normal">Chance por bola</th>
              <th scope="col" colSpan={4} className="text-right font-normal">Bolas para 50 / 90 / 95 / 99%</th>
            </tr>
          </thead>
          <tbody>
            {plan.balls.map((ball) => (
              <BallOddsRow key={ball.ball} ball={ball} />
            ))}
          </tbody>
        </table>
      )}
    </div>
  )
}

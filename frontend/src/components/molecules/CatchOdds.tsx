import type { CatchView } from '../../viewmodels/usePredictionResultViewModel'
import { BallOddsRow } from './BallOddsRow'

export function CatchOdds({ plan }: { plan: CatchView }) {
  return (
    <details className="mt-2 text-xs">
      <summary className="cursor-pointer font-semibold text-ink">
        Preparação e Poké Balls{plan.pokeBallChance !== null && ` · ${plan.pokeBallChance} por bola`}
      </summary>
      <p className="mt-1 text-muted">
        <span className="font-semibold text-ink">Preparar:</span>{' '}
        {plan.hit === null ? 'sem golpe de dano; bolas com HP cheio' : `${plan.hits} ${plan.hit.move}`} · risco de KO {plan.koRisk} · alvo com {plan.targetHp}
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
    </details>
  )
}

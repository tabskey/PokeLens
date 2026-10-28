import { useTranslation } from '../../i18n/useTranslation'
import type { CatchView } from '../../viewmodels/usePredictionResultViewModel'
import { BallOddsRow } from './BallOddsRow'

export function CatchOdds({ plan }: { plan: CatchView }) {
  const { t } = useTranslation()

  return (
    <details className="mt-2 text-xs">
      <summary className="cursor-pointer font-semibold text-ink">
        {plan.pokeBallChance === null ? t('catch.summary') : t('catch.summaryChance', { chance: plan.pokeBallChance })}
      </summary>
      <p className="mt-1 text-muted">
        <span className="font-semibold text-ink">{t('catch.prepare')}</span>{' '}
        {plan.hit === null ? t('catch.noHit') : `${plan.hits} ${plan.hit.move}`} · {t('catch.risk', { risk: plan.koRisk })} · {t('catch.targetWith', { hp: plan.targetHp })}
      </p>
      {plan.balls.length === 0 ? (
        <p className="mt-1 text-bad">{t('catch.alwaysKo')}</p>
      ) : (
        <table className="mt-1 w-full">
          <thead className="text-muted">
            <tr>
              <th scope="col" className="text-left font-normal">{t('catch.ball')}</th>
              <th scope="col" className="text-right font-normal">{t('catch.chancePerBall')}</th>
              <th scope="col" colSpan={4} className="text-right font-normal">{t('catch.ballsFor')}</th>
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

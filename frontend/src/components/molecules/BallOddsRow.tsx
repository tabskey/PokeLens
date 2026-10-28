import { useTranslation } from '../../i18n/useTranslation'
import type { BallView } from '../../viewmodels/usePredictionResultViewModel'

/**
 * Chance per ball and balls needed for each confidence are different metrics, so they live in
 * separate columns with their own headings.
 */
export function BallOddsRow({ ball }: { ball: BallView }) {
  const { t } = useTranslation()

  return (
    <tr className="border-t border-line">
      <th scope="row" className="py-1 pr-2 text-left font-semibold">
        {ball.ball}
      </th>
      <td className="py-1 pr-2 text-right font-mono font-medium">{ball.chancePerBall}</td>
      {ball.ballsFor.map((entry) => (
        <td key={entry.confidence} className="py-1 pl-2 text-right font-mono font-medium" aria-label={t('catch.ballsForCell', { balls: entry.balls, confidence: entry.confidence })}>
          {entry.balls}
        </td>
      ))}
    </tr>
  )
}

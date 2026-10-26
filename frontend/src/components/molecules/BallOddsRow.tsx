import type { BallView } from '../../viewmodels/usePredictionResultViewModel'

/**
 * Chance per ball and balls needed for each confidence are different metrics, so they live in
 * separate columns with their own headings.
 */
export function BallOddsRow({ ball }: { ball: BallView }) {
  return (
    <tr className="border-t border-line">
      <th scope="row" className="py-1 pr-2 text-left font-semibold">
        {ball.ball}
      </th>
      <td className="py-1 pr-2 text-right">{ball.chancePerBall}</td>
      {ball.ballsFor.map((entry) => (
        <td key={entry.confidence} className="py-1 pl-2 text-right" aria-label={`${entry.balls} bolas para ${entry.confidence}`}>
          {entry.balls}
        </td>
      ))}
    </tr>
  )
}

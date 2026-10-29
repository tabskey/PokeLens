import { useTranslation } from '../../i18n/useTranslation'
import type { AuditRow } from '../../viewmodels/useAuditViewModel'

const columns = ['when', 'event', 'user', 'target', 'where', 'progress', 'objective', 'candidates'] as const

export function AuditTable({ rows }: { rows: AuditRow[] }) {
  const { t } = useTranslation()

  if (rows.length === 0) {
    return <p className="text-muted">{t('audit.empty')}</p>
  }

  return (
    <div className="overflow-x-auto rounded-[10px] border border-line bg-panel">
      <table className="w-full text-left text-sm">
        <caption className="sr-only">{t('audit.heading')}</caption>
        <thead className="font-mono text-[11px] uppercase tracking-[0.15em] text-muted">
          <tr>
            {columns.map((column) => (
              <th key={column} scope="col" className="whitespace-nowrap border-b border-line px-3 py-2 font-medium">
                {t(`audit.columns.${column}`)}
              </th>
            ))}
          </tr>
        </thead>
        <tbody>
          {rows.map((row) => (
            <tr key={row.id} className="border-b border-line last:border-0">
              {columns.map((column) => (
                <td key={column} className={`px-3 py-2 ${column === 'candidates' ? 'min-w-56' : 'whitespace-nowrap'}`}>
                  {row[column]}
                </td>
              ))}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}

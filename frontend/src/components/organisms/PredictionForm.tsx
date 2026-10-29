import { useTranslation } from '../../i18n/useTranslation'
import type { PredictionFormViewModel } from '../../viewmodels/usePredictionFormViewModel'
import { Button } from '../atoms/Button'
import { Hint } from '../atoms/Hint'
import { Label } from '../atoms/Label'
import { SegmentedControl } from '../atoms/SegmentedControl'
import { Field } from '../molecules/Field'
import { TargetSearch } from '../molecules/TargetSearch'

export function PredictionForm({ form }: { form: PredictionFormViewModel }) {
  const { t } = useTranslation()

  return (
    <form
      className="rounded-[10px] border border-line bg-panel p-[18px]"
      onSubmit={(event) => {
        event.preventDefault()
        void form.submit()
      }}
    >
      <h2 className="mb-4 font-mono text-[11px] font-medium uppercase tracking-[0.2em] text-muted">{t('form.heading')}</h2>

      {form.optionsError && <Hint error>{form.optionsError}</Hint>}

      <Field
        label={t('form.milestone')}
        value={form.milestone}
        options={form.milestoneOptions}
        placeholder={form.optionsLoaded ? t('form.choose') : t('form.loading')}
        onChange={form.setMilestone}
        hint={t('form.milestoneHint')}
        errors={form.fieldErrors.milestone}
        disabled={!form.optionsLoaded}
      />
      <TargetSearch search={form.search} disabled={!form.optionsLoaded} />

      <Label>{t('form.objective')}</Label>
      <SegmentedControl label={t('form.objective')} options={form.objectives} value={form.objective} onChange={form.setObjective} />

      <Label>{t('form.rankBy')}</Label>
      <SegmentedControl label={t('form.rankBy')} options={form.scenarios} value={form.scenario} onChange={form.setScenario} />
      <Hint>{t('form.rankHint')}</Hint>

      <Button type="submit" className="mt-5" disabled={form.loading}>
        {form.loading ? t('form.submitting') : t('form.submit')}
      </Button>
      {form.error && <Hint error>{form.error}</Hint>}
    </form>
  )
}

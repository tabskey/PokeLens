import { useTranslation } from '../../i18n/useTranslation'
import type { PredictionFormViewModel } from '../../viewmodels/usePredictionFormViewModel'
import { Button } from '../atoms/Button'
import { Hint } from '../atoms/Hint'
import { Label } from '../atoms/Label'
import { SegmentedControl } from '../atoms/SegmentedControl'
import { Field } from '../molecules/Field'

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
      <h2 className="mb-4 font-pixel text-[10px] text-accent">{t('form.heading')}</h2>

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
      <Field
        label={t('form.area')}
        value={form.area}
        options={form.areaOptions}
        placeholder={t('form.areaPlaceholder')}
        onChange={form.setArea}
        errors={form.fieldErrors.area}
        disabled={form.areaOptions.length === 0}
      />
      <Field
        label={t('form.target')}
        value={form.target}
        options={form.targetOptions}
        placeholder={t('form.targetPlaceholder')}
        onChange={form.setTarget}
        errors={form.fieldErrors.target}
        disabled={form.targetOptions.length === 0}
      />

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

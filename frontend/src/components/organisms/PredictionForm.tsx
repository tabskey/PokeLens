import type { PredictionFormViewModel } from '../../viewmodels/usePredictionFormViewModel'
import { Button } from '../atoms/Button'
import { Hint } from '../atoms/Hint'
import { Label } from '../atoms/Label'
import { SegmentedControl } from '../atoms/SegmentedControl'
import { Field } from '../molecules/Field'

export function PredictionForm({ form }: { form: PredictionFormViewModel }) {
  return (
    <form
      className="rounded-[10px] border border-line bg-panel p-[18px]"
      onSubmit={(event) => {
        event.preventDefault()
        void form.submit()
      }}
    >
      <h2 className="mb-4 font-pixel text-[10px] text-accent">SUA BATALHA</h2>

      {form.optionsError && <Hint error>{form.optionsError}</Hint>}

      <Field
        label="Até onde você chegou?"
        value={form.milestone}
        options={form.milestoneOptions}
        placeholder={form.optionsLoaded ? 'Escolha…' : 'Carregando…'}
        onChange={form.setMilestone}
        hint="Só sugerimos Pokémon e golpes que você já consegue obter até aqui."
        errors={form.fieldErrors.milestone}
        disabled={!form.optionsLoaded}
      />
      <Field
        label="Onde está o Pokémon-alvo?"
        value={form.area}
        options={form.areaOptions}
        placeholder="Escolha a área…"
        onChange={form.setArea}
        errors={form.fieldErrors.area}
        disabled={form.areaOptions.length === 0}
      />
      <Field
        label="Pokémon-alvo"
        value={form.target}
        options={form.targetOptions}
        placeholder="Escolha o Pokémon…"
        onChange={form.setTarget}
        errors={form.fieldErrors.target}
        disabled={form.targetOptions.length === 0}
      />

      <Label>Objetivo</Label>
      <SegmentedControl label="Objetivo" options={form.objectives} value={form.objective} onChange={form.setObjective} />

      <Label>Ordenar sugestões por</Label>
      <SegmentedControl label="Ordenar sugestões por" options={form.scenarios} value={form.scenario} onChange={form.setScenario} />
      <Hint>Mais seguro usa o cenário pessimista; maior potencial, o otimista. Os três aparecem sempre.</Hint>

      <Button type="submit" className="mt-5" disabled={form.loading}>
        {form.loading ? 'CALCULANDO…' : 'PREVER BATALHA'}
      </Button>
      {form.error && <Hint error>{form.error}</Hint>}
    </form>
  )
}

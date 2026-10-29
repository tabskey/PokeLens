import { useId, useState, type KeyboardEvent } from 'react'

import { useTranslation } from '../../i18n/useTranslation'
import type { TargetSearchViewModel } from '../../viewmodels/usePredictionFormViewModel'
import { Hint } from '../atoms/Hint'
import { Label } from '../atoms/Label'
import { Tag } from '../atoms/Tag'

/**
 * Single target search (docs/arquitetura.md §7.1): an ARIA combobox with suggestions grouped in
 * Pokémon and routes, then a choice of area and method when the target shows up in more than one place.
 */
export function TargetSearch({ search, disabled }: { search: TargetSearchViewModel; disabled?: boolean }) {
  const { t } = useTranslation()
  const id = useId()
  const [open, setOpen] = useState(false)
  const [active, setActive] = useState(0)

  const options = search.groups.flatMap((group) => group.options)
  const expanded = open && options.length > 0
  const optionId = (index: number) => `${id}-option-${index}`
  const hasErrors = search.errors.length > 0

  const pick = (index: number) => {
    const option = options[index]
    if (!option) {
      return
    }
    search.pick(option.id)
    // Picking a route lists its Pokémon; picking a Pokémon closes the list.
    setOpen(option.id.startsWith('area:'))
    setActive(0)
  }

  const onKeyDown = (event: KeyboardEvent<HTMLInputElement>) => {
    if (event.key === 'ArrowDown' || event.key === 'ArrowUp') {
      event.preventDefault()
      const step = event.key === 'ArrowDown' ? 1 : -1
      setActive((current) => (open && options.length > 0 ? (current + step + options.length) % options.length : 0))
      setOpen(true)
    } else if (event.key === 'Enter' && expanded) {
      event.preventDefault()
      pick(active)
    } else if (event.key === 'Escape') {
      setOpen(false)
    }
  }

  return (
    <div className="relative">
      <Label htmlFor={id}>{t('form.target')}</Label>
      <input
        id={id}
        type="text"
        role="combobox"
        autoComplete="off"
        aria-autocomplete="list"
        aria-expanded={expanded}
        aria-controls={`${id}-listbox`}
        aria-activedescendant={expanded ? optionId(active) : undefined}
        aria-invalid={hasErrors}
        aria-describedby={`${id}-hint`}
        placeholder={t('form.searchPlaceholder')}
        value={search.query}
        disabled={disabled}
        onChange={(event) => {
          search.setQuery(event.target.value)
          setOpen(true)
          setActive(0)
        }}
        onKeyDown={onKeyDown}
        onFocus={() => setOpen(true)}
        onBlur={() => setOpen(false)}
        className="w-full rounded-lg border border-line bg-surface px-2.5 py-2 aria-invalid:border-bad"
      />
      <div
        id={`${id}-listbox`}
        role="listbox"
        aria-label={t('form.target')}
        hidden={!expanded}
        className="absolute z-10 mt-1 max-h-72 w-full overflow-y-auto rounded-lg border border-line bg-panel py-1 shadow-lg"
      >
        {search.groups.map((group) => (
          <div key={group.label} role="group" aria-label={group.label}>
            <div className="px-2.5 pb-1 pt-2 font-mono text-[11px] uppercase tracking-[0.2em] text-muted" aria-hidden="true">
              {group.label}
            </div>
            {group.options.map((option) => {
              const current = options.indexOf(option)
              return (
                <div
                  key={option.id}
                  id={optionId(current)}
                  role="option"
                  aria-selected={current === active}
                  // Keeps focus in the input so the blur does not close the list before the click.
                  onMouseDown={(event) => event.preventDefault()}
                  onClick={() => pick(current)}
                  onMouseEnter={() => setActive(current)}
                  className="cursor-pointer px-2.5 py-1.5 aria-selected:bg-surface aria-selected:text-accent"
                >
                  <span className="font-semibold">{option.label}</span>
                  {option.detail && <span className="block text-xs text-muted">{option.detail}</span>}
                </div>
              )
            })}
          </div>
        ))}
      </div>

      {hasErrors ? (
        <Hint id={`${id}-hint`} error>
          {search.errors.join(' ')}
        </Hint>
      ) : (
        <Hint id={`${id}-hint`}>{search.noMatches ? t('search.empty') : t('form.searchHint')}</Hint>
      )}

      {search.choices.length > 0 && (
        <fieldset className="mt-3">
          <legend className="mb-1.5 font-semibold">{t('search.choose')}</legend>
          <div className="grid gap-1.5">
            {search.choices.map((choice) => (
              <label
                key={choice.id}
                className="flex cursor-pointer items-center gap-2 rounded-lg border border-line bg-surface px-2.5 py-2 has-checked:border-ink"
              >
                <input type="radio" name={`${id}-encounter`} value={choice.id} checked={search.chosen === choice.id} onChange={() => search.choose(choice.id)} />
                <span className="flex flex-1 flex-wrap items-center gap-x-2 gap-y-1">
                  <span className="font-semibold">{choice.area}</span>
                  <Tag>{choice.requirement ?? choice.method}</Tag>
                  <span className="text-xs text-muted">
                    {choice.levels} · {choice.chance}
                  </span>
                </span>
              </label>
            ))}
          </div>
        </fieldset>
      )}
    </div>
  )
}

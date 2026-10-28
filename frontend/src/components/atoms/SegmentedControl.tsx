export interface Segment<T extends string> {
  value: T
  label: string
  /** Background of the pressed segment on the dark variant (e.g. the game version colour). */
  activeClassName?: string
}

interface SegmentedControlProps<T extends string> {
  label: string
  options: Segment<T>[]
  value: T
  onChange: (value: T) => void
  variant?: 'light' | 'dark'
}

const variants = {
  light: 'border-line bg-surface font-semibold aria-pressed:border-ink aria-pressed:bg-ink aria-pressed:text-bg',
  dark: 'border-white/30 font-mono text-[11px] font-medium tracking-wider text-white',
}

export function SegmentedControl<T extends string>({ label, options, value, onChange, variant = 'light' }: SegmentedControlProps<T>) {
  return (
    <div role="group" aria-label={label} className="grid auto-cols-fr grid-flow-col gap-1.5">
      {options.map((option) => {
        const pressed = option.value === value
        return (
          <button
            key={option.value}
            type="button"
            aria-pressed={pressed}
            onClick={() => onChange(option.value)}
            className={`cursor-pointer rounded-lg border p-2.5 ${variants[variant]} ${variant === 'dark' ? (pressed ? (option.activeClassName ?? 'bg-white/15') : 'bg-transparent') : ''}`}
          >
            {option.label}
          </button>
        )
      })}
    </div>
  )
}
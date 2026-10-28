import type { SelectHTMLAttributes } from 'react'

export interface SelectOption {
  value: string
  label: string
}

interface SelectProps extends Omit<SelectHTMLAttributes<HTMLSelectElement>, 'onChange'> {
  options: SelectOption[]
  placeholder?: string
  onChange: (value: string) => void
}

export function Select({ options, placeholder, onChange, className = '', ...props }: SelectProps) {
  return (
    <select
      className={`w-full rounded-lg border border-line bg-surface px-2.5 py-2 aria-invalid:border-bad ${className}`}
      onChange={(event) => onChange(event.target.value)}
      {...props}
    >
      {placeholder !== undefined && <option value="">{placeholder}</option>}
      {options.map((option) => (
        <option key={option.value} value={option.value}>
          {option.label}
        </option>
      ))}
    </select>
  )
}

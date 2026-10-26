import { useId } from 'react'

import { Hint } from '../atoms/Hint'
import { Label } from '../atoms/Label'
import { Select, type SelectOption } from '../atoms/Select'

interface FieldProps {
  label: string
  value: string
  options: SelectOption[]
  placeholder: string
  onChange: (value: string) => void
  hint?: string
  errors?: string[]
  disabled?: boolean
}

export function Field({ label, value, options, placeholder, onChange, hint, errors, disabled }: FieldProps) {
  const id = useId()
  const hasErrors = errors !== undefined && errors.length > 0
  return (
    <div>
      <Label htmlFor={id}>{label}</Label>
      <Select
        id={id}
        value={value}
        options={options}
        placeholder={placeholder}
        onChange={onChange}
        disabled={disabled}
        aria-invalid={hasErrors}
        aria-describedby={hasErrors || hint ? `${id}-hint` : undefined}
      />
      {hasErrors ? (
        <Hint id={`${id}-hint`} error>
          {errors.join(' ')}
        </Hint>
      ) : (
        hint && <Hint id={`${id}-hint`}>{hint}</Hint>
      )}
    </div>
  )
}

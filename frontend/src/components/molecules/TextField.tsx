import { useId, type InputHTMLAttributes } from 'react'

import { Hint } from '../atoms/Hint'
import { Label } from '../atoms/Label'

interface TextFieldProps extends Omit<InputHTMLAttributes<HTMLInputElement>, 'onChange' | 'value'> {
  label: string
  value: string
  onChange: (value: string) => void
  hint?: string
  errors?: string[]
}

export function TextField({ label, value, onChange, hint, errors = [], ...props }: TextFieldProps) {
  const id = useId()
  const hasErrors = errors.length > 0
  return (
    <div>
      <Label htmlFor={id}>{label}</Label>
      <input
        id={id}
        value={value}
        onChange={(event) => onChange(event.target.value)}
        aria-invalid={hasErrors}
        aria-describedby={hasErrors || hint ? `${id}-hint` : undefined}
        className="w-full rounded-lg border border-line bg-surface px-2.5 py-2 aria-invalid:border-bad"
        {...props}
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

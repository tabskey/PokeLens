import type { ButtonHTMLAttributes } from 'react'

export function Button({ className = '', ...props }: ButtonHTMLAttributes<HTMLButtonElement>) {
  return (
    <button
      className={`w-full cursor-pointer rounded-lg bg-accent p-3 font-display text-sm font-bold tracking-wide text-on-status hover:brightness-110 disabled:cursor-wait disabled:opacity-60 ${className}`}
      {...props}
    />
  )
}

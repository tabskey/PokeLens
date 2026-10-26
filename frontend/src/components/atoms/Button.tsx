import type { ButtonHTMLAttributes } from 'react'

export function Button({ className = '', ...props }: ButtonHTMLAttributes<HTMLButtonElement>) {
  return (
    <button
      className={`w-full cursor-pointer rounded-lg bg-accent p-3 font-pixel text-[10px] text-white hover:brightness-110 disabled:cursor-wait disabled:opacity-60 ${className}`}
      {...props}
    />
  )
}

import { useState } from 'react'

const sizes = { md: 'size-14 text-xs', lg: 'size-20 text-base' }

/** The Pokémon sprite; falls back to the name's initials when the image cannot load. */
export function Avatar({ name, src, size = 'md' }: { name: string; src?: string; size?: 'md' | 'lg' }) {
  const [failed, setFailed] = useState(false)

  return (
    <div className={`grid shrink-0 place-items-center rounded-lg border border-line bg-sprite font-pixel text-ink ${sizes[size]}`}>
      {src && !failed ? (
        <img src={src} alt={name} className="size-full object-contain [image-rendering:pixelated]" onError={() => setFailed(true)} />
      ) : (
        <span aria-hidden="true">{name.slice(0, 2).toUpperCase()}</span>
      )}
    </div>
  )
}

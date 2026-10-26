/** Initials in a tile: the data package has no sprites yet. */
export function Avatar({ name, size = 'md' }: { name: string; size?: 'md' | 'lg' }) {
  return (
    <div
      aria-hidden="true"
      className={`grid shrink-0 place-items-center rounded-lg border border-line bg-[#f1ede4] font-pixel text-ink ${size === 'lg' ? 'size-20 text-base' : 'size-14 text-xs'}`}
    >
      {name.slice(0, 2).toUpperCase()}
    </div>
  )
}

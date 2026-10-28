export function Spinner({ label }: { label: string }) {
  return (
    <div role="status" className="flex items-center gap-2 text-muted">
      <span className="size-4 animate-spin rounded-full border-2 border-line border-t-accent" aria-hidden="true" />
      {label}
    </div>
  )
}

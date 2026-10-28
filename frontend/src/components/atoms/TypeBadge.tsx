// Gen III type colours, as in docs/pokebattle-mockup.html. Full class names so Tailwind can find them.
const colours: Record<string, string> = {
  Normal: 'bg-[#a8a878] text-white',
  Fighting: 'bg-[#c03028] text-white',
  Flying: 'bg-[#a890f0] text-white',
  Poison: 'bg-[#a040a0] text-white',
  Ground: 'bg-[#e0c068] text-[#333]',
  Rock: 'bg-[#b8a038] text-white',
  Bug: 'bg-[#a8b820] text-white',
  Ghost: 'bg-[#705898] text-white',
  Steel: 'bg-[#b8b8d0] text-[#333]',
  Fire: 'bg-[#f08030] text-white',
  Water: 'bg-[#6890f0] text-white',
  Grass: 'bg-[#78c850] text-white',
  Electric: 'bg-[#f8d030] text-[#333]',
  Psychic: 'bg-[#f85888] text-white',
  Ice: 'bg-[#98d8d8] text-[#333]',
  Dragon: 'bg-[#7038f8] text-white',
  Dark: 'bg-[#705848] text-white',
}

export function TypeBadge({ type }: { type: string }) {
  return <span className={`rounded px-2 py-0.5 text-[10px] font-bold uppercase ${colours[type] ?? 'bg-muted text-white'}`}>{type}</span>
}

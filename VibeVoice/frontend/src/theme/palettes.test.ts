import { describe, expect, it } from 'vitest'
import css from './palettes.css?raw'
import { PALETTES, THEME_COLORS } from './theme'

type Tokens = Record<string, string>

/** Collects the custom properties of the first rule whose selector list contains `selector`. */
function block(selector: string): Tokens {
  const re = /([^{}]+)\{([^{}]*)\}/g
  for (const m of css.matchAll(re)) {
    const selectors = m[1]
      .replace(/\/\*[\s\S]*?\*\//g, '')
      .split(',')
      .map((s) => s.trim())
    if (!selectors.includes(selector)) continue
    const out: Tokens = {}
    for (const d of m[2].matchAll(/--([\w-]+):\s*([^;]+);/g)) out[d[1]] = d[2].trim()
    return out
  }
  throw new Error(`No block for ${selector}`)
}

function luminance(hex: string): number {
  const h = hex.replace('#', '')
  const [r, g, b] = [0, 2, 4].map((i) => parseInt(h.slice(i, i + 2), 16) / 255)
  const f = (c: number) => (c <= 0.03928 ? c / 12.92 : ((c + 0.055) / 1.055) ** 2.4)
  return 0.2126 * f(r) + 0.7152 * f(g) + 0.0722 * f(b)
}

function contrast(a: string, b: string): number {
  const [hi, lo] = [luminance(a), luminance(b)].sort((x, y) => y - x)
  return (hi + 0.05) / (lo + 0.05)
}

const combos = PALETTES.flatMap((p) => [
  { name: `${p} light`, sel: `[data-palette='${p}']`, mode: 'light' as const, p },
  { name: `${p} dark`, sel: `[data-palette='${p}'][data-mode='dark']`, mode: 'dark' as const, p },
])

/** Text/background pairs that must reach WCAG AA (4.5:1) in every identity × mode. */
const PAIRS: [fg: string, bg: string][] = [
  ['text', 'bg'],
  ['text', 'surface'],
  ['text', 'surface-3'],
  ['muted', 'surface'],
  ['muted', 'surface-2'],
  ['subtle', 'surface'],
  ['subtle', 'bg'],
  ['accent-fg', 'accent'],
  ['accent-text', 'surface'],
  ['accent-text', 'bg'],
  ['accent-text', 'accent-soft'],
  ['accent-text', 'surface-2'],
]

describe('visual identities (palettes.css)', () => {
  const keys = Object.keys(block(':root')).sort()

  it.each(combos)('$name defines the full token set', ({ sel }) => {
    expect(Object.keys(block(sel)).sort()).toEqual(keys)
  })

  it.each(combos)('$name meets WCAG AA for text pairs', ({ sel }) => {
    const t = block(sel)
    const failures = PAIRS.map(([fg, bg]) => ({ pair: `${fg} on ${bg}`, ratio: contrast(t[fg], t[bg]) })).filter(
      (r) => r.ratio < 4.5,
    )
    expect(failures).toEqual([])
  })

  it.each(combos)('$name theme-color matches the JS map', ({ sel, mode, p }) => {
    expect(block(sel)['theme-color']).toBe(THEME_COLORS[p][mode])
  })

  it('neon uses dark text on lime buttons', () => {
    expect(block("[data-palette='neon']")['accent']).toBe('#a3e635')
    expect(block("[data-palette='neon']")['accent-fg']).toBe('#0a0a0b')
  })

  it('sunset is the default identity with the brand colours', () => {
    const t = block(':root')
    expect([t['grad-from'], t['grad-via'], t['grad-to']]).toEqual(['#f2545b', '#c2338e', '#6d4aff'])
    expect(t['bg']).toBe('#fff8f3')
    expect(block('.dark')['bg']).toBe('#1a1220')
  })
})

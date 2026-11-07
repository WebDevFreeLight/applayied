import type { ReactNode } from 'react'

const ARABIC_MARKS = '[\\u064B-\\u065F\\u0670\\u0640]*'

const VARIANTS: Record<string, string> = {
  ا: '[اأإآ]',
  أ: '[اأإآ]',
  إ: '[اأإآ]',
  آ: '[اأإآ]',
  ي: '[يى]',
  ى: '[يى]',
  ه: '[هة]',
  ة: '[هة]',
}

function escapeRegex(ch: string): string {
  return ch.replace(/[.*+?^${}()|[\]\\]/g, '\\$&')
}

/** Strips Arabic diacritics / tatweel from the query itself. */
function cleanTerm(term: string): string {
  return term.replace(/[ً-ٰٟـ]/g, '')
}

/**
 * Builds a case-insensitive regex for the query terms that also tolerates Arabic
 * normalization (diacritics, tatweel, أ/إ/آ→ا, ى→ي, ة→ه) — mirroring the API search.
 */
export function buildHighlightRegex(query: string): RegExp | null {
  const terms = query
    .split(/\s+/)
    .map(cleanTerm)
    .filter((t) => t.length > 0)
  if (!terms.length) return null
  const parts = terms.map((term) =>
    Array.from(term)
      .map((ch) => VARIANTS[ch] ?? escapeRegex(ch))
      .join(ARABIC_MARKS),
  )
  return new RegExp(`(${parts.join('|')})`, 'giu')
}

export function highlight(text: string, query: string): ReactNode {
  const re = buildHighlightRegex(query)
  if (!re || !text) return text
  const out: ReactNode[] = []
  let last = 0
  for (const m of text.matchAll(re)) {
    const start = m.index ?? 0
    if (!m[0]) continue
    if (start > last) out.push(text.slice(last, start))
    out.push(
      <mark key={start} className="hl">
        {m[0]}
      </mark>,
    )
    last = start + m[0].length
  }
  if (last < text.length) out.push(text.slice(last))
  return out
}

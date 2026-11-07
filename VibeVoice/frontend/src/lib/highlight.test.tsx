import { render } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { buildHighlightRegex, highlight } from './highlight'

describe('highlight', () => {
  it('highlights case-insensitively', () => {
    const { container } = render(<p>{highlight('Morning Walk in the park', 'walk')}</p>)
    expect(container.querySelector('mark')?.textContent).toBe('Walk')
  })

  it('matches Arabic letter variants and ignores diacritics like the API search', () => {
    const re = buildHighlightRegex('احمد')!
    expect('أحمد'.match(re)?.[0]).toBe('أحمد')
    expect('أَحْمَد'.match(new RegExp(re.source, 'gu'))?.[0]).toBe('أَحْمَد')
    const re2 = buildHighlightRegex('مدرسه')!
    expect('ذهبت إلى المدرسة'.match(re2)?.[0]).toBe('مدرسة')
  })

  it('returns the text unchanged for an empty query', () => {
    expect(highlight('abc', '   ')).toBe('abc')
  })
})

import { act, render, screen } from '@testing-library/react'
import { useTranslation } from 'react-i18next'
import { afterEach, describe, expect, it } from 'vitest'
import ar from './ar.json'
import en from './en.json'
import i18n, { isRtl, normalizeLanguage, setLanguage } from './index'

function Probe() {
  const { t } = useTranslation()
  return <h1>{t('nav.notes')}</h1>
}

/** Flattens nested keys, collapsing i18next plural suffixes (_one, _other, …). */
function keysOf(obj: Record<string, unknown>, prefix = ''): Set<string> {
  const out = new Set<string>()
  for (const [k, v] of Object.entries(obj)) {
    const key = `${prefix}${k.replace(/_(zero|one|two|few|many|other)$/, '')}`
    if (v && typeof v === 'object') keysOf(v as Record<string, unknown>, `${key}.`).forEach((x) => out.add(x))
    else out.add(key)
  }
  return out
}

describe('i18n', () => {
  afterEach(async () => {
    await act(async () => {
      await setLanguage('en')
    })
  })

  it('switching to Arabic sets <html lang="ar" dir="rtl"> and translates the UI', async () => {
    render(<Probe />)
    expect(screen.getByRole('heading')).toHaveTextContent('Notes')
    expect(document.documentElement.dir).toBe('ltr')

    await act(async () => {
      await setLanguage('ar')
    })
    expect(document.documentElement.lang).toBe('ar')
    expect(document.documentElement.dir).toBe('rtl')
    expect(screen.getByRole('heading')).toHaveTextContent('الملاحظات')
    expect(localStorage.getItem('vv.lang')).toBe('ar')

    await act(async () => {
      await setLanguage('en')
    })
    expect(document.documentElement.lang).toBe('en')
    expect(document.documentElement.dir).toBe('ltr')
    expect(screen.getByRole('heading')).toHaveTextContent('Notes')
  })

  it('uses Arabic plural forms', async () => {
    await act(async () => {
      await setLanguage('ar')
    })
    expect(i18n.t('search.results', { count: 2 })).toBe('نتيجتان')
    expect(i18n.t('search.results', { count: 5 })).toBe('5 نتائج')
  })

  it('normalizes language codes', () => {
    expect(normalizeLanguage('ar-SA')).toBe('ar')
    expect(normalizeLanguage('en-US')).toBe('en')
    expect(normalizeLanguage(undefined)).toBe('en')
    expect(isRtl('ar')).toBe(true)
    expect(isRtl('en')).toBe(false)
  })

  it('has every English key translated to Arabic (and vice versa)', () => {
    const enKeys = keysOf(en)
    const arKeys = keysOf(ar)
    expect([...enKeys].filter((k) => !arKeys.has(k))).toEqual([])
    expect([...arKeys].filter((k) => !enKeys.has(k))).toEqual([])
  })
})

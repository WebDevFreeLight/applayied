import { format, formatDistanceToNow, isValid, parseISO, type Locale } from 'date-fns'
import { ar, enUS } from 'date-fns/locale'
import { useMemo } from 'react'
import { useTranslation } from 'react-i18next'
import { parseDateOnly } from './utils'

export function dateLocale(lang: string): Locale {
  return lang.startsWith('ar') ? ar : enUS
}

function toDate(value: string | Date): Date | null {
  if (value instanceof Date) return value
  if (/^\d{4}-\d{2}-\d{2}$/.test(value)) return parseDateOnly(value)
  const d = parseISO(value)
  return isValid(d) ? d : null
}

/** Locale-aware date helpers bound to the active UI language. */
export function useDates() {
  const { i18n } = useTranslation()
  const lang = i18n.language
  return useMemo(() => {
    const locale = dateLocale(lang)
    return {
      locale,
      format(value: string | Date | null | undefined, pattern = 'PP'): string {
        if (!value) return ''
        const d = toDate(value)
        return d ? format(d, pattern, { locale }) : ''
      },
      relative(value: string | Date | null | undefined): string {
        if (!value) return ''
        const d = toDate(value)
        return d ? formatDistanceToNow(d, { addSuffix: true, locale }) : ''
      },
    }
  }, [lang])
}

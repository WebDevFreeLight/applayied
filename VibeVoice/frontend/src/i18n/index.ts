import i18n from 'i18next'
import { initReactI18next } from 'react-i18next'
import ar from './ar.json'
import en from './en.json'

export type AppLanguage = 'en' | 'ar'

export const LANGUAGES: readonly AppLanguage[] = ['en', 'ar']
const STORAGE_KEY = 'vv.lang'

export function isRtl(lang: string): boolean {
  return lang.startsWith('ar')
}

export function normalizeLanguage(lang: string | null | undefined): AppLanguage {
  return lang && lang.toLowerCase().startsWith('ar') ? 'ar' : 'en'
}

/** Reflects the active language on <html lang dir> so the whole layout mirrors for RTL. */
export function applyDocumentLanguage(lang: string): void {
  const l = normalizeLanguage(lang)
  document.documentElement.lang = l
  document.documentElement.dir = isRtl(l) ? 'rtl' : 'ltr'
}

function initialLanguage(): AppLanguage {
  try {
    const stored = localStorage.getItem(STORAGE_KEY)
    if (stored) return normalizeLanguage(stored)
  } catch {
    /* ignore */
  }
  return normalizeLanguage(typeof navigator !== 'undefined' ? navigator.language : 'en')
}

i18n.on('languageChanged', (lng) => {
  applyDocumentLanguage(lng)
  try {
    localStorage.setItem(STORAGE_KEY, normalizeLanguage(lng))
  } catch {
    /* ignore */
  }
})

void i18n.use(initReactI18next).init({
  resources: { en: { translation: en }, ar: { translation: ar } },
  lng: initialLanguage(),
  fallbackLng: 'en',
  supportedLngs: ['en', 'ar'],
  interpolation: { escapeValue: false },
  returnNull: false,
  initAsync: false,
})

applyDocumentLanguage(i18n.language)

export function setLanguage(lang: AppLanguage): Promise<unknown> {
  return i18n.changeLanguage(lang)
}

export default i18n

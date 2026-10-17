import i18n from 'i18next'
import { initReactI18next } from 'react-i18next'
import en from './en'
import ar from './ar'

// Bilingual (English / Arabic) with persisted choice. `dir` is applied in applyDir() below.
const saved = localStorage.getItem('pmo.lang') || 'en'

i18n.use(initReactI18next).init({
  resources: { en: { translation: en }, ar: { translation: ar } },
  lng: saved,
  fallbackLng: 'en',
  interpolation: { escapeValue: false },
})

/** Set <html lang/dir> so the whole app flips to RTL for Arabic. */
export function applyDir(lang: string) {
  const dir = lang === 'ar' ? 'rtl' : 'ltr'
  document.documentElement.setAttribute('lang', lang)
  document.documentElement.setAttribute('dir', dir)
}

export function changeLanguage(lang: string) {
  localStorage.setItem('pmo.lang', lang)
  i18n.changeLanguage(lang)
  applyDir(lang)
}

applyDir(saved)
export default i18n

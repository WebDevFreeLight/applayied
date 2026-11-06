import { useTranslation } from 'react-i18next'
import type { VoiceDto } from '@/api/types'
import { Select, type SelectProps } from '../ui/Field'
import { groupVoices, isFallbackVoice } from './voiceUtils'

/** Voice <select> grouped by language; fallback-engine voices (e.g. Arabic) are flagged. */
export function VoiceSelect({
  voices,
  value,
  onChange,
  emptyLabel,
  ...rest
}: Omit<SelectProps, 'value' | 'onChange'> & {
  voices: VoiceDto[]
  value: string
  onChange: (v: string) => void
  /** When set, adds an empty option with this label. */
  emptyLabel?: string
}) {
  const { t } = useTranslation()
  const langName = (code: string) =>
    code === 'en' || code === 'ar' ? t(`studio.languages.${code}`) : code.toUpperCase()
  return (
    <Select value={value} onChange={(e) => onChange(e.target.value)} {...rest}>
      {emptyLabel !== undefined && <option value="">{emptyLabel}</option>}
      {groupVoices(voices).map(([lang, list]) => (
        <optgroup key={lang} label={langName(lang)}>
          {list.map((v) => (
            <option key={v.id} value={v.id}>
              {v.name}
              {v.gender ? ` · ${v.gender === 'female' ? '♀' : '♂'}` : ''}
              {lang === 'ar' || isFallbackVoice(v) ? ` — ${t('readAloud.fallbackHint')} (${v.engine})` : ''}
            </option>
          ))}
        </optgroup>
      ))}
    </Select>
  )
}

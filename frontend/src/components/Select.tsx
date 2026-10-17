import ReactSelect, { type StylesConfig } from 'react-select'
import { useTranslation } from 'react-i18next'

export interface Option { value: string; label: string }

// Theme-aware styling via CSS variables, so the dropdown follows the light/dark theme.
// menuPortalTarget=document.body ensures the menu renders above tables/modals (never clipped).
const styles: StylesConfig<Option, boolean> = {
  control: (base, s) => ({
    ...base,
    backgroundColor: 'var(--panel-2)',
    borderColor: s.isFocused ? 'var(--brand)' : 'var(--border)',
    boxShadow: 'none',
    minHeight: 36,
    ':hover': { borderColor: 'var(--brand)' },
  }),
  singleValue: (base) => ({ ...base, color: 'var(--text)' }),
  multiValue: (base) => ({ ...base, backgroundColor: 'var(--border)' }),
  multiValueLabel: (base) => ({ ...base, color: 'var(--text)' }),
  input: (base) => ({ ...base, color: 'var(--text)' }),
  placeholder: (base) => ({ ...base, color: 'var(--muted)' }),
  menu: (base) => ({ ...base, backgroundColor: 'var(--panel)', border: '1px solid var(--border)', zIndex: 80 }),
  menuPortal: (base) => ({ ...base, zIndex: 9999 }),
  option: (base, s) => ({
    ...base,
    backgroundColor: s.isSelected ? 'var(--brand)' : s.isFocused ? 'var(--panel-2)' : 'transparent',
    color: s.isSelected ? '#fff' : 'var(--text)',
    cursor: 'pointer',
  }),
  dropdownIndicator: (base) => ({ ...base, color: 'var(--muted)' }),
  indicatorSeparator: (base) => ({ ...base, backgroundColor: 'var(--border)' }),
}

interface Props {
  options: Option[]
  value?: string | null
  onChange: (value: string | null) => void
  placeholder?: string
  isClearable?: boolean
  isDisabled?: boolean
}

/** Single-select dropdown (react-select) used across the app in place of native <select>. */
export default function Select({ options, value, onChange, placeholder, isClearable = true, isDisabled }: Props) {
  const { i18n } = useTranslation()
  const selected = options.find((o) => o.value === value) || null
  return (
    <ReactSelect<Option>
      options={options}
      value={selected}
      onChange={(opt) => onChange(opt ? opt.value : null)}
      placeholder={placeholder || ''}
      isClearable={isClearable}
      isDisabled={isDisabled}
      isRtl={i18n.language === 'ar'}
      styles={styles}
      menuPortalTarget={typeof document !== 'undefined' ? document.body : undefined}
      classNamePrefix="rs"
    />
  )
}

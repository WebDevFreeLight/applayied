import { Hash, User, X } from 'lucide-react'
import { useId, useMemo, useRef, useState, type KeyboardEvent } from 'react'
import { useTranslation } from 'react-i18next'
import type { NameCount } from '@/api/types'
import { cn } from '@/lib/utils'

export interface TagInputProps {
  value: string[]
  onChange: (value: string[]) => void
  /** Autocomplete source (e.g. /api/tags or /api/people). */
  suggestions?: NameCount[]
  placeholder?: string
  label: string
  variant?: 'tag' | 'person'
  maxLength?: number
  id?: string
}

const norm = (s: string) => s.trim().toLocaleLowerCase()

/** Chip input with keyboard-navigable autocomplete. Enter/comma adds, Backspace removes. */
export function TagInput({
  value,
  onChange,
  suggestions = [],
  placeholder,
  label,
  variant = 'tag',
  maxLength = 50,
  id,
}: TagInputProps) {
  const { t } = useTranslation()
  const [text, setText] = useState('')
  const [open, setOpen] = useState(false)
  const [active, setActive] = useState(0)
  const inputRef = useRef<HTMLInputElement>(null)
  const autoId = useId()
  const inputId = id ?? autoId
  const listId = `${inputId}-list`
  const Icon = variant === 'person' ? User : Hash

  const options = useMemo(() => {
    const q = norm(text)
    const taken = new Set(value.map(norm))
    const list = suggestions
      .filter((s) => !taken.has(norm(s.name)) && (!q || norm(s.name).includes(q)))
      .sort((a, b) => b.count - a.count)
      .slice(0, 8)
      .map((s) => ({ name: s.name, count: s.count as number | null }))
    if (q && !taken.has(q) && !list.some((s) => norm(s.name) === q)) {
      list.push({ name: text.trim(), count: null })
    }
    return list
  }, [text, suggestions, value])

  const add = (raw: string) => {
    const v = raw.trim().replace(/^#/, '').slice(0, maxLength)
    if (!v) return
    if (!value.some((x) => norm(x) === norm(v))) onChange([...value, v])
    setText('')
    setActive(0)
  }

  const remove = (tag: string) => onChange(value.filter((x) => x !== tag))

  const onKeyDown = (e: KeyboardEvent<HTMLInputElement>) => {
    if (e.key === 'Enter' || e.key === ',' || e.key === '،') {
      if (!text.trim() && e.key === 'Enter') return
      e.preventDefault()
      const pick = open && options[active] ? options[active].name : text
      add(pick)
    } else if (e.key === 'Backspace' && !text && value.length) {
      remove(value[value.length - 1])
    } else if (e.key === 'ArrowDown') {
      e.preventDefault()
      setOpen(true)
      setActive((a) => Math.min(a + 1, Math.max(options.length - 1, 0)))
    } else if (e.key === 'ArrowUp') {
      e.preventDefault()
      setActive((a) => Math.max(a - 1, 0))
    } else if (e.key === 'Escape') {
      setOpen(false)
    }
  }

  const showList = open && options.length > 0

  return (
    <div className="relative">
      <div
        className="flex min-h-10 flex-wrap items-center gap-1.5 rounded-xl border border-border bg-surface px-2 py-1.5 transition-colors hover:border-border-strong focus-within:border-accent focus-within:ring-2 focus-within:ring-accent/25"
        onClick={() => inputRef.current?.focus()}
      >
        {value.map((tag) => (
          <span
            key={tag}
            className="inline-flex items-center gap-1 rounded-lg bg-accent-soft py-0.5 ps-2 pe-1 text-xs font-medium text-accent-text"
          >
            <Icon className="size-3" aria-hidden />
            <bdi>{tag}</bdi>
            <button
              type="button"
              onClick={(e) => {
                e.stopPropagation()
                remove(tag)
              }}
              aria-label={t('tags.remove', { value: tag })}
              className="rounded p-0.5 hover:bg-accent/15"
            >
              <X className="size-3" aria-hidden />
            </button>
          </span>
        ))}
        <input
          ref={inputRef}
          id={inputId}
          dir="auto"
          value={text}
          aria-label={label}
          role="combobox"
          aria-expanded={showList}
          aria-controls={listId}
          aria-autocomplete="list"
          aria-activedescendant={showList ? `${listId}-${active}` : undefined}
          placeholder={value.length ? undefined : placeholder}
          maxLength={maxLength}
          onChange={(e) => {
            setText(e.target.value)
            setOpen(true)
            setActive(0)
          }}
          onFocus={() => setOpen(true)}
          onBlur={() => {
            setOpen(false)
            if (text.trim()) add(text)
          }}
          onKeyDown={onKeyDown}
          className="h-7 min-w-24 flex-1 bg-transparent px-1 text-sm text-fg placeholder:text-subtle focus:outline-none"
        />
      </div>
      {showList && (
        <ul
          id={listId}
          role="listbox"
          aria-label={t('tags.suggestions')}
          className="absolute inset-x-0 top-full z-30 mt-1 max-h-60 overflow-auto rounded-xl border border-border bg-surface p-1 shadow-pop"
        >
          {options.map((o, i) => (
            <li
              key={`${o.name}-${i}`}
              id={`${listId}-${i}`}
              role="option"
              aria-selected={i === active}
              onMouseDown={(e) => {
                e.preventDefault()
                add(o.name)
              }}
              onMouseEnter={() => setActive(i)}
              className={cn(
                'flex cursor-pointer items-center justify-between gap-2 rounded-lg px-2.5 py-1.5 text-sm',
                i === active ? 'bg-accent-soft text-accent-text' : 'text-fg',
              )}
            >
              <span className="flex min-w-0 items-center gap-1.5">
                <Icon className="size-3.5 shrink-0 opacity-70" aria-hidden />
                <bdi className="truncate">{o.count === null ? t('tags.add', { value: o.name }) : o.name}</bdi>
              </span>
              {o.count !== null && <span className="text-xs text-subtle">{o.count}</span>}
            </li>
          ))}
        </ul>
      )}
    </div>
  )
}

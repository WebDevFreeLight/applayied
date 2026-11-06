import { useQuery } from '@tanstack/react-query'
import { CornerDownLeft, Loader2, Search } from 'lucide-react'
import { useId, useRef, useState, type KeyboardEvent, type RefObject } from 'react'
import { useTranslation } from 'react-i18next'
import { useNavigate } from 'react-router'
import { qk } from '@/api/keys'
import { searchApi } from '@/api/misc'
import { useDebouncedValue } from '@/hooks/misc'
import { highlight } from '@/lib/highlight'
import { KIND_ICON, searchResultLink } from '@/lib/links'
import { cn } from '@/lib/utils'
import { Modal } from '../ui/Modal'

/** ⌘/Ctrl+K command-palette style search. */
export function QuickSearch({ open, onClose }: { open: boolean; onClose: () => void }) {
  const { t } = useTranslation()
  const inputRef = useRef<HTMLInputElement>(null)
  return (
    <Modal open={open} onClose={onClose} title={t('search.quickTitle')} size="lg" initialFocusRef={inputRef}>
      {open && <QuickSearchBody inputRef={inputRef} onClose={onClose} />}
    </Modal>
  )
}

function QuickSearchBody({
  inputRef,
  onClose,
}: {
  inputRef: RefObject<HTMLInputElement | null>
  onClose: () => void
}) {
  const { t } = useTranslation()
  const navigate = useNavigate()
  const [q, setQ] = useState('')
  const [active, setActive] = useState(0)
  const debounced = useDebouncedValue(q.trim(), 250)
  const listId = useId()
  const query = { q: debounced, page: 1, pageSize: 8 }
  const { data, isFetching } = useQuery({
    queryKey: qk.search.query(query),
    queryFn: ({ signal }) => searchApi.search(query, signal),
    enabled: debounced.length > 0,
    staleTime: 30_000,
  })
  const items = debounced ? (data?.items ?? []) : []

  const go = (to: string) => {
    onClose()
    navigate(to)
  }

  const onKeyDown = (e: KeyboardEvent<HTMLInputElement>) => {
    if (e.key === 'ArrowDown') {
      e.preventDefault()
      setActive((a) => Math.min(a + 1, items.length))
    } else if (e.key === 'ArrowUp') {
      e.preventDefault()
      setActive((a) => Math.max(a - 1, 0))
    } else if (e.key === 'Enter') {
      e.preventDefault()
      if (active < items.length && items[active]) go(searchResultLink(items[active]))
      else if (q.trim()) go(`/search?q=${encodeURIComponent(q.trim())}`)
    }
  }

  return (
    <div className="flex flex-col gap-3">
      <div className="relative">
        <Search className="pointer-events-none absolute start-3 top-1/2 size-4 -translate-y-1/2 text-subtle" aria-hidden />
        <input
          ref={inputRef}
          dir="auto"
          value={q}
          onChange={(e) => {
            setQ(e.target.value)
            setActive(0)
          }}
          onKeyDown={onKeyDown}
          placeholder={t('search.placeholder')}
          aria-label={t('search.label')}
          role="combobox"
          aria-expanded={items.length > 0}
          aria-controls={listId}
          aria-activedescendant={items.length ? `${listId}-${active}` : undefined}
          className="h-12 w-full rounded-xl border border-border bg-surface-2 ps-10 pe-10 text-base text-fg placeholder:text-subtle focus:border-accent focus:ring-2 focus:ring-accent/25 focus:outline-none"
        />
        {isFetching && (
          <Loader2 className="absolute end-3 top-1/2 size-4 -translate-y-1/2 animate-spin text-subtle" aria-hidden />
        )}
      </div>

      <ul id={listId} role="listbox" aria-label={t('search.title')} className="flex flex-col gap-1">
        {items.map((r, i) => {
          const Icon = KIND_ICON[r.kind]
          return (
            <li
              key={`${r.kind}-${r.id}`}
              id={`${listId}-${i}`}
              role="option"
              aria-selected={i === active}
              onMouseEnter={() => setActive(i)}
              onClick={() => go(searchResultLink(r))}
              className={cn(
                'flex cursor-pointer items-start gap-3 rounded-xl px-3 py-2.5',
                i === active ? 'bg-accent-soft' : 'hover:bg-surface-2',
              )}
            >
              <Icon className="mt-0.5 size-4 shrink-0 text-accent-text" aria-hidden />
              <div className="min-w-0 flex-1">
                <p className="truncate text-sm font-medium text-fg" dir="auto">
                  {highlight(r.title || t('common.untitled'), debounced)}
                </p>
                {r.snippet && (
                  <p className="line-clamp-1 text-xs text-muted" dir="auto">
                    {highlight(r.snippet, debounced)}
                  </p>
                )}
              </div>
              <span className="shrink-0 text-[11px] text-subtle">{t(`search.kinds.${r.kind}`)}</span>
            </li>
          )
        })}
      </ul>

      {debounced && !isFetching && items.length === 0 && (
        <p className="py-6 text-center text-sm text-muted">{t('search.noResults', { q: debounced })}</p>
      )}

      <div className="flex flex-wrap items-center justify-between gap-2 border-t border-border pt-3 text-xs text-subtle">
        <span>{t('search.quickHint')}</span>
        {q.trim() && (
          <button
            type="button"
            onClick={() => go(`/search?q=${encodeURIComponent(q.trim())}`)}
            className={cn(
              'inline-flex items-center gap-1 rounded-lg px-2 py-1 font-medium text-accent-text hover:bg-accent-soft',
              active === items.length && 'bg-accent-soft',
            )}
          >
            {t('search.seeAll')}
            <CornerDownLeft className="size-3.5 rtl:-scale-x-100" aria-hidden />
          </button>
        )}
      </div>
    </div>
  )
}

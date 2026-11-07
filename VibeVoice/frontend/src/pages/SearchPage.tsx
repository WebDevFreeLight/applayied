import { useInfiniteQuery } from '@tanstack/react-query'
import { ScanText, Search } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { Link, useSearchParams } from 'react-router'
import { qk } from '@/api/keys'
import { searchApi } from '@/api/misc'
import type { SearchKind, SearchResultDto, SearchType } from '@/api/types'
import { Button } from '@/components/ui/Button'
import { Badge, Chip, EmptyState, ErrorState, PageHeader, Skeleton } from '@/components/ui/Display'
import { Input } from '@/components/ui/Field'
import { useDebouncedValue } from '@/hooks/misc'
import { useDates } from '@/lib/dates'
import { highlight } from '@/lib/highlight'
import { KIND_ICON, searchResultLink } from '@/lib/links'
import { ResultThumb } from '@/components/ResultThumb'

const TYPES: SearchType[] = ['notes', 'memories', 'audio', 'images']
const KIND_ORDER: SearchKind[] = ['note', 'memory', 'audio', 'image']
const TYPE_OF: Record<SearchKind, SearchType> = { note: 'notes', memory: 'memories', audio: 'audio', image: 'images' }
const KIND_OF: Record<SearchType, SearchKind> = { notes: 'note', memories: 'memory', audio: 'audio', images: 'image' }

export function SearchPage() {
  const { t } = useTranslation()
  const dates = useDates()
  const [params, setParams] = useSearchParams()
  const q = params.get('q') ?? ''
  const typesParam = params.get('types')
  const types = typesParam ? (typesParam.split(',').filter((x) => (TYPES as string[]).includes(x)) as SearchType[]) : []
  const debounced = useDebouncedValue(q.trim(), 300)

  const setQ = (value: string) => {
    const next = new URLSearchParams(params)
    if (value) next.set('q', value)
    else next.delete('q')
    setParams(next, { replace: true })
  }
  const toggleType = (type: SearchType) => {
    const nextTypes = types.includes(type) ? types.filter((x) => x !== type) : [...types, type]
    const next = new URLSearchParams(params)
    if (nextTypes.length && nextTypes.length < TYPES.length) next.set('types', nextTypes.join(','))
    else next.delete('types')
    setParams(next, { replace: true })
  }

  const query = { q: debounced, types: types.length ? types : undefined, pageSize: 20 }
  const results = useInfiniteQuery({
    queryKey: qk.search.query(query),
    queryFn: ({ pageParam, signal }) => searchApi.search({ ...query, page: pageParam }, signal),
    initialPageParam: 1,
    getNextPageParam: (last) => (last.page * last.pageSize < last.total ? last.page + 1 : undefined),
    enabled: debounced.length > 0,
  })
  const items = results.data?.pages.flatMap((p) => p.items) ?? []
  const total = results.data?.pages[0]?.total ?? 0
  const grouped = KIND_ORDER.map((kind) => ({ kind, items: items.filter((r) => r.kind === kind) })).filter((g) => g.items.length)

  return (
    <div>
      <PageHeader art="search" title={t('search.title')} subtitle={t('search.subtitle')} />
      <div className="mb-6 flex flex-col gap-3">
        <div className="relative">
          <Search className="pointer-events-none absolute start-3.5 top-1/2 size-5 -translate-y-1/2 text-subtle" aria-hidden />
          <Input
            type="search"
            autoFocus
            value={q}
            onChange={(e) => setQ(e.target.value)}
            placeholder={t('search.placeholder')}
            aria-label={t('search.label')}
            className="h-12 ps-11 text-base"
          />
        </div>
        <div className="flex flex-wrap items-center gap-2" role="group" aria-label={t('common.filters')}>
          {TYPES.map((type) => (
            <Chip key={type} active={types.includes(type)} onClick={() => toggleType(type)} icon={KIND_ICON[KIND_OF[type]]}>
              {t(`search.types.${type}`)}
            </Chip>
          ))}
          {debounced && results.data && (
            <span className="ms-auto text-sm text-muted" role="status">
              {t('search.results', { count: total })}
            </span>
          )}
        </div>
      </div>

      {!debounced ? (
        <EmptyState art="search" title={t('search.start')} body={t('search.startBody')} />
      ) : results.isLoading ? (
        <div className="flex flex-col gap-3">
          {Array.from({ length: 5 }, (_, i) => (
            <Skeleton key={i} className="h-20" />
          ))}
        </div>
      ) : results.error ? (
        <ErrorState error={results.error} onRetry={() => void results.refetch()} />
      ) : items.length === 0 ? (
        <EmptyState art="search" title={t('search.noResults', { q: debounced })} />
      ) : (
        <div className="flex flex-col gap-8">
          {grouped.map((g) => (
            <section key={g.kind} aria-labelledby={`search-${g.kind}`}>
              <h2 id={`search-${g.kind}`} className="mb-3 text-xs font-semibold tracking-wider text-subtle uppercase">
                {t(`search.types.${TYPE_OF[g.kind]}`)} ({g.items.length})
              </h2>
              <ul className="flex flex-col gap-2">
                {g.items.map((r) => (
                  <ResultRow key={`${r.kind}-${r.id}`} r={r} q={debounced} date={dates.format(r.date, 'PP')} />
                ))}
              </ul>
            </section>
          ))}
          {results.hasNextPage && (
            <div className="flex justify-center">
              <Button onClick={() => void results.fetchNextPage()} loading={results.isFetchingNextPage}>
                {t('common.loadMore')}
              </Button>
            </div>
          )}
        </div>
      )}
    </div>
  )
}

function ResultRow({ r, q, date }: { r: SearchResultDto; q: string; date: string }) {
  const { t } = useTranslation()
  return (
    <li>
      <Link
        to={searchResultLink(r)}
        className="flex items-start gap-3 rounded-2xl border border-border bg-surface p-4 shadow-card transition hover:border-border-strong"
      >
        <ResultThumb r={r} />
        <span className="min-w-0 flex-1">
          <span className="flex flex-wrap items-center gap-2">
            <span className="truncate font-medium text-fg" dir="auto">
              {highlight(r.title || t('common.untitled'), q)}
            </span>
            <Badge tone="accent">{t(`search.kinds.${r.kind}`)}</Badge>
            {r.matchedIn === 'imageText' ? (
              <Badge tone="warning">
                <ScanText className="size-3" aria-hidden />
                {t('ocr.textInImage')}
              </Badge>
            ) : (
              <Badge>{t('search.matchedIn', { field: t(`search.fields.${r.matchedIn}`) })}</Badge>
            )}
          </span>
          {r.snippet && (
            <span className="mt-1 line-clamp-2 block text-sm text-muted" dir="auto">
              {highlight(r.snippet, q)}
            </span>
          )}
          <span className="mt-1 block text-xs text-subtle">{date}</span>
        </span>
      </Link>
    </li>
  )
}

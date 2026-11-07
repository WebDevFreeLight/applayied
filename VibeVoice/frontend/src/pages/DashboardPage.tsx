import { useQuery } from '@tanstack/react-query'
import {
  Archive,
  BookHeart,
  CalendarHeart,
  History,
  NotebookPen,
  Plus,
  type LucideIcon,
} from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { Link } from 'react-router'
import { useCreateMemory, useCreateNote } from '@/api/hooks'
import { qk } from '@/api/keys'
import { dashboardApi } from '@/api/misc'
import type { DashboardStats } from '@/api/types'
import { useAuth } from '@/auth/context'
import { ArtTile, type ArtName } from '@/components/art/ArtTile'
import { CardSkeletonGrid, MemoryCard, NoteCard } from '@/components/Cards'
import { VoiceStatusChip } from '@/components/layout/VoiceStatusChip'
import { Button } from '@/components/ui/Button'
import { Card, ErrorState, Skeleton } from '@/components/ui/Display'
import { useDates } from '@/lib/dates'
import { activityLink, KIND_ICON } from '@/lib/links'
import { parseDateOnly } from '@/lib/utils'

const STAT_META: { key: keyof DashboardStats; art: ArtName; to: string }[] = [
  { key: 'notes', art: 'notes', to: '/notes' },
  { key: 'memories', art: 'memories', to: '/memories' },
  { key: 'audioClips', art: 'audio', to: '/audio' },
  { key: 'images', art: 'images', to: '/images' },
  { key: 'aiVoiceClips', art: 'ai', to: '/studio' },
  { key: 'transcripts', art: 'transcript', to: '/audio' },
  { key: 'pinned', art: 'pin', to: '/notes' },
  { key: 'trash', art: 'trash', to: '/trash' },
]

/** Module shortcut cards (illustrated). */
const SHORTCUTS: { to: string; art: ArtName; title: string; body: string }[] = [
  { to: '/notes', art: 'notes', title: 'nav.notes', body: 'notes.subtitle' },
  { to: '/memories', art: 'memories', title: 'nav.memories', body: 'memories.subtitle' },
  { to: '/audio', art: 'audio', title: 'nav.audio', body: 'audio.subtitle' },
  { to: '/studio', art: 'studio', title: 'nav.studio', body: 'studio.subtitle' },
  { to: '/images', art: 'ocr', title: 'nav.images', body: 'images.subtitle' },
  { to: '/search', art: 'search', title: 'nav.search', body: 'search.subtitle' },
]

function SectionTitle({ icon: Icon, children, to }: { icon: LucideIcon; children: string; to?: string }) {
  const { t } = useTranslation()
  return (
    <div className="mb-3 flex items-center justify-between gap-2">
      <h2 className="flex items-center gap-2 text-base font-semibold text-fg">
        <Icon className="size-4 text-accent-text" aria-hidden />
        {children}
      </h2>
      {to && (
        <Link to={to} className="text-sm font-medium text-accent-text hover:underline">
          {t('dashboard.viewAll')}
        </Link>
      )}
    </div>
  )
}

export function DashboardPage() {
  const { t } = useTranslation()
  const { user } = useAuth()
  const dates = useDates()
  const createNote = useCreateNote()
  const createMemory = useCreateMemory()
  const { data, isLoading, error, refetch } = useQuery({ queryKey: qk.dashboard, queryFn: dashboardApi.get })
  const thisYear = new Date().getFullYear()

  return (
    <div className="flex flex-col gap-8">
      <div className="relative flex flex-wrap items-end justify-between gap-4 overflow-hidden rounded-3xl border border-border bg-brand-soft p-5 sm:p-6">
        <div>
          <h1 className="text-fluid-title font-semibold tracking-tight text-fg" dir="auto">
            {t('dashboard.greeting', { name: user?.displayName ?? '' })}
          </h1>
          <p className="mt-1 text-sm text-muted">{t('dashboard.subtitle')}</p>
        </div>
        <div className="flex flex-wrap items-center gap-2">
          <VoiceStatusChip />
          <Button
            onClick={() => createMemory.mutate()}
            loading={createMemory.isPending}
            icon={<BookHeart className="size-4" aria-hidden />}
          >
            {t('dashboard.newMemory')}
          </Button>
          <Button
            variant="primary"
            onClick={() => createNote.mutate(undefined)}
            loading={createNote.isPending}
            icon={<Plus className="size-4" aria-hidden />}
          >
            {t('dashboard.newNote')}
          </Button>
        </div>
      </div>

      {error ? (
        <ErrorState error={error} onRetry={() => void refetch()} />
      ) : (
        <>
          <section aria-label={t('dashboard.title')}>
            <ul className="grid grid-cols-2 gap-3 md:grid-cols-4">
              {STAT_META.map(({ key, art, to }) => (
                <li key={key} className="min-w-0">
                  <Link
                    to={to}
                    className="flex items-center gap-3 rounded-2xl border border-border bg-surface p-3 shadow-card transition hover:border-border-strong sm:p-4"
                  >
                    <ArtTile name={art} size="md" />
                    <span className="min-w-0">
                      {isLoading || !data ? (
                        <Skeleton className="h-6 w-10" />
                      ) : (
                        <span className="block text-xl font-semibold tabular-nums text-fg">{data.stats[key]}</span>
                      )}
                      <span className="block truncate text-xs text-muted">{t(`dashboard.stats.${key}`)}</span>
                    </span>
                  </Link>
                </li>
              ))}
            </ul>
          </section>

          <section aria-labelledby="shortcuts-title">
            <h2 id="shortcuts-title" className="mb-3 text-base font-semibold text-fg">
              {t('dashboard.shortcuts')}
            </h2>
            <ul className="grid grid-cols-2 gap-3 sm:grid-cols-3 xl:grid-cols-6">
              {SHORTCUTS.map((s) => (
                <li key={s.to} className="min-w-0">
                  <Link
                    to={s.to}
                    className="group flex h-full flex-col items-start gap-3 rounded-2xl border border-border bg-surface p-4 shadow-card transition hover:-translate-y-0.5 hover:border-border-strong hover:shadow-pop"
                  >
                    <ArtTile name={s.art} size="lg" />
                    <span className="min-w-0">
                      <span className="block text-sm font-semibold text-fg">{t(s.title)}</span>
                      <span className="line-clamp-2 text-xs text-muted">{t(s.body)}</span>
                    </span>
                  </Link>
                </li>
              ))}
            </ul>
          </section>

          <section>
            <SectionTitle icon={CalendarHeart}>{t('dashboard.onThisDay')}</SectionTitle>
            {isLoading ? (
              <CardSkeletonGrid count={3} />
            ) : data?.onThisDay.length ? (
              <ul className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
                {data.onThisDay.map((m) => {
                  const years = thisYear - parseDateOnly(m.memoryDate).getFullYear()
                  return (
                    <li key={m.id} className="flex flex-col gap-1.5">
                      <span className="text-xs font-medium text-accent-text">{t('dashboard.yearsAgo', { count: years })}</span>
                      <MemoryCard memory={m} compact />
                    </li>
                  )
                })}
              </ul>
            ) : (
              <Card className="flex items-center gap-3 p-4 text-sm text-muted">
                <CalendarHeart className="size-5 text-subtle" aria-hidden />
                {t('dashboard.onThisDayEmpty')}
              </Card>
            )}
          </section>

          <div className="grid gap-8 lg:grid-cols-3">
            <div className="flex flex-col gap-8 lg:col-span-2">
              <section>
                <SectionTitle icon={NotebookPen} to="/notes">
                  {t('dashboard.recentNotes')}
                </SectionTitle>
                {isLoading ? (
                  <CardSkeletonGrid count={4} className="lg:grid-cols-2" />
                ) : data?.recentNotes.length ? (
                  <ul className="grid gap-4 sm:grid-cols-2">
                    {data.recentNotes.slice(0, 4).map((n) => (
                      <li key={n.id}>
                        <NoteCard note={n} />
                      </li>
                    ))}
                  </ul>
                ) : (
                  <Card className="p-4 text-sm text-muted">{t('notes.emptyBody')}</Card>
                )}
              </section>
              <section>
                <SectionTitle icon={BookHeart} to="/memories">
                  {t('dashboard.recentMemories')}
                </SectionTitle>
                {isLoading ? (
                  <CardSkeletonGrid count={2} className="lg:grid-cols-2" />
                ) : data?.recentMemories.length ? (
                  <ul className="grid gap-4 sm:grid-cols-2">
                    {data.recentMemories.slice(0, 4).map((m) => (
                      <li key={m.id}>
                        <MemoryCard memory={m} />
                      </li>
                    ))}
                  </ul>
                ) : (
                  <Card className="p-4 text-sm text-muted">{t('memories.emptyBody')}</Card>
                )}
              </section>
            </div>

            <section>
              <SectionTitle icon={History}>{t('dashboard.activity')}</SectionTitle>
              <Card className="p-2">
                {isLoading ? (
                  <div className="flex flex-col gap-2 p-2">
                    {Array.from({ length: 6 }, (_, i) => (
                      <Skeleton key={i} className="h-10" />
                    ))}
                  </div>
                ) : data?.activity.length ? (
                  <ol className="flex flex-col">
                    {data.activity.slice(0, 12).map((a, i) => {
                      const Icon = KIND_ICON[a.kind] ?? Archive
                      return (
                        <li key={`${a.kind}-${a.id}-${i}`}>
                          <Link
                            to={activityLink(a)}
                            className="flex items-start gap-3 rounded-xl px-2.5 py-2 hover:bg-surface-2"
                          >
                            <span className="mt-0.5 flex size-7 shrink-0 items-center justify-center rounded-lg bg-surface-2 text-muted">
                              <Icon className="size-3.5" aria-hidden />
                            </span>
                            <span className="min-w-0 flex-1">
                              <span className="block truncate text-sm text-fg" dir="auto">
                                {a.title || t('common.untitled')}
                              </span>
                              <span className="block text-xs text-subtle">
                                {t(`dashboard.activityKind.${a.kind}`)} · {t(`dashboard.activityAction.${a.action}`)} ·{' '}
                                {dates.relative(a.at)}
                              </span>
                            </span>
                          </Link>
                        </li>
                      )
                    })}
                  </ol>
                ) : (
                  <p className="p-4 text-sm text-muted">{t('dashboard.activityEmpty')}</p>
                )}
              </Card>
            </section>
          </div>
        </>
      )}
    </div>
  )
}

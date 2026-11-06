import { useQuery } from '@tanstack/react-query'
import {
  Archive,
  AudioLines,
  BookHeart,
  CalendarHeart,
  FileText,
  History,
  Images,
  NotebookPen,
  Pin,
  Plus,
  Sparkles,
  Trash2,
  type LucideIcon,
} from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { Link } from 'react-router'
import { useCreateMemory, useCreateNote } from '@/api/hooks'
import { qk } from '@/api/keys'
import { dashboardApi } from '@/api/misc'
import type { DashboardStats } from '@/api/types'
import { useAuth } from '@/auth/context'
import { CardSkeletonGrid, MemoryCard, NoteCard } from '@/components/Cards'
import { VoiceStatusChip } from '@/components/layout/VoiceStatusChip'
import { Button } from '@/components/ui/Button'
import { Card, ErrorState, Skeleton } from '@/components/ui/Display'
import { useDates } from '@/lib/dates'
import { activityLink, KIND_ICON } from '@/lib/links'
import { parseDateOnly } from '@/lib/utils'

const STAT_META: { key: keyof DashboardStats; icon: LucideIcon; to: string }[] = [
  { key: 'notes', icon: NotebookPen, to: '/notes' },
  { key: 'memories', icon: BookHeart, to: '/memories' },
  { key: 'audioClips', icon: AudioLines, to: '/audio' },
  { key: 'images', icon: Images, to: '/images' },
  { key: 'aiVoiceClips', icon: Sparkles, to: '/studio' },
  { key: 'transcripts', icon: FileText, to: '/audio' },
  { key: 'pinned', icon: Pin, to: '/notes' },
  { key: 'trash', icon: Trash2, to: '/trash' },
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
      <div className="flex flex-wrap items-end justify-between gap-4">
        <div>
          <h1 className="text-2xl font-semibold tracking-tight text-fg sm:text-3xl" dir="auto">
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
            <ul className="grid grid-cols-2 gap-3 sm:grid-cols-4">
              {STAT_META.map(({ key, icon: Icon, to }) => (
                <li key={key}>
                  <Link
                    to={to}
                    className="flex items-center gap-3 rounded-2xl border border-border bg-surface p-4 shadow-card transition hover:border-border-strong"
                  >
                    <span
                      className={
                        key === 'aiVoiceClips'
                          ? 'flex size-10 shrink-0 items-center justify-center rounded-xl bg-ai-soft text-ai'
                          : key === 'trash'
                            ? 'flex size-10 shrink-0 items-center justify-center rounded-xl bg-surface-2 text-muted'
                            : 'flex size-10 shrink-0 items-center justify-center rounded-xl bg-accent-soft text-accent-text'
                      }
                    >
                      <Icon className="size-5" aria-hidden />
                    </span>
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

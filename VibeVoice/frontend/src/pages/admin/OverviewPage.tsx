import { useQuery } from '@tanstack/react-query'
import { ArrowRight } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { Link } from 'react-router'
import { adminApi } from '@/api/auth'
import { qk } from '@/api/keys'
import type { AdminOverviewDto } from '@/api/types'
import { ArtTile, type ArtName } from '@/components/art/ArtTile'
import { Badge, Card, ErrorState, PageHeader, Skeleton } from '@/components/ui/Display'
import { roleLabel } from '@/lib/admin'
import { useDates } from '@/lib/dates'
import { cn, formatBytes } from '@/lib/utils'

type NumericKey = Exclude<keyof AdminOverviewDto, 'voice' | 'recentUsers'>

const STATS: { key: NumericKey; label: string; art: ArtName; to?: string; bytes?: boolean }[] = [
  { key: 'users', label: 'users', art: 'users', to: '/users' },
  { key: 'activeUsers', label: 'activeUsers', art: 'users', to: '/users?active=true' },
  { key: 'admins', label: 'admins', art: 'roles', to: '/users' },
  { key: 'roles', label: 'roles', art: 'roles', to: '/roles' },
  { key: 'notes', label: 'notes', art: 'notes' },
  { key: 'memories', label: 'memories', art: 'memories' },
  { key: 'audioClips', label: 'audioClips', art: 'audio' },
  { key: 'images', label: 'images', art: 'images' },
  { key: 'storageBytes', label: 'storage', art: 'storage', bytes: true },
]

export function OverviewPage() {
  const { t } = useTranslation()
  const dates = useDates()
  const { data, isLoading, error, refetch } = useQuery({ queryKey: qk.adminOverview, queryFn: adminApi.overview })
  const voice = data?.voice

  return (
    <div className="flex flex-col gap-8">
      <PageHeader art="overview" title={t('admin.title')} subtitle={t('admin.subtitle')} />
      {error ? (
        <ErrorState error={error} onRetry={() => void refetch()} />
      ) : (
        <>
          <section aria-label={t('admin.title')}>
            <ul className="grid grid-cols-2 gap-3 md:grid-cols-3 xl:grid-cols-5">
              {STATS.map(({ key, label, art, to, bytes }) => {
                const value = data ? (bytes ? formatBytes(data[key]) : String(data[key])) : null
                const body = (
                  <>
                    <ArtTile name={art} size="md" />
                    <span className="min-w-0">
                      {isLoading || value === null ? (
                        <Skeleton className="h-6 w-12" />
                      ) : (
                        <span className="block text-xl font-semibold tabular-nums text-fg" dir="ltr" data-testid={`stat-${key}`}>
                          {value}
                        </span>
                      )}
                      <span className="block truncate text-xs text-muted">{t(`admin.stats.${label}`)}</span>
                    </span>
                  </>
                )
                const cls = 'flex h-full items-center gap-3 rounded-2xl border border-border bg-surface p-3 shadow-card transition sm:p-4'
                return (
                  <li key={key} className="min-w-0">
                    {to ? (
                      <Link to={to} className={cn(cls, 'hover:border-border-strong')}>
                        {body}
                      </Link>
                    ) : (
                      <div className={cls}>{body}</div>
                    )}
                  </li>
                )
              })}
            </ul>
          </section>

          <div className="grid gap-6 lg:grid-cols-3">
            <Card className="flex flex-col gap-3 p-5">
              <h2 className="flex items-center gap-2 text-base font-semibold text-fg">
                <ArtTile name="studio" size="sm" />
                {t('admin.voiceService')}
              </h2>
              {isLoading || !voice ? (
                <Skeleton className="h-16" />
              ) : (
                <>
                  <Badge tone={voice.available ? 'success' : 'danger'} className="self-start">
                    {voice.available ? t('voice.statusReady') : t('voice.statusOffline')}
                  </Badge>
                  <p className="text-sm text-muted" dir="ltr">
                    {t('admin.engines', {
                      tts: voice.ttsEngine ?? '—',
                      asr: voice.asrEngine ?? '—',
                      ocr: voice.ocrEngine ?? '—',
                      device: voice.device ?? '—',
                    })}
                  </p>
                  {voice.message && <p className="text-sm text-danger">{voice.message}</p>}
                </>
              )}
            </Card>

            <Card className="flex flex-col gap-3 p-5 lg:col-span-2">
              <div className="flex flex-wrap items-center justify-between gap-2">
                <h2 className="text-base font-semibold text-fg">{t('admin.recentUsers')}</h2>
                <div className="flex flex-wrap gap-3 text-sm font-medium">
                  <Link to="/users" className="inline-flex min-h-10 items-center gap-1 text-accent-text hover:underline">
                    {t('admin.manageUsers')}
                    <ArrowRight className="size-4 rtl:rotate-180" aria-hidden />
                  </Link>
                  <Link to="/roles" className="inline-flex min-h-10 items-center gap-1 text-accent-text hover:underline">
                    {t('admin.manageRoles')}
                    <ArrowRight className="size-4 rtl:rotate-180" aria-hidden />
                  </Link>
                </div>
              </div>
              {isLoading || !data ? (
                <div className="flex flex-col gap-2">
                  {Array.from({ length: 4 }, (_, i) => (
                    <Skeleton key={i} className="h-12" />
                  ))}
                </div>
              ) : (
                <ul className="flex flex-col divide-y divide-border">
                  {data.recentUsers.map((u) => (
                    <li key={u.id} className="flex flex-wrap items-center gap-3 py-2.5">
                      <span
                        aria-hidden
                        className="flex size-9 shrink-0 items-center justify-center rounded-full bg-brand text-sm font-semibold text-white uppercase"
                      >
                        {(u.displayName || u.email).charAt(0)}
                      </span>
                      <span className="min-w-0 flex-1">
                        <span className="block font-medium text-fg" dir="auto">
                          {u.displayName}
                        </span>
                        <span className="block text-xs text-muted" dir="ltr">
                          {u.email}
                        </span>
                      </span>
                      <Badge tone={u.role === 'Admin' ? 'accent' : 'neutral'}>{roleLabel(t, u.role)}</Badge>
                      {!u.isActive && <Badge tone="danger">{t('users.inactive')}</Badge>}
                      <span className="w-full text-xs text-subtle sm:w-auto">{dates.relative(u.createdAt)}</span>
                    </li>
                  ))}
                </ul>
              )}
            </Card>
          </div>
        </>
      )}
    </div>
  )
}

import { keepPreviousData, useQuery, useQueryClient } from '@tanstack/react-query'
import {
  AudioLines,
  CheckCircle2,
  Clock,
  Copy,
  ExternalLink,
  Info,
  Loader2,
  RefreshCw,
  RotateCcw,
  ScanText,
  Trash2,
  XCircle,
  type LucideIcon,
} from 'lucide-react'
import { useState, type ReactNode } from 'react'
import { useTranslation } from 'react-i18next'
import { Link } from 'react-router'
import { toast } from 'sonner'
import type { JobDto, JobKind, JobStatus } from '@/api/client'
import { jobOwnerLink, jobResultText, jobsApi, type JobListQuery, type JobScope, type JobSummary } from '@/api/jobs'
import { qk } from '@/api/keys'
import { useAuth } from '@/auth/context'
import { Button } from '@/components/ui/Button'
import { Badge, Card, EmptyState, ErrorState, PageHeader, Segmented, Skeleton, type BadgeTone } from '@/components/ui/Display'
import { Select } from '@/components/ui/Field'
import { ConfirmDialog, Modal } from '@/components/ui/Modal'
import { useDates } from '@/lib/dates'
import { toastError } from '@/lib/notify'
import { cn } from '@/lib/utils'

const PAGE_SIZE = 20
const KINDS: JobKind[] = ['ImageOcr', 'AudioTranscribe', 'StudioOcr', 'StudioTranscribe']
type StatusFilter = 'all' | 'active' | 'Succeeded' | 'Failed'

const STATUS_STYLE: Record<JobStatus, { tone: BadgeTone; icon: LucideIcon }> = {
  Queued: { tone: 'neutral', icon: Clock },
  Running: { tone: 'accent', icon: Loader2 },
  Succeeded: { tone: 'success', icon: CheckCircle2 },
  Failed: { tone: 'danger', icon: XCircle },
}

function KindIcon({ kind, className }: { kind: string; className?: string }) {
  return kind.endsWith('Ocr') ? <ScanText className={className} aria-hidden /> : <AudioLines className={className} aria-hidden />
}

function useJobLabels() {
  const { t } = useTranslation()
  return {
    kind: (kind: string) => t(`jobs.kind.${kind}`, { defaultValue: kind }),
    status: (status: string) => t(`jobs.status.${status}`, { defaultValue: status }),
    duration: (seconds: number | null | undefined) =>
      seconds == null ? '—' : seconds < 60 ? t('jobs.seconds', { n: seconds.toFixed(1) }) : t('jobs.minutes', { n: (seconds / 60).toFixed(1) }),
  }
}

export function StatusBadge({ status }: { status: JobStatus }) {
  const labels = useJobLabels()
  const style = STATUS_STYLE[status] ?? STATUS_STYLE.Queued
  return (
    <Badge tone={style.tone}>
      <style.icon className={cn('size-3.5', status === 'Running' && 'animate-spin')} aria-hidden />
      {labels.status(status)}
    </Badge>
  )
}

export function JobsPage() {
  const { t } = useTranslation()
  const { can } = useAuth()
  const qc = useQueryClient()
  const isAdmin = can('users.view')
  const canManageAll = can('users.manage')

  const [scope, setScope] = useState<JobScope>('mine')
  const [status, setStatus] = useState<StatusFilter>('all')
  const [kind, setKind] = useState<JobKind | ''>('')
  const [page, setPage] = useState(1)
  const [selected, setSelected] = useState<JobDto | null>(null)
  const [pendingDelete, setPendingDelete] = useState<JobDto | null>(null)
  const [confirmClear, setConfirmClear] = useState(false)
  const [busy, setBusy] = useState<string | null>(null)

  const query: JobListQuery = {
    status: status === 'all' ? undefined : status,
    kind: kind || undefined,
    page,
    pageSize: PAGE_SIZE,
  }

  const summary = useQuery({
    queryKey: qk.jobs.summary(scope),
    queryFn: ({ signal }) => jobsApi.summary(scope, signal),
    refetchInterval: (q) => (busyCount(q.state.data) > 0 ? 3000 : 15000),
  })
  const list = useQuery({
    queryKey: qk.jobs.list(scope, query),
    queryFn: ({ signal }) => jobsApi.list(scope, query, signal),
    placeholderData: keepPreviousData,
    refetchInterval: (q) =>
      q.state.data?.items.some((j) => j.status === 'Queued' || j.status === 'Running') || busyCount(summary.data) > 0
        ? 3000
        : 15000,
  })

  const refresh = () => void qc.invalidateQueries({ queryKey: qk.jobs.all })
  const changeFilter = (fn: () => void) => {
    fn()
    setPage(1)
  }

  const retry = async (job: JobDto) => {
    setBusy(job.id)
    try {
      const updated = await jobsApi.retry(scope, job.id)
      toast.success(t('jobs.retried'))
      if (selected?.id === job.id) setSelected(updated)
      refresh()
    } catch (err) {
      toastError(err)
    } finally {
      setBusy(null)
    }
  }

  const remove = async () => {
    if (!pendingDelete) return
    setBusy(pendingDelete.id)
    try {
      await jobsApi.remove(scope, pendingDelete.id)
      toast.success(t('jobs.deleted'))
      if (selected?.id === pendingDelete.id) setSelected(null)
      setPendingDelete(null)
      refresh()
    } catch (err) {
      toastError(err)
    } finally {
      setBusy(null)
    }
  }

  const clearFinished = async () => {
    setBusy('clear')
    try {
      const { deleted } = await jobsApi.clearFinished()
      toast.success(t('jobs.cleared', { count: deleted }))
      setConfirmClear(false)
      refresh()
    } catch (err) {
      toastError(err)
    } finally {
      setBusy(null)
    }
  }

  const items = list.data?.items ?? []
  const total = list.data?.total ?? 0
  const pages = Math.max(1, Math.ceil(total / PAGE_SIZE))
  const canMutate = scope === 'mine' || canManageAll

  return (
    <div>
      <PageHeader
        art="ai"
        title={t('jobs.title')}
        subtitle={t('jobs.subtitle')}
        actions={
          <div className="flex flex-wrap gap-2">
            <Button
              variant="outline"
              size="sm"
              onClick={refresh}
              icon={<RefreshCw className={cn('size-4', list.isFetching && 'animate-spin')} aria-hidden />}
            >
              {t('jobs.refresh')}
            </Button>
            {scope === 'mine' && (
              <Button
                variant="ghost"
                size="sm"
                disabled={!summary.data || summary.data.succeeded + summary.data.failed === 0}
                onClick={() => setConfirmClear(true)}
                icon={<Trash2 className="size-4" aria-hidden />}
              >
                {t('jobs.clearFinished')}
              </Button>
            )}
          </div>
        }
      />

      {isAdmin && (
        <Segmented
          className="mb-4"
          label={t('jobs.scope')}
          value={scope}
          onChange={(v) => changeFilter(() => setScope(v))}
          options={[
            { value: 'mine', label: t('jobs.scopeMine') },
            { value: 'all', label: t('jobs.scopeAll') },
          ]}
        />
      )}

      <SummaryTiles summary={summary.data} active={status} onPick={(s) => changeFilter(() => setStatus(s))} />

      <div className="mb-4 flex flex-wrap items-center gap-3">
        <Segmented
          label={t('jobs.filterStatus')}
          value={status}
          onChange={(v) => changeFilter(() => setStatus(v))}
          options={[
            { value: 'all', label: t('common.all') },
            { value: 'active', label: t('jobs.active') },
            { value: 'Succeeded', label: t('jobs.status.Succeeded') },
            { value: 'Failed', label: t('jobs.status.Failed') },
          ]}
        />
        <Select
          aria-label={t('jobs.filterKind')}
          className="w-auto min-w-48"
          value={kind}
          onChange={(e) => changeFilter(() => setKind(e.target.value as JobKind | ''))}
        >
          <option value="">{t('jobs.allKinds')}</option>
          {KINDS.map((k) => (
            <option key={k} value={k}>
              {t(`jobs.kind.${k}`)}
            </option>
          ))}
        </Select>
      </div>

      {list.isLoading ? (
        <div className="flex flex-col gap-3">
          {Array.from({ length: 4 }, (_, i) => (
            <Skeleton key={i} className="h-24" />
          ))}
        </div>
      ) : list.error ? (
        <ErrorState error={list.error} onRetry={() => void list.refetch()} />
      ) : items.length === 0 ? (
        <EmptyState art="studio" title={t('jobs.empty')} body={t('jobs.emptyBody')} />
      ) : (
        <>
          <ul className="flex flex-col gap-2" aria-label={t('jobs.title')}>
            {items.map((job) => (
              <li key={job.id}>
                <JobRow
                  job={job}
                  showUser={scope === 'all'}
                  busy={busy === job.id}
                  canMutate={canMutate}
                  onDetails={() => setSelected(job)}
                  onRetry={() => void retry(job)}
                  onDelete={() => setPendingDelete(job)}
                />
              </li>
            ))}
          </ul>
          {pages > 1 && (
            <nav className="mt-5 flex items-center justify-between gap-3" aria-label={t('jobs.pageOf', { page, pages })}>
              <Button disabled={page <= 1} onClick={() => setPage(page - 1)}>
                {t('jobs.previous')}
              </Button>
              <span className="text-sm text-muted tabular-nums">{t('jobs.pageOf', { page, pages })}</span>
              <Button disabled={page >= pages} onClick={() => setPage(page + 1)}>
                {t('jobs.next')}
              </Button>
            </nav>
          )}
        </>
      )}

      <JobDetails
        scope={scope}
        job={selected}
        canMutate={canMutate}
        busy={!!selected && busy === selected.id}
        onClose={() => setSelected(null)}
        onRetry={(job) => void retry(job)}
        onDelete={(job) => setPendingDelete(job)}
      />

      <ConfirmDialog
        open={!!pendingDelete}
        title={t('jobs.deleteTitle')}
        body={t('jobs.deleteBody')}
        confirmLabel={t('common.delete')}
        loading={!!pendingDelete && busy === pendingDelete.id}
        onConfirm={() => void remove()}
        onCancel={() => setPendingDelete(null)}
      />
      <ConfirmDialog
        open={confirmClear}
        title={t('jobs.clearTitle')}
        body={t('jobs.clearBody')}
        confirmLabel={t('jobs.clearFinished')}
        loading={busy === 'clear'}
        onConfirm={() => void clearFinished()}
        onCancel={() => setConfirmClear(false)}
      />
    </div>
  )
}

function busyCount(s: JobSummary | undefined): number {
  return s ? s.queued + s.running : 0
}

function SummaryTiles({
  summary,
  active,
  onPick,
}: {
  summary: JobSummary | undefined
  active: StatusFilter
  onPick: (s: StatusFilter) => void
}) {
  const { t } = useTranslation()
  const tiles: { key: StatusFilter; label: string; value: number | undefined; tone: string }[] = [
    { key: 'all', label: t('jobs.total'), value: summary?.total, tone: 'text-fg' },
    { key: 'active', label: t('jobs.active'), value: summary ? summary.queued + summary.running : undefined, tone: 'text-accent-text' },
    { key: 'Succeeded', label: t('jobs.status.Succeeded'), value: summary?.succeeded, tone: 'text-success' },
    { key: 'Failed', label: t('jobs.status.Failed'), value: summary?.failed, tone: 'text-danger' },
  ]
  return (
    <div className="mb-5 grid grid-cols-2 gap-2 sm:grid-cols-4">
      {tiles.map((tile) => (
        <button
          key={tile.key}
          type="button"
          onClick={() => onPick(tile.key)}
          aria-pressed={active === tile.key}
          className={cn(
            'rounded-2xl border border-border bg-surface p-3 text-start transition-colors hover:border-accent',
            active === tile.key && 'border-accent ring-1 ring-accent',
          )}
        >
          <span className="block text-xs text-muted">{tile.label}</span>
          <span className={cn('text-2xl font-semibold tabular-nums', tile.tone)}>{tile.value ?? '–'}</span>
        </button>
      ))}
    </div>
  )
}

function JobRow({
  job,
  showUser,
  busy,
  canMutate,
  onDetails,
  onRetry,
  onDelete,
}: {
  job: JobDto
  showUser: boolean
  busy: boolean
  canMutate: boolean
  onDetails: () => void
  onRetry: () => void
  onDelete: () => void
}) {
  const { t } = useTranslation()
  const dates = useDates()
  const labels = useJobLabels()
  const text = jobResultText(job)
  const link = jobOwnerLink(job)

  return (
    <Card className="flex flex-col gap-3 p-4 sm:flex-row sm:items-start">
      <div className="flex min-w-0 flex-1 gap-3">
        <span className="mt-0.5 inline-flex size-9 shrink-0 items-center justify-center rounded-xl bg-surface-2 text-accent-text">
          <KindIcon kind={job.kind} className="size-5" />
        </span>
        <div className="min-w-0 flex-1">
          <div className="flex flex-wrap items-center gap-2">
            <p className="font-medium text-fg">{labels.kind(job.kind)}</p>
            <StatusBadge status={job.status} />
            {job.attempts > 1 && <Badge tone="warning">{t('jobs.attemptsN', { n: job.attempts })}</Badge>}
          </div>
          <p className="mt-0.5 flex flex-wrap gap-x-3 gap-y-0.5 text-xs text-subtle">
            <span title={dates.format(job.createdAt, 'PPpp')}>{dates.relative(job.createdAt)}</span>
            {job.durationSeconds != null && <span>{t('jobs.took', { time: labels.duration(job.durationSeconds) })}</span>}
            {job.payload?.fileName && (
              <span className="truncate" dir="auto">
                {job.payload.fileName}
              </span>
            )}
            {job.payload?.language && <span>{t('jobs.languageShort', { lang: job.payload.language })}</span>}
            {showUser && job.userEmail && <span className="text-muted">{job.userEmail}</span>}
          </p>
          {job.status === 'Failed' && (
            <p className="mt-1.5 text-sm text-danger" dir="auto">
              <span className="font-medium">
                {job.errorTitle ?? t('jobs.failed')}
                {job.errorStatus ? ` (${job.errorStatus})` : ''}:
              </span>{' '}
              {job.error}
            </p>
          )}
          {job.status !== 'Failed' && job.error && (
            <p className="mt-1.5 text-sm text-warning" dir="auto">
              {t('jobs.waitingRetry', { error: job.error })}
            </p>
          )}
          {text && (
            <p className="mt-1.5 line-clamp-2 text-sm whitespace-pre-line text-muted" dir="auto">
              {text}
            </p>
          )}
        </div>
      </div>
      <div className="flex shrink-0 flex-wrap gap-2">
        <Button size="sm" variant="outline" onClick={onDetails} icon={<Info className="size-4" aria-hidden />}>
          {t('jobs.details')}
        </Button>
        {link && (
          <Link
            to={link}
            className="inline-flex min-h-9 items-center gap-1.5 rounded-lg px-3 text-sm font-medium text-accent-text hover:bg-surface-2"
          >
            <ExternalLink className="size-4" aria-hidden />
            {t('jobs.open')}
          </Link>
        )}
        {canMutate && job.canRetry && (
          <Button size="sm" loading={busy} onClick={onRetry} icon={<RotateCcw className="size-4" aria-hidden />}>
            {t('jobs.retry')}
          </Button>
        )}
        {canMutate && job.canDelete && (
          <Button
            size="sm"
            variant="ghost"
            className="text-danger hover:bg-danger-soft hover:text-danger"
            disabled={busy}
            onClick={onDelete}
            icon={<Trash2 className="size-4" aria-hidden />}
          >
            {t('common.delete')}
          </Button>
        )}
      </div>
    </Card>
  )
}

function JobDetails({
  scope,
  job,
  canMutate,
  busy,
  onClose,
  onRetry,
  onDelete,
}: {
  scope: JobScope
  job: JobDto | null
  canMutate: boolean
  busy: boolean
  onClose: () => void
  onRetry: (job: JobDto) => void
  onDelete: (job: JobDto) => void
}) {
  const { t } = useTranslation()
  const dates = useDates()
  const labels = useJobLabels()
  // Keep the open dialog live while the job is still queued or running.
  const live = useQuery({
    queryKey: qk.jobs.detail(scope, job?.id ?? ''),
    queryFn: ({ signal }) => jobsApi.get(scope, job!.id, signal),
    enabled: !!job,
    initialData: job ?? undefined,
    refetchInterval: (q) => (q.state.data?.status === 'Queued' || q.state.data?.status === 'Running' ? 2000 : false),
  })
  const current = live.data ?? job
  if (!current) return null
  const text = jobResultText(current)
  const link = jobOwnerLink(current)
  const p = current.payload

  const copy = async (value: string) => {
    try {
      await navigator.clipboard.writeText(value)
      toast.success(t('common.copied'))
    } catch (err) {
      toastError(err)
    }
  }

  const rows: [string, ReactNode][] = [
    [t('jobs.field.id'), <code className="text-xs break-all">{current.id}</code>],
    [t('jobs.field.kind'), labels.kind(current.kind)],
    [t('jobs.field.status'), <StatusBadge status={current.status} />],
    ...(current.userEmail ? [[t('jobs.field.user'), `${current.userName ?? ''} <${current.userEmail}>`] as [string, ReactNode]] : []),
    [t('jobs.field.created'), dates.format(current.createdAt, 'PPpp')],
    [t('jobs.field.started'), current.startedAt ? dates.format(current.startedAt, 'PPpp') : '—'],
    [t('jobs.field.finished'), current.finishedAt ? dates.format(current.finishedAt, 'PPpp') : '—'],
    [t('jobs.field.duration'), labels.duration(current.durationSeconds)],
    [t('jobs.field.attempts'), String(current.attempts)],
    ...(p?.fileName ? [[t('jobs.field.file'), `${p.fileName}${p.contentType ? ` (${p.contentType})` : ''}`] as [string, ReactNode]] : []),
    ...(p?.ownerType ? [[t('jobs.field.owner'), link ? <Link className="text-accent-text underline" to={link}>{p.ownerType}</Link> : p.ownerType] as [string, ReactNode]] : []),
    [t('jobs.field.language'), p?.language || t('jobs.auto')],
    ...(p?.appendToBody ? [[t('jobs.field.append'), t('common.yes')] as [string, ReactNode]] : []),
    ...(current.kind.startsWith('Studio') ? [[t('jobs.field.save'), p?.save ? t('common.yes') : t('common.no')] as [string, ReactNode]] : []),
  ]

  return (
    <Modal
      open={!!job}
      onClose={onClose}
      size="lg"
      title={labels.kind(current.kind)}
      description={t('jobs.detailsTitle')}
      footer={
        <div className="flex flex-wrap justify-end gap-2">
          {canMutate && current.canRetry && (
            <Button loading={busy} onClick={() => onRetry(current)} icon={<RotateCcw className="size-4" aria-hidden />}>
              {t('jobs.retry')}
            </Button>
          )}
          {canMutate && current.canDelete && (
            <Button variant="ghost" className="text-danger" onClick={() => onDelete(current)} icon={<Trash2 className="size-4" aria-hidden />}>
              {t('common.delete')}
            </Button>
          )}
          <Button variant="outline" onClick={onClose}>
            {t('common.close')}
          </Button>
        </div>
      }
    >
      <dl className="grid grid-cols-[minmax(7rem,auto)_1fr] gap-x-4 gap-y-2 text-sm">
        {rows.map(([label, value]) => (
          <div key={label} className="contents">
            <dt className="text-muted">{label}</dt>
            <dd className="min-w-0 text-fg" dir="auto">
              {value}
            </dd>
          </div>
        ))}
      </dl>

      {(current.error || current.errorDetail) && (
        <section className="mt-5 rounded-xl border border-danger/30 bg-danger-soft p-3" aria-labelledby="job-error">
          <h3 id="job-error" className="mb-1 text-sm font-semibold text-danger">
            {current.status === 'Failed' ? t('jobs.errorTitle') : t('jobs.lastError')}
            {current.errorStatus ? ` · HTTP ${current.errorStatus}` : ''}
            {current.errorTitle ? ` · ${current.errorTitle}` : ''}
          </h3>
          {current.error && (
            <p className="text-sm text-fg" dir="auto">
              {current.error}
            </p>
          )}
          {current.errorDetail && (
            <pre className="mt-2 max-h-48 overflow-auto rounded-lg bg-surface p-2 text-xs whitespace-pre-wrap text-muted" dir="ltr">
              {current.errorDetail}
            </pre>
          )}
        </section>
      )}

      {text && (
        <section className="mt-5" aria-labelledby="job-text">
          <div className="mb-1 flex items-center justify-between gap-2">
            <h3 id="job-text" className="text-sm font-semibold text-fg">
              {current.kind.endsWith('Ocr') ? t('jobs.extractedText') : t('jobs.transcript')}
            </h3>
            <Button size="sm" variant="ghost" onClick={() => void copy(text)} icon={<Copy className="size-4" aria-hidden />}>
              {t('common.copy')}
            </Button>
          </div>
          <p className="max-h-60 overflow-auto rounded-xl bg-surface-2 p-3 text-sm whitespace-pre-line text-fg" dir="auto">
            {text}
          </p>
        </section>
      )}

      <details className="mt-5 text-sm">
        <summary className="cursor-pointer text-muted">{t('jobs.raw')}</summary>
        <pre className="mt-2 max-h-72 overflow-auto rounded-xl bg-surface-2 p-3 text-xs" dir="ltr">
          {JSON.stringify({ payload: current.payload, result: current.result }, null, 2)}
        </pre>
      </details>
    </Modal>
  )
}

import { AlertTriangle, Loader2, type LucideIcon } from 'lucide-react'
import type { HTMLAttributes, ReactNode } from 'react'
import { useTranslation } from 'react-i18next'
import { errorMessage } from '@/api/client'
import { cn } from '@/lib/utils'
import { Button } from './Button'

export function Card({ className, ...rest }: HTMLAttributes<HTMLDivElement>) {
  return (
    <div
      className={cn('rounded-2xl border border-border bg-surface shadow-card', className)}
      {...rest}
    />
  )
}

export type BadgeTone = 'neutral' | 'accent' | 'success' | 'warning' | 'danger' | 'ai'

const tones: Record<BadgeTone, string> = {
  neutral: 'bg-surface-2 text-muted border-border',
  accent: 'bg-accent-soft text-accent-text border-transparent',
  success: 'bg-success-soft text-success border-transparent',
  warning: 'bg-warning-soft text-warning border-transparent',
  danger: 'bg-danger-soft text-danger border-transparent',
  ai: 'bg-ai-soft text-ai border-transparent',
}

export function Badge({
  tone = 'neutral',
  className,
  children,
  ...rest
}: HTMLAttributes<HTMLSpanElement> & { tone?: BadgeTone }) {
  return (
    <span
      className={cn(
        'inline-flex max-w-full items-center gap-1 rounded-full border px-2 py-0.5 text-xs font-medium whitespace-nowrap',
        tones[tone],
        className,
      )}
      {...rest}
    >
      {children}
    </span>
  )
}

export function Skeleton({ className }: { className?: string }) {
  return <div aria-hidden className={cn('animate-pulse-soft rounded-xl bg-surface-3', className)} />
}

export function Spinner({ className, label }: { className?: string; label?: string }) {
  return (
    <span role="status" className={cn('inline-flex items-center gap-2 text-muted', className)}>
      <Loader2 className="size-5 animate-spin" aria-hidden />
      {label ? <span className="text-sm">{label}</span> : <span className="sr-only">Loading</span>}
    </span>
  )
}

export function EmptyState({
  icon: Icon,
  title,
  body,
  action,
  className,
}: {
  icon: LucideIcon
  title: ReactNode
  body?: ReactNode
  action?: ReactNode
  className?: string
}) {
  return (
    <div
      className={cn(
        'flex flex-col items-center justify-center rounded-2xl border border-dashed border-border-strong px-6 py-14 text-center',
        className,
      )}
    >
      <div className="relative mb-4">
        <div className="absolute inset-0 -z-10 scale-150 rounded-full bg-accent/10 blur-2xl" aria-hidden />
        <div className="flex size-16 items-center justify-center rounded-2xl bg-accent-soft text-accent-text shadow-sm">
          <Icon className="size-8" aria-hidden />
        </div>
      </div>
      <h3 className="text-base font-semibold text-fg">{title}</h3>
      {body && <p className="mt-1 max-w-sm text-sm text-muted">{body}</p>}
      {action && <div className="mt-5">{action}</div>}
    </div>
  )
}

export function ErrorState({
  error,
  onRetry,
  className,
}: {
  error: unknown
  onRetry?: () => void
  className?: string
}) {
  const { t } = useTranslation()
  return (
    <div
      role="alert"
      className={cn(
        'flex flex-col items-center gap-3 rounded-2xl border border-danger/30 bg-danger-soft/50 px-6 py-10 text-center',
        className,
      )}
    >
      <AlertTriangle className="size-8 text-danger" aria-hidden />
      <div>
        <p className="font-semibold text-fg">{t('common.error')}</p>
        <p className="mt-1 text-sm text-muted">{errorMessage(error, t('common.errorDetail'))}</p>
      </div>
      {onRetry && (
        <Button size="sm" onClick={onRetry}>
          {t('common.retry')}
        </Button>
      )}
    </div>
  )
}

export function PageHeader({
  title,
  subtitle,
  actions,
  icon: Icon,
}: {
  title: ReactNode
  subtitle?: ReactNode
  actions?: ReactNode
  icon?: LucideIcon
}) {
  return (
    <div className="mb-6 flex flex-wrap items-start justify-between gap-4">
      <div className="flex min-w-0 items-start gap-3">
        {Icon && (
          <div className="mt-0.5 hidden size-10 shrink-0 items-center justify-center rounded-xl bg-accent-soft text-accent-text sm:flex">
            <Icon className="size-5" aria-hidden />
          </div>
        )}
        <div className="min-w-0">
          <h1 className="text-2xl font-semibold tracking-tight text-fg">{title}</h1>
          {subtitle && <p className="mt-0.5 text-sm text-muted">{subtitle}</p>}
        </div>
      </div>
      {actions && <div className="flex flex-wrap items-center gap-2">{actions}</div>}
    </div>
  )
}

/** Segmented control (radio group semantics). */
export function Segmented<T extends string>({
  value,
  onChange,
  options,
  label,
  className,
}: {
  value: T
  onChange: (v: T) => void
  options: { value: T; label: ReactNode; icon?: LucideIcon; title?: string }[]
  label: string
  className?: string
}) {
  return (
    <div
      role="radiogroup"
      aria-label={label}
      className={cn('inline-flex rounded-xl border border-border bg-surface-2 p-0.5', className)}
    >
      {options.map((o) => {
        const active = o.value === value
        const Icon = o.icon
        return (
          <button
            key={o.value}
            type="button"
            role="radio"
            aria-checked={active}
            title={o.title}
            aria-label={o.title}
            onClick={() => onChange(o.value)}
            className={cn(
              'inline-flex h-8 items-center gap-1.5 rounded-[10px] px-3 text-sm font-medium transition-colors',
              active ? 'bg-surface text-fg shadow-sm' : 'text-muted hover:text-fg',
            )}
          >
            {Icon && <Icon className="size-4" aria-hidden />}
            {o.label}
          </button>
        )
      })}
    </div>
  )
}

/** Toggleable filter chip. */
export function Chip({
  active,
  onClick,
  children,
  icon: Icon,
}: {
  active: boolean
  onClick: () => void
  children: ReactNode
  icon?: LucideIcon
}) {
  return (
    <button
      type="button"
      aria-pressed={active}
      onClick={onClick}
      className={cn(
        'inline-flex h-8 items-center gap-1.5 rounded-full border px-3 text-sm font-medium transition-colors',
        active
          ? 'border-accent bg-accent-soft text-accent-text'
          : 'border-border bg-surface text-muted hover:text-fg hover:border-border-strong',
      )}
    >
      {Icon && <Icon className="size-3.5" aria-hidden />}
      {children}
    </button>
  )
}

export function ProgressBar({ value, className, label }: { value?: number; className?: string; label?: string }) {
  const indeterminate = value === undefined
  return (
    <div
      role="progressbar"
      aria-label={label}
      aria-valuemin={0}
      aria-valuemax={100}
      aria-valuenow={indeterminate ? undefined : Math.round(value * 100)}
      className={cn('h-1.5 w-full overflow-hidden rounded-full bg-surface-3', className)}
    >
      <div
        className={cn(
          'h-full rounded-full bg-accent transition-[width] duration-200',
          indeterminate && 'w-1/3 animate-pulse-soft',
        )}
        style={indeterminate ? undefined : { width: `${Math.round(value * 100)}%` }}
      />
    </div>
  )
}

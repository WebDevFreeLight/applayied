import { AlertCircle, Check, CircleDot, Loader2 } from 'lucide-react'
import type { Blocker } from 'react-router'
import { useTranslation } from 'react-i18next'
import type { AutosaveStatus } from '@/hooks/useAutosave'
import { cn } from '@/lib/utils'
import { Button } from '../ui/Button'
import { Modal } from '../ui/Modal'

/** "Saved / Saving… / Unsaved changes" indicator (live region). */
export function SaveIndicator({
  status,
  className,
  compact,
}: {
  status: AutosaveStatus
  className?: string
  /** Icon-only on very small phones (text stays available to screen readers). */
  compact?: boolean
}) {
  const { t } = useTranslation()
  const map = {
    saved: { icon: Check, text: t('save.saved'), cls: 'text-success' },
    saving: { icon: Loader2, text: t('save.saving'), cls: 'text-muted' },
    unsaved: { icon: CircleDot, text: t('save.unsaved'), cls: 'text-warning' },
    error: { icon: AlertCircle, text: t('save.error'), cls: 'text-danger' },
  } as const
  const { icon: Icon, text, cls } = map[status]
  return (
    <span
      role="status"
      aria-live="polite"
      title={t('save.shortcut')}
      className={cn('inline-flex items-center gap-1.5 text-xs font-medium whitespace-nowrap', cls, className)}
    >
      <Icon className={cn('size-3.5 shrink-0', status === 'saving' && 'animate-spin')} aria-hidden />
      <span className={cn('truncate', compact && 'sr-only min-[420px]:not-sr-only')}>{text}</span>
    </span>
  )
}

/** Confirm dialog shown when the router blocker intercepts navigation with unsaved changes. */
export function UnsavedChangesDialog({
  blocker,
  onSaveAndLeave,
}: {
  blocker: Blocker
  onSaveAndLeave: () => Promise<void>
}) {
  const { t } = useTranslation()
  const open = blocker.state === 'blocked'
  return (
    <Modal
      open={open}
      onClose={() => blocker.reset?.()}
      title={t('save.leaveTitle')}
      description={t('save.leaveBody')}
      size="sm"
      footer={
        <>
          <Button onClick={() => blocker.reset?.()}>{t('save.stay')}</Button>
          <Button variant="danger" onClick={() => blocker.proceed?.()}>
            {t('save.leaveWithoutSaving')}
          </Button>
          <Button
            variant="primary"
            onClick={async () => {
              try {
                await onSaveAndLeave()
                blocker.proceed?.()
              } catch {
                blocker.reset?.()
              }
            }}
          >
            {t('save.saveAndLeave')}
          </Button>
        </>
      }
    />
  )
}

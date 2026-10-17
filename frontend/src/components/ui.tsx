import { useEffect, type ReactNode } from 'react'
import { useTranslation } from 'react-i18next'

// Map backend enum values to badge colors (shared across modules).
const COLOR: Record<string, string> = {
  Active: 'green', Completed: 'blue', Planning: 'gray', OnHold: 'yellow', Cancelled: 'red',
  ToDo: 'gray', InProgress: 'blue', Blocked: 'red', Done: 'green',
  Pending: 'yellow', Achieved: 'green', Missed: 'red',
  Open: 'red', Mitigating: 'yellow', Closed: 'gray',
  Draft: 'gray', Submitted: 'blue', UnderReview: 'yellow', Approved: 'green', Rejected: 'red', Returned: 'yellow',
  Posted: 'green', Low: 'gray', Medium: 'blue', High: 'yellow', Critical: 'red',
  Risk: 'yellow', Issue: 'red',
}

/** Localized, color-coded status/priority pill. */
export function Badge({ value }: { value?: string }) {
  const { t } = useTranslation()
  if (!value) return <span className="muted">—</span>
  return <span className={`badge ${COLOR[value] || 'gray'}`}>{t(`status.${value}`, value)}</span>
}

export function Loading() {
  const { t } = useTranslation()
  return <div className="muted center" style={{ padding: 30 }}>{t('common.loading')}</div>
}
export function Empty() {
  const { t } = useTranslation()
  return <div className="muted center" style={{ padding: 30 }}>{t('common.empty')}</div>
}
export function ErrorBox({ message }: { message: string }) {
  return <div className="err" style={{ padding: 12 }}>{message}</div>
}

export function Modal({ title, onClose, children, footer }:
  { title: string; onClose: () => void; children: ReactNode; footer?: ReactNode }) {
  useEffect(() => {
    const h = (e: KeyboardEvent) => { if (e.key === 'Escape') onClose() }
    window.addEventListener('keydown', h)
    return () => window.removeEventListener('keydown', h)
  }, [onClose])
  return (
    <div className="overlay" onClick={onClose} role="dialog" aria-modal="true">
      <div className="modal" onClick={(e) => e.stopPropagation()}>
        <header>{title}</header>
        <div className="body">{children}</div>
        {footer && <footer>{footer}</footer>}
      </div>
    </div>
  )
}

export function Toast({ message }: { message: string }) {
  return <div className="toast">{message}</div>
}

/** AED / number formatting helpers (respect current locale for digit grouping). */
export const fmtMoney = (n?: number, currency = 'AED') =>
  n == null ? '—' : `${currency} ${n.toLocaleString(undefined, { maximumFractionDigits: 0 })}`
export const fmtPct = (n?: number) => (n == null ? '—' : `${(n * (n <= 1 ? 100 : 1)).toFixed(0)}%`)
export const fmtDate = (s?: string) => (s ? s.substring(0, 10) : '—')

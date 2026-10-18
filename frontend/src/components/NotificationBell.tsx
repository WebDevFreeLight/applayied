import { useEffect, useRef, useState } from 'react'
import { useTranslation } from 'react-i18next'
import * as sys from '../api/system'
import { onHub } from '../realtime/hub'
import { useAuth } from '../auth/auth'
import { fmtDate } from './ui'

interface Item { msgId: string; content?: string; isRead?: number; addTime?: string; msgType?: string }

// Header notification bell: shows unread count, a dropdown of recent messages, mark-one / mark-all
// read, and pops a transient toast when a realtime notification or notice arrives over SignalR.
export default function NotificationBell() {
  const { t } = useTranslation()
  const { signOut } = useAuth()
  const [items, setItems] = useState<Item[]>([])
  const [unread, setUnread] = useState(0)
  const [open, setOpen] = useState(false)
  const [toast, setToast] = useState<{ title: string; body?: string } | null>(null)
  const boxRef = useRef<HTMLDivElement>(null)

  const refresh = () => {
    sys.myMessages({ pageNum: 1, pageSize: 15 })
      .then((p) => {
        const rows = (p.result || []) as Item[]
        setItems(rows)
        setUnread(rows.filter((r) => !r.isRead).length)
      })
      .catch(() => { /* personal endpoint; ignore transient errors */ })
  }

  useEffect(refresh, [])

  // Realtime: a new in-app notification / published notice -> toast + prepend + bump count.
  useEffect(() => {
    const flash = (title: string, body?: string) => {
      setToast({ title, body })
      setTimeout(() => setToast(null), 5000)
      refresh()
    }
    const offN = onHub('notification', (p) => {
      const d = p as { title?: string; body?: string }
      flash(d?.title || 'Notification', d?.body)
    })
    const offNotice = onHub('notice', (p) => {
      const d = p as { title?: string; body?: string }
      flash(d?.title || 'Notice', d?.body)
    })
    const offForce = onHub('force', (p) => {
      const d = p as { reason?: string }
      alert(d?.reason || 'Your session was ended by an administrator.')
      signOut()
    })
    return () => { offN(); offNotice(); offForce() }
  }, [signOut])

  // Close dropdown on outside click.
  useEffect(() => {
    const onDoc = (e: MouseEvent) => { if (boxRef.current && !boxRef.current.contains(e.target as Node)) setOpen(false) }
    document.addEventListener('mousedown', onDoc)
    return () => document.removeEventListener('mousedown', onDoc)
  }, [])

  const markOne = async (m: Item) => {
    if (m.isRead) return
    try {
      await sys.readMessage(m.msgId, String(m.msgType ?? 1))
      setItems((xs) => xs.map((x) => (x.msgId === m.msgId ? { ...x, isRead: 1 } : x)))
      setUnread((n) => Math.max(0, n - 1))
    } catch { /* ignore */ }
  }
  const markAll = async () => {
    try { await sys.readAllMessages(); setItems((xs) => xs.map((x) => ({ ...x, isRead: 1 }))); setUnread(0) }
    catch { /* ignore */ }
  }

  return (
    <div ref={boxRef} style={{ position: 'relative' }}>
      <button className="btn sm" onClick={() => { setOpen((o) => !o); if (!open) refresh() }} title={t('notify.title')}>
        🔔{unread > 0 && <span className="badge-dot">{unread > 99 ? '99+' : unread}</span>}
      </button>

      {toast && (
        <div className="notify-toast">
          <strong>{toast.title}</strong>
          {toast.body && <div className="muted" style={{ fontSize: 12 }}>{toast.body}</div>}
        </div>
      )}

      {open && (
        <div className="notify-panel">
          <div className="row" style={{ justifyContent: 'space-between', padding: '8px 10px', borderBottom: '1px solid var(--border)' }}>
            <strong>{t('notify.title')}</strong>
            <button className="btn sm" onClick={markAll} disabled={unread === 0}>{t('notify.readAll')}</button>
          </div>
          <div style={{ maxHeight: 360, overflowY: 'auto' }}>
            {items.length === 0 ? (
              <div className="muted" style={{ padding: 16, textAlign: 'center' }}>{t('notify.empty')}</div>
            ) : items.map((m) => (
              <div key={m.msgId} onClick={() => markOne(m)}
                className="notify-item" style={{ opacity: m.isRead ? 0.6 : 1 }}>
                <div style={{ display: 'flex', gap: 6 }}>
                  {!m.isRead && <span className="unread-dot" />}
                  <div style={{ flex: 1 }}>{m.content}</div>
                </div>
                <div className="muted" style={{ fontSize: 11, marginTop: 2 }}>{fmtDate(m.addTime)}</div>
              </div>
            ))}
          </div>
        </div>
      )}
    </div>
  )
}

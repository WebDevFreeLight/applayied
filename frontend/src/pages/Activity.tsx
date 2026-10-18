import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import * as pmo from '../api/pmo'
import type { AutomationRun, OutboxNotification, AuditEvent } from '../api/types'
import { useAuth } from '../auth/auth'
import { Badge, Empty, ErrorBox, Loading, fmtDate } from '../components/ui'

type Tab = 'automation' | 'outbox' | 'audit'

// Automation & logging module (pmo_automation_run / pmo_outbox_notification / pmo_audit_event):
// read-only evidence of executed workflow behaviour, deduped runs/sends, and the durable audit trail.
export default function Activity() {
  const { t } = useTranslation()
  const { hasPerm } = useAuth()
  const [tab, setTab] = useState<Tab>('automation')
  const [runs, setRuns] = useState<AutomationRun[]>([])
  const [outbox, setOutbox] = useState<OutboxNotification[]>([])
  const [audit, setAudit] = useState<AuditEvent[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  useEffect(() => {
    setLoading(true); setError('')
    const p = { pageNum: 1, pageSize: 200 }
    Promise.allSettled([pmo.listAutomation(p), pmo.listOutbox(p), pmo.listAudit(p)]).then(([a, o, u]) => {
      if (a.status === 'fulfilled') setRuns(a.value.result || [])
      if (o.status === 'fulfilled') setOutbox(o.value.result || [])
      if (u.status === 'fulfilled') setAudit(u.value.result || [])
      const err = [a, o, u].find((r) => r.status === 'rejected') as PromiseRejectedResult | undefined
      if (err) setError(String(err.reason?.message || err.reason))
    }).finally(() => setLoading(false))
  }, [])

  if (!hasPerm('pmo:automation:list') && !hasPerm('pmo:audit:list')) return <div className="panel">{t('admin.noAccess')}</div>
  const TABS: Tab[] = ['automation', 'outbox', 'audit']
  const label: Record<Tab, string> = { automation: t('activity.automation'), outbox: t('activity.outbox'), audit: t('activity.audit') }

  return (
    <div>
      <h2 style={{ margin: 0 }}>{t('activity.title')}</h2>
      <div className="row" style={{ marginTop: 12, gap: 6, borderBottom: '1px solid var(--border)', paddingBottom: 8 }}>
        {TABS.map((x) => <button key={x} className={`btn sm ${tab === x ? 'primary' : ''}`} onClick={() => setTab(x)}>{label[x]}</button>)}
      </div>
      {error && <ErrorBox message={error} />}
      {loading ? <Loading /> : (
        <div style={{ marginTop: 14 }}>
          {tab === 'automation' && (runs.length === 0 ? <Empty /> : (
            <div className="table-wrap"><table><thead><tr>
              <th>{t('activity.rule')}</th><th>{t('activity.when')}</th><th>{t('activity.result')}</th><th>{t('activity.detail')}</th></tr></thead>
              <tbody>{runs.map((r) => (
                <tr key={r.runId}><td><code>{r.ruleKey}</code></td><td>{fmtDate(r.triggeredAt)}</td>
                  <td><Badge value={r.status} /></td><td>{r.detail}</td></tr>))}</tbody></table></div>))}
          {tab === 'outbox' && (outbox.length === 0 ? <Empty /> : (
            <div className="table-wrap"><table><thead><tr>
              <th>{t('activity.type')}</th><th>{t('activity.subject')}</th><th>{t('activity.when')}</th><th>{t('activity.result')}</th></tr></thead>
              <tbody>{outbox.map((n) => (
                <tr key={n.notificationId}><td>{n.type}</td><td>{n.subject}</td><td>{fmtDate(n.createdAt)}</td>
                  <td><Badge value={n.status} /></td></tr>))}</tbody></table></div>))}
          {tab === 'audit' && (audit.length === 0 ? <Empty /> : (
            <div className="table-wrap"><table><thead><tr>
              <th>{t('activity.entity')}</th><th>{t('activity.action')}</th><th>{t('activity.actor')}</th><th>{t('activity.when')}</th><th>{t('activity.detail')}</th></tr></thead>
              <tbody>{audit.map((a) => (
                <tr key={a.auditId}><td>{a.entityType} #{a.entityId}</td><td>{a.action}</td><td>{a.actorName}</td>
                  <td>{fmtDate(a.actedAt)}</td><td>{a.detail}</td></tr>))}</tbody></table></div>))}
        </div>
      )}
    </div>
  )
}

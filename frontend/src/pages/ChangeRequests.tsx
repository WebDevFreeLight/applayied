import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import * as pmo from '../api/pmo'
import type { ChangeRequest, ChangeHistory } from '../api/types'
import { useLookups } from '../hooks/useLookups'
import { useAuth } from '../auth/auth'
import { Badge, Empty, ErrorBox, Loading, Modal, Toast, fmtDate } from '../components/ui'

export default function ChangeRequests() {
  const { t } = useTranslation()
  const lk = useLookups()
  const { hasRole } = useAuth()
  const [rows, setRows] = useState<ChangeRequest[]>([])
  const [projectId, setProjectId] = useState('')
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [edit, setEdit] = useState<Partial<ChangeRequest> | null>(null)
  const [history, setHistory] = useState<{ cr: ChangeRequest; rows: ChangeHistory[] } | null>(null)
  const [decide, setDecide] = useState<{ cr: ChangeRequest; action: string; reason: string } | null>(null)
  const [toast, setToast] = useState('')

  const load = () => {
    setLoading(true); setError('')
    const params: Record<string, unknown> = { pageNum: 1, pageSize: 200 }
    if (projectId) params.projectId = projectId
    pmo.listChangeRequests(params).then((p) => setRows(p.result || []))
      .catch((e) => setError((e as Error).message)).finally(() => setLoading(false))
  }
  useEffect(load, [projectId])
  const notify = (m: string) => { setToast(m); setTimeout(() => setToast(''), 2500) }

  const create = async () => {
    if (!edit) return
    try { await pmo.createChangeRequest(edit); setEdit(null); notify(t('common.saved')); load() }
    catch (e) { setError((e as Error).message) }
  }
  const submit = async (cr: ChangeRequest) => {
    try { await pmo.submitChangeRequest(cr.changeRequestId); notify(t('common.saved')); load() }
    catch (e) { setError((e as Error).message) }
  }
  const sendDecision = async () => {
    if (!decide) return
    try {
      await pmo.decideChangeRequest({ changeRequestId: decide.cr.changeRequestId, action: decide.action, reason: decide.reason, version: decide.cr.version })
      setDecide(null); notify(t('common.saved')); load()
    } catch (e) { setError((e as Error).message) }
  }
  const openHistory = async (cr: ChangeRequest) =>
    setHistory({ cr, rows: await pmo.getChangeHistory(cr.changeRequestId) })

  const canDecide = (cr: ChangeRequest) => hasRole('pm') && ['Submitted', 'UnderReview'].includes(cr.status)

  return (
    <div>
      <div className="row">
        <h2 style={{ margin: 0 }}>{t('cr.title')}</h2>
        <div className="spacer" />
        <select value={projectId} onChange={(e) => setProjectId(e.target.value)}>
          <option value="">{t('project.title')}: {t('common.all')}</option>
          {lk.projects.map((p) => <option key={p.id} value={p.id}>{p.label}</option>)}
        </select>
        <button className="btn primary" onClick={() => setEdit({})}>{t('cr.newCr')}</button>
      </div>
      {error && <ErrorBox message={error} />}
      {loading ? <Loading /> : rows.length === 0 ? <Empty /> : (
        <div className="table-wrap" style={{ marginTop: 12 }}>
          <table><thead><tr>
            <th>{t('cr.crTitle')}</th><th>{t('project.title')}</th><th>{t('cr.requestedBy')}</th>
            <th>{t('cr.status')}</th><th>{t('cr.costImpact')}</th><th>{t('cr.decisionBy')}</th><th>{t('common.actions')}</th></tr></thead>
            <tbody>{rows.map((c) => (
              <tr key={c.changeRequestId}>
                <td>{c.title}</td><td>{c.projectName}</td><td>{c.requestedByName}</td>
                <td><Badge value={c.status} /></td><td>{c.estimatedCostImpact ?? '—'}</td><td>{c.decisionByName || '—'}</td>
                <td className="row">
                  <button className="btn sm" onClick={() => openHistory(c)}>{t('cr.history')}</button>
                  {c.status === 'Draft' && <button className="btn sm" onClick={() => submit(c)}>{t('cr.submit')}</button>}
                  {canDecide(c) && <>
                    <button className="btn sm" onClick={() => setDecide({ cr: c, action: 'Approve', reason: '' })}>{t('cr.approve')}</button>
                    <button className="btn sm" onClick={() => setDecide({ cr: c, action: 'Reject', reason: '' })}>{t('cr.reject')}</button>
                    <button className="btn sm" onClick={() => setDecide({ cr: c, action: 'Return', reason: '' })}>{t('cr.return')}</button>
                  </>}
                </td>
              </tr>))}</tbody>
          </table>
        </div>
      )}

      {edit && (
        <Modal title={t('cr.newCr')} onClose={() => setEdit(null)}
          footer={<><button className="btn" onClick={() => setEdit(null)}>{t('common.cancel')}</button>
            <button className="btn primary" onClick={create}>{t('common.save')}</button></>}>
          <label className="field full">{t('cr.crTitle')}<input value={edit.title || ''} onChange={(e) => setEdit({ ...edit, title: e.target.value })} /></label>
          <label className="field">{t('project.title')}
            <select value={edit.projectId || ''} onChange={(e) => setEdit({ ...edit, projectId: e.target.value })}>
              <option value="">—</option>{lk.projects.map((p) => <option key={p.id} value={p.id}>{p.label}</option>)}</select></label>
          <label className="field">{t('cr.costImpact')}<input type="number" value={edit.estimatedCostImpact ?? 0} onChange={(e) => setEdit({ ...edit, estimatedCostImpact: Number(e.target.value) })} /></label>
          <label className="field full">{t('cr.description')}<textarea rows={3} value={edit.description || ''} onChange={(e) => setEdit({ ...edit, description: e.target.value })} /></label>
        </Modal>
      )}

      {decide && (
        <Modal title={`${t('cr.decision')}: ${t(`cr.${decide.action.toLowerCase()}`)}`} onClose={() => setDecide(null)}
          footer={<><button className="btn" onClick={() => setDecide(null)}>{t('common.cancel')}</button>
            <button className="btn primary" onClick={sendDecision}>{t('common.confirm')}</button></>}>
          <div className="full"><b>{decide.cr.title}</b></div>
          <label className="field full">{t('cr.reason')}
            <textarea rows={3} value={decide.reason} onChange={(e) => setDecide({ ...decide, reason: e.target.value })} /></label>
        </Modal>
      )}

      {history && (
        <Modal title={`${t('cr.history')} — ${history.cr.title}`} onClose={() => setHistory(null)}
          footer={<button className="btn" onClick={() => setHistory(null)}>{t('common.close')}</button>}>
          <div className="full table-wrap">
            {history.rows.length === 0 ? <Empty /> : (
              <table><thead><tr><th>{t('cr.from')}</th><th>{t('cr.to')}</th><th>{t('cr.actor')}</th><th>{t('cr.reason')}</th><th>{t('cr.when')}</th></tr></thead>
                <tbody>{history.rows.map((h) => (
                  <tr key={h.historyId}><td><Badge value={h.fromStatus} /></td><td><Badge value={h.toStatus} /></td>
                    <td>{h.actorName}</td><td>{h.reason || '—'}</td><td>{fmtDate(h.actedAt)}</td></tr>))}</tbody>
              </table>)}
          </div>
        </Modal>
      )}
      {toast && <Toast message={toast} />}
    </div>
  )
}

import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import * as pmo from '../api/pmo'
import AiFill from '../components/AiFill'
import type { Risk } from '../api/types'
import { useLookups } from '../hooks/useLookups'
import { useAuth } from '../auth/auth'
import { Badge, Empty, ErrorBox, Loading, Modal, Toast, fmtDate } from '../components/ui'
import Select from '../components/Select'
import { EntityDetail } from '../components/EntityDetail'
import Tasks from './Tasks'

const STATUSES = ['Open', 'Mitigating', 'Closed']
const KINDS = ['Risk', 'Issue']

export default function Risks({ projectId: pin }: { projectId?: string } = {}) {
  const { t } = useTranslation()
  const lk = useLookups()
  const { hasPerm } = useAuth()
  const [rows, setRows] = useState<Risk[]>([])
  const [query, setQuery] = useState({ projectId: pin || '', status: '', highExposureOnly: false })
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [edit, setEdit] = useState<Partial<Risk> | null>(null)
  const [view, setView] = useState<Risk | null>(null)
  const [toast, setToast] = useState('')

  const load = () => {
    setLoading(true); setError('')
    const params: Record<string, unknown> = { pageNum: 1, pageSize: 200 }
    if (query.projectId) params.projectId = query.projectId
    if (query.status) params.status = query.status
    if (query.highExposureOnly) params.highExposureOnly = true
    pmo.listRisks(params).then((p) => setRows(p.result || []))
      .catch((e) => setError((e as Error).message)).finally(() => setLoading(false))
  }
  useEffect(load, [query.projectId, query.status, query.highExposureOnly])

  const save = async () => {
    if (!edit) return
    try {
      if (edit.riskId) await pmo.updateRisk(edit); else await pmo.createRisk(edit)
      setEdit(null); setToast(t('common.saved')); setTimeout(() => setToast(''), 2500); load()
    } catch (e) { setError((e as Error).message) }
  }
  const score = (edit?.probability || 0) * (edit?.impact || 0)

  return (
    <div>
      <div className="row">
        <h2 style={{ margin: 0 }}>{t('risk.title')}</h2>
        <div className="spacer" />
        {hasPerm('pmo:risk:add') && <button className="btn primary" onClick={() => setEdit({ kind: 'Risk', status: 'Open', probability: 3, impact: 3, projectId: pin || undefined })}>{t('risk.newRisk')}</button>}
      </div>
      <div className="row panel" style={{ marginTop: 12 }}>
        {!pin && <div style={{ minWidth: 200 }}><Select options={lk.projects.map((p) => ({ value: p.id, label: p.name }))}
          value={query.projectId} onChange={(v) => setQuery((q) => ({ ...q, projectId: v || '' }))}
          placeholder={`${t('project.title')}: ${t('common.all')}`} /></div>}
        <div style={{ minWidth: 170 }}><Select options={STATUSES.map((s) => ({ value: s, label: t(`status.${s}`) }))}
          value={query.status} onChange={(v) => setQuery((q) => ({ ...q, status: v || '' }))}
          placeholder={`${t('risk.status')}: ${t('common.all')}`} /></div>
        <label className="row" style={{ gap: 6 }}>
          <input type="checkbox" checked={query.highExposureOnly} onChange={(e) => setQuery((q) => ({ ...q, highExposureOnly: e.target.checked }))} />
          {t('risk.highExposure')}
        </label>
      </div>
      {error && <ErrorBox message={error} />}
      {loading ? <Loading /> : rows.length === 0 ? <Empty /> : (
        <div className="table-wrap" style={{ marginTop: 12 }}>
          <table><thead><tr>
            <th>{t('risk.kind')}</th><th>{t('risk.riskTitle')}</th><th>{t('project.title')}</th><th>{t('risk.owner')}</th>
            <th>{t('risk.probability')}</th><th>{t('risk.impact')}</th><th>{t('risk.score')}</th>
            <th>{t('risk.status')}</th><th>{t('common.actions')}</th></tr></thead>
            <tbody>{rows.map((r) => (
              <tr key={r.riskId}><td><Badge value={r.kind} /></td><td>{r.title}</td><td>{r.projectName}</td><td>{r.ownerName}</td>
                <td>{r.probability}</td><td>{r.impact}</td>
                <td><b className={r.score >= 12 ? 'badge red' : ''}>{r.score}</b></td>
                <td><Badge value={r.status} /></td>
                <td className="row">
                  <button className="btn sm" onClick={() => setView(r)}>{t('project.details')}</button>
                  {hasPerm('pmo:risk:edit') && <button className="btn sm" onClick={() => setEdit({ ...r })}>{t('common.edit')}</button>}
                </td></tr>))}</tbody>
          </table>
        </div>
      )}
      {edit && (
        <Modal title={edit.riskId ? t('risk.editRisk') : t('risk.newRisk')} onClose={() => setEdit(null)}
          footer={<><button className="btn" onClick={() => setEdit(null)}>{t('common.cancel')}</button>
            <button className="btn primary" onClick={save}>{t('common.save')}</button></>}>
          <AiFill kind="project risk or issue" fields="title, mitigation, kind(Risk|Issue)" onApply={(o) => setEdit({ ...edit, ...o })} />
          <label className="field">{t('risk.kind')}
            <Select isClearable={false} options={KINDS.map((s) => ({ value: s, label: t(`status.${s}`) }))}
              value={edit.kind} onChange={(v) => setEdit({ ...edit, kind: v || 'Risk' })} /></label>
          {!pin && <label className="field">{t('project.title')}
            <Select options={lk.projects.map((p) => ({ value: p.id, label: p.name }))}
              value={edit.projectId || null} onChange={(v) => setEdit({ ...edit, projectId: v || undefined })} /></label>}
          <label className="field full">{t('risk.riskTitle')}<input value={edit.title || ''} onChange={(e) => setEdit({ ...edit, title: e.target.value })} /></label>
          <label className="field">{t('risk.owner')}
            <Select options={lk.users.map((u) => ({ value: u.id, label: u.name }))}
              value={edit.ownerUserId || null} onChange={(v) => setEdit({ ...edit, ownerUserId: v || undefined })} /></label>
          <label className="field">{t('risk.status')}
            <Select isClearable={false} options={STATUSES.map((s) => ({ value: s, label: t(`status.${s}`) }))}
              value={edit.status} onChange={(v) => setEdit({ ...edit, status: v || 'Open' })} /></label>
          <label className="field">{t('risk.probability')} (1–5)<input type="number" min={1} max={5} value={edit.probability ?? 3} onChange={(e) => setEdit({ ...edit, probability: Number(e.target.value) })} /></label>
          <label className="field">{t('risk.impact')} (1–5)<input type="number" min={1} max={5} value={edit.impact ?? 3} onChange={(e) => setEdit({ ...edit, impact: Number(e.target.value) })} /></label>
          <label className="field">{t('risk.score')}<input value={score} disabled /></label>
          <label className="field">{t('risk.due')}<input type="date" value={(edit.dueDate || '').substring(0, 10)} onChange={(e) => setEdit({ ...edit, dueDate: e.target.value })} /></label>
          <label className="field full">{t('risk.mitigation')}<textarea rows={2} value={edit.mitigation || ''} onChange={(e) => setEdit({ ...edit, mitigation: e.target.value })} /></label>
        </Modal>
      )}
      {view && (
        <EntityDetail title={view.title} projectId={view.projectId} onClose={() => setView(null)} rows={[
          [t('risk.kind'), <Badge value={view.kind} />],
          [t('project.title'), view.projectName],
          [t('risk.owner'), view.ownerName],
          [t('risk.probability'), view.probability],
          [t('risk.impact'), view.impact],
          [t('risk.score'), <b className={view.score >= 12 ? 'badge red' : ''}>{view.score}</b>],
          [t('risk.status'), <Badge value={view.status} />],
          [t('risk.due'), fmtDate(view.dueDate)],
          [t('risk.mitigation'), view.mitigation],
        ]} extra={<><h3>{t('task.title')}</h3><Tasks projectId={view.projectId} /></>} />
      )}
      {toast && <Toast message={toast} />}
    </div>
  )
}

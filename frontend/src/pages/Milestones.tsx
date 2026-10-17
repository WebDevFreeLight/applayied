import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import * as pmo from '../api/pmo'
import type { Milestone } from '../api/types'
import { useLookups } from '../hooks/useLookups'
import { useAuth } from '../auth/auth'
import { Badge, Empty, ErrorBox, Loading, Modal, Toast, fmtDate } from '../components/ui'

const STATUSES = ['Pending', 'Achieved', 'Missed']

export default function Milestones() {
  const { t } = useTranslation()
  const lk = useLookups()
  const { hasRole } = useAuth()
  const [rows, setRows] = useState<Milestone[]>([])
  const [projectId, setProjectId] = useState('')
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [edit, setEdit] = useState<Partial<Milestone> | null>(null)
  const [toast, setToast] = useState('')

  const load = () => {
    setLoading(true); setError('')
    const params: Record<string, unknown> = { pageNum: 1, pageSize: 200 }
    if (projectId) params.projectId = projectId
    pmo.listMilestones(params).then((p) => setRows(p.result || []))
      .catch((e) => setError((e as Error).message)).finally(() => setLoading(false))
  }
  useEffect(load, [projectId])

  const save = async () => {
    if (!edit) return
    try {
      if (edit.milestoneId) await pmo.updateMilestone(edit); else await pmo.createMilestone(edit)
      setEdit(null); setToast(t('common.saved')); setTimeout(() => setToast(''), 2500); load()
    } catch (e) { setError((e as Error).message) }
  }

  return (
    <div>
      <div className="row">
        <h2 style={{ margin: 0 }}>{t('milestone.title')}</h2>
        <div className="spacer" />
        <select value={projectId} onChange={(e) => setProjectId(e.target.value)}>
          <option value="">{t('project.title')}: {t('common.all')}</option>
          {lk.projects.map((p) => <option key={p.id} value={p.id}>{p.label}</option>)}
        </select>
        {hasRole('pm') && <button className="btn primary" onClick={() => setEdit({ status: 'Pending', projectId: projectId || undefined })}>{t('milestone.newMilestone')}</button>}
      </div>
      {error && <ErrorBox message={error} />}
      {loading ? <Loading /> : rows.length === 0 ? <Empty /> : (
        <div className="table-wrap" style={{ marginTop: 12 }}>
          <table><thead><tr>
            <th>{t('milestone.name')}</th><th>{t('project.title')}</th><th>{t('milestone.due')}</th>
            <th>{t('milestone.status')}</th><th>{t('common.actions')}</th></tr></thead>
            <tbody>{rows.map((m) => (
              <tr key={m.milestoneId}><td>{m.name}</td><td>{m.projectName}</td><td>{fmtDate(m.dueDate)}</td>
                <td><Badge value={m.status} /></td>
                <td>{hasRole('pm') && <button className="btn sm" onClick={() => setEdit({ ...m })}>{t('common.edit')}</button>}</td></tr>))}</tbody>
          </table>
        </div>
      )}
      {edit && (
        <Modal title={edit.milestoneId ? t('milestone.editMilestone') : t('milestone.newMilestone')} onClose={() => setEdit(null)}
          footer={<><button className="btn" onClick={() => setEdit(null)}>{t('common.cancel')}</button>
            <button className="btn primary" onClick={save}>{t('common.save')}</button></>}>
          <label className="field full">{t('milestone.name')}<input value={edit.name || ''} onChange={(e) => setEdit({ ...edit, name: e.target.value })} /></label>
          <label className="field">{t('project.title')}
            <select value={edit.projectId || ''} onChange={(e) => setEdit({ ...edit, projectId: e.target.value })}>
              <option value="">—</option>{lk.projects.map((p) => <option key={p.id} value={p.id}>{p.label}</option>)}</select></label>
          <label className="field">{t('milestone.due')}<input type="date" value={(edit.dueDate || '').substring(0, 10)} onChange={(e) => setEdit({ ...edit, dueDate: e.target.value })} /></label>
          <label className="field">{t('milestone.status')}
            <select value={edit.status} onChange={(e) => setEdit({ ...edit, status: e.target.value })}>
              {STATUSES.map((s) => <option key={s} value={s}>{t(`status.${s}`)}</option>)}</select></label>
        </Modal>
      )}
      {toast && <Toast message={toast} />}
    </div>
  )
}

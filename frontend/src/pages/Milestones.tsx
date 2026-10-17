import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import * as pmo from '../api/pmo'
import type { Milestone } from '../api/types'
import { useLookups } from '../hooks/useLookups'
import { useAuth } from '../auth/auth'
import { Badge, Empty, ErrorBox, Loading, Modal, Toast, fmtDate } from '../components/ui'
import Select from '../components/Select'
import { EntityDetail } from '../components/EntityDetail'
import Tasks from './Tasks'

const STATUSES = ['Pending', 'Achieved', 'Missed']

export default function Milestones({ projectId: pin }: { projectId?: string } = {}) {
  const { t } = useTranslation()
  const lk = useLookups()
  const { hasRole } = useAuth()
  const [rows, setRows] = useState<Milestone[]>([])
  const [projectId, setProjectId] = useState(pin || '')
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [edit, setEdit] = useState<Partial<Milestone> | null>(null)
  const [view, setView] = useState<Milestone | null>(null)
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
        {!pin && <div style={{ minWidth: 220 }}><Select options={lk.projects.map((p) => ({ value: p.id, label: p.name }))}
          value={projectId} onChange={(v) => setProjectId(v || '')}
          placeholder={`${t('project.title')}: ${t('common.all')}`} /></div>}
        {hasRole('pm') && <button className="btn primary" onClick={() => setEdit({ status: 'Pending', projectId: projectId || pin || undefined })}>{t('milestone.newMilestone')}</button>}
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
                <td className="row">
                  <button className="btn sm" onClick={() => setView(m)}>{t('project.details')}</button>
                  {hasRole('pm') && <button className="btn sm" onClick={() => setEdit({ ...m })}>{t('common.edit')}</button>}
                </td></tr>))}</tbody>
          </table>
        </div>
      )}
      {edit && (
        <Modal title={edit.milestoneId ? t('milestone.editMilestone') : t('milestone.newMilestone')} onClose={() => setEdit(null)}
          footer={<><button className="btn" onClick={() => setEdit(null)}>{t('common.cancel')}</button>
            <button className="btn primary" onClick={save}>{t('common.save')}</button></>}>
          <label className="field full">{t('milestone.name')}<input value={edit.name || ''} onChange={(e) => setEdit({ ...edit, name: e.target.value })} /></label>
          {!pin && <label className="field">{t('project.title')}
            <Select options={lk.projects.map((p) => ({ value: p.id, label: p.name }))}
              value={edit.projectId || null} onChange={(v) => setEdit({ ...edit, projectId: v || undefined })} /></label>}
          <label className="field">{t('milestone.due')}<input type="date" value={(edit.dueDate || '').substring(0, 10)} onChange={(e) => setEdit({ ...edit, dueDate: e.target.value })} /></label>
          <label className="field">{t('milestone.status')}
            <Select isClearable={false} options={STATUSES.map((s) => ({ value: s, label: t(`status.${s}`) }))}
              value={edit.status} onChange={(v) => setEdit({ ...edit, status: v || 'Pending' })} /></label>
        </Modal>
      )}
      {view && (
        <EntityDetail title={view.name} projectId={view.projectId} onClose={() => setView(null)} rows={[
          [t('project.title'), view.projectName],
          [t('milestone.due'), fmtDate(view.dueDate)],
          [t('milestone.status'), <Badge value={view.status} />],
        ]} extra={<><h3>{t('task.title')}</h3><Tasks projectId={view.projectId} /></>} />
      )}
      {toast && <Toast message={toast} />}
    </div>
  )
}

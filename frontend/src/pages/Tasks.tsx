import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import * as pmo from '../api/pmo'
import type { Task } from '../api/types'
import { useLookups } from '../hooks/useLookups'
import { useAuth } from '../auth/auth'
import { Badge, Empty, ErrorBox, Loading, Modal, Toast, fmtDate } from '../components/ui'

const STATUSES = ['ToDo', 'InProgress', 'Blocked', 'Done']
const PRIORITIES = ['Low', 'Medium', 'High', 'Critical']

export default function Tasks() {
  const { t } = useTranslation()
  const lk = useLookups()
  const { hasRole } = useAuth()
  const [rows, setRows] = useState<Task[]>([])
  const [total, setTotal] = useState(0)
  const [query, setQuery] = useState({ pageNum: 1, pageSize: 10, projectId: '', status: '', assigneeUserId: '' })
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [edit, setEdit] = useState<Partial<Task> | null>(null)
  const [progress, setProgress] = useState<Task | null>(null)
  const [toast, setToast] = useState('')

  const load = () => {
    setLoading(true); setError('')
    const params: Record<string, unknown> = { pageNum: query.pageNum, pageSize: query.pageSize }
    if (query.projectId) params.projectId = query.projectId
    if (query.status) params.status = query.status
    if (query.assigneeUserId) params.assigneeUserId = query.assigneeUserId
    pmo.listTasks(params)
      .then((p) => { setRows(p.result || []); setTotal(p.totalNum || 0) })
      .catch((e) => setError((e as Error).message)).finally(() => setLoading(false))
  }
  useEffect(load, [query.pageNum, query.projectId, query.status, query.assigneeUserId])

  const notify = (m: string) => { setToast(m); setTimeout(() => setToast(''), 2500) }
  const save = async () => {
    if (!edit) return
    try {
      if (edit.taskId) await pmo.updateTask(edit); else await pmo.createTask(edit)
      setEdit(null); notify(t('common.saved')); load()
    } catch (e) { setError((e as Error).message) }
  }
  const saveProgress = async () => {
    if (!progress) return
    try {
      await pmo.updateTaskProgress(progress.taskId, progress.progressPct, progress.version)
      setProgress(null); notify(t('common.saved')); load()
    } catch (e) { setError((e as Error).message) }
  }

  const pages = Math.max(1, Math.ceil(total / query.pageSize))
  return (
    <div>
      <div className="row">
        <h2 style={{ margin: 0 }}>{t('task.title')}</h2>
        <div className="spacer" />
        {hasRole('pm') && <button className="btn primary" onClick={() => setEdit({ status: 'ToDo', priority: 'Medium', weight: 1, progressPct: 0 })}>{t('task.newTask')}</button>}
      </div>

      <div className="row panel" style={{ marginTop: 12 }}>
        <select value={query.projectId} onChange={(e) => setQuery((q) => ({ ...q, projectId: e.target.value, pageNum: 1 }))}>
          <option value="">{t('project.title')}: {t('common.all')}</option>
          {lk.projects.map((p) => <option key={p.id} value={p.id}>{p.label}</option>)}
        </select>
        <select value={query.status} onChange={(e) => setQuery((q) => ({ ...q, status: e.target.value, pageNum: 1 }))}>
          <option value="">{t('task.status')}: {t('common.all')}</option>
          {STATUSES.map((s) => <option key={s} value={s}>{t(`status.${s}`)}</option>)}
        </select>
        <select value={query.assigneeUserId} onChange={(e) => setQuery((q) => ({ ...q, assigneeUserId: e.target.value, pageNum: 1 }))}>
          <option value="">{t('task.assignee')}: {t('common.all')}</option>
          {lk.users.map((u) => <option key={u.id} value={u.id}>{u.label}</option>)}
        </select>
      </div>

      {error && <ErrorBox message={error} />}
      {loading ? <Loading /> : rows.length === 0 ? <Empty /> : (
        <div className="table-wrap" style={{ marginTop: 12 }}>
          <table>
            <thead><tr>
              <th>{t('task.taskTitle')}</th><th>{t('project.title')}</th><th>{t('task.assignee')}</th>
              <th>{t('task.status')}</th><th>{t('task.priority')}</th><th>{t('task.due')}</th>
              <th>{t('task.progress')}</th><th>{t('common.actions')}</th>
            </tr></thead>
            <tbody>{rows.map((x) => (
              <tr key={x.taskId}>
                <td>{x.title}{x.isOverdue && <span className="badge red" style={{ marginInlineStart: 6 }}>{t('task.overdue')}</span>}</td>
                <td>{x.projectName}</td><td>{x.assigneeName || <span className="muted">{t('task.unassigned')}</span>}</td>
                <td><Badge value={x.status} /></td><td><Badge value={x.priority} /></td><td>{fmtDate(x.dueDate)}</td>
                <td>{x.progressPct}%</td>
                <td className="row">
                  <button className="btn sm" onClick={() => setProgress({ ...x })}>{t('task.updateProgress')}</button>
                  {hasRole('pm') && <button className="btn sm" onClick={() => setEdit({ ...x })}>{t('common.edit')}</button>}
                </td>
              </tr>))}</tbody>
          </table>
        </div>
      )}
      <div className="row" style={{ marginTop: 10 }}>
        <span className="muted">{total} · {t('common.page')} {query.pageNum}/{pages}</span>
        <div className="spacer" />
        <button className="btn sm" disabled={query.pageNum <= 1} onClick={() => setQuery((q) => ({ ...q, pageNum: q.pageNum - 1 }))}>‹</button>
        <button className="btn sm" disabled={query.pageNum >= pages} onClick={() => setQuery((q) => ({ ...q, pageNum: q.pageNum + 1 }))}>›</button>
      </div>

      {progress && (
        <Modal title={t('task.updateProgress')} onClose={() => setProgress(null)}
          footer={<><button className="btn" onClick={() => setProgress(null)}>{t('common.cancel')}</button>
            <button className="btn primary" onClick={saveProgress}>{t('common.save')}</button></>}>
          <div className="full"><b>{progress.title}</b></div>
          <label className="field full">{t('task.progress')}: {progress.progressPct}%
            <input type="range" min={0} max={100} value={progress.progressPct}
              onChange={(e) => setProgress({ ...progress, progressPct: Number(e.target.value) })} /></label>
          <label className="field full">{t('task.status')}
            <select value={progress.status} onChange={(e) => setProgress({ ...progress, status: e.target.value })}>
              {STATUSES.map((s) => <option key={s} value={s}>{t(`status.${s}`)}</option>)}
            </select></label>
        </Modal>
      )}

      {edit && (
        <Modal title={edit.taskId ? t('task.editTask') : t('task.newTask')} onClose={() => setEdit(null)}
          footer={<><button className="btn" onClick={() => setEdit(null)}>{t('common.cancel')}</button>
            <button className="btn primary" onClick={save}>{t('common.save')}</button></>}>
          <label className="field full">{t('task.taskTitle')}<input value={edit.title || ''} onChange={(e) => setEdit({ ...edit, title: e.target.value })} /></label>
          <label className="field">{t('project.title')}
            <select value={edit.projectId || ''} onChange={(e) => setEdit({ ...edit, projectId: e.target.value })}>
              <option value="">—</option>{lk.projects.map((p) => <option key={p.id} value={p.id}>{p.label}</option>)}
            </select></label>
          <label className="field">{t('task.assignee')}
            <select value={edit.assigneeUserId || ''} onChange={(e) => setEdit({ ...edit, assigneeUserId: e.target.value || undefined })}>
              <option value="">{t('task.unassigned')}</option>{lk.users.map((u) => <option key={u.id} value={u.id}>{u.label}</option>)}
            </select></label>
          <label className="field">{t('task.status')}
            <select value={edit.status} onChange={(e) => setEdit({ ...edit, status: e.target.value })}>
              {STATUSES.map((s) => <option key={s} value={s}>{t(`status.${s}`)}</option>)}</select></label>
          <label className="field">{t('task.priority')}
            <select value={edit.priority} onChange={(e) => setEdit({ ...edit, priority: e.target.value })}>
              {PRIORITIES.map((s) => <option key={s} value={s}>{t(`status.${s}`)}</option>)}</select></label>
          <label className="field">{t('task.start')}<input type="date" value={(edit.startDate || '').substring(0, 10)} onChange={(e) => setEdit({ ...edit, startDate: e.target.value })} /></label>
          <label className="field">{t('task.due')}<input type="date" value={(edit.dueDate || '').substring(0, 10)} onChange={(e) => setEdit({ ...edit, dueDate: e.target.value })} /></label>
          <label className="field">{t('task.weight')}<input type="number" value={edit.weight ?? 1} onChange={(e) => setEdit({ ...edit, weight: Number(e.target.value) })} /></label>
          <label className="field">{t('task.progress')}<input type="number" min={0} max={100} value={edit.progressPct ?? 0} onChange={(e) => setEdit({ ...edit, progressPct: Number(e.target.value) })} /></label>
        </Modal>
      )}
      {toast && <Toast message={toast} />}
    </div>
  )
}

import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import * as pmo from '../api/pmo'
import type { Project } from '../api/types'
import { useLookups } from '../hooks/useLookups'
import { useAuth } from '../auth/auth'
import { Badge, Empty, ErrorBox, Loading, Modal, Toast, fmtDate, fmtMoney } from '../components/ui'

const STATUSES = ['Planning', 'Active', 'OnHold', 'Completed', 'Cancelled']
const PRIORITIES = ['Low', 'Medium', 'High', 'Critical']
const empty: Partial<Project> = { status: 'Planning', priority: 'Medium', currency: 'AED', initialApprovedBudget: 0 }

export default function Projects() {
  const { t } = useTranslation()
  const lk = useLookups()
  const { hasRole } = useAuth()
  const [rows, setRows] = useState<Project[]>([])
  const [total, setTotal] = useState(0)
  const [query, setQuery] = useState({ pageNum: 1, pageSize: 10, keyword: '', deptId: '', ownerUserId: '', status: '' })
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [edit, setEdit] = useState<Partial<Project> | null>(null)
  const [toast, setToast] = useState('')

  const load = () => {
    setLoading(true); setError('')
    const params: Record<string, unknown> = { pageNum: query.pageNum, pageSize: query.pageSize }
    if (query.keyword) params.keyword = query.keyword
    if (query.deptId) params.deptId = query.deptId
    if (query.ownerUserId) params.ownerUserId = query.ownerUserId
    if (query.status) params.status = query.status
    pmo.listProjects(params)
      .then((p) => { setRows(p.result || []); setTotal(p.totalNum || 0) })
      .catch((e) => setError((e as Error).message))
      .finally(() => setLoading(false))
  }
  useEffect(load, [query.pageNum, query.pageSize])

  const save = async () => {
    if (!edit) return
    try {
      if (edit.projectId) await pmo.updateProject(edit)
      else await pmo.createProject(edit)
      setEdit(null); setToast(t('common.saved')); setTimeout(() => setToast(''), 2500); load()
    } catch (e) { setError((e as Error).message) }
  }

  const pages = Math.max(1, Math.ceil(total / query.pageSize))
  return (
    <div>
      <div className="row">
        <h2 style={{ margin: 0 }}>{t('project.title')}</h2>
        <div className="spacer" />
        {hasRole('pm') && <button className="btn primary" onClick={() => setEdit({ ...empty })}>{t('project.newProject')}</button>}
      </div>

      <div className="row panel" style={{ marginTop: 12 }}>
        <input placeholder={t('project.keyword')} value={query.keyword}
          onChange={(e) => setQuery((q) => ({ ...q, keyword: e.target.value }))} />
        <select value={query.deptId} onChange={(e) => setQuery((q) => ({ ...q, deptId: e.target.value }))}>
          <option value="">{t('project.department')}: {t('common.all')}</option>
          {lk.departments.map((d) => <option key={d.id} value={d.id}>{d.label}</option>)}
        </select>
        <select value={query.ownerUserId} onChange={(e) => setQuery((q) => ({ ...q, ownerUserId: e.target.value }))}>
          <option value="">{t('project.owner')}: {t('common.all')}</option>
          {lk.managers.map((m) => <option key={m.id} value={m.id}>{m.label}</option>)}
        </select>
        <select value={query.status} onChange={(e) => setQuery((q) => ({ ...q, status: e.target.value }))}>
          <option value="">{t('project.status')}: {t('common.all')}</option>
          {STATUSES.map((s) => <option key={s} value={s}>{t(`status.${s}`)}</option>)}
        </select>
        <button className="btn primary" onClick={() => setQuery((q) => ({ ...q, pageNum: 1 }))}>{t('common.apply')}</button>
        <button className="btn" onClick={() => setQuery({ pageNum: 1, pageSize: 10, keyword: '', deptId: '', ownerUserId: '', status: '' })}>{t('common.reset')}</button>
      </div>

      {error && <ErrorBox message={error} />}
      {loading ? <Loading /> : rows.length === 0 ? <Empty /> : (
        <div className="table-wrap" style={{ marginTop: 12 }}>
          <table>
            <thead><tr>
              <th>{t('project.code')}</th><th>{t('project.name')}</th><th>{t('project.department')}</th>
              <th>{t('project.owner')}</th><th>{t('project.status')}</th><th>{t('project.priority')}</th>
              <th>{t('project.budget')}</th><th>{t('project.plannedEnd')}</th><th>{t('common.actions')}</th>
            </tr></thead>
            <tbody>{rows.map((p) => (
              <tr key={p.projectId}>
                <td>{p.code}</td><td>{p.name}</td><td>{p.deptName}</td><td>{p.ownerName}</td>
                <td><Badge value={p.status} /></td><td><Badge value={p.priority} /></td>
                <td>{fmtMoney(p.initialApprovedBudget, p.currency)}</td><td>{fmtDate(p.plannedEndDate)}</td>
                <td>{hasRole('pm') && <button className="btn sm" onClick={() => setEdit({ ...p })}>{t('common.edit')}</button>}</td>
              </tr>))}</tbody>
          </table>
        </div>
      )}
      <div className="row" style={{ marginTop: 10 }}>
        <span className="muted">{total} {t('common.of')} · {t('common.page')} {query.pageNum}/{pages}</span>
        <div className="spacer" />
        <button className="btn sm" disabled={query.pageNum <= 1} onClick={() => setQuery((q) => ({ ...q, pageNum: q.pageNum - 1 }))}>‹</button>
        <button className="btn sm" disabled={query.pageNum >= pages} onClick={() => setQuery((q) => ({ ...q, pageNum: q.pageNum + 1 }))}>›</button>
      </div>

      {edit && (
        <Modal title={edit.projectId ? t('project.editProject') : t('project.newProject')} onClose={() => setEdit(null)}
          footer={<>
            <button className="btn" onClick={() => setEdit(null)}>{t('common.cancel')}</button>
            <button className="btn primary" onClick={save}>{t('common.save')}</button>
          </>}>
          <label className="field">{t('project.code')}<input value={edit.code || ''} onChange={(e) => setEdit({ ...edit, code: e.target.value })} /></label>
          <label className="field">{t('project.name')}<input value={edit.name || ''} onChange={(e) => setEdit({ ...edit, name: e.target.value })} /></label>
          <label className="field">{t('project.department')}
            <select value={edit.deptId || ''} onChange={(e) => setEdit({ ...edit, deptId: e.target.value })}>
              <option value="">—</option>{lk.departments.map((d) => <option key={d.id} value={d.id}>{d.label}</option>)}
            </select></label>
          <label className="field">{t('project.owner')}
            <select value={edit.ownerUserId || ''} onChange={(e) => setEdit({ ...edit, ownerUserId: e.target.value })}>
              <option value="">—</option>{lk.managers.map((m) => <option key={m.id} value={m.id}>{m.label}</option>)}
            </select></label>
          <label className="field">{t('project.status')}
            <select value={edit.status} onChange={(e) => setEdit({ ...edit, status: e.target.value })}>
              {STATUSES.map((s) => <option key={s} value={s}>{t(`status.${s}`)}</option>)}
            </select></label>
          <label className="field">{t('project.priority')}
            <select value={edit.priority} onChange={(e) => setEdit({ ...edit, priority: e.target.value })}>
              {PRIORITIES.map((s) => <option key={s} value={s}>{t(`status.${s}`)}</option>)}
            </select></label>
          <label className="field">{t('project.startDate')}<input type="date" value={(edit.startDate || '').substring(0, 10)} onChange={(e) => setEdit({ ...edit, startDate: e.target.value })} /></label>
          <label className="field">{t('project.plannedEnd')}<input type="date" value={(edit.plannedEndDate || '').substring(0, 10)} onChange={(e) => setEdit({ ...edit, plannedEndDate: e.target.value })} /></label>
          <label className="field">{t('project.budget')} ({t('common.currency')})<input type="number" value={edit.initialApprovedBudget ?? 0} onChange={(e) => setEdit({ ...edit, initialApprovedBudget: Number(e.target.value) })} /></label>
          <label className="field full">{t('project.description')}<textarea rows={3} value={edit.description || ''} onChange={(e) => setEdit({ ...edit, description: e.target.value })} /></label>
        </Modal>
      )}
      {toast && <Toast message={toast} />}
    </div>
  )
}

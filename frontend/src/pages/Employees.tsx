import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import * as pmo from '../api/pmo'
import type { Employee } from '../api/types'
import { useAuth } from '../auth/auth'
import { Badge, Empty, ErrorBox, Loading, Modal, Toast, fmtDate } from '../components/ui'
import Select from '../components/Select'

// PMO employee directory over hr_employee_core. Viewing is PM+ (pmo:employee:list); create/edit/
// delete are admin-only (pmo:employee:add/edit/remove) — the backend enforces the same.
export default function Employees() {
  const { t } = useTranslation()
  const { hasPerm } = useAuth()
  const [rows, setRows] = useState<Employee[]>([])
  const [userOpts, setUserOpts] = useState<{ value: string; label: string }[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [edit, setEdit] = useState<Partial<Employee> | null>(null)
  const [toast, setToast] = useState('')
  const notify = (m: string) => { setToast(m); setTimeout(() => setToast(''), 2500) }

  const canAdd = hasPerm('pmo:employee:add')
  const canEdit = hasPerm('pmo:employee:edit')
  const canRemove = hasPerm('pmo:employee:remove')
  const canManage = canAdd || canEdit || canRemove

  const load = () => {
    setLoading(true); setError('')
    pmo.listEmployees({ pageNum: 1, pageSize: 100 })
      .then((p) => setRows(p.result || []))
      .catch((e) => setError((e as Error).message)).finally(() => setLoading(false))
  }
  useEffect(load, [])

  const openEdit = async (e?: Employee) => {
    setEdit(e ? { ...e } : { isActive: true })
    try {
      const unlinked = await pmo.unlinkedUsers()
      const opts = unlinked.map((u) => ({ value: u.id, label: u.name }))
      // When editing a linked employee, keep its current user selectable in the dropdown.
      if (e?.userId && e.userName && !opts.some((o) => o.value === e.userId))
        opts.unshift({ value: e.userId, label: e.userName })
      setUserOpts(opts)
    } catch { setUserOpts([]) }
  }

  const save = async () => {
    if (!edit) return
    try {
      const dto = {
        employeeId: edit.employeeId, userId: edit.userId || null, employeeCode: edit.employeeCode,
        hireDate: edit.hireDate || null, terminationDate: edit.terminationDate || null,
        isActive: edit.isActive ?? true,
      }
      if (edit.employeeId) await pmo.updateEmployee(dto); else await pmo.addEmployee(dto)
      setEdit(null); notify(t('common.saved')); load()
    } catch (e) { setError((e as Error).message) }
  }

  const remove = async (e: Employee) => {
    if (!confirm(`${t('common.delete')}: ${e.employeeCode}?`)) return
    try { await pmo.removeEmployee(e.employeeId); notify(t('common.saved')); load() }
    catch (err) { setError((err as Error).message) }
  }

  return (
    <div>
      <div className="row">
        <h2 style={{ margin: 0 }}>{t('employee.title')}</h2>
        <div className="spacer" />
        {canAdd && <button className="btn primary" onClick={() => openEdit()}>{t('employee.newEmp')}</button>}
      </div>
      {error && <ErrorBox message={error} />}
      {loading ? <Loading /> : rows.length === 0 ? <Empty /> : (
        <div className="table-wrap" style={{ marginTop: 12 }}>
          <table><thead><tr>
            <th>{t('employee.code')}</th><th>{t('employee.user')}</th><th>{t('employee.hire')}</th>
            <th>{t('employee.termination')}</th><th>{t('employee.status')}</th>
            {canManage && <th>{t('common.actions')}</th>}</tr></thead>
            <tbody>{rows.map((e) => (
              <tr key={e.employeeId}>
                <td><code>{e.employeeCode}</code></td>
                <td>{e.userName || <span className="muted">—</span>}</td>
                <td>{fmtDate(e.hireDate)}</td>
                <td>{fmtDate(e.terminationDate)}</td>
                <td><Badge value={e.isActive ? 'Active' : 'Disabled'} /></td>
                {canManage && <td className="row">
                  {canEdit && <button className="btn sm" onClick={() => openEdit(e)}>{t('common.edit')}</button>}
                  {canRemove && <button className="btn sm" onClick={() => remove(e)}>{t('common.delete')}</button>}
                </td>}
              </tr>))}</tbody>
          </table>
        </div>
      )}
      {edit && (
        <Modal title={edit.employeeId ? t('employee.editEmp') : t('employee.newEmp')} onClose={() => setEdit(null)}
          footer={<><button className="btn" onClick={() => setEdit(null)}>{t('common.cancel')}</button>
            <button className="btn primary" onClick={save}>{t('common.save')}</button></>}>
          <label className="field">{t('employee.code')}
            <input value={edit.employeeCode || ''} onChange={(ev) => setEdit({ ...edit, employeeCode: ev.target.value })} /></label>
          <label className="field">{t('employee.user')}
            <Select options={userOpts} value={edit.userId || ''}
              onChange={(v) => setEdit({ ...edit, userId: v || undefined })} placeholder={t('employee.linkUser')} isClearable /></label>
          <label className="field">{t('employee.hire')}
            <input type="date" value={(edit.hireDate || '').substring(0, 10)} onChange={(ev) => setEdit({ ...edit, hireDate: ev.target.value })} /></label>
          <label className="field">{t('employee.termination')}
            <input type="date" value={(edit.terminationDate || '').substring(0, 10)} onChange={(ev) => setEdit({ ...edit, terminationDate: ev.target.value })} /></label>
          <label className="field" style={{ flexDirection: 'row', alignItems: 'center', gap: 8 }}>
            <input type="checkbox" checked={edit.isActive ?? true} onChange={(ev) => setEdit({ ...edit, isActive: ev.target.checked })} />
            {t('employee.active')}</label>
        </Modal>
      )}
      {toast && <Toast message={toast} />}
    </div>
  )
}

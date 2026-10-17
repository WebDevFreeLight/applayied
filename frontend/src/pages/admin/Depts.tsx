import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import * as sys from '../../api/system'
import type { SysDept } from '../../api/system'
import { useAuth } from '../../auth/auth'
import { Empty, ErrorBox, Loading, Modal, Toast } from '../../components/ui'

const P = { list: 'system:dept:list', add: 'system:dept:add', edit: 'system:dept:update', remove: 'system:dept:remove' }

export default function Depts() {
  const { t } = useTranslation()
  const { hasPerm } = useAuth()
  const [rows, setRows] = useState<SysDept[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [edit, setEdit] = useState<Partial<SysDept> | null>(null)
  const [toast, setToast] = useState('')
  const notify = (m: string) => { setToast(m); setTimeout(() => setToast(''), 2500) }

  const load = () => {
    setLoading(true); setError('')
    sys.listDept({}).then((d) => setRows(Array.isArray(d) ? d : []))
      .catch((e) => setError((e as Error).message)).finally(() => setLoading(false))
  }
  useEffect(load, [])
  const save = async () => {
    if (!edit) return
    try { if (edit.deptId) await sys.updateDept(edit); else await sys.addDept(edit); setEdit(null); notify(t('common.saved')); load() }
    catch (e) { setError((e as Error).message) }
  }
  const remove = async (d: SysDept) => {
    if (!confirm(`${t('common.delete')}: ${d.deptName}?`)) return
    try { await sys.removeDept(d.deptId); notify(t('common.saved')); load() } catch (e) { setError((e as Error).message) }
  }

  if (!hasPerm(P.list)) return <div className="panel">{t('admin.noAccess')}</div>
  const canAct = hasPerm(P.edit) || hasPerm(P.remove)
  return (
    <div>
      <div className="row"><h2 style={{ margin: 0 }}>{t('sys.depts')}</h2><div className="spacer" />
        {hasPerm(P.add) && <button className="btn primary" onClick={() => setEdit({ parentId: '0', orderNum: 1, status: '0' })}>{t('common.add')}</button>}
      </div>
      {error && <ErrorBox message={error} />}
      {loading ? <Loading /> : rows.length === 0 ? <Empty /> : (
        <div className="table-wrap" style={{ marginTop: 12 }}>
          <table><thead><tr><th>{t('sys.deptName')}</th><th>{t('sys.leader')}</th><th>{t('admin.orderNum')}</th><th>{t('admin.status')}</th>{canAct && <th>{t('common.actions')}</th>}</tr></thead>
            <tbody>{rows.map((d) => (
              <tr key={d.deptId}><td>{d.deptName}</td><td>{d.leader}</td><td>{d.orderNum}</td>
                <td><span className={`badge ${d.status === '0' ? 'green' : 'gray'}`}>{d.status === '0' ? t('admin.active') : t('admin.disabled')}</span></td>
                {canAct && <td className="row">
                  {hasPerm(P.edit) && <button className="btn sm" onClick={() => setEdit({ ...d })}>{t('common.edit')}</button>}
                  {hasPerm(P.remove) && <button className="btn sm" onClick={() => remove(d)}>{t('common.delete')}</button>}
                </td>}</tr>))}</tbody>
          </table>
        </div>
      )}
      {edit && (
        <Modal title={edit.deptId ? t('common.edit') : t('common.add')} onClose={() => setEdit(null)}
          footer={<><button className="btn" onClick={() => setEdit(null)}>{t('common.cancel')}</button><button className="btn primary" onClick={save}>{t('common.save')}</button></>}>
          <label className="field">{t('sys.deptName')}<input value={edit.deptName || ''} onChange={(e) => setEdit({ ...edit, deptName: e.target.value })} /></label>
          <label className="field">{t('sys.leader')}<input value={edit.leader || ''} onChange={(e) => setEdit({ ...edit, leader: e.target.value })} /></label>
          <label className="field">{t('admin.orderNum')}<input type="number" value={edit.orderNum ?? 1} onChange={(e) => setEdit({ ...edit, orderNum: Number(e.target.value) })} /></label>
          <label className="field">{t('sys.ip')}<input value={edit.phone || ''} onChange={(e) => setEdit({ ...edit, phone: e.target.value })} placeholder="phone" /></label>
        </Modal>
      )}
      {toast && <Toast message={toast} />}
    </div>
  )
}

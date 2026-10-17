import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import * as admin from '../../api/admin'
import type { SysRole, TreeSelect } from '../../api/admin'
import { useAuth } from '../../auth/auth'
import { Empty, ErrorBox, Loading, Modal, Toast } from '../../components/ui'

const P = {
  list: 'system:role:list', add: 'system:role:add', edit: 'system:role:edit',
  authorize: 'system:role:authorize', remove: 'system:role:remove',
}

export default function Roles() {
  const { t } = useTranslation()
  const { hasPerm } = useAuth()
  const [rows, setRows] = useState<SysRole[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [edit, setEdit] = useState<Partial<SysRole> | null>(null)
  const [perm, setPerm] = useState<{ role: SysRole; tree: TreeSelect[]; checked: Set<string> } | null>(null)
  const [toast, setToast] = useState('')
  const notify = (m: string) => { setToast(m); setTimeout(() => setToast(''), 2500) }

  const load = () => {
    setLoading(true); setError('')
    admin.listRoles({ pageNum: 1, pageSize: 100 })
      .then((p) => setRows(p.result || [])).catch((e) => setError((e as Error).message))
      .finally(() => setLoading(false))
  }
  useEffect(load, [])

  const save = async () => {
    if (!edit) return
    try {
      if (edit.roleId) await admin.editRole(edit); else await admin.addRole(edit)
      setEdit(null); notify(t('common.saved')); load()
    } catch (e) { setError((e as Error).message) }
  }
  const remove = async (r: SysRole) => {
    if (!confirm(`${t('common.delete')}: ${r.roleName}?`)) return
    try { await admin.removeRole(r.roleId); notify(t('common.saved')); load() }
    catch (e) { setError((e as Error).message) }
  }
  const openPerms = async (r: SysRole) => {
    try {
      const res = await admin.roleMenuTree(r.roleId)
      setPerm({ role: r, tree: res.menus || [], checked: new Set((res.checkedKeys || []).map(String)) })
    } catch (e) { setError((e as Error).message) }
  }
  const savePerms = async () => {
    if (!perm) return
    try { await admin.authorizeRoleMenus(perm.role.roleId, [...perm.checked]); setPerm(null); notify(t('common.saved')) }
    catch (e) { setError((e as Error).message) }
  }

  if (!hasPerm(P.list)) return <div className="panel">{t('admin.noAccess')}</div>
  const canAct = hasPerm(P.edit) || hasPerm(P.authorize) || hasPerm(P.remove)

  return (
    <div>
      <div className="row">
        <h2 style={{ margin: 0 }}>{t('admin.roles')}</h2>
        <div className="spacer" />
        {hasPerm(P.add) && <button className="btn primary" onClick={() => setEdit({ status: 0, roleSort: 1, dataScope: 1 })}>{t('admin.newRole')}</button>}
      </div>
      {error && <ErrorBox message={error} />}
      {loading ? <Loading /> : rows.length === 0 ? <Empty /> : (
        <div className="table-wrap" style={{ marginTop: 12 }}>
          <table><thead><tr>
            <th>{t('admin.roleName')}</th><th>{t('admin.roleKey')}</th><th>{t('admin.roleSort')}</th>
            <th>{t('admin.status')}</th>{canAct && <th>{t('common.actions')}</th>}</tr></thead>
            <tbody>{rows.map((r) => (
              <tr key={r.roleId}>
                <td>{r.roleName}</td><td><code>{r.roleKey}</code></td><td>{r.roleSort}</td>
                <td><span className={`badge ${r.status === 0 ? 'green' : 'gray'}`}>{r.status === 0 ? t('admin.active') : t('admin.disabled')}</span></td>
                {canAct && <td className="row">
                  {hasPerm(P.edit) && <button className="btn sm" onClick={() => setEdit({ ...r })}>{t('common.edit')}</button>}
                  {hasPerm(P.authorize) && <button className="btn sm" onClick={() => openPerms(r)}>{t('admin.assignPerms')}</button>}
                  {hasPerm(P.remove) && <button className="btn sm" onClick={() => remove(r)}>{t('common.delete')}</button>}
                </td>}
              </tr>))}</tbody>
          </table>
        </div>
      )}

      {edit && (
        <Modal title={edit.roleId ? t('admin.editRole') : t('admin.newRole')} onClose={() => setEdit(null)}
          footer={<><button className="btn" onClick={() => setEdit(null)}>{t('common.cancel')}</button>
            <button className="btn primary" onClick={save}>{t('common.save')}</button></>}>
          <label className="field">{t('admin.roleName')}<input value={edit.roleName || ''}
            onChange={(e) => setEdit({ ...edit, roleName: e.target.value })} /></label>
          <label className="field">{t('admin.roleKey')}<input value={edit.roleKey || ''}
            onChange={(e) => setEdit({ ...edit, roleKey: e.target.value })} /></label>
          <label className="field">{t('admin.roleSort')}<input type="number" value={edit.roleSort ?? 1}
            onChange={(e) => setEdit({ ...edit, roleSort: Number(e.target.value) })} /></label>
          <label className="field full">{t('common.actions')}<textarea rows={2} value={edit.remark || ''}
            onChange={(e) => setEdit({ ...edit, remark: e.target.value })} /></label>
        </Modal>
      )}

      {perm && (
        <Modal title={`${t('admin.assignPerms')} — ${perm.role.roleName}`} onClose={() => setPerm(null)}
          footer={<><button className="btn" onClick={() => setPerm(null)}>{t('common.cancel')}</button>
            <button className="btn primary" onClick={savePerms}>{t('common.save')}</button></>}>
          <div className="full" style={{ maxHeight: '55vh', overflow: 'auto' }}>
            <PermTree nodes={perm.tree} checked={perm.checked}
              onToggle={(id, on) => {
                const next = new Set(perm.checked)
                on ? next.add(id) : next.delete(id)
                setPerm({ ...perm, checked: next })
              }} />
          </div>
        </Modal>
      )}
      {toast && <Toast message={toast} />}
    </div>
  )
}

/** Recursive checkbox tree for the role's menu/permission assignment. */
function PermTree({ nodes, checked, onToggle }:
  { nodes: TreeSelect[]; checked: Set<string>; onToggle: (id: string, on: boolean) => void }) {
  return (
    <ul style={{ listStyle: 'none', paddingInlineStart: 16, margin: 0 }}>
      {nodes.map((n) => (
        <li key={n.id} style={{ padding: '2px 0' }}>
          <label className="row" style={{ gap: 6 }}>
            <input type="checkbox" checked={checked.has(String(n.id))}
              onChange={(e) => onToggle(String(n.id), e.target.checked)} />
            {n.label}
          </label>
          {n.children && n.children.length > 0 && (
            <PermTree nodes={n.children} checked={checked} onToggle={onToggle} />
          )}
        </li>
      ))}
    </ul>
  )
}

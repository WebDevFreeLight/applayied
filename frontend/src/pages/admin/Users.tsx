import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import * as admin from '../../api/admin'
import type { SysUser, SysRole } from '../../api/admin'
import { useAuth } from '../../auth/auth'
import { Empty, ErrorBox, Loading, Modal, Toast } from '../../components/ui'
import Select from '../../components/Select'

// Every mutating control is gated by the exact backend permission the route enforces,
// so the UI shows only what this user is actually allowed to do.
const P = {
  list: 'system:user:list', add: 'system:user:add', edit: 'system:user:edit',
  remove: 'system:user:remove', resetPwd: 'system:user:resetPwd', status: 'system:user:update',
}

export default function Users() {
  const { t } = useTranslation()
  const { hasPerm } = useAuth()
  const [rows, setRows] = useState<SysUser[]>([])
  const [roles, setRoles] = useState<SysRole[]>([])
  const [total, setTotal] = useState(0)
  const [query, setQuery] = useState({ pageNum: 1, pageSize: 10, userName: '', status: '' })
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [edit, setEdit] = useState<(Partial<SysUser> & { password?: string; roleIds?: string[] }) | null>(null)
  const [pwd, setPwd] = useState<{ user: SysUser; value: string } | null>(null)
  const [toast, setToast] = useState('')
  const notify = (m: string) => { setToast(m); setTimeout(() => setToast(''), 2500) }

  const load = () => {
    setLoading(true); setError('')
    const params: Record<string, unknown> = { pageNum: query.pageNum, pageSize: query.pageSize }
    if (query.userName) params.userName = query.userName
    if (query.status !== '') params.status = query.status
    admin.listUsers(params)
      .then((p) => { setRows(p.result || []); setTotal(p.totalNum || 0) })
      .catch((e) => setError((e as Error).message)).finally(() => setLoading(false))
  }
  useEffect(load, [query.pageNum])
  useEffect(() => { admin.allRoles().then((r) => setRoles(r.result || [])).catch(() => {}) }, [])

  const openEdit = async (u?: SysUser) => {
    if (!u) { setEdit({ status: 0, roleIds: [] }); return }
    try {
      const d = await admin.getUser(u.userId)
      const roleIds = (d.roleIds || (d as { roleIds?: string[] }).roleIds || []).map(String)
      setEdit({ ...u, roleIds })
    } catch { setEdit({ ...u, roleIds: [] }) }
  }
  const save = async () => {
    if (!edit) return
    try {
      if (edit.userId) await admin.editUser(edit); else await admin.addUser(edit)
      setEdit(null); notify(t('common.saved')); load()
    } catch (e) { setError((e as Error).message) }
  }
  const toggleStatus = async (u: SysUser) => {
    try { await admin.changeUserStatus(u.userId, u.status === 0 ? 1 : 0); notify(t('common.saved')); load() }
    catch (e) { setError((e as Error).message) }
  }
  const remove = async (u: SysUser) => {
    if (!confirm(`${t('common.delete')}: ${u.userName}?`)) return
    try { await admin.removeUser(u.userId); notify(t('common.saved')); load() }
    catch (e) { setError((e as Error).message) }
  }
  const doReset = async () => {
    if (!pwd) return
    try { await admin.resetUserPwd(pwd.user.userId, pwd.value); setPwd(null); notify(t('common.saved')) }
    catch (e) { setError((e as Error).message) }
  }

  if (!hasPerm(P.list)) return <div className="panel">{t('admin.noAccess')}</div>
  const pages = Math.max(1, Math.ceil(total / query.pageSize))
  const canAct = hasPerm(P.edit) || hasPerm(P.remove) || hasPerm(P.resetPwd) || hasPerm(P.status)

  return (
    <div>
      <div className="row">
        <h2 style={{ margin: 0 }}>{t('admin.users')}</h2>
        <div className="spacer" />
        {hasPerm(P.add) && <button className="btn primary" onClick={() => openEdit()}>{t('admin.newUser')}</button>}
      </div>

      <div className="row panel" style={{ marginTop: 12 }}>
        <input placeholder={t('admin.userName')} value={query.userName}
          onChange={(e) => setQuery((q) => ({ ...q, userName: e.target.value }))} />
        <div style={{ minWidth: 160 }}><Select
          options={[{ value: '0', label: t('admin.active') }, { value: '1', label: t('admin.disabled') }]}
          value={query.status} onChange={(v) => setQuery((q) => ({ ...q, status: v || '' }))}
          placeholder={t('admin.status')} /></div>
        <button className="btn primary" onClick={() => setQuery((q) => ({ ...q, pageNum: 1 }))}>{t('common.apply')}</button>
      </div>

      {error && <ErrorBox message={error} />}
      {loading ? <Loading /> : rows.length === 0 ? <Empty /> : (
        <div className="table-wrap" style={{ marginTop: 12 }}>
          <table><thead><tr>
            <th>{t('admin.userName')}</th><th>{t('admin.nickName')}</th><th>{t('admin.email')}</th>
            <th>{t('admin.status')}</th>{canAct && <th>{t('common.actions')}</th>}</tr></thead>
            <tbody>{rows.map((u) => (
              <tr key={u.userId}>
                <td>{u.userName}</td><td>{u.nickName}</td><td>{u.email}</td>
                <td><span className={`badge ${u.status === 0 ? 'green' : 'gray'}`}>{u.status === 0 ? t('admin.active') : t('admin.disabled')}</span></td>
                {canAct && <td className="row">
                  {hasPerm(P.edit) && <button className="btn sm" onClick={() => openEdit(u)}>{t('common.edit')}</button>}
                  {hasPerm(P.status) && <button className="btn sm" onClick={() => toggleStatus(u)}>{u.status === 0 ? t('admin.disabled') : t('admin.active')}</button>}
                  {hasPerm(P.resetPwd) && <button className="btn sm" onClick={() => setPwd({ user: u, value: '' })}>{t('admin.resetPwd')}</button>}
                  {hasPerm(P.remove) && <button className="btn sm" onClick={() => remove(u)}>{t('common.delete')}</button>}
                </td>}
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

      {edit && (
        <Modal title={edit.userId ? t('admin.editUser') : t('admin.newUser')} onClose={() => setEdit(null)}
          footer={<><button className="btn" onClick={() => setEdit(null)}>{t('common.cancel')}</button>
            <button className="btn primary" onClick={save}>{t('common.save')}</button></>}>
          <label className="field">{t('admin.userName')}<input value={edit.userName || ''} disabled={!!edit.userId}
            onChange={(e) => setEdit({ ...edit, userName: e.target.value })} /></label>
          <label className="field">{t('admin.nickName')}<input value={edit.nickName || ''}
            onChange={(e) => setEdit({ ...edit, nickName: e.target.value })} /></label>
          <label className="field">{t('admin.email')}<input value={edit.email || ''}
            onChange={(e) => setEdit({ ...edit, email: e.target.value })} /></label>
          <label className="field">{t('admin.phone')}<input value={edit.phonenumber || ''}
            onChange={(e) => setEdit({ ...edit, phonenumber: e.target.value })} /></label>
          {!edit.userId && <label className="field">{t('auth.password')}<input type="password" value={edit.password || ''}
            onChange={(e) => setEdit({ ...edit, password: e.target.value })} /></label>}
          <div className="field full">{t('admin.assignRoles')}
            <div className="row" style={{ gap: 12, marginTop: 4 }}>
              {roles.map((r) => (
                <label key={r.roleId} className="row" style={{ gap: 4 }}>
                  <input type="checkbox" checked={(edit.roleIds || []).includes(String(r.roleId))}
                    onChange={(e) => {
                      const set = new Set(edit.roleIds || [])
                      e.target.checked ? set.add(String(r.roleId)) : set.delete(String(r.roleId))
                      setEdit({ ...edit, roleIds: [...set] })
                    }} />
                  {r.roleName}
                </label>))}
            </div>
          </div>
        </Modal>
      )}

      {pwd && (
        <Modal title={`${t('admin.resetPwd')} — ${pwd.user.userName}`} onClose={() => setPwd(null)}
          footer={<><button className="btn" onClick={() => setPwd(null)}>{t('common.cancel')}</button>
            <button className="btn primary" onClick={doReset}>{t('common.save')}</button></>}>
          <label className="field full">{t('admin.newPwd')}<input type="password" value={pwd.value}
            onChange={(e) => setPwd({ ...pwd, value: e.target.value })} /></label>
        </Modal>
      )}
      {toast && <Toast message={toast} />}
    </div>
  )
}

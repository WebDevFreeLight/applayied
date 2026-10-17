import { useEffect, useMemo, useState } from 'react'
import { useTranslation } from 'react-i18next'
import * as admin from '../../api/admin'
import type { SysMenu } from '../../api/admin'
import { useAuth } from '../../auth/auth'
import { Empty, ErrorBox, Loading, Modal, Toast } from '../../components/ui'
import Select from '../../components/Select'

const P = {
  list: 'system:menu:list', add: 'system:menu:add', edit: 'system:menu:edit',
  remove: 'system:menu:remove', sync: 'system:menu:sync',
}
const TYPES = [
  { value: 'M', key: 'dir' }, { value: 'C', key: 'menu' }, { value: 'F', key: 'button' },
]

export default function Menus() {
  const { t } = useTranslation()
  const { hasPerm, hasRole } = useAuth()
  const [rows, setRows] = useState<SysMenu[]>([])
  const [filter, setFilter] = useState('')
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [busy, setBusy] = useState('')
  const [edit, setEdit] = useState<Partial<SysMenu> | null>(null)
  const [toast, setToast] = useState('')
  const notify = (m: string) => { setToast(m); setTimeout(() => setToast(''), 2500) }

  const load = () => {
    setLoading(true); setError('')
    admin.listMenus().then((m) => setRows(Array.isArray(m) ? m : []))
      .catch((e) => setError((e as Error).message)).finally(() => setLoading(false))
  }
  useEffect(load, [])

  const typeLabel = (v?: string) => t(`admin.${TYPES.find((x) => x.value === v)?.key || 'menu'}`)
  const flat = useMemo(() => {
    // The backend may return a tree; flatten to a table for a simple permission overview.
    const out: SysMenu[] = []
    const walk = (list: SysMenu[]) => list.forEach((m) => { out.push(m); if (m.children) walk(m.children) })
    walk(rows)
    return out.filter((m) => !filter || (m.menuName?.includes(filter) || m.perms?.includes(filter)))
  }, [rows, filter])

  const save = async () => {
    if (!edit) return
    try {
      if (edit.menuId) await admin.editMenu(edit); else await admin.addMenu(edit)
      setEdit(null); notify(t('common.saved')); load()
    } catch (e) { setError((e as Error).message) }
  }
  const remove = async (m: SysMenu) => {
    if (!confirm(`${t('common.delete')}: ${m.menuName}?`)) return
    try { await admin.removeMenu(m.menuId); notify(t('common.saved')); load() }
    catch (e) { setError((e as Error).message) }
  }
  const sync = async () => {
    setBusy('sync')
    try { await admin.syncPermissions(); notify(t('admin.syncDone')); load() }
    catch (e) { setError((e as Error).message) } finally { setBusy('') }
  }
  const setup = async () => {
    setBusy('setup')
    try { await admin.setupRbac(); notify(t('admin.syncDone')); load() }
    catch (e) { setError((e as Error).message) } finally { setBusy('') }
  }

  if (!hasPerm(P.list)) return <div className="panel">{t('admin.noAccess')}</div>
  const canAct = hasPerm(P.edit) || hasPerm(P.remove)

  return (
    <div>
      <div className="row">
        <h2 style={{ margin: 0 }}>{t('admin.menus')}</h2>
        <div className="spacer" />
        {hasPerm(P.sync) && <button className="btn" disabled={busy === 'sync'} onClick={sync}>{t('admin.syncPerms')}</button>}
        {hasRole('admin') && <button className="btn" disabled={busy === 'setup'} onClick={setup}>{t('admin.setupRbac')}</button>}
        {hasPerm(P.add) && <button className="btn primary" onClick={() => setEdit({ menuType: 'F', parentId: '0', orderNum: 1 })}>{t('admin.newMenu')}</button>}
      </div>

      <div className="row panel" style={{ marginTop: 12 }}>
        <input placeholder={`${t('admin.menuName')} / ${t('admin.perm')}`} value={filter}
          onChange={(e) => setFilter(e.target.value)} />
        <span className="muted">{flat.length} {t('admin.permissions')}</span>
      </div>

      {error && <ErrorBox message={error} />}
      {loading ? <Loading /> : flat.length === 0 ? <Empty /> : (
        <div className="table-wrap" style={{ marginTop: 12 }}>
          <table><thead><tr>
            <th>{t('admin.menuName')}</th><th>{t('admin.menuType')}</th><th>{t('admin.perm')}</th>
            <th>{t('admin.orderNum')}</th>{canAct && <th>{t('common.actions')}</th>}</tr></thead>
            <tbody>{flat.map((m) => (
              <tr key={m.menuId}>
                <td>{m.menuName}</td>
                <td><span className="badge blue">{typeLabel(m.menuType)}</span></td>
                <td>{m.perms ? <code>{m.perms}</code> : <span className="muted">—</span>}</td>
                <td>{m.orderNum}</td>
                {canAct && <td className="row">
                  {hasPerm(P.edit) && <button className="btn sm" onClick={() => setEdit({ ...m })}>{t('common.edit')}</button>}
                  {hasPerm(P.remove) && <button className="btn sm" onClick={() => remove(m)}>{t('common.delete')}</button>}
                </td>}
              </tr>))}</tbody>
          </table>
        </div>
      )}

      {edit && (
        <Modal title={edit.menuId ? t('admin.editMenu') : t('admin.newMenu')} onClose={() => setEdit(null)}
          footer={<><button className="btn" onClick={() => setEdit(null)}>{t('common.cancel')}</button>
            <button className="btn primary" onClick={save}>{t('common.save')}</button></>}>
          <label className="field">{t('admin.menuName')}<input value={edit.menuName || ''}
            onChange={(e) => setEdit({ ...edit, menuName: e.target.value })} /></label>
          <label className="field">{t('admin.menuType')}
            <Select isClearable={false} options={TYPES.map((x) => ({ value: x.value, label: t(`admin.${x.key}`) }))}
              value={edit.menuType} onChange={(v) => setEdit({ ...edit, menuType: v || 'F' })} /></label>
          <label className="field">{t('admin.perm')}<input value={edit.perms || ''}
            onChange={(e) => setEdit({ ...edit, perms: e.target.value })} placeholder="system:user:add" /></label>
          <label className="field">{t('admin.orderNum')}<input type="number" value={edit.orderNum ?? 1}
            onChange={(e) => setEdit({ ...edit, orderNum: Number(e.target.value) })} /></label>
          <label className="field">{t('admin.path')}<input value={edit.path || ''}
            onChange={(e) => setEdit({ ...edit, path: e.target.value })} /></label>
          <label className="field">{t('admin.icon')}<input value={edit.icon || ''}
            onChange={(e) => setEdit({ ...edit, icon: e.target.value })} /></label>
        </Modal>
      )}
      {toast && <Toast message={toast} />}
    </div>
  )
}

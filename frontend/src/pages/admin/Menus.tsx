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
const TYPES = [{ value: 'M', key: 'dir' }, { value: 'C', key: 'menu' }, { value: 'F', key: 'button' }]

export default function Menus() {
  const { t } = useTranslation()
  const { hasPerm, hasRole } = useAuth()
  const [tree, setTree] = useState<SysMenu[]>([])
  const [filter, setFilter] = useState('')
  const [expanded, setExpanded] = useState<Set<string>>(new Set())
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [busy, setBusy] = useState('')
  const [edit, setEdit] = useState<Partial<SysMenu> | null>(null)
  const [toast, setToast] = useState('')
  const notify = (m: string) => { setToast(m); setTimeout(() => setToast(''), 2500) }

  const load = () => {
    setLoading(true); setError('')
    admin.treeMenus().then((m) => setTree(Array.isArray(m) ? m : []))
      .catch((e) => setError((e as Error).message)).finally(() => setLoading(false))
  }
  useEffect(load, [])

  const typeLabel = (v?: string) => t(`admin.${TYPES.find((x) => x.value === v)?.key || 'menu'}`)
  const typeColor = (v?: string) => (v === 'M' ? 'blue' : v === 'C' ? 'green' : 'gray')

  // Filter the tree, keeping ancestors of any matching node and auto-expanding them.
  const { shown, autoOpen } = useMemo(() => {
    const open = new Set<string>()
    const q = filter.trim().toLowerCase()
    const match = (m: SysMenu) => !q || (m.menuName?.toLowerCase().includes(q) || m.perms?.toLowerCase().includes(q))
    const walk = (nodes: SysMenu[]): SysMenu[] => nodes.map((n) => {
      const kids = n.children ? walk(n.children) : []
      if (match(n) || kids.length) { if (kids.length) open.add(String(n.menuId)); return { ...n, children: kids } }
      return null
    }).filter(Boolean) as SysMenu[]
    return { shown: q ? walk(tree) : tree, autoOpen: open }
  }, [tree, filter])

  const isOpen = (id: string) => (filter ? autoOpen.has(id) : expanded.has(id))
  const toggle = (id: string) => setExpanded((s) => { const n = new Set(s); n.has(id) ? n.delete(id) : n.add(id); return n })

  const save = async () => {
    if (!edit) return
    try { if (edit.menuId) await admin.editMenu(edit); else await admin.addMenu(edit); setEdit(null); notify(t('common.saved')); load() }
    catch (e) { setError((e as Error).message) }
  }
  const remove = async (m: SysMenu) => {
    if (!confirm(`${t('common.delete')}: ${m.menuName}?`)) return
    try { await admin.removeMenu(m.menuId); notify(t('common.saved')); load() } catch (e) { setError((e as Error).message) }
  }
  const sync = async () => { setBusy('sync'); try { await admin.syncPermissions(); notify(t('admin.syncDone')); load() } catch (e) { setError((e as Error).message) } finally { setBusy('') } }
  const setup = async () => { setBusy('setup'); try { await admin.setupRbac(); notify(t('admin.syncDone')); load() } catch (e) { setError((e as Error).message) } finally { setBusy('') } }

  if (!hasPerm(P.list)) return <div className="panel">{t('admin.noAccess')}</div>
  const canAct = hasPerm(P.edit) || hasPerm(P.remove) || hasPerm(P.add)

  // Recursive rows: indentation + expand toggle per node.
  const renderRows = (nodes: SysMenu[], depth: number): React.ReactNode[] =>
    nodes.flatMap((m) => {
      const id = String(m.menuId)
      const kids = m.children || []
      const open = isOpen(id)
      const rows: React.ReactNode[] = [
        <tr key={id}>
          <td>
            <span style={{ paddingInlineStart: depth * 20, display: 'inline-flex', alignItems: 'center', gap: 6 }}>
              {kids.length > 0
                ? <button className="btn sm" style={{ padding: '0 6px' }} onClick={() => toggle(id)}>{open ? '▾' : '▸'}</button>
                : <span style={{ display: 'inline-block', width: 20 }} />}
              {m.menuName}
            </span>
          </td>
          <td><span className={`badge ${typeColor(m.menuType)}`}>{typeLabel(m.menuType)}</span></td>
          <td>{m.perms ? <code>{m.perms}</code> : <span className="muted">—</span>}</td>
          <td>{m.orderNum}</td>
          {canAct && <td className="row">
            {hasPerm(P.add) && m.menuType !== 'F' && <button className="btn sm" title={t('admin.newMenu')}
              onClick={() => setEdit({ menuType: 'C', parentId: id, orderNum: 1 })}>＋</button>}
            {hasPerm(P.edit) && <button className="btn sm" onClick={() => setEdit({ ...m, children: undefined })}>{t('common.edit')}</button>}
            {hasPerm(P.remove) && <button className="btn sm" onClick={() => remove(m)}>{t('common.delete')}</button>}
          </td>}
        </tr>,
      ]
      if (open && kids.length) rows.push(...renderRows(kids, depth + 1))
      return rows
    })

  return (
    <div>
      <div className="row">
        <h2 style={{ margin: 0 }}>{t('admin.menus')}</h2>
        <div className="spacer" />
        {hasPerm(P.sync) && <button className="btn" disabled={busy === 'sync'} onClick={sync}>{busy === 'sync' ? '…' : t('admin.syncPerms')}</button>}
        {hasRole('admin') && <button className="btn" disabled={busy === 'setup'} onClick={setup}>{busy === 'setup' ? '…' : t('admin.setupRbac')}</button>}
        {hasPerm(P.add) && <button className="btn primary" onClick={() => setEdit({ menuType: 'M', parentId: '0', orderNum: 1 })}>{t('admin.newMenu')}</button>}
      </div>

      <div className="row panel" style={{ marginTop: 12 }}>
        <input placeholder={`${t('admin.menuName')} / ${t('admin.perm')}`} value={filter} onChange={(e) => setFilter(e.target.value)} style={{ minWidth: 260 }} />
        <button className="btn sm" onClick={() => setExpanded(new Set(allParentIds(tree)))}>⊞ {t('common.all')}</button>
        <button className="btn sm" onClick={() => setExpanded(new Set())}>⊟</button>
      </div>

      {error && <ErrorBox message={error} />}
      {loading ? <Loading /> : shown.length === 0 ? <Empty /> : (
        <div className="table-wrap" style={{ marginTop: 12 }}>
          <table><thead><tr>
            <th>{t('admin.menuName')}</th><th>{t('admin.menuType')}</th><th>{t('admin.perm')}</th>
            <th>{t('admin.orderNum')}</th>{canAct && <th>{t('common.actions')}</th>}</tr></thead>
            <tbody>{renderRows(shown, 0)}</tbody>
          </table>
        </div>
      )}

      {edit && (
        <Modal title={edit.menuId ? t('admin.editMenu') : t('admin.newMenu')} onClose={() => setEdit(null)}
          footer={<><button className="btn" onClick={() => setEdit(null)}>{t('common.cancel')}</button>
            <button className="btn primary" onClick={save}>{t('common.save')}</button></>}>
          <label className="field">{t('admin.menuName')}<input value={edit.menuName || ''} onChange={(e) => setEdit({ ...edit, menuName: e.target.value })} /></label>
          <label className="field">{t('admin.menuType')}
            <Select isClearable={false} options={TYPES.map((x) => ({ value: x.value, label: t(`admin.${x.key}`) }))}
              value={edit.menuType} onChange={(v) => setEdit({ ...edit, menuType: v || 'F' })} /></label>
          <label className="field">{t('admin.perm')}<input value={edit.perms || ''} onChange={(e) => setEdit({ ...edit, perms: e.target.value })} placeholder="system:user:add" /></label>
          <label className="field">{t('admin.orderNum')}<input type="number" value={edit.orderNum ?? 1} onChange={(e) => setEdit({ ...edit, orderNum: Number(e.target.value) })} /></label>
          <label className="field">{t('admin.path')}<input value={edit.path || ''} onChange={(e) => setEdit({ ...edit, path: e.target.value })} /></label>
          <label className="field">{t('admin.icon')}<input value={edit.icon || ''} onChange={(e) => setEdit({ ...edit, icon: e.target.value })} /></label>
        </Modal>
      )}
      {toast && <Toast message={toast} />}
    </div>
  )
}

function allParentIds(nodes: SysMenu[]): string[] {
  const out: string[] = []
  const walk = (list: SysMenu[]) => list.forEach((n) => { if (n.children?.length) { out.push(String(n.menuId)); walk(n.children) } })
  walk(nodes)
  return out
}

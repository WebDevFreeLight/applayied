import { useEffect, useRef, useState } from 'react'
import { useTranslation } from 'react-i18next'
import * as admin from '../../api/admin'
import type { SysRole, TreeSelect } from '../../api/admin'
import { useAuth } from '../../auth/auth'
import { Empty, ErrorBox, Loading, Modal, Toast } from '../../components/ui'

const P = {
  list: 'system:role:list', add: 'system:role:add', edit: 'system:role:edit',
  authorize: 'system:role:authorize', remove: 'system:role:remove',
}

// Every id in a node's subtree (self + descendants) — drives the parent↔child cascade.
const subtreeIds = (n: TreeSelect): string[] =>
  [String(n.id), ...((n.children || []).flatMap(subtreeIds))]

// The backend tree builder can emit the same menu under several parents (duplicate ids);
// keep the first occurrence of each id so React keys stay unique and checkboxes behave.
function dedupeTree(nodes: TreeSelect[], seen = new Set<string>()): TreeSelect[] {
  const out: TreeSelect[] = []
  for (const n of nodes) {
    const id = String(n.id)
    if (seen.has(id)) continue
    seen.add(id)
    out.push({ ...n, children: n.children ? dedupeTree(n.children, seen) : undefined })
  }
  return out
}

export default function Roles() {
  const { t } = useTranslation()
  const { hasPerm } = useAuth()
  const [rows, setRows] = useState<SysRole[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [edit, setEdit] = useState<Partial<SysRole> | null>(null)
  const [perm, setPerm] = useState<{ role: SysRole; tree: TreeSelect[]; checked: Set<string> } | null>(null)
  // Broadcast an expand/collapse-all command to every tree node (n forces the effect to re-run).
  const [openSignal, setOpenSignal] = useState<{ v: boolean; n: number }>({ v: false, n: 0 })
  const expandAll = (v: boolean) => setOpenSignal((s) => ({ v, n: s.n + 1 }))
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
  const toggleStatus = async (r: SysRole) => {
    try { await admin.changeRoleStatus(r.roleId, r.status === 0 ? 1 : 0); notify(t('common.saved')); load() }
    catch (e) { setError((e as Error).message) }
  }
  const openPerms = async (r: SysRole) => {
    try {
      const res = await admin.roleMenuTree(r.roleId)
      setPerm({ role: r, tree: dedupeTree(res.menus || []), checked: new Set((res.checkedKeys || []).map(String)) })
    } catch (e) { setError((e as Error).message) }
  }
  const setMany = (ids: string[], on: boolean) => setPerm((p) => {
    if (!p) return p
    const next = new Set(p.checked)
    ids.forEach((id) => (on ? next.add(id) : next.delete(id)))
    return { ...p, checked: next }
  })
  const setAll = (on: boolean) => setPerm((p) => p && ({ ...p, checked: new Set(on ? p.tree.flatMap(subtreeIds) : []) }))
  const savePerms = async () => {
    if (!perm) return
    try { await admin.authorizeRoleMenus(perm.role.roleId, [...perm.checked]); setPerm(null); notify(t('common.saved')); load() }
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
                  {hasPerm(P.edit) && <button className="btn sm" onClick={() => toggleStatus(r)}>{r.status === 0 ? t('admin.disabled') : t('admin.active')}</button>}
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
          <label className="field full">{t('sys.content')}
            <textarea rows={2} value={edit.remark || ''} placeholder={t('common.none')}
              onChange={(e) => setEdit({ ...edit, remark: e.target.value })} /></label>
        </Modal>
      )}

      {perm && (
        <Modal title={`${t('admin.assignPerms')} — ${perm.role.roleName}`} onClose={() => setPerm(null)}
          footer={<><button className="btn" onClick={() => setPerm(null)}>{t('common.cancel')}</button>
            <button className="btn primary" onClick={savePerms}>{t('common.save')}</button></>}>
          <div className="full">
            <div className="row" style={{ marginBottom: 8 }}>
              <button className="btn sm" onClick={() => setAll(true)}>⊞ {t('common.all')}</button>
              <button className="btn sm" onClick={() => setAll(false)}>⊟ {t('common.none')}</button>
              <button className="btn sm" onClick={() => expandAll(true)}>▾ {t('admin.expandAll')}</button>
              <button className="btn sm" onClick={() => expandAll(false)}>▸ {t('admin.collapseAll')}</button>
              <span className="muted">· {perm.checked.size} {t('admin.permissions')}</span>
            </div>
            <div style={{ maxHeight: '52vh', overflow: 'auto', border: '1px solid var(--border)', borderRadius: 8, padding: 8 }}>
              <PermTree nodes={perm.tree} checked={perm.checked} setMany={setMany} openSignal={openSignal} />
            </div>
          </div>
        </Modal>
      )}
      {toast && <Toast message={toast} />}
    </div>
  )
}

type OpenSignal = { v: boolean; n: number }

/** Recursive checkbox tree with parent↔child cascade, indeterminate parents, and collapse/expand. */
function PermTree({ nodes, checked, setMany, openSignal }:
  { nodes: TreeSelect[]; checked: Set<string>; setMany: (ids: string[], on: boolean) => void; openSignal: OpenSignal }) {
  return (
    <ul style={{ listStyle: 'none', paddingInlineStart: 16, margin: 0 }}>
      {nodes.map((n) => <PermNode key={n.id} node={n} checked={checked} setMany={setMany} openSignal={openSignal} />)}
    </ul>
  )
}

function PermNode({ node, checked, setMany, openSignal }:
  { node: TreeSelect; checked: Set<string>; setMany: (ids: string[], on: boolean) => void; openSignal: OpenSignal }) {
  const ids = subtreeIds(node)
  const checkedCount = ids.filter((id) => checked.has(id)).length
  const all = checkedCount === ids.length
  const some = checkedCount > 0 && !all
  const ref = useRef<HTMLInputElement>(null)
  if (ref.current) ref.current.indeterminate = some
  const hasKids = !!(node.children && node.children.length > 0)
  const [open, setOpen] = useState(false)
  // Apply a global expand/collapse-all command (only to nodes that have children).
  useEffect(() => { if (hasKids) setOpen(openSignal.v) }, [openSignal, hasKids])

  return (
    <li style={{ padding: '2px 0' }}>
      <div className="row" style={{ gap: 4, alignItems: 'center' }}>
        {hasKids
          ? <button type="button" className="tree-caret" aria-label={open ? 'collapse' : 'expand'}
              onClick={() => setOpen((o) => !o)}>{open ? '▾' : '▸'}</button>
          : <span className="tree-caret" style={{ visibility: 'hidden' }}>•</span>}
        <label className="row" style={{ gap: 6, margin: 0 }}>
          <input ref={ref} type="checkbox" checked={all} onChange={(e) => setMany(ids, e.target.checked)} />
          {node.label}
          {hasKids && <span className="muted" style={{ fontSize: 11 }}>({checkedCount}/{ids.length - 1})</span>}
        </label>
      </div>
      {hasKids && open && (
        <PermTree nodes={node.children || []} checked={checked} setMany={setMany} openSignal={openSignal} />
      )}
    </li>
  )
}

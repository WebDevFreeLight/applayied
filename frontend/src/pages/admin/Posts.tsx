import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import * as sys from '../../api/system'
import type { SysPost } from '../../api/system'
import { useAuth } from '../../auth/auth'
import { Empty, ErrorBox, Loading, Modal, Toast } from '../../components/ui'

const P = { list: 'system:post:list', add: 'system:post:add', edit: 'system:post:edit', remove: 'system:post:remove' }

export default function Posts() {
  const { t } = useTranslation()
  const { hasPerm } = useAuth()
  const [rows, setRows] = useState<SysPost[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [edit, setEdit] = useState<Partial<SysPost> | null>(null)
  const [toast, setToast] = useState('')
  const notify = (m: string) => { setToast(m); setTimeout(() => setToast(''), 2500) }

  const load = () => {
    setLoading(true); setError('')
    sys.listPost({ pageNum: 1, pageSize: 100 }).then((p) => setRows(p.result || []))
      .catch((e) => setError((e as Error).message)).finally(() => setLoading(false))
  }
  useEffect(load, [])
  const save = async () => {
    if (!edit) return
    try { if (edit.postId) await sys.updatePost(edit); else await sys.addPost(edit); setEdit(null); notify(t('common.saved')); load() }
    catch (e) { setError((e as Error).message) }
  }
  const remove = async (p: SysPost) => {
    if (!confirm(`${t('common.delete')}: ${p.postName}?`)) return
    try { await sys.removePost(p.postId); notify(t('common.saved')); load() } catch (e) { setError((e as Error).message) }
  }

  if (!hasPerm(P.list)) return <div className="panel">{t('admin.noAccess')}</div>
  const canAct = hasPerm(P.edit) || hasPerm(P.remove)
  return (
    <div>
      <div className="row"><h2 style={{ margin: 0 }}>{t('sys.posts')}</h2><div className="spacer" />
        {hasPerm(P.add) && <button className="btn primary" onClick={() => setEdit({ postSort: 1, status: '0' })}>{t('common.add')}</button>}
      </div>
      {error && <ErrorBox message={error} />}
      {loading ? <Loading /> : rows.length === 0 ? <Empty /> : (
        <div className="table-wrap" style={{ marginTop: 12 }}>
          <table><thead><tr><th>{t('sys.postCode')}</th><th>{t('sys.postName')}</th><th>{t('admin.orderNum')}</th><th>{t('admin.status')}</th>{canAct && <th>{t('common.actions')}</th>}</tr></thead>
            <tbody>{rows.map((p) => (
              <tr key={p.postId}><td><code>{p.postCode}</code></td><td>{p.postName}</td><td>{p.postSort}</td>
                <td><span className={`badge ${p.status === '0' ? 'green' : 'gray'}`}>{p.status === '0' ? t('admin.active') : t('admin.disabled')}</span></td>
                {canAct && <td className="row">
                  {hasPerm(P.edit) && <button className="btn sm" onClick={() => setEdit({ ...p })}>{t('common.edit')}</button>}
                  {hasPerm(P.remove) && <button className="btn sm" onClick={() => remove(p)}>{t('common.delete')}</button>}
                </td>}</tr>))}</tbody>
          </table>
        </div>
      )}
      {edit && (
        <Modal title={edit.postId ? t('common.edit') : t('common.add')} onClose={() => setEdit(null)}
          footer={<><button className="btn" onClick={() => setEdit(null)}>{t('common.cancel')}</button><button className="btn primary" onClick={save}>{t('common.save')}</button></>}>
          <label className="field">{t('sys.postCode')}<input value={edit.postCode || ''} onChange={(e) => setEdit({ ...edit, postCode: e.target.value })} /></label>
          <label className="field">{t('sys.postName')}<input value={edit.postName || ''} onChange={(e) => setEdit({ ...edit, postName: e.target.value })} /></label>
          <label className="field">{t('admin.orderNum')}<input type="number" value={edit.postSort ?? 1} onChange={(e) => setEdit({ ...edit, postSort: Number(e.target.value) })} /></label>
        </Modal>
      )}
      {toast && <Toast message={toast} />}
    </div>
  )
}

import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import * as pmo from '../api/pmo'
import AiFill from '../components/AiFill'
import type { CostCategory } from '../api/types'
import { useAuth } from '../auth/auth'
import { Empty, ErrorBox, Loading, Modal, Toast } from '../components/ui'

// PMO cost-category CRUD. Viewing is open to any PMO user; add/edit is admin-only
// (backend enforces pmo:category:list vs pmo:category:add/edit).
export default function Categories() {
  const { t } = useTranslation()
  const { hasPerm } = useAuth()
  const [rows, setRows] = useState<CostCategory[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [edit, setEdit] = useState<Partial<CostCategory> | null>(null)
  const [toast, setToast] = useState('')
  const notify = (m: string) => { setToast(m); setTimeout(() => setToast(''), 2500) }

  const load = () => {
    setLoading(true); setError('')
    pmo.getCategories().then((c) => setRows(c || []))
      .catch((e) => setError((e as Error).message)).finally(() => setLoading(false))
  }
  useEffect(load, [])

  const save = async () => {
    if (!edit) return
    try {
      if (edit.categoryId) await pmo.updateCategory(edit); else await pmo.createCategory(edit)
      setEdit(null); notify(t('common.saved')); load()
    } catch (e) { setError((e as Error).message) }
  }

  const canEdit = hasPerm('pmo:category:add') || hasPerm('pmo:category:edit')
  return (
    <div>
      <div className="row">
        <h2 style={{ margin: 0 }}>{t('category.title')}</h2>
        <div className="spacer" />
        {canEdit && <button className="btn primary" onClick={() => setEdit({})}>{t('category.newCat')}</button>}
      </div>
      {error && <ErrorBox message={error} />}
      {loading ? <Loading /> : rows.length === 0 ? <Empty /> : (
        <div className="table-wrap" style={{ marginTop: 12 }}>
          <table><thead><tr>
            <th>{t('category.code')}</th><th>{t('category.name')}</th>{canEdit && <th>{t('common.actions')}</th>}</tr></thead>
            <tbody>{rows.map((c) => (
              <tr key={c.categoryId}>
                <td><code>{c.code}</code></td><td>{c.name}</td>
                {canEdit && <td><button className="btn sm" onClick={() => setEdit({ ...c })}>{t('common.edit')}</button></td>}
              </tr>))}</tbody>
          </table>
        </div>
      )}
      {edit && (
        <Modal title={edit.categoryId ? t('category.editCat') : t('category.newCat')} onClose={() => setEdit(null)}
          footer={<><button className="btn" onClick={() => setEdit(null)}>{t('common.cancel')}</button>
            <button className="btn primary" onClick={save}>{t('common.save')}</button></>}>
          <AiFill kind="cost category" fields="code, name" onApply={(o) => setEdit({ ...edit, ...o })} />
          <label className="field">{t('category.code')}<input value={edit.code || ''} onChange={(e) => setEdit({ ...edit, code: e.target.value })} /></label>
          <label className="field">{t('category.name')}<input value={edit.name || ''} onChange={(e) => setEdit({ ...edit, name: e.target.value })} /></label>
        </Modal>
      )}
      {toast && <Toast message={toast} />}
    </div>
  )
}

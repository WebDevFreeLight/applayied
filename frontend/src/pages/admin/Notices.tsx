import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import * as sys from '../../api/system'
import type { SysNotice } from '../../api/system'
import { useAuth } from '../../auth/auth'
import { Empty, ErrorBox, Loading, Modal, Toast, fmtDate } from '../../components/ui'
import Select from '../../components/Select'

const P = { list: 'system:notice:list', add: 'system:notice:add', edit: 'system:notice:update', remove: 'system:notice:delete' }
const TYPES = [{ value: '1', label: 'Notification' }, { value: '2', label: 'Announcement' }]

export default function Notices() {
  const { t } = useTranslation()
  const { hasPerm } = useAuth()
  const [rows, setRows] = useState<SysNotice[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [edit, setEdit] = useState<Partial<SysNotice> | null>(null)
  const [toast, setToast] = useState('')
  const notify = (m: string) => { setToast(m); setTimeout(() => setToast(''), 2500) }

  const load = () => {
    setLoading(true); setError('')
    sys.listNotice({ pageNum: 1, pageSize: 50 }).then((p) => setRows(p.result || []))
      .catch((e) => setError((e as Error).message)).finally(() => setLoading(false))
  }
  useEffect(load, [])
  const save = async () => {
    if (!edit) return
    try { if (edit.noticeId) await sys.updateNotice(edit); else await sys.addNotice(edit); setEdit(null); notify(t('common.saved')); load() }
    catch (e) { setError((e as Error).message) }
  }
  const send = async (n: SysNotice) => { try { await sys.sendNotice(n.noticeId); notify(t('common.saved')) } catch (e) { setError((e as Error).message) } }
  const remove = async (n: SysNotice) => {
    if (!confirm(`${t('common.delete')}: ${n.noticeTitle}?`)) return
    try { await sys.removeNotice(n.noticeId); notify(t('common.saved')); load() } catch (e) { setError((e as Error).message) }
  }

  if (!hasPerm(P.list)) return <div className="panel">{t('admin.noAccess')}</div>
  const canAct = hasPerm(P.edit) || hasPerm(P.remove)
  return (
    <div>
      <div className="row"><h2 style={{ margin: 0 }}>{t('sys.notices')}</h2><div className="spacer" />
        {hasPerm(P.add) && <button className="btn primary" onClick={() => setEdit({ noticeType: '1', status: '0' })}>{t('common.add')}</button>}
      </div>
      {error && <ErrorBox message={error} />}
      {loading ? <Loading /> : rows.length === 0 ? <Empty /> : (
        <div className="table-wrap" style={{ marginTop: 12 }}>
          <table><thead><tr><th>{t('sys.title')}</th><th>{t('sys.type')}</th><th>{t('sys.createTime')}</th>{canAct && <th>{t('common.actions')}</th>}</tr></thead>
            <tbody>{rows.map((n) => (
              <tr key={n.noticeId}><td>{n.noticeTitle}</td><td>{TYPES.find((x) => x.value === n.noticeType)?.label || n.noticeType}</td><td>{fmtDate(n.createTime)}</td>
                {canAct && <td className="row">
                  {hasPerm(P.edit) && <button className="btn sm" onClick={() => setEdit({ ...n })}>{t('common.edit')}</button>}
                  {hasPerm(P.edit) && <button className="btn sm" onClick={() => send(n)}>{t('sys.send')}</button>}
                  {hasPerm(P.remove) && <button className="btn sm" onClick={() => remove(n)}>{t('common.delete')}</button>}
                </td>}</tr>))}</tbody>
          </table>
        </div>
      )}
      {edit && (
        <Modal title={edit.noticeId ? t('common.edit') : t('common.add')} onClose={() => setEdit(null)}
          footer={<><button className="btn" onClick={() => setEdit(null)}>{t('common.cancel')}</button><button className="btn primary" onClick={save}>{t('common.save')}</button></>}>
          <label className="field">{t('sys.title')}<input value={edit.noticeTitle || ''} onChange={(e) => setEdit({ ...edit, noticeTitle: e.target.value })} /></label>
          <label className="field">{t('sys.type')}<Select isClearable={false} options={TYPES} value={edit.noticeType} onChange={(v) => setEdit({ ...edit, noticeType: v || '1' })} /></label>
          <label className="field full">{t('sys.content')}<textarea rows={4} value={edit.noticeContent || ''} onChange={(e) => setEdit({ ...edit, noticeContent: e.target.value })} /></label>
        </Modal>
      )}
      {toast && <Toast message={toast} />}
    </div>
  )
}

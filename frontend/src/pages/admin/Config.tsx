import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import * as sys from '../../api/system'
import type { SysConfig } from '../../api/system'
import { useAuth } from '../../auth/auth'
import { Empty, ErrorBox, Loading, Modal, Toast } from '../../components/ui'

const P = { list: 'system:config:list', add: 'system:config:add', edit: 'system:config:update', remove: 'system:config:remove' }

export default function Config() {
  const { t } = useTranslation()
  const { hasPerm } = useAuth()
  const [rows, setRows] = useState<SysConfig[]>([])
  const [total, setTotal] = useState(0)
  const [q, setQ] = useState({ pageNum: 1, pageSize: 10, configName: '' })
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [edit, setEdit] = useState<Partial<SysConfig> | null>(null)
  const [toast, setToast] = useState('')
  const notify = (m: string) => { setToast(m); setTimeout(() => setToast(''), 2500) }

  const load = () => {
    setLoading(true); setError('')
    const params: Record<string, unknown> = { pageNum: q.pageNum, pageSize: q.pageSize }
    if (q.configName) params.configName = q.configName
    sys.listConfig(params).then((p) => { setRows(p.result || []); setTotal(p.totalNum || 0) })
      .catch((e) => setError((e as Error).message)).finally(() => setLoading(false))
  }
  useEffect(load, [q.pageNum])

  const save = async () => {
    if (!edit) return
    try { if (edit.configId) await sys.updateConfig(edit); else await sys.addConfig(edit); setEdit(null); notify(t('common.saved')); load() }
    catch (e) { setError((e as Error).message) }
  }
  const remove = async (c: SysConfig) => {
    if (!confirm(`${t('common.delete')}: ${c.configName}?`)) return
    try { await sys.removeConfig(c.configId); notify(t('common.saved')); load() } catch (e) { setError((e as Error).message) }
  }

  if (!hasPerm(P.list)) return <div className="panel">{t('admin.noAccess')}</div>
  const canAct = hasPerm(P.edit) || hasPerm(P.remove)
  const pages = Math.max(1, Math.ceil(total / q.pageSize))
  return (
    <div>
      <div className="row">
        <h2 style={{ margin: 0 }}>{t('sys.config')}</h2><div className="spacer" />
        {hasPerm(P.add) && <button className="btn primary" onClick={() => setEdit({ configType: 'N' })}>{t('common.add')}</button>}
      </div>
      <div className="row panel" style={{ marginTop: 12 }}>
        <input placeholder={t('sys.configName')} value={q.configName} onChange={(e) => setQ((s) => ({ ...s, configName: e.target.value }))} />
        <button className="btn primary" onClick={() => setQ((s) => ({ ...s, pageNum: 1 }))}>{t('common.apply')}</button>
      </div>
      {error && <ErrorBox message={error} />}
      {loading ? <Loading /> : rows.length === 0 ? <Empty /> : (
        <div className="table-wrap" style={{ marginTop: 12 }}>
          <table><thead><tr><th>{t('sys.configName')}</th><th>{t('sys.configKey')}</th><th>{t('sys.configValue')}</th>{canAct && <th>{t('common.actions')}</th>}</tr></thead>
            <tbody>{rows.map((c) => (
              <tr key={c.configId}><td>{c.configName}</td><td><code>{c.configKey}</code></td><td style={{ maxWidth: 320, overflow: 'hidden', textOverflow: 'ellipsis' }}>{c.configValue}</td>
                {canAct && <td className="row">
                  {hasPerm(P.edit) && <button className="btn sm" onClick={() => setEdit({ ...c })}>{t('common.edit')}</button>}
                  {hasPerm(P.remove) && c.configType !== 'Y' && <button className="btn sm" onClick={() => remove(c)}>{t('common.delete')}</button>}
                </td>}</tr>))}</tbody>
          </table>
        </div>
      )}
      <div className="row" style={{ marginTop: 10 }}><span className="muted">{total} · {t('common.page')} {q.pageNum}/{pages}</span><div className="spacer" />
        <button className="btn sm" disabled={q.pageNum <= 1} onClick={() => setQ((s) => ({ ...s, pageNum: s.pageNum - 1 }))}>‹</button>
        <button className="btn sm" disabled={q.pageNum >= pages} onClick={() => setQ((s) => ({ ...s, pageNum: s.pageNum + 1 }))}>›</button>
      </div>
      {edit && (
        <Modal title={edit.configId ? t('common.edit') : t('common.add')} onClose={() => setEdit(null)}
          footer={<><button className="btn" onClick={() => setEdit(null)}>{t('common.cancel')}</button><button className="btn primary" onClick={save}>{t('common.save')}</button></>}>
          <label className="field">{t('sys.configName')}<input value={edit.configName || ''} onChange={(e) => setEdit({ ...edit, configName: e.target.value })} /></label>
          <label className="field">{t('sys.configKey')}<input value={edit.configKey || ''} onChange={(e) => setEdit({ ...edit, configKey: e.target.value })} /></label>
          <label className="field full">{t('sys.configValue')}<input value={edit.configValue || ''} onChange={(e) => setEdit({ ...edit, configValue: e.target.value })} /></label>
          <label className="field full">{t('common.actions')}<input value={edit.remark || ''} onChange={(e) => setEdit({ ...edit, remark: e.target.value })} /></label>
        </Modal>
      )}
      {toast && <Toast message={toast} />}
    </div>
  )
}

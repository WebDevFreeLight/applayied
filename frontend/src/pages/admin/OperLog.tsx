import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import * as sys from '../../api/system'
import type { SysOperLog } from '../../api/system'
import { useAuth } from '../../auth/auth'
import { Empty, ErrorBox, Loading, Toast, fmtDate } from '../../components/ui'

const BIZ = ['Other', 'Insert', 'Update', 'Delete', 'Grant', 'Export', 'Import', 'Force logout', 'Gen code', 'Clean']

export default function OperLog() {
  const { t } = useTranslation()
  const { hasPerm } = useAuth()
  const [rows, setRows] = useState<SysOperLog[]>([])
  const [q, setQ] = useState({ pageNum: 1, pageSize: 20, operName: '' })
  const [total, setTotal] = useState(0)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [toast, setToast] = useState('')

  const load = () => {
    setLoading(true); setError('')
    const params: Record<string, unknown> = { pageNum: q.pageNum, pageSize: q.pageSize }
    if (q.operName) params.operName = q.operName
    sys.listOperLog(params).then((p) => { setRows(p.result || []); setTotal(p.totalNum || 0) })
      .catch((e) => setError((e as Error).message)).finally(() => setLoading(false))
  }
  useEffect(load, [q.pageNum])
  const clean = async () => {
    if (!confirm(t('sys.clean') + '?')) return
    try { await sys.cleanOperLog(); setToast(t('common.saved')); setTimeout(() => setToast(''), 2000); load() }
    catch (e) { setError((e as Error).message) }
  }

  const pages = Math.max(1, Math.ceil(total / q.pageSize))
  return (
    <div>
      <div className="row"><h2 style={{ margin: 0 }}>{t('sys.operLog')}</h2><div className="spacer" />
        {hasPerm('monitor:operlog:delete') && <button className="btn" onClick={clean}>{t('sys.clean')}</button>}
      </div>
      <div className="row panel" style={{ marginTop: 12 }}>
        <input placeholder={t('sys.operName')} value={q.operName} onChange={(e) => setQ((s) => ({ ...s, operName: e.target.value }))} />
        <button className="btn primary" onClick={() => setQ((s) => ({ ...s, pageNum: 1 }))}>{t('common.apply')}</button>
      </div>
      {error && <ErrorBox message={error} />}
      {loading ? <Loading /> : rows.length === 0 ? <Empty /> : (
        <div className="table-wrap" style={{ marginTop: 12 }}>
          <table><thead><tr><th>{t('sys.title')}</th><th>{t('sys.businessType')}</th><th>{t('sys.operName')}</th><th>{t('sys.ip')}</th><th>{t('sys.result')}</th><th>{t('sys.loginTime')}</th></tr></thead>
            <tbody>{rows.map((l) => (
              <tr key={l.operId}><td>{l.title}</td><td>{BIZ[l.businessType || 0] || l.businessType}</td><td>{l.operName}</td><td>{l.operIp}</td>
                <td><span className={`badge ${l.status === 0 ? 'green' : 'red'}`}>{l.status === 0 ? t('sys.success') : t('sys.fail')}</span></td>
                <td>{fmtDate(l.operTime)}</td></tr>))}</tbody>
          </table>
        </div>
      )}
      <div className="row" style={{ marginTop: 10 }}><span className="muted">{total} · {t('common.page')} {q.pageNum}/{pages}</span><div className="spacer" />
        <button className="btn sm" disabled={q.pageNum <= 1} onClick={() => setQ((s) => ({ ...s, pageNum: s.pageNum - 1 }))}>‹</button>
        <button className="btn sm" disabled={q.pageNum >= pages} onClick={() => setQ((s) => ({ ...s, pageNum: s.pageNum + 1 }))}>›</button>
      </div>
      {toast && <Toast message={toast} />}
    </div>
  )
}

import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import * as sys from '../../api/system'
import type { SysLogininfor } from '../../api/system'
import { useAuth } from '../../auth/auth'
import { Empty, ErrorBox, Loading, Toast, fmtDate } from '../../components/ui'

export default function LoginLog() {
  const { t } = useTranslation()
  const { hasPerm } = useAuth()
  const [rows, setRows] = useState<SysLogininfor[]>([])
  const [q, setQ] = useState({ pageNum: 1, pageSize: 20 })
  const [total, setTotal] = useState(0)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [toast, setToast] = useState('')

  const load = () => {
    setLoading(true); setError('')
    sys.listLoginLog({ pageNum: q.pageNum, pageSize: q.pageSize }).then((p) => { setRows(p.result || []); setTotal(p.totalNum || 0) })
      .catch((e) => setError((e as Error).message)).finally(() => setLoading(false))
  }
  useEffect(load, [q.pageNum])
  const clean = async () => {
    if (!confirm(t('sys.clean') + '?')) return
    try { await sys.cleanLoginLog(); setToast(t('common.saved')); setTimeout(() => setToast(''), 2000); load() } catch (e) { setError((e as Error).message) }
  }

  if (!hasPerm('monitor:logininfor:list')) return <div className="panel">{t('admin.noAccess')}</div>
  const pages = Math.max(1, Math.ceil(total / q.pageSize))
  return (
    <div>
      <div className="row"><h2 style={{ margin: 0 }}>{t('sys.loginLog')}</h2><div className="spacer" />
        {hasPerm('monitor:logininfor:remove') && <button className="btn" onClick={clean}>{t('sys.clean')}</button>}
      </div>
      {error && <ErrorBox message={error} />}
      {loading ? <Loading /> : rows.length === 0 ? <Empty /> : (
        <div className="table-wrap" style={{ marginTop: 12 }}>
          <table><thead><tr><th>{t('admin.userName')}</th><th>{t('sys.ip')}</th><th>{t('sys.location')}</th><th>{t('sys.browser')}</th><th>{t('sys.result')}</th><th>{t('sys.loginTime')}</th></tr></thead>
            <tbody>{rows.map((l) => (
              <tr key={l.infoId}><td>{l.userName}</td><td>{l.ipaddr}</td><td>{l.loginLocation}</td><td>{l.browser}</td>
                <td><span className={`badge ${l.status === '0' ? 'green' : 'red'}`}>{l.msg || (l.status === '0' ? t('sys.success') : t('sys.fail'))}</span></td>
                <td>{fmtDate(l.loginTime)}</td></tr>))}</tbody>
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

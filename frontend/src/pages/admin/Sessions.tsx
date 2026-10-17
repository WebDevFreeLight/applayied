import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import * as sys from '../../api/system'
import type { OnlineUser } from '../../api/system'
import { useAuth } from '../../auth/auth'
import { Empty, ErrorBox, Loading, Toast, fmtDate } from '../../components/ui'

export default function Sessions() {
  const { t } = useTranslation()
  const { hasPerm } = useAuth()
  const [rows, setRows] = useState<OnlineUser[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [toast, setToast] = useState('')

  const load = () => {
    setLoading(true); setError('')
    sys.listSessions({ pageNum: 1, pageSize: 100 }).then((p) => setRows(p.result || []))
      .catch((e) => setError((e as Error).message)).finally(() => setLoading(false))
  }
  useEffect(load, [])
  const kick = async (s: OnlineUser) => {
    if (!confirm(`${t('sys.forceLogout')}: ${s.userName}?`)) return
    try { await sys.forceLogout(s.tokenId); setToast(t('common.saved')); setTimeout(() => setToast(''), 2000); load() }
    catch (e) { setError((e as Error).message) }
  }

  const canKick = hasPerm('monitor:online:forceLogout')
  return (
    <div>
      <h2 style={{ margin: 0 }}>{t('sys.sessions')}</h2>
      {error && <ErrorBox message={error} />}
      {loading ? <Loading /> : rows.length === 0 ? <Empty /> : (
        <div className="table-wrap" style={{ marginTop: 12 }}>
          <table><thead><tr><th>{t('admin.userName')}</th><th>{t('sys.ip')}</th><th>{t('sys.location')}</th><th>{t('sys.browser')}</th><th>{t('sys.loginTime')}</th>{canKick && <th>{t('common.actions')}</th>}</tr></thead>
            <tbody>{rows.map((s) => (
              <tr key={s.tokenId}><td>{s.userName}</td><td>{s.ipaddr}</td><td>{s.loginLocation}</td><td>{s.browser}</td><td>{fmtDate(s.loginTime)}</td>
                {canKick && <td><button className="btn sm" onClick={() => kick(s)}>{t('sys.forceLogout')}</button></td>}</tr>))}</tbody>
          </table>
        </div>
      )}
      {toast && <Toast message={toast} />}
    </div>
  )
}

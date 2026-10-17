import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import * as sys from '../../api/system'
import type { UserOnlineLog } from '../../api/system'
import { Empty, ErrorBox, Loading, fmtDate } from '../../components/ui'

export default function Online() {
  const { t } = useTranslation()
  const [rows, setRows] = useState<UserOnlineLog[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  useEffect(() => {
    sys.listOnline({ pageNum: 1, pageSize: 100 }).then((p) => setRows(p.result || []))
      .catch((e) => setError((e as Error).message)).finally(() => setLoading(false))
  }, [])

  return (
    <div>
      <h2 style={{ margin: 0 }}>{t('sys.online')}</h2>
      {error && <ErrorBox message={error} />}
      {loading ? <Loading /> : rows.length === 0 ? <Empty /> : (
        <div className="table-wrap" style={{ marginTop: 12 }}>
          <table><thead><tr><th>{t('admin.userName')}</th><th>{t('sys.userIp')}</th><th>{t('sys.duration')}</th><th>{t('sys.loginTime')}</th></tr></thead>
            <tbody>{rows.map((r) => (
              <tr key={r.id}><td>{r.userName || r.userId}</td><td>{r.userIP}</td>
                <td>{r.onlineTime != null ? `${Math.round(Number(r.onlineTime) / 60)} min` : '—'}</td><td>{fmtDate(r.addTime)}</td></tr>))}</tbody>
          </table>
        </div>
      )}
    </div>
  )
}

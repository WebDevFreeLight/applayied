import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import * as sys from '../../api/system'
import type { SysUserMsg } from '../../api/system'
import { Empty, ErrorBox, Loading, Toast, fmtDate } from '../../components/ui'

// Personal inbox — available to every signed-in user (mylist is scoped to the caller server-side).
export default function Messages() {
  const { t } = useTranslation()
  const [rows, setRows] = useState<SysUserMsg[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [toast, setToast] = useState('')

  const load = () => {
    setLoading(true); setError('')
    sys.myMessages({ pageNum: 1, pageSize: 50 }).then((p) => setRows(p.result || []))
      .catch((e) => setError((e as Error).message)).finally(() => setLoading(false))
  }
  useEffect(load, [])
  const markRead = async (m: SysUserMsg) => {
    try { await sys.readMessage(m.msgId, m.msgType || '1'); setToast(t('common.saved')); setTimeout(() => setToast(''), 2000); load() }
    catch (e) { setError((e as Error).message) }
  }

  return (
    <div>
      <h2 style={{ margin: 0 }}>{t('sys.messages')}</h2>
      {error && <ErrorBox message={error} />}
      {loading ? <Loading /> : rows.length === 0 ? <Empty /> : (
        <div className="table-wrap" style={{ marginTop: 12 }}>
          <table><thead><tr><th>{t('sys.title')}</th><th>{t('sys.content')}</th><th>{t('sys.createTime')}</th><th>{t('sys.result')}</th><th /></tr></thead>
            <tbody>{rows.map((m) => (
              <tr key={m.msgId}>
                <td>{m.title}</td><td style={{ maxWidth: 360 }}>{m.content}</td><td>{fmtDate(m.addTime)}</td>
                <td><span className={`badge ${m.isRead ? 'gray' : 'yellow'}`}>{m.isRead ? t('sys.markedRead') : t('sys.unread')}</span></td>
                <td>{!m.isRead && <button className="btn sm" onClick={() => markRead(m)}>{t('sys.read')}</button>}</td>
              </tr>))}</tbody>
          </table>
        </div>
      )}
      {toast && <Toast message={toast} />}
    </div>
  )
}

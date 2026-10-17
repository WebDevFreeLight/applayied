import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import * as sys from '../../api/system'
import type { SysJobLog } from '../../api/system'
import { useAuth } from '../../auth/auth'
import { Empty, ErrorBox, Loading, Toast, fmtDate } from '../../components/ui'

export default function JobLogs() {
  const { t } = useTranslation()
  const { hasPerm } = useAuth()
  const [rows, setRows] = useState<SysJobLog[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [toast, setToast] = useState('')

  const load = () => {
    setLoading(true); setError('')
    sys.listJobLogs({ pageNum: 1, pageSize: 100 }).then((p) => setRows(p.result || []))
      .catch((e) => setError((e as Error).message)).finally(() => setLoading(false))
  }
  useEffect(load, [])
  const clean = async () => {
    if (!confirm(t('sys.clean') + '?')) return
    try { await sys.cleanJobLogs(); setToast(t('common.saved')); setTimeout(() => setToast(''), 2000); load() }
    catch (e) { setError((e as Error).message) }
  }

  return (
    <div>
      <div className="row"><h2 style={{ margin: 0 }}>{t('sys.jobLogs')}</h2><div className="spacer" />
        {hasPerm('PRIV_JOBLOG_REMOVE') && <button className="btn" onClick={clean}>{t('sys.clean')}</button>}
      </div>
      {error && <ErrorBox message={error} />}
      {loading ? <Loading /> : rows.length === 0 ? <Empty /> : (
        <div className="table-wrap" style={{ marginTop: 12 }}>
          <table><thead><tr><th>{t('sys.jobName')}</th><th>{t('sys.jobGroup')}</th><th>{t('sys.result')}</th><th>{t('sys.content')}</th><th>{t('sys.createTime')}</th></tr></thead>
            <tbody>{rows.map((l) => (
              <tr key={l.jobLogId}><td>{l.jobName}</td><td>{l.jobGroup}</td>
                <td><span className={`badge ${l.status === '0' ? 'green' : 'red'}`}>{l.status === '0' ? t('sys.success') : t('sys.fail')}</span></td>
                <td style={{ maxWidth: 360 }}>{l.jobMessage}</td><td>{fmtDate(l.createTime)}</td></tr>))}</tbody>
          </table>
        </div>
      )}
      {toast && <Toast message={toast} />}
    </div>
  )
}

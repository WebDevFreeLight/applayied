import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import * as pmo from '../../api/pmo'
import type { FinancialForecast } from '../../api/types'
import { useAuth } from '../../auth/auth'
import { Empty, ErrorBox, Loading, Modal, Toast, fmtDate, fmtMoney } from '../../components/ui'
import Select from '../../components/Select'

const KINDS = ['ForecastToComplete', 'MonthlyEstimate']

// Financial forecasts (pmo_financial_forecast) — drives EAC = actual + Σ forecast-to-complete.
export default function ProjectForecasts({ projectId }: { projectId: string }) {
  const { t } = useTranslation()
  const { hasRole } = useAuth()
  const [rows, setRows] = useState<FinancialForecast[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [add, setAdd] = useState<{ periodMonth: string; forecastAmount: number; kind: string } | null>(null)
  const [toast, setToast] = useState('')

  const load = () => {
    setLoading(true); setError('')
    pmo.listForecasts(projectId).then((f) => setRows(f || []))
      .catch((e) => setError((e as Error).message)).finally(() => setLoading(false))
  }
  useEffect(load, [projectId])

  const save = async () => {
    if (!add) return
    try { await pmo.createForecast({ projectId, ...add }); setAdd(null); setToast(t('common.saved')); setTimeout(() => setToast(''), 2000); load() }
    catch (e) { setError((e as Error).message) }
  }

  return (
    <div>
      {hasRole('pm') && (
        <div className="row" style={{ marginBottom: 12 }}>
          <div className="spacer" />
          <button className="btn primary" onClick={() => setAdd({ periodMonth: '2026-10-01', forecastAmount: 0, kind: 'ForecastToComplete' })}>{t('forecast.add')}</button>
        </div>
      )}
      {error && <ErrorBox message={error} />}
      {loading ? <Loading /> : rows.length === 0 ? <Empty /> : (
        <div className="table-wrap">
          <table><thead><tr><th>{t('forecast.period')}</th><th>{t('forecast.kind')}</th><th>{t('forecast.amount')}</th></tr></thead>
            <tbody>{rows.map((f) => (
              <tr key={f.forecastId}><td>{fmtDate(f.periodMonth)}</td><td>{f.kind}</td><td>{fmtMoney(f.forecastAmount)}</td></tr>))}</tbody>
          </table>
        </div>
      )}
      {add && (
        <Modal title={t('forecast.add')} onClose={() => setAdd(null)}
          footer={<><button className="btn" onClick={() => setAdd(null)}>{t('common.cancel')}</button>
            <button className="btn primary" onClick={save}>{t('common.save')}</button></>}>
          <label className="field">{t('forecast.period')}<input type="date" value={(add.periodMonth || '').substring(0, 10)} onChange={(e) => setAdd({ ...add, periodMonth: e.target.value })} /></label>
          <label className="field">{t('forecast.kind')}
            <Select isClearable={false} options={KINDS.map((k) => ({ value: k, label: k }))}
              value={add.kind} onChange={(v) => setAdd({ ...add, kind: v || 'ForecastToComplete' })} /></label>
          <label className="field">{t('forecast.amount')}<input type="number" value={add.forecastAmount} onChange={(e) => setAdd({ ...add, forecastAmount: Number(e.target.value) })} /></label>
        </Modal>
      )}
      {toast && <Toast message={toast} />}
    </div>
  )
}

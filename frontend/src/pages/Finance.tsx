import { useEffect, useMemo, useState } from 'react'
import { useTranslation } from 'react-i18next'
import ReactECharts from 'echarts-for-react'
import * as pmo from '../api/pmo'
import type { CostTransaction, BudgetRevision, FinanceSummary, DashboardAnalytics, DashboardKpis } from '../api/types'
import { useLookups } from '../hooks/useLookups'
import { useAuth } from '../auth/auth'
import { Badge, Empty, ErrorBox, Loading, Modal, Toast, fmtDate, fmtMoney } from '../components/ui'
import Select from '../components/Select'

const BASELINE_CUTOFF = '2026-09-30'

export default function Finance({ projectId: pin }: { projectId?: string } = {}) {
  const { t } = useTranslation()
  const lk = useLookups()
  const { hasRole } = useAuth()
  const [projectId, setProjectId] = useState(pin || '')
  const [deptId, setDeptId] = useState('')
  const [cutoff, setCutoff] = useState(BASELINE_CUTOFF)
  const [summary, setSummary] = useState<FinanceSummary | null>(null)
  const [txns, setTxns] = useState<CostTransaction[]>([])
  const [revs, setRevs] = useState<BudgetRevision[]>([])
  const [portfolio, setPortfolio] = useState<{ kpis: DashboardKpis; analytics: DashboardAnalytics } | null>(null)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')
  const [addCost, setAddCost] = useState<Partial<CostTransaction> | null>(null)
  const [addRev, setAddRev] = useState<Partial<BudgetRevision> | null>(null)
  const [toast, setToast] = useState('')
  const notify = (m: string) => { setToast(m); setTimeout(() => setToast(''), 2500) }

  const load = () => {
    setError('')
    // No project selected → portfolio finance across ALL projects, grouped by project.
    if (!projectId) {
      setSummary(null); setTxns([]); setRevs([]); setLoading(true)
      const filter = { cutoff, deptId: deptId || undefined }
      Promise.all([pmo.getKpis(filter), pmo.getAnalytics(filter)])
        .then(([kpis, analytics]) => setPortfolio({ kpis, analytics }))
        .catch((e) => setError((e as Error).message)).finally(() => setLoading(false))
      return
    }
    setPortfolio(null); setLoading(true)
    Promise.all([
      pmo.getFinanceSummary(projectId, cutoff),
      pmo.listTransactions({ projectId, pageNum: 1, pageSize: 200 }),
      pmo.listRevisions({ projectId, pageNum: 1, pageSize: 200 }),
    ]).then(([s, tx, rv]) => { setSummary(s); setTxns(tx.result || []); setRevs(rv.result || []) })
      .catch((e) => setError((e as Error).message)).finally(() => setLoading(false))
  }
  useEffect(load, [projectId, cutoff, deptId])

  const saveCost = async () => {
    if (!addCost) return
    try { await pmo.createTransaction({ ...addCost, projectId }); setAddCost(null); notify(t('common.saved')); load() }
    catch (e) { setError((e as Error).message) }
  }
  const post = async (tx: CostTransaction) => {
    try { await pmo.postTransaction(tx.transactionId); notify(t('common.saved')); load() }
    catch (e) { setError((e as Error).message) }
  }
  const saveRev = async () => {
    if (!addRev) return
    try { await pmo.createRevision({ ...addRev, projectId }); setAddRev(null); notify(t('common.saved')); load() }
    catch (e) { setError((e as Error).message) }
  }
  const decideRev = async (rev: BudgetRevision, approve: boolean) => {
    try { await pmo.decideRevision({ revisionId: rev.revisionId, approve }); notify(t('common.saved')); load() }
    catch (e) { setError((e as Error).message) }
  }

  return (
    <div>
      <div className="row">
        <h2 style={{ margin: 0 }}>{t('finance.title')}</h2>
        <div className="spacer" />
        {!pin && !projectId && <div style={{ minWidth: 180 }}><Select options={lk.departments.map((d) => ({ value: d.id, label: d.name }))}
          value={deptId} onChange={(v) => setDeptId(v || '')} placeholder={`${t('project.department')}: ${t('common.all')}`} /></div>}
        {!pin && <div style={{ minWidth: 240 }}><Select options={lk.projects.map((p) => ({ value: p.id, label: p.name }))}
          value={projectId} onChange={(v) => setProjectId(v || '')} placeholder={`${t('project.title')}: ${t('common.all')}`} /></div>}
        <input type="date" value={cutoff} onChange={(e) => setCutoff(e.target.value)} title={t('dash.cutoff')} />
      </div>

      {error && <ErrorBox message={error} />}
      {!projectId ? (
        loading && !portfolio ? <Loading /> : portfolio ? (
          <Portfolio data={portfolio} onOpen={(id) => setProjectId(id)} />
        ) : <Empty />
      ) : loading && !summary ? <Loading /> : (
        <>
          {summary && (
            <div className="cards" style={{ marginTop: 12 }}>
              <Kpi label={t('finance.approvedBudget')} value={fmtMoney(summary.approvedBudget, summary.currency)} />
              <Kpi label={t('finance.actualSpend')} value={fmtMoney(summary.actualSpend, summary.currency)} />
              <Kpi label={t('finance.remaining')} value={fmtMoney(summary.remainingBudget, summary.currency)} />
              <Kpi label={t('finance.utilization')} value={summary.utilizationPct == null ? '—' : `${Math.round(summary.utilizationPct * 100)}%`} />
              <Kpi label={t('finance.eac')} value={fmtMoney(summary.eac, summary.currency)} />
              <Kpi label={t('finance.variance')} value={fmtMoney(summary.projectedVariance, summary.currency)} />
            </div>
          )}

          <div className="row" style={{ marginTop: 18 }}>
            <h3 style={{ margin: 0 }}>{t('finance.transactions')}</h3>
            <div className="spacer" />
            {hasRole('pm') && <button className="btn primary sm" onClick={() => setAddCost({ status: 'Draft', txnDate: cutoff })}>{t('finance.addCost')}</button>}
          </div>
          <div className="table-wrap" style={{ marginTop: 8 }}>
            {txns.length === 0 ? <Empty /> : (
              <table><thead><tr>
                <th>{t('finance.date')}</th><th>{t('finance.category')}</th><th>{t('finance.amount')}</th>
                <th>{t('finance.description')}</th><th>{t('finance.status')}</th><th>{t('common.actions')}</th></tr></thead>
                <tbody>{txns.map((x) => (
                  <tr key={x.transactionId}><td>{fmtDate(x.txnDate)}</td><td>{x.categoryName}</td>
                    <td>{fmtMoney(x.amount)}</td><td>{x.description}</td><td><Badge value={x.status} /></td>
                    <td>{x.status === 'Draft' && hasRole('pm') && <button className="btn sm" onClick={() => post(x)}>{t('finance.post')}</button>}</td></tr>))}</tbody>
              </table>)}
          </div>

          <div className="row" style={{ marginTop: 18 }}>
            <h3 style={{ margin: 0 }}>{t('finance.revisions')}</h3>
            <div className="spacer" />
            {hasRole('pm') && <button className="btn primary sm" onClick={() => setAddRev({ status: 'Pending' })}>{t('finance.newRevision')}</button>}
          </div>
          <div className="table-wrap" style={{ marginTop: 8 }}>
            {revs.length === 0 ? <Empty /> : (
              <table><thead><tr>
                <th>{t('finance.amount')}</th><th>{t('cr.reason')}</th><th>{t('finance.status')}</th><th>{t('common.actions')}</th></tr></thead>
                <tbody>{revs.map((r) => (
                  <tr key={r.revisionId}><td>{fmtMoney(r.amount)}</td><td>{r.reason}</td><td><Badge value={r.status} /></td>
                    <td className="row">{r.status === 'Pending' && hasRole('admin') && <>
                      <button className="btn sm" onClick={() => decideRev(r, true)}>{t('cr.approve')}</button>
                      <button className="btn sm" onClick={() => decideRev(r, false)}>{t('cr.reject')}</button></>}</td></tr>))}</tbody>
              </table>)}
          </div>
        </>
      )}

      {addCost && (
        <Modal title={t('finance.addCost')} onClose={() => setAddCost(null)}
          footer={<><button className="btn" onClick={() => setAddCost(null)}>{t('common.cancel')}</button>
            <button className="btn primary" onClick={saveCost}>{t('common.save')}</button></>}>
          <label className="field">{t('finance.category')}
            <Select options={lk.categories.map((c) => ({ value: c.id, label: c.name }))}
              value={addCost.categoryId || null} onChange={(v) => setAddCost({ ...addCost, categoryId: v || undefined })} /></label>
          <label className="field">{t('finance.amount')}<input type="number" value={addCost.amount ?? 0} onChange={(e) => setAddCost({ ...addCost, amount: Number(e.target.value) })} /></label>
          <label className="field">{t('finance.date')}<input type="date" value={(addCost.txnDate || '').substring(0, 10)} onChange={(e) => setAddCost({ ...addCost, txnDate: e.target.value })} /></label>
          <label className="field full">{t('finance.description')}<input value={addCost.description || ''} onChange={(e) => setAddCost({ ...addCost, description: e.target.value })} /></label>
        </Modal>
      )}
      {addRev && (
        <Modal title={t('finance.newRevision')} onClose={() => setAddRev(null)}
          footer={<><button className="btn" onClick={() => setAddRev(null)}>{t('common.cancel')}</button>
            <button className="btn primary" onClick={saveRev}>{t('common.save')}</button></>}>
          <label className="field">{t('finance.amount')}<input type="number" value={addRev.amount ?? 0} onChange={(e) => setAddRev({ ...addRev, amount: Number(e.target.value) })} /></label>
          <label className="field full">{t('cr.reason')}<textarea rows={3} value={addRev.reason || ''} onChange={(e) => setAddRev({ ...addRev, reason: e.target.value })} /></label>
        </Modal>
      )}
      {toast && <Toast message={toast} />}
    </div>
  )
}

function Kpi({ label, value }: { label: string; value: React.ReactNode }) {
  return <div className="kpi"><div className="label">{label}</div><div className="value" style={{ fontSize: 20 }}>{value}</div></div>
}

// Portfolio finance: all projects grouped, with derived per-project metrics + charts. Click a row
// to drill into that project's detailed finance view.
function Portfolio({ data, onOpen }: { data: { kpis: DashboardKpis; analytics: DashboardAnalytics }; onOpen: (id: string) => void }) {
  const { t, i18n } = useTranslation()
  const { kpis, analytics } = data
  const cur = kpis.currency
  const rows = useMemo(() => analytics.budgetVsActual.map((b) => {
    const remaining = b.approvedBudget - b.actualSpend
    const utilization = b.approvedBudget > 0 ? b.actualSpend / b.approvedBudget : null
    const variance = b.approvedBudget - b.eac
    return { ...b, remaining, utilization, variance, overrun: b.eac > b.approvedBudget }
  }), [analytics])

  const base = {
    textStyle: { color: 'var(--text)', fontFamily: 'inherit' }, backgroundColor: 'transparent',
    grid: { left: 50, right: 20, top: 30, bottom: 60, containLabel: true }, tooltip: { trigger: 'axis' as const },
  }
  const bva = {
    ...base, legend: { top: 0, textStyle: { color: 'var(--muted)' } },
    xAxis: { type: 'category' as const, data: rows.map((r) => r.projectName), axisLabel: { interval: 0, rotate: 30 } },
    yAxis: { type: 'value' as const },
    series: [
      { name: t('finance.approvedBudget'), type: 'bar', data: rows.map((r) => r.approvedBudget), itemStyle: { color: '#3498db' } },
      { name: t('finance.actualSpend'), type: 'bar', data: rows.map((r) => r.actualSpend), itemStyle: { color: '#e30613' } },
      { name: t('finance.eac'), type: 'bar', data: rows.map((r) => r.eac), itemStyle: { color: '#f1c40f' } },
    ],
  }
  const trend = analytics.monthlyTrend.map((m) => ({ x: (m.period || m.month || '').substring(0, 7), y: m.amount }))
  const monthly = { ...base, xAxis: { type: 'category' as const, data: trend.map((p) => p.x) }, yAxis: { type: 'value' as const },
    series: [{ type: 'line', smooth: true, areaStyle: {}, data: trend.map((p) => p.y), itemStyle: { color: '#2ecc71' } }] }
  const cats = analytics.spendByCategory.map((c) => ({ name: c.categoryName || c.category || '—', value: c.amount }))
  const byCat = { ...base, tooltip: { trigger: 'item' as const }, legend: { bottom: 0, textStyle: { color: 'var(--muted)' } },
    series: [{ type: 'pie', radius: ['45%', '70%'], data: cats, label: { color: 'var(--text)' } }] }

  const pct = (n: number | null) => (n == null ? '—' : `${Math.round(n * 100)}%`)
  return (
    <div dir={i18n.language === 'ar' ? 'rtl' : 'ltr'}>
      <div className="cards" style={{ marginTop: 12 }}>
        <Kpi label={t('finance.approvedBudget')} value={fmtMoney(kpis.approvedBudget, cur)} />
        <Kpi label={t('finance.actualSpend')} value={fmtMoney(kpis.actualSpend, cur)} />
        <Kpi label={t('finance.remaining')} value={fmtMoney(kpis.remainingBudget, cur)} />
        <Kpi label={t('finance.utilization')} value={kpis.utilizationPct == null ? '—' : `${Math.round(kpis.utilizationPct * 100)}%`} />
        <Kpi label={t('finance.eac')} value={fmtMoney(kpis.eac, cur)} />
        <Kpi label={t('dash.overrunCount')} value={kpis.projectedOverrunCount} />
      </div>

      <div className="panel" style={{ marginTop: 14 }}>
        <h3>{t('finance.summary')} — {t('project.title')}</h3>
        <div className="table-wrap">
          <table><thead><tr>
            <th>{t('project.title')}</th><th>{t('finance.approvedBudget')}</th><th>{t('finance.actualSpend')}</th>
            <th>{t('finance.remaining')}</th><th>{t('finance.utilization')}</th><th>{t('finance.eac')}</th><th>{t('finance.variance')}</th>
          </tr></thead>
          <tbody>{rows.map((r) => (
            <tr key={r.projectId} style={{ cursor: 'pointer' }} onClick={() => onOpen(r.projectId)}>
              <td>{r.projectName}{r.overrun && <span className="badge red" style={{ marginInlineStart: 6 }}>{t('dash.overrunCount')}</span>}</td>
              <td>{fmtMoney(r.approvedBudget, cur)}</td><td>{fmtMoney(r.actualSpend, cur)}</td>
              <td>{fmtMoney(r.remaining, cur)}</td><td>{pct(r.utilization)}</td><td>{fmtMoney(r.eac, cur)}</td>
              <td style={{ color: r.variance < 0 ? 'var(--bad)' : undefined }}>{fmtMoney(r.variance, cur)}</td>
            </tr>))}</tbody>
          </table>
        </div>
      </div>

      <div className="grid2" style={{ marginTop: 14 }}>
        <div className="panel"><h3>{t('dash.budgetVsActual')} ({cur})</h3><ReactECharts option={bva} style={{ height: 300 }} notMerge /></div>
        <div className="panel"><h3>{t('dash.monthlyTrend')} ({cur})</h3><ReactECharts option={monthly} style={{ height: 300 }} notMerge /></div>
        <div className="panel"><h3>{t('dash.spendByCategory')} ({cur})</h3><ReactECharts option={byCat} style={{ height: 300 }} notMerge /></div>
      </div>
    </div>
  )
}

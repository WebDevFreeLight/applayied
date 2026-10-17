import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import * as pmo from '../api/pmo'
import type { CostTransaction, BudgetRevision, FinanceSummary } from '../api/types'
import { useLookups } from '../hooks/useLookups'
import { useAuth } from '../auth/auth'
import { Badge, Empty, ErrorBox, Loading, Modal, Toast, fmtDate, fmtMoney } from '../components/ui'

const BASELINE_CUTOFF = '2026-09-30'

export default function Finance() {
  const { t } = useTranslation()
  const lk = useLookups()
  const { hasRole } = useAuth()
  const [projectId, setProjectId] = useState('')
  const [cutoff, setCutoff] = useState(BASELINE_CUTOFF)
  const [summary, setSummary] = useState<FinanceSummary | null>(null)
  const [txns, setTxns] = useState<CostTransaction[]>([])
  const [revs, setRevs] = useState<BudgetRevision[]>([])
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')
  const [addCost, setAddCost] = useState<Partial<CostTransaction> | null>(null)
  const [addRev, setAddRev] = useState<Partial<BudgetRevision> | null>(null)
  const [toast, setToast] = useState('')
  const notify = (m: string) => { setToast(m); setTimeout(() => setToast(''), 2500) }

  const load = () => {
    if (!projectId) { setSummary(null); setTxns([]); setRevs([]); return }
    setLoading(true); setError('')
    Promise.all([
      pmo.getFinanceSummary(projectId, cutoff),
      pmo.listTransactions({ projectId, pageNum: 1, pageSize: 200 }),
      pmo.listRevisions({ projectId, pageNum: 1, pageSize: 200 }),
    ]).then(([s, tx, rv]) => { setSummary(s); setTxns(tx.result || []); setRevs(rv.result || []) })
      .catch((e) => setError((e as Error).message)).finally(() => setLoading(false))
  }
  useEffect(load, [projectId, cutoff])

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
        <select value={projectId} onChange={(e) => setProjectId(e.target.value)}>
          <option value="">{t('project.title')}…</option>
          {lk.projects.map((p) => <option key={p.id} value={p.id}>{p.label}</option>)}
        </select>
        <input type="date" value={cutoff} onChange={(e) => setCutoff(e.target.value)} title={t('dash.cutoff')} />
      </div>

      {error && <ErrorBox message={error} />}
      {!projectId ? <Empty /> : loading && !summary ? <Loading /> : (
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
            <select value={addCost.categoryId || ''} onChange={(e) => setAddCost({ ...addCost, categoryId: e.target.value })}>
              <option value="">—</option>{lk.categories.map((c) => <option key={c.categoryId} value={c.categoryId}>{c.name}</option>)}</select></label>
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

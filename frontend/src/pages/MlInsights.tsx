import { useCallback, useEffect, useMemo, useState } from 'react'
import { useTranslation } from 'react-i18next'
import ReactECharts from 'echarts-for-react'
import * as ml from '../api/ml'
import { useAuth } from '../auth/auth'
import { useLookups } from '../hooks/useLookups'
import { Badge, Empty, ErrorBox, Loading, Toast, fmtDate, fmtMoney } from '../components/ui'
import Select from '../components/Select'

// Machine-learning insights (scikit-learn models served by the Python PMO ML service).
// Finance-derived tabs (spend forecast, cost anomalies) need pmo:finance:list - the same rule as
// the Finance page - and "Retrain now" needs pmo:ml:train.

type Tab = 'delay' | 'spend' | 'anomaly' | 'segments' | 'severity' | 'models'

const cssVar = (name: string, fallback: string) =>
  (typeof window !== 'undefined' && getComputedStyle(document.documentElement).getPropertyValue(name).trim()) || fallback

const pct = (v?: number | null) => (v == null ? '—' : `${Math.round(v * 100)}%`)
const BAND_COLOR: Record<string, string> = { High: '#e74c3c', Medium: '#f39c12', Low: '#2ecc71' }

function Kpi({ label, value, sub }: { label: string; value: React.ReactNode; sub?: string }) {
  return (
    <div className="kpi">
      <div className="label">{label}</div>
      <div className="value">{value}</div>
      {sub && <div className="sub">{sub}</div>}
    </div>
  )
}

function ProbBar({ p }: { p: number }) {
  const band = p >= 0.66 ? 'High' : p >= 0.4 ? 'Medium' : 'Low'
  return (
    <div style={{ display: 'flex', alignItems: 'center', gap: 8, minWidth: 140 }}>
      <div style={{ flex: 1, height: 8, borderRadius: 4, background: 'var(--border)', overflow: 'hidden' }}>
        <div style={{ width: `${Math.round(p * 100)}%`, height: '100%', background: BAND_COLOR[band] }} />
      </div>
      <span style={{ fontVariantNumeric: 'tabular-nums', minWidth: 36 }}>{pct(p)}</span>
    </div>
  )
}

function Unavailable({ r }: { r: any }) {
  const { t } = useTranslation()
  return <ErrorBox message={`${t('ml.unavailable')}${r?.message ? ` — ${r.message}` : ''}`} />
}

function ChartImg({ path, alt }: { path: string; alt: string }) {
  const [src, setSrc] = useState<string | null>(null)
  useEffect(() => {
    let url: string | null = null
    ml.mlChartUrl(path).then((u) => { url = u; setSrc(u) })
    return () => { if (url) URL.revokeObjectURL(url) }
  }, [path])
  return src ? <img src={src} alt={alt} style={{ width: '100%', borderRadius: 8, background: '#fff' }} />
    : <div className="muted" style={{ fontSize: 12 }}>{alt}</div>
}

export default function MlInsights() {
  const { t, i18n } = useTranslation()
  const { hasPerm } = useAuth()
  const lk = useLookups()
  const rtl = i18n.language === 'ar'
  const canFinance = hasPerm('pmo:finance:list')
  const canTrain = hasPerm('pmo:ml:train')

  const [tab, setTab] = useState<Tab>('delay')
  const [projectId, setProjectId] = useState<string | null>(null)
  const [health, setHealth] = useState<any>(null)
  const [delay, setDelay] = useState<any>(null)
  const [spend, setSpend] = useState<any>(null)
  const [anoms, setAnoms] = useState<any>(null)
  const [segs, setSegs] = useState<any>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [toast, setToast] = useState('')
  const [training, setTraining] = useState(false)

  const load = useCallback(async () => {
    setLoading(true)
    setError('')
    try {
      const [h, d, s, a, g] = await Promise.all([
        ml.mlStatus(), ml.mlTaskDelay(projectId), canFinance ? ml.mlSpendForecast(projectId) : Promise.resolve(null),
        canFinance ? ml.mlAnomalies(projectId, 15) : Promise.resolve(null), ml.mlSegments(),
      ])
      setHealth(h); setDelay(d); setSpend(s); setAnoms(a); setSegs(g)
    } catch (e) {
      setError((e as Error).message)
    } finally {
      setLoading(false)
    }
  }, [projectId, canFinance, i18n.language])

  useEffect(() => { load() }, [load])

  async function retrain() {
    setTraining(true)
    try {
      const r = await ml.mlTrain()
      if (r?.status === 'ok') {
        const ok = Object.values<any>(r.results || {}).filter((x) => x.status === 'trained').length
        setToast(t('ml.retrained', { n: ok, s: r.seconds }))
      } else setToast(r?.message || t('common.error'))
      await load()
    } catch (e) {
      setToast((e as Error).message)
    } finally {
      setTraining(false)
      setTimeout(() => setToast(''), 4000)
    }
  }

  const projectOptions = lk.projects.map((p) => ({ value: String(p.id), label: p.name }))
  const tabs: [Tab, string, boolean][] = [
    ['delay', t('ml.tabDelay'), true],
    ['spend', t('ml.tabSpend'), canFinance],
    ['anomaly', t('ml.tabAnomaly'), canFinance],
    ['segments', t('ml.tabSegments'), true],
    ['severity', t('ml.tabSeverity'), true],
    ['models', t('ml.tabModels'), true],
  ]

  const delayItems: ml.TaskDelayItem[] = ml.isAvailable<ml.TaskDelay>(delay) ? delay.items : []
  const likelyLate = delayItems.filter((i) => i.probability >= 0.5).length
  const highRisk = delayItems.filter((i) => i.risk_band === 'High').length
  const nextSpend = ml.isAvailable<ml.SpendForecast>(spend) ? spend.items.reduce((s, i) => s + i.forecast, 0) : null
  const serviceUp = ml.isAvailable<ml.MlHealth>(health) && health.status === 'ok'

  return (
    <div>
      <div className="row">
        <h2 style={{ margin: 0 }}>🤖 {t('ml.title')}</h2>
        <span className="muted" style={{ fontSize: 12 }}>{t('ml.subtitle')}</span>
        <div className="spacer" />
        <div style={{ minWidth: 220 }}>
          <Select options={projectOptions} value={projectId} onChange={setProjectId} placeholder={t('ml.allProjects')} />
        </div>
        <button className="btn sm" onClick={load} disabled={loading}>↻ {t('ml.refresh')}</button>
        {canTrain && <button className="btn sm primary" onClick={retrain} disabled={training || !serviceUp}>
          {training ? t('ml.training') : `⚙ ${t('ml.retrain')}`}</button>}
      </div>

      <div className="muted" style={{ fontSize: 12, margin: '6px 0 0' }}>
        {serviceUp
          ? <>● {t('ml.serviceUp')} · {t('ml.asOf')} {health.as_of} · {t('ml.scheduler')}: {health.scheduler}</>
          : <span style={{ color: '#e74c3c' }}>● {t('ml.serviceDown')}</span>}
      </div>

      {error && <ErrorBox message={error} />}
      {toast && <Toast message={toast} />}

      <div className="cards" style={{ marginTop: 12 }}>
        <Kpi label={t('ml.kpiLikelyLate')} value={ml.isAvailable(delay) ? likelyLate : '—'}
          sub={ml.isAvailable(delay) ? t('ml.ofOpen', { n: delayItems.length }) : undefined} />
        <Kpi label={t('ml.kpiHighRisk')} value={ml.isAvailable(delay) ? highRisk : '—'} />
        {canFinance && <Kpi label={t('ml.kpiNextSpend')} value={nextSpend == null ? '—' : fmtMoney(nextSpend)}
          sub={ml.isAvailable<ml.SpendForecast>(spend) ? spend.items[0]?.month?.substring(0, 7) : undefined} />}
        {canFinance && <Kpi label={t('ml.kpiAnomalies')} value={ml.isAvailable<ml.Anomalies>(anoms) ? anoms.flagged_total : '—'} />}
        <Kpi label={t('ml.kpiSegments')} value={ml.isAvailable<ml.Segments>(segs) ? segs.segments.length : '—'}
          sub={ml.isAvailable<ml.Segments>(segs) ? `silhouette ${segs.silhouette?.toFixed(2)}` : undefined} />
      </div>

      <div className="row" style={{ marginTop: 14, flexWrap: 'wrap', gap: 6 }}>
        {tabs.filter(([, , show]) => show).map(([k, label]) => (
          <button key={k} className={`btn sm ${tab === k ? 'primary' : ''}`} onClick={() => setTab(k)}>{label}</button>
        ))}
      </div>

      <div style={{ marginTop: 12 }}>
        {loading && !delay ? <Loading /> : (
          <>
            {tab === 'delay' && <DelayTab r={delay} />}
            {tab === 'spend' && canFinance && <SpendTab r={spend} rtl={rtl} />}
            {tab === 'anomaly' && canFinance && <AnomalyTab r={anoms} />}
            {tab === 'segments' && <SegmentsTab r={segs} rtl={rtl} />}
            {tab === 'severity' && <SeverityTab projectOptions={projectOptions} />}
            {tab === 'models' && <ModelsTab />}
          </>
        )}
      </div>
    </div>
  )
}

// ---------------------------------------------------------------- tabs
function DelayTab({ r }: { r: any }) {
  const { t } = useTranslation()
  if (!ml.isAvailable<ml.TaskDelay>(r)) return <Unavailable r={r} />
  return (
    <div className="panel">
      <h3>{t('ml.delayTitle')}</h3>
      <p className="muted" style={{ fontSize: 12, marginTop: -6 }}>
        {t('ml.drivers')}: {r.drivers.join(' · ') || '—'} · {t('ml.model')}: {r.model} ({r.version})
      </p>
      {r.items.length === 0 ? <Empty /> : (
        <div className="table-wrap">
          <table>
            <thead><tr><th>{t('task.taskTitle')}</th><th>{t('task.priority')}</th><th>{t('task.status')}</th>
              <th>{t('task.due')}</th><th>{t('ml.delayProb')}</th><th>{t('ml.verdict')}</th></tr></thead>
            <tbody>{r.items.map((i: ml.TaskDelayItem) => (
              <tr key={i.task_id}>
                <td>{i.title}</td><td><Badge value={i.priority} /></td><td><Badge value={i.status} /></td>
                <td>{fmtDate(i.due_date || undefined)}</td><td><ProbBar p={i.probability} /></td>
                <td style={{ color: BAND_COLOR[i.risk_band] }}>{i.verdict}</td>
              </tr>))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  )
}

function SpendTab({ r, rtl }: { r: any; rtl: boolean }) {
  const { t } = useTranslation()
  const option = useMemo(() => {
    if (!ml.isAvailable<ml.SpendForecast>(r)) return null
    const hist = r.history
    const next = r.items.length ? r.items[0].month.substring(0, 7) : null
    const total = r.items.reduce((s, i) => s + i.forecast, 0)
    const low = r.items.reduce((s, i) => s + i.low, 0)
    const high = r.items.reduce((s, i) => s + i.high, 0)
    const months = [...hist.map((h) => h.month.substring(0, 7)), ...(next ? [next] : [])]
    const text = cssVar('--text', '#e6e9ef')
    return {
      textStyle: { fontFamily: 'inherit', color: text }, backgroundColor: 'transparent',
      tooltip: { trigger: 'axis' }, legend: { top: 0, textStyle: { color: cssVar('--muted', '#9aa3b2') } },
      grid: { left: 40, right: 20, top: 34, bottom: 30, containLabel: true },
      xAxis: { type: 'category', data: months, inverse: rtl },
      yAxis: { type: 'value', position: rtl ? 'right' : 'left' },
      series: [
        { name: t('ml.actual'), type: 'line', smooth: true, data: [...hist.map((h) => h.spend), ...(next ? [null] : [])],
          itemStyle: { color: '#3498db' } },
        { name: t('ml.forecast'), type: 'line', symbolSize: 12,
          data: [...hist.map((_, i) => (i === hist.length - 1 ? hist[i].spend : null)), ...(next ? [total] : [])],
          lineStyle: { type: 'dashed' }, itemStyle: { color: '#e30613' } },
        { name: t('ml.range'), type: 'scatter', symbol: 'rect', symbolSize: [4, 4],
          data: next ? [[next, low], [next, high]] : [], itemStyle: { color: '#f1c40f' } },
      ],
    }
  }, [r, rtl, t])
  if (!ml.isAvailable<ml.SpendForecast>(r)) return <Unavailable r={r} />
  return (
    <>
      <div className="panel">
        <h3>{t('ml.spendTitle')}</h3>
        <p className="muted" style={{ fontSize: 12, marginTop: -6 }}>
          {t('ml.mae')}: {fmtMoney(r.mae)} · {t('ml.drivers')}: {r.drivers.join(' · ')} · {t('ml.model')}: {r.model}
        </p>
        {option && <ReactECharts option={option} style={{ height: 300 }} notMerge />}
      </div>
      <div className="panel" style={{ marginTop: 14 }}>
        <div className="table-wrap">
          <table>
            <thead><tr><th>{t('project.name')}</th><th>{t('ml.month')}</th><th>{t('ml.lastMonth')}</th>
              <th>{t('ml.forecast')}</th><th>{t('ml.range')}</th><th>{t('ml.plan')}</th><th /></tr></thead>
            <tbody>{r.items.map((i: ml.SpendItem) => (
              <tr key={i.project_id}>
                <td>{i.code} — {i.name}</td><td>{i.month.substring(0, 7)}</td><td>{fmtMoney(i.last_month ?? undefined)}</td>
                <td><b>{fmtMoney(i.forecast)}</b></td><td className="muted">{fmtMoney(i.low)} – {fmtMoney(i.high)}</td>
                <td>{fmtMoney(i.planned_estimate ?? undefined)}</td>
                <td>{i.over_plan && <span style={{ color: '#e74c3c' }}>▲ {t('ml.overPlan')}</span>}</td>
              </tr>))}
            </tbody>
          </table>
        </div>
      </div>
    </>
  )
}

function AnomalyTab({ r }: { r: any }) {
  const { t } = useTranslation()
  if (!ml.isAvailable<ml.Anomalies>(r)) return <Unavailable r={r} />
  return (
    <div className="panel">
      <h3>{t('ml.anomalyTitle')}</h3>
      <p className="muted" style={{ fontSize: 12, marginTop: -6 }}>{t('ml.anomalyHint')} · {t('ml.model')}: {r.model}</p>
      {r.items.length === 0 ? <Empty /> : (
        <div className="table-wrap">
          <table>
            <thead><tr><th>{t('finance.date')}</th><th>{t('finance.category')}</th><th>{t('finance.amount')}</th>
              <th>{t('finance.description')}</th><th>{t('ml.score')}</th><th>{t('ml.why')}</th></tr></thead>
            <tbody>{r.items.map((i: ml.AnomalyItem) => (
              <tr key={i.transaction_id} style={i.flagged ? { background: 'rgba(231,76,60,.08)' } : undefined}>
                <td>{fmtDate(i.txn_date)}</td><td>{i.category}</td><td>{fmtMoney(i.amount)}</td><td>{i.description}</td>
                <td>{i.score.toFixed(2)} {i.flagged && <span style={{ color: '#e74c3c' }}>● {i.label}</span>}</td>
                <td className="muted">{i.reasons.join(' · ') || '—'}</td>
              </tr>))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  )
}

function SegmentsTab({ r, rtl }: { r: any; rtl: boolean }) {
  const { t } = useTranslation()
  const option = useMemo(() => {
    if (!ml.isAvailable<ml.Segments>(r)) return null
    const palette = ['#3498db', '#e74c3c', '#2ecc71', '#f39c12']
    return {
      textStyle: { fontFamily: 'inherit', color: cssVar('--text', '#e6e9ef') }, backgroundColor: 'transparent',
      legend: { top: 0, textStyle: { color: cssVar('--muted', '#9aa3b2') } },
      tooltip: { trigger: 'item', formatter: (p: any) => `${p.data.name}<br/>${t('ml.lateShare')}: ${pct(p.data.value[0])}<br/>${t('finance.utilization')}: ${pct(p.data.value[1])}` },
      grid: { left: 50, right: 30, top: 34, bottom: 40, containLabel: true },
      xAxis: { type: 'value', name: t('ml.lateShare'), min: 0, max: 1, inverse: rtl, axisLabel: { formatter: (v: number) => pct(v) } },
      yAxis: { type: 'value', name: t('finance.utilization'), position: rtl ? 'right' : 'left', axisLabel: { formatter: (v: number) => pct(v) } },
      series: r.segments.map((s, k) => ({
        name: s.segment_label, type: 'scatter', symbolSize: 18, itemStyle: { color: palette[k % palette.length] },
        label: { show: true, position: 'top', formatter: (p: any) => p.data.code, color: cssVar('--muted', '#9aa3b2'), fontSize: 10 },
        data: r.items.filter((i) => i.segment === s.segment).map((i) => ({
          name: i.name, code: i.code, value: [i.late_task_share ?? 0, i.utilization ?? 0] })),
      })),
    }
  }, [r, rtl, t])
  if (!ml.isAvailable<ml.Segments>(r)) return <Unavailable r={r} />
  return (
    <div className="grid2">
      <div className="panel"><h3>{t('ml.segmentsTitle')}</h3>{option && <ReactECharts option={option} style={{ height: 320 }} notMerge />}</div>
      <div className="panel">
        <div className="table-wrap">
          <table>
            <thead><tr><th>{t('project.name')}</th><th>{t('ml.segment')}</th><th>{t('ml.lateShare')}</th>
              <th>{t('finance.utilization')}</th><th>{t('project.progress')}</th></tr></thead>
            <tbody>{r.items.map((i: ml.SegmentItem) => (
              <tr key={i.project_id}><td>{i.code} — {i.name}</td><td><b>{i.segment_label}</b></td>
                <td>{pct(i.late_task_share)}</td><td>{pct(i.utilization)}</td>
                <td>{i.avg_progress == null ? '—' : `${Math.round(i.avg_progress)}%`}</td></tr>))}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  )
}

function SeverityTab({ projectOptions }: { projectOptions: { value: string; label: string }[] }) {
  const { t } = useTranslation()
  const [text, setText] = useState('')
  const [kind, setKind] = useState('Risk')
  const [projectId, setProjectId] = useState<string | null>(null)
  const [res, setRes] = useState<any>(null)
  const [busy, setBusy] = useState(false)
  async function suggest() {
    setBusy(true)
    try { setRes(await ml.mlRiskSeverity(text, kind, projectId)) } catch (e) { setRes({ available: false, message: (e as Error).message }) }
    finally { setBusy(false) }
  }
  return (
    <div className="grid2">
      <div className="panel">
        <h3>{t('ml.severityTitle')}</h3>
        <label className="field">{t('ml.riskText')}
          <textarea rows={4} value={text} onChange={(e) => setText(e.target.value)} placeholder={t('ml.riskTextHint')} />
        </label>
        <div className="row" style={{ marginTop: 8, gap: 8 }}>
          <div style={{ minWidth: 140 }}>
            <Select options={[{ value: 'Risk', label: t('ml.kindRisk') }, { value: 'Issue', label: t('ml.kindIssue') }]}
              value={kind} onChange={(v) => setKind(v || 'Risk')} isClearable={false} />
          </div>
          <div style={{ flex: 1 }}><Select options={projectOptions} value={projectId} onChange={setProjectId} placeholder={t('project.name')} /></div>
          <button className="btn primary" onClick={suggest} disabled={busy || text.trim().length < 3}>✨ {t('ml.suggest')}</button>
        </div>
      </div>
      <div className="panel">
        {busy ? <Loading /> : !res ? <p className="muted">{t('ml.severityHint')}</p> : !ml.isAvailable<ml.RiskSuggestion>(res) ? <Unavailable r={res} /> : (
          <>
            <div style={{ fontSize: 28, fontWeight: 800, color: BAND_COLOR[res.severity] }}>{res.severity_label}</div>
            <div className="muted" style={{ fontSize: 12 }}>{t('ml.confidence')}: {pct(res.confidence)} · {res.model}</div>
            <div style={{ marginTop: 12, display: 'grid', gap: 6 }}>
              {['High', 'Medium', 'Low'].filter((c) => c in res.probabilities).map((c) => (
                <div key={c} className="row" style={{ gap: 8 }}>
                  <span style={{ minWidth: 70 }}>{res.labels[c]}</span><ProbBar p={res.probabilities[c]} />
                </div>))}
            </div>
            <p className="muted" style={{ fontSize: 12, marginTop: 12 }}>ⓘ {res.note}</p>
          </>
        )}
      </div>
    </div>
  )
}

function ModelsTab() {
  const { t, i18n } = useTranslation()
  const [models, setModels] = useState<any>(null)
  const [mon, setMon] = useState<any>(null)
  const [hist, setHist] = useState<any>(null)
  useEffect(() => {
    Promise.all([ml.mlModels(), ml.mlMonitoring(), ml.mlHistory()])
      .then(([m, mo, h]) => { setModels(m); setMon(mo); setHist(h) })
      .catch((e) => setModels({ available: false, message: (e as Error).message }))
  }, [i18n.language])
  if (!models) return <Loading />
  if (!ml.isAvailable<{ models: ml.MlModel[] }>(models)) return <Unavailable r={models} />
  const metricKeys = ['f1', 'f1_macro', 'roc_auc', 'accuracy', 'mae', 'mape', 'r2', 'injected_recall', 'silhouette']
  return (
    <>
      {ml.isAvailable<ml.Monitoring>(mon) && (
        <div className="panel">
          <h3>{t('ml.monitoring')}</h3>
          <p style={{ margin: 0 }}>
            {mon.retrain_recommended ? <span style={{ color: '#f39c12' }}>▲ {t('ml.retrainYes')}</span>
              : <span style={{ color: '#2ecc71' }}>✔ {t('ml.retrainNo')}</span>}
            <span className="muted" style={{ fontSize: 12 }}> · {t('ml.checked')} {mon.checked_at.replace('T', ' ')}
              · {t('ml.liveAccuracy')}: {mon.live_task_delay.accuracy == null ? t('ml.pending', { n: mon.live_task_delay.pending ?? 0 }) : pct(mon.live_task_delay.accuracy)}</span>
          </p>
          <div className="table-wrap" style={{ marginTop: 8 }}>
            <table>
              <thead><tr><th>{t('ml.model')}</th><th>{t('ml.rowsTrained')}</th><th>{t('ml.rowsNow')}</th><th>{t('ml.drift')}</th></tr></thead>
              <tbody>{Object.entries(mon.problems).map(([k, p]) => (
                <tr key={k}><td>{models.models.find((m) => m.key === k)?.name || k}</td>
                  <td>{p.rows_at_training ?? '—'}</td><td>{p.rows_now ?? '—'}</td>
                  <td>{p.status === 'no_model' ? '—' : p.drift ? <span style={{ color: '#f39c12' }}>{(p.drifted_features || []).join(', ')}</span> : '✔'}</td></tr>))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {models.models.map((m) => (
        <div className="panel" key={m.key} style={{ marginTop: 14 }}>
          <div className="row">
            <h3 style={{ margin: 0 }}>{m.name}</h3>
            <span className="muted" style={{ fontSize: 12 }}>{m.type}</span>
            <div className="spacer" />
            {m.trained ? (m.beats_baseline
              ? <span style={{ color: '#2ecc71', fontSize: 12 }}>✔ {t('ml.beatsBaseline')}</span>
              : <span style={{ color: '#f39c12', fontSize: 12 }}>▲ {t('ml.notBetter')}</span>)
              : <span className="muted">{t('ml.notTrained')}</span>}
          </div>
          <p style={{ margin: '6px 0' }}>{m.objective}</p>
          <div className="muted" style={{ fontSize: 12, lineHeight: 1.7 }}>
            <div><b>{t('ml.label')}:</b> {m.label}</div>
            <div><b>{t('ml.success')}:</b> {m.success}</div>
            {m.trained && <>
              <div><b>{t('ml.best')}:</b> {m.best_model} · {m.rows} {t('ml.rows')} · v{m.version}</div>
              <div><b>{t('ml.evaluation')}:</b> {m.evaluation_strategy}</div>
            </>}
          </div>
          {m.trained && m.metrics && (
            <div className="cards" style={{ marginTop: 10 }}>
              {metricKeys.filter((k) => m.metrics![k] != null).map((k) => (
                <Kpi key={k} label={k.toUpperCase()} value={k === 'mae' ? fmtMoney(m.metrics![k] as number) : (m.metrics![k] as number).toFixed(3)}
                  sub={m.baseline?.metrics?.[k] != null ? `${t('ml.baseline')} ${k === 'mae' ? fmtMoney(m.baseline.metrics[k]) : Number(m.baseline.metrics[k]).toFixed(3)}` : undefined} />
              ))}
            </div>
          )}
          {m.charts && m.charts.length > 0 && (
            <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(260px, 1fr))', gap: 10, marginTop: 10 }}>
              {m.charts.map((c) => <ChartImg key={c} path={c} alt={c.split('/').pop() || c} />)}
            </div>
          )}
        </div>
      ))}

      {ml.isAvailable<{ runs: any[] }>(hist) && hist.runs.length > 0 && (
        <div className="panel" style={{ marginTop: 14 }}>
          <h3>{t('ml.history')}</h3>
          <div className="table-wrap">
            <table>
              <thead><tr><th>{t('ml.when')}</th><th>{t('ml.reason')}</th><th>{t('ml.seconds')}</th><th>{t('ml.results')}</th></tr></thead>
              <tbody>{hist.runs.map((r: any, i: number) => (
                <tr key={i}><td>{(r.started_at || '').replace('T', ' ')}</td><td>{r.reason}</td><td>{r.seconds}</td>
                  <td className="muted">{Object.entries<any>(r.results || {}).map(([k, v]) => `${k}: ${v.status}`).join(' · ')}</td></tr>))}
              </tbody>
            </table>
          </div>
        </div>
      )}
    </>
  )
}

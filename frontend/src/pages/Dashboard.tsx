import { useEffect, useMemo, useState, type ReactNode } from 'react'
import { useTranslation } from 'react-i18next'
import ReactECharts from 'echarts-for-react'
import * as pmo from '../api/pmo'
import type { DashboardKpis, DashboardAnalytics, DashboardFilter, Task, Risk } from '../api/types'
import { useLookups } from '../hooks/useLookups'
import { Badge, Empty, ErrorBox, Loading, Modal, fmtDate, fmtMoney } from '../components/ui'
import Select from '../components/Select'

const BASELINE_CUTOFF = '2026-09-30' // fixed baseline snapshot; production would use today.

export default function Dashboard() {
  const { t, i18n } = useTranslation()
  const lk = useLookups()
  const [filter, setFilter] = useState<DashboardFilter>({ cutoff: BASELINE_CUTOFF })
  const [kpis, setKpis] = useState<DashboardKpis | null>(null)
  const [analytics, setAnalytics] = useState<DashboardAnalytics | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [drill, setDrill] = useState<{ title: string; tasks?: Task[]; risks?: Risk[] } | null>(null)

  const load = () => {
    setLoading(true); setError('')
    Promise.all([pmo.getKpis(filter), pmo.getAnalytics(filter)])
      .then(([k, a]) => { setKpis(k); setAnalytics(a) })
      .catch((e) => setError((e as Error).message))
      .finally(() => setLoading(false))
  }
  // Recompute from the backend whenever the filter context changes (no client-side KPI patching).
  useEffect(load, [filter.deptId, filter.projectId, filter.managerId, filter.status, filter.categoryId, filter.cutoff])

  const set = (patch: Partial<DashboardFilter>) => setFilter((f) => ({ ...f, ...patch }))
  const openTasks = async (mode: string, title: string) =>
    setDrill({ title, tasks: await pmo.drilldownTasks(filter, mode) })
  const openRisks = async (title: string) =>
    setDrill({ title, risks: await pmo.drilldownRisks(filter, true) })

  return (
    <div>
      <div className="row">
        <h2 style={{ margin: 0 }}>{t('dash.title')}</h2>
        <div className="spacer" />
        <FilterBar filter={filter} set={set} lk={lk} />
      </div>

      {error && <ErrorBox message={error} />}
      {loading && !kpis ? <Loading /> : kpis && (
        <>
          <div className="section-title">{t('dash.delivery')}</div>
          <div className="cards">
            <Kpi label={t('dash.totalProjects')} value={kpis.totalProjects} />
            <Kpi label={t('dash.activeProjects')} value={kpis.activeProjects} />
            <Kpi label={t('dash.portfolioProgress')} value={`${Math.round(kpis.portfolioProgress)}%`} />
            <Kpi label={t('dash.overdueTasks')} value={kpis.overdueTasks} onClick={() => openTasks('overdue', t('dash.overdueTasks'))} />
            <Kpi label={t('dash.milestonesDue')} value={kpis.milestonesDue14} />
          </div>

          <div className="section-title">{t('dash.riskGov')}</div>
          <div className="cards">
            <Kpi label={t('dash.openHighRisks')} value={kpis.openHighExposureRisks} onClick={() => openRisks(t('dash.drillRisks'))} />
            <Kpi label={t('dash.blockedTasks')} value={kpis.blockedTasks} onClick={() => openTasks('blocked', t('dash.blockedTasks'))} />
            <Kpi label={t('dash.pendingCrs')} value={kpis.pendingChangeRequests} />
          </div>

          <div className="section-title">{t('dash.financial')}</div>
          <div className="cards">
            <Kpi label={t('dash.approvedBudget')} value={fmtMoney(kpis.approvedBudget, kpis.currency)} />
            <Kpi label={t('dash.actualSpend')} value={fmtMoney(kpis.actualSpend, kpis.currency)} />
            <Kpi label={t('dash.remaining')} value={fmtMoney(kpis.remainingBudget, kpis.currency)} />
            <Kpi label={t('dash.utilization')} value={kpis.utilizationPct == null ? '—' : `${Math.round(kpis.utilizationPct * 100)}%`} />
            <Kpi label={t('dash.eac')} value={fmtMoney(kpis.eac, kpis.currency)} />
            <Kpi label={t('dash.overrunCount')} value={kpis.projectedOverrunCount} sub={fmtMoney(kpis.projectedOverrunAmount, kpis.currency)} />
          </div>

          {analytics && <Charts analytics={analytics} rtl={i18n.language === 'ar'} />}
        </>
      )}

      {drill && (
        <Modal title={drill.title} onClose={() => setDrill(null)}
          footer={<button className="btn" onClick={() => setDrill(null)}>{t('common.close')}</button>}>
          <div className="full table-wrap">
            {drill.tasks && (drill.tasks.length === 0 ? <Empty /> : (
              <table><thead><tr>
                <th>{t('task.taskTitle')}</th><th>{t('project.title')}</th><th>{t('task.assignee')}</th>
                <th>{t('task.status')}</th><th>{t('task.due')}</th></tr></thead>
                <tbody>{drill.tasks.map((x) => (
                  <tr key={x.taskId}><td>{x.title}</td><td>{x.projectName}</td>
                    <td>{x.assigneeName || t('task.unassigned')}</td><td><Badge value={x.status} /></td>
                    <td>{fmtDate(x.dueDate)}</td></tr>))}</tbody>
              </table>))}
            {drill.risks && (drill.risks.length === 0 ? <Empty /> : (
              <table><thead><tr>
                <th>{t('risk.riskTitle')}</th><th>{t('project.title')}</th><th>{t('risk.score')}</th>
                <th>{t('risk.status')}</th></tr></thead>
                <tbody>{drill.risks.map((x) => (
                  <tr key={x.riskId}><td>{x.title}</td><td>{x.projectName}</td><td><b>{x.score}</b></td>
                    <td><Badge value={x.status} /></td></tr>))}</tbody>
              </table>))}
          </div>
        </Modal>
      )}
    </div>
  )
}

function Kpi({ label, value, sub, onClick }: { label: string; value: React.ReactNode; sub?: string; onClick?: () => void }) {
  return (
    <div className="kpi" onClick={onClick} style={onClick ? { cursor: 'pointer' } : undefined} title={onClick ? '⇲' : undefined}>
      <div className="label">{label}</div>
      <div className="value">{value}</div>
      {sub && <div className="sub">{sub}</div>}
    </div>
  )
}

function FilterBar({ filter, set, lk }: { filter: DashboardFilter; set: (p: Partial<DashboardFilter>) => void; lk: ReturnType<typeof useLookups> }) {
  const { t } = useTranslation()
  const w = (node: ReactNode) => <div style={{ minWidth: 190 }}>{node}</div>
  return (
    <div className="row">
      {w(<Select options={lk.departments.map((d) => ({ value: d.id, label: d.label }))}
        value={filter.deptId} onChange={(v) => set({ deptId: v || undefined })}
        placeholder={`${t('project.department')}: ${t('common.all')}`} />)}
      {w(<Select options={lk.managers.map((m) => ({ value: m.id, label: m.label }))}
        value={filter.managerId} onChange={(v) => set({ managerId: v || undefined })}
        placeholder={`${t('project.owner')}: ${t('common.all')}`} />)}
      {w(<Select options={lk.projects.map((p) => ({ value: p.id, label: p.label }))}
        value={filter.projectId} onChange={(v) => set({ projectId: v || undefined })}
        placeholder={`${t('project.title')}: ${t('common.all')}`} />)}
      {w(<Select options={['Planning', 'Active', 'OnHold', 'Completed', 'Cancelled'].map((s) => ({ value: s, label: t(`status.${s}`) }))}
        value={filter.status} onChange={(v) => set({ status: v || undefined })}
        placeholder={`${t('project.status')}: ${t('common.all')}`} />)}
      {w(<Select options={lk.categories.map((c) => ({ value: c.categoryId, label: c.name }))}
        value={filter.categoryId} onChange={(v) => set({ categoryId: v || undefined })}
        placeholder={`${t('finance.category')}: ${t('common.all')}`} />)}
      <input type="date" value={filter.cutoff || ''} onChange={(e) => set({ cutoff: e.target.value || undefined })} title={t('dash.cutoff')} />
    </div>
  )
}

function Charts({ analytics, rtl }: { analytics: DashboardAnalytics; rtl: boolean }) {
  const { t } = useTranslation()
  const base = useMemo(() => ({
    textStyle: { fontFamily: 'inherit', color: '#e6e9ef' },
    grid: { left: 40, right: 20, top: 30, bottom: 30, containLabel: true },
    tooltip: { trigger: 'axis' as const },
    backgroundColor: 'transparent',
  }), [])

  const health = analytics.projectHealth
  const bva = analytics.budgetVsActual
  const trend = analytics.monthlyTrend.map((m) => ({ x: (m.period || m.month || '').substring(0, 7), y: m.amount }))
  const cats = analytics.spendByCategory.map((c) => ({ name: c.categoryName || c.category || '—', value: c.amount }))
  const matrix = analytics.riskMatrix.map((c) => [c.impact, c.probability, c.count])
  const owners = analytics.overdueByOwner

  const budgetVsActual = {
    ...base, legend: { top: 0, textStyle: { color: '#9aa3b2' } },
    tooltip: { trigger: 'axis' as const },
    xAxis: { type: 'category' as const, data: bva.map((b) => b.projectName), axisLabel: { interval: 0, rotate: 30 } },
    yAxis: { type: 'value' as const },
    series: [
      { name: t('finance.approvedBudget'), type: 'bar', data: bva.map((b) => b.approvedBudget), itemStyle: { color: '#3498db' } },
      { name: t('finance.actualSpend'), type: 'bar', data: bva.map((b) => b.actualSpend), itemStyle: { color: '#e30613' } },
      { name: t('finance.eac'), type: 'bar', data: bva.map((b) => b.eac), itemStyle: { color: '#f1c40f' } },
    ],
  }
  const monthly = {
    ...base, xAxis: { type: 'category' as const, data: trend.map((p) => p.x) },
    yAxis: { type: 'value' as const },
    series: [{ type: 'line', smooth: true, areaStyle: {}, data: trend.map((p) => p.y), itemStyle: { color: '#2ecc71' } }],
  }
  const byCategory = {
    ...base, tooltip: { trigger: 'item' as const }, legend: { bottom: 0, textStyle: { color: '#9aa3b2' } },
    series: [{ type: 'pie', radius: ['45%', '70%'], data: cats, label: { color: '#e6e9ef' } }],
  }
  const risk = {
    ...base, tooltip: { trigger: 'item' as const, formatter: (p: any) => `P${p.value[1]} × I${p.value[0]}: ${p.value[2]}` },
    xAxis: { type: 'value' as const, min: 0, max: 6, name: t('risk.impact') },
    yAxis: { type: 'value' as const, min: 0, max: 6, name: t('risk.probability') },
    series: [{ type: 'scatter', data: matrix, symbolSize: (v: number[]) => 10 + v[2] * 8,
      itemStyle: { color: '#e74c3c' }, label: { show: true, formatter: (p: any) => p.value[2], color: '#fff' } }],
  }
  const health_ = {
    ...base, tooltip: { trigger: 'item' as const },
    xAxis: { type: 'value' as const, min: 0, max: 100, name: t('project.progress') },
    yAxis: { type: 'value' as const, name: t('finance.utilization'), axisLabel: { formatter: (v: number) => `${Math.round(v * 100)}%` } },
    series: [{ type: 'scatter', symbolSize: 16,
      data: health.map((h) => ({ value: [h.progress, h.utilizationPct ?? 0], name: h.projectName,
        itemStyle: { color: h.isProjectedOverrun ? '#e74c3c' : '#2ecc71' } })),
      label: { show: true, formatter: (p: any) => p.name, position: 'top', color: '#9aa3b2', fontSize: 10 } }],
  }
  const ownersChart = {
    ...base, xAxis: { type: 'value' as const }, yAxis: { type: 'category' as const, data: owners.map((o) => o.ownerName), inverse: true },
    series: [{ type: 'bar', data: owners.map((o) => o.overdueCount), itemStyle: { color: '#f39c12' } }],
  }

  const C = (opt: any) => <ReactECharts option={opt} style={{ height: 280 }} opts={{ renderer: 'canvas' }} notMerge />
  return (
    <>
      <div className="grid2" style={{ marginTop: 14 }}>
        <div className="panel"><h3>{t('dash.budgetVsActual')} ({t('common.currency')})</h3>{C(budgetVsActual)}</div>
        <div className="panel"><h3>{t('dash.monthlyTrend')} ({t('common.currency')})</h3>{C(monthly)}</div>
        <div className="panel"><h3>{t('dash.spendByCategory')} ({t('common.currency')})</h3>{C(byCategory)}</div>
        <div className="panel"><h3>{t('dash.riskMatrix')}</h3>{C(risk)}</div>
        <div className="panel"><h3>{t('dash.projectHealth')}</h3>{C(health_)}</div>
        <div className="panel"><h3>{t('dash.overdueByOwner')}</h3>{C(ownersChart)}</div>
      </div>
      <div className="panel" style={{ marginTop: 14 }}>
        <h3>{t('dash.milestoneRoadmap')}</h3>
        <div className="table-wrap">
          <table><thead><tr><th>{t('project.title')}</th><th>{t('milestone.name')}</th><th>{t('milestone.due')}</th><th>{t('milestone.status')}</th></tr></thead>
            <tbody>{analytics.milestoneRoadmap.map((m, i) => (
              <tr key={i}><td>{m.projectName}</td><td>{m.name}</td><td>{fmtDate(m.dueDate)}</td><td><Badge value={m.status} /></td></tr>))}</tbody>
          </table>
        </div>
      </div>
    </>
  )
}

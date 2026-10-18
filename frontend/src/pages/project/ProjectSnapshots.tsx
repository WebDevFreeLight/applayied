import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import ReactECharts from 'echarts-for-react'
import * as pmo from '../../api/pmo'
import type { ProgressSnapshot } from '../../api/types'
import { Empty, ErrorBox, Loading, fmtDate } from '../../components/ui'

// Historical progress trend (pmo_progress_snapshot) — source-of-truth captured points, not derived.
export default function ProjectSnapshots({ projectId }: { projectId: string }) {
  const { t } = useTranslation()
  const [rows, setRows] = useState<ProgressSnapshot[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  useEffect(() => {
    setLoading(true); setError('')
    pmo.listSnapshots({ projectId, pageNum: 1, pageSize: 200 }).then((p) => setRows(p.result || []))
      .catch((e) => setError((e as Error).message)).finally(() => setLoading(false))
  }, [projectId])

  if (loading) return <Loading />
  if (error) return <ErrorBox message={error} />
  if (rows.length === 0) return <Empty />

  const ordered = [...rows].sort((a, b) => a.snapshotDate.localeCompare(b.snapshotDate))
  const chart = {
    textStyle: { color: 'var(--text)', fontFamily: 'inherit' }, backgroundColor: 'transparent',
    grid: { left: 40, right: 20, top: 20, bottom: 30, containLabel: true }, tooltip: { trigger: 'axis' as const },
    xAxis: { type: 'category' as const, data: ordered.map((s) => fmtDate(s.snapshotDate)) },
    yAxis: { type: 'value' as const, max: 100 },
    series: [{ type: 'line', smooth: true, areaStyle: {}, data: ordered.map((s) => s.progressPct), itemStyle: { color: '#2ecc71' } }],
  }
  return (
    <div>
      <div className="panel"><h3>{t('project.progress')} %</h3><ReactECharts option={chart} style={{ height: 240 }} notMerge /></div>
      <div className="table-wrap" style={{ marginTop: 12 }}>
        <table><thead><tr>
          <th>{t('snapshot.date')}</th><th>{t('project.progress')}</th><th>{t('snapshot.tasksDone')}</th>
          <th>{t('snapshot.openRisks')}</th></tr></thead>
          <tbody>{[...rows].reverse().map((s) => (
            <tr key={s.snapshotId}><td>{fmtDate(s.snapshotDate)}</td><td>{s.progressPct}%</td>
              <td>{s.tasksDone}/{s.tasksTotal}</td><td>{s.openRiskCount}</td></tr>))}</tbody>
        </table>
      </div>
    </div>
  )
}

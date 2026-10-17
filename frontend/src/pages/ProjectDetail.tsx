import { useEffect, useState } from 'react'
import { useParams, useNavigate } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import * as pmo from '../api/pmo'
import type { Project } from '../api/types'
import { Badge, ErrorBox, Loading, fmtDate, fmtMoney } from '../components/ui'
import Tasks from './Tasks'
import Milestones from './Milestones'
import Risks from './Risks'
import ChangeRequests from './ChangeRequests'
import Finance from './Finance'

// Project workspace: header + tabbed, project-scoped modules. Creating a task / milestone / risk /
// change request / cost from here is automatically tied to this project (the module components are
// rendered with the projectId pinned, so their project selector is hidden and new items pre-fill it).
const TABS = ['overview', 'tasks', 'milestones', 'risks', 'change-requests', 'finance'] as const
type Tab = typeof TABS[number]

export default function ProjectDetail() {
  const { projectId = '' } = useParams()
  const nav = useNavigate()
  const { t } = useTranslation()
  const [project, setProject] = useState<Project | null>(null)
  const [tab, setTab] = useState<Tab>('overview')
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  useEffect(() => {
    setLoading(true); setError('')
    pmo.getProject(projectId).then(setProject).catch((e) => setError((e as Error).message)).finally(() => setLoading(false))
  }, [projectId])

  const label: Record<Tab, string> = {
    overview: t('project.overview'), tasks: t('nav.tasks'), milestones: t('nav.milestones'),
    risks: t('nav.risks'), 'change-requests': t('nav.changeRequests'), finance: t('nav.finance'),
  }

  if (loading) return <Loading />
  if (error) return <ErrorBox message={error} />
  if (!project) return null

  return (
    <div>
      <div className="row">
        <button className="btn sm" onClick={() => nav('/projects')}>{t('project.back')}</button>
        <h2 style={{ margin: 0 }}>{project.code} · {project.name}</h2>
        <Badge value={project.status} /><Badge value={project.priority} />
      </div>

      {/* tab bar */}
      <div className="row" style={{ marginTop: 12, gap: 6, borderBottom: '1px solid var(--border)', paddingBottom: 8 }}>
        {TABS.map((x) => (
          <button key={x} className={`btn sm ${tab === x ? 'primary' : ''}`} onClick={() => setTab(x)}>{label[x]}</button>
        ))}
      </div>

      <div style={{ marginTop: 16 }}>
        {tab === 'overview' && (
          <div className="grid2">
            <div className="panel">
              <h3>{t('project.details')}</h3>
              <table><tbody>
                <Row k={t('project.department')} v={project.deptName} />
                <Row k={t('project.owner')} v={project.ownerName} />
                <Row k={t('project.status')} v={<Badge value={project.status} />} />
                <Row k={t('project.priority')} v={<Badge value={project.priority} />} />
                <Row k={t('project.startDate')} v={fmtDate(project.startDate)} />
                <Row k={t('project.plannedEnd')} v={fmtDate(project.plannedEndDate)} />
                <Row k={t('project.actualEnd')} v={fmtDate(project.actualEndDate)} />
                <Row k={t('project.budget')} v={fmtMoney(project.initialApprovedBudget, project.currency)} />
                <Row k={t('project.tasks')} v={project.taskCount ?? '—'} />
                <Row k={t('project.progress')} v={project.progressPct != null ? `${Math.round(project.progressPct)}%` : '—'} />
              </tbody></table>
            </div>
            <div className="panel">
              <h3>{t('project.description')}</h3>
              <p className="muted" style={{ whiteSpace: 'pre-wrap' }}>{project.description || '—'}</p>
            </div>
          </div>
        )}
        {tab === 'tasks' && <Tasks projectId={projectId} />}
        {tab === 'milestones' && <Milestones projectId={projectId} />}
        {tab === 'risks' && <Risks projectId={projectId} />}
        {tab === 'change-requests' && <ChangeRequests projectId={projectId} />}
        {tab === 'finance' && <Finance projectId={projectId} />}
      </div>
    </div>
  )
}

function Row({ k, v }: { k: string; v: React.ReactNode }) {
  return <tr><td className="muted" style={{ width: 160 }}>{k}</td><td>{v}</td></tr>
}

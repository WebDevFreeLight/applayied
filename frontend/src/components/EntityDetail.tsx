import type { ReactNode } from 'react'
import { useNavigate } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { Modal } from './ui'

// Reusable read-only detail popup used by every module list (Tasks, Milestones, Risks, Change
// Requests). Shows the full record as a key/value table and links to the parent project workspace.
export function EntityDetail({ title, rows, projectId, onClose, extra, wide }: {
  title: string
  rows: [string, ReactNode][]
  projectId?: string
  onClose: () => void
  extra?: ReactNode
  wide?: boolean
}) {
  const { t } = useTranslation()
  const nav = useNavigate()
  return (
    <Modal title={title} onClose={onClose} wide={wide || !!extra}
      footer={<>
        {projectId && <button className="btn primary" onClick={() => nav(`/projects/${projectId}`)}>{t('nav.projects')} →</button>}
        <button className="btn" onClick={onClose}>{t('common.close')}</button>
      </>}>
      <table className="full"><tbody>
        {rows.map(([k, v], i) => (
          <tr key={i}><td className="muted" style={{ width: 150, verticalAlign: 'top' }}>{k}</td><td>{v ?? '—'}</td></tr>
        ))}
      </tbody></table>
      {extra && <div className="full" style={{ marginTop: 16, borderTop: '1px solid var(--border)', paddingTop: 12 }}>{extra}</div>}
    </Modal>
  )
}

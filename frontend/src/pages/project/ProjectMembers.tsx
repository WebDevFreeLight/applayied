import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import * as pmo from '../../api/pmo'
import type { ProjectMember } from '../../api/types'
import { useLookups } from '../../hooks/useLookups'
import { useAuth } from '../../auth/auth'
import { Empty, ErrorBox, Loading, Toast } from '../../components/ui'
import Select from '../../components/Select'

// Project membership module — drives Contributor scoped access (pmo_project_member).
export default function ProjectMembers({ projectId }: { projectId: string }) {
  const { t } = useTranslation()
  const lk = useLookups()
  const { hasRole } = useAuth()
  const [rows, setRows] = useState<ProjectMember[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [add, setAdd] = useState<{ userId: string; roleInProject: string }>({ userId: '', roleInProject: '' })
  const [toast, setToast] = useState('')
  const notify = (m: string) => { setToast(m); setTimeout(() => setToast(''), 2000) }

  const load = () => {
    setLoading(true); setError('')
    pmo.listMembers({ projectId, pageNum: 1, pageSize: 200 }).then((p) => setRows(p.result || []))
      .catch((e) => setError((e as Error).message)).finally(() => setLoading(false))
  }
  useEffect(load, [projectId])

  const save = async () => {
    if (!add.userId) return
    try { await pmo.addMember({ projectId, userId: add.userId, roleInProject: add.roleInProject || 'Member' }); setAdd({ userId: '', roleInProject: '' }); notify(t('common.saved')); load() }
    catch (e) { setError((e as Error).message) }
  }
  const remove = async (m: ProjectMember) => {
    try { await pmo.removeMember(m.memberId); notify(t('common.saved')); load() } catch (e) { setError((e as Error).message) }
  }

  return (
    <div>
      {hasRole('pm') && (
        <div className="row panel" style={{ marginBottom: 12 }}>
          <div style={{ minWidth: 220 }}><Select options={lk.users.map((u) => ({ value: u.id, label: u.name }))}
            value={add.userId} onChange={(v) => setAdd((a) => ({ ...a, userId: v || '' }))} placeholder={t('member.user')} /></div>
          <input placeholder={t('member.role')} value={add.roleInProject} onChange={(e) => setAdd((a) => ({ ...a, roleInProject: e.target.value }))} />
          <button className="btn primary" disabled={!add.userId} onClick={save}>{t('member.add')}</button>
        </div>
      )}
      {error && <ErrorBox message={error} />}
      {loading ? <Loading /> : rows.length === 0 ? <Empty /> : (
        <div className="table-wrap">
          <table><thead><tr><th>{t('member.user')}</th><th>{t('member.role')}</th>{hasRole('pm') && <th>{t('common.actions')}</th>}</tr></thead>
            <tbody>{rows.map((m) => (
              <tr key={m.memberId}><td>{m.userName}</td><td>{m.roleInProject}</td>
                {hasRole('pm') && <td><button className="btn sm" onClick={() => remove(m)}>{t('common.delete')}</button></td>}</tr>))}</tbody>
          </table>
        </div>
      )}
      {toast && <Toast message={toast} />}
    </div>
  )
}

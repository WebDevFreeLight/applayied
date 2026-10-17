import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import * as sys from '../../api/system'
import type { SysJob } from '../../api/system'
import { useAuth } from '../../auth/auth'
import { Empty, ErrorBox, Loading, Modal, Toast } from '../../components/ui'

const P = {
  list: 'monitor:job:list', add: 'monitor:job:add', edit: 'monitor:job:edit', remove: 'monitor:job:delete',
  start: 'monitor:job:start', stop: 'monitor:job:stop', run: 'monitor:job:run',
}

export default function Jobs() {
  const { t } = useTranslation()
  const { hasPerm } = useAuth()
  const [rows, setRows] = useState<SysJob[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [edit, setEdit] = useState<Partial<SysJob> | null>(null)
  const [toast, setToast] = useState('')
  const notify = (m: string) => { setToast(m); setTimeout(() => setToast(''), 2500) }

  const load = () => {
    setLoading(true); setError('')
    sys.listJobs({ pageNum: 1, pageSize: 50 }).then((p) => setRows(p.result || []))
      .catch((e) => setError((e as Error).message)).finally(() => setLoading(false))
  }
  useEffect(load, [])
  const act = async (a: 'start' | 'stop' | 'run', j: SysJob) => {
    try { await sys.jobAction(a, j.id); notify(t('common.saved')); load() } catch (e) { setError((e as Error).message) }
  }
  const save = async () => {
    if (!edit) return
    try { if (edit.id) await sys.updateJob(edit); else await sys.createJob(edit); setEdit(null); notify(t('common.saved')); load() }
    catch (e) { setError((e as Error).message) }
  }
  const remove = async (j: SysJob) => {
    if (!confirm(`${t('common.delete')}: ${j.name}?`)) return
    try { await sys.deleteJob(j.id); notify(t('common.saved')); load() } catch (e) { setError((e as Error).message) }
  }

  if (!hasPerm(P.list)) return <div className="panel">{t('admin.noAccess')}</div>
  const canAct = hasPerm(P.edit) || hasPerm(P.remove) || hasPerm(P.start) || hasPerm(P.run)
  return (
    <div>
      <div className="row"><h2 style={{ margin: 0 }}>{t('sys.jobs')}</h2><div className="spacer" />
        {hasPerm(P.add) && <button className="btn primary" onClick={() => setEdit({ triggerType: 1, taskType: 1 })}>{t('common.add')}</button>}
      </div>
      {error && <ErrorBox message={error} />}
      {loading ? <Loading /> : rows.length === 0 ? <Empty /> : (
        <div className="table-wrap" style={{ marginTop: 12 }}>
          <table><thead><tr><th>{t('sys.jobName')}</th><th>{t('sys.jobGroup')}</th><th>{t('sys.cron')}</th><th>{t('sys.result')}</th>{canAct && <th>{t('common.actions')}</th>}</tr></thead>
            <tbody>{rows.map((j) => (
              <tr key={j.id}><td>{j.name}</td><td>{j.jobGroup}</td><td><code>{j.cron}</code></td>
                <td><span className={`badge ${j.isStart ? 'green' : 'gray'}`}>{j.isStart ? t('sys.running') : t('sys.stopped')}</span></td>
                {canAct && <td className="row">
                  {hasPerm(P.run) && <button className="btn sm" onClick={() => act('run', j)}>{t('sys.runNow')}</button>}
                  {!j.isStart && hasPerm(P.start) && <button className="btn sm" onClick={() => act('start', j)}>{t('sys.start')}</button>}
                  {j.isStart && hasPerm(P.stop) && <button className="btn sm" onClick={() => act('stop', j)}>{t('sys.stop')}</button>}
                  {hasPerm(P.edit) && <button className="btn sm" onClick={() => setEdit({ ...j })}>{t('common.edit')}</button>}
                  {hasPerm(P.remove) && <button className="btn sm" onClick={() => remove(j)}>{t('common.delete')}</button>}
                </td>}</tr>))}</tbody>
          </table>
        </div>
      )}
      {edit && (
        <Modal title={edit.id ? t('common.edit') : t('common.add')} onClose={() => setEdit(null)}
          footer={<><button className="btn" onClick={() => setEdit(null)}>{t('common.cancel')}</button><button className="btn primary" onClick={save}>{t('common.save')}</button></>}>
          <label className="field">{t('sys.jobName')}<input value={edit.name || ''} onChange={(e) => setEdit({ ...edit, name: e.target.value })} /></label>
          <label className="field">{t('sys.jobGroup')}<input value={edit.jobGroup || ''} onChange={(e) => setEdit({ ...edit, jobGroup: e.target.value })} /></label>
          <label className="field">{t('sys.cron')}<input value={edit.cron || ''} onChange={(e) => setEdit({ ...edit, cron: e.target.value })} placeholder="0 0/5 * * * ?" /></label>
          <label className="field full">{t('common.actions')}<input value={edit.remark || ''} onChange={(e) => setEdit({ ...edit, remark: e.target.value })} /></label>
        </Modal>
      )}
      {toast && <Toast message={toast} />}
    </div>
  )
}

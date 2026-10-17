import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import * as sys from '../../api/system'
import type { ServerInfo } from '../../api/system'
import { ErrorBox, Loading } from '../../components/ui'

// Read-only server monitor (CPU / memory / OS / runtime). Shape varies by backend, so we render
// whatever sections are present defensively.
export default function Server() {
  const { t } = useTranslation()
  const [info, setInfo] = useState<ServerInfo | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  useEffect(() => {
    sys.serverInfo().then(setInfo).catch((e) => setError((e as Error).message)).finally(() => setLoading(false))
  }, [])

  const Section = ({ title, obj }: { title: string; obj?: Record<string, unknown> }) => {
    if (!obj) return null
    return (
      <div className="panel">
        <h3>{title}</h3>
        <table><tbody>{Object.entries(obj).map(([k, v]) => (
          <tr key={k}><td className="muted">{k}</td><td>{typeof v === 'object' ? JSON.stringify(v) : String(v)}</td></tr>
        ))}</tbody></table>
      </div>
    )
  }

  if (loading) return <Loading />
  if (error) return <ErrorBox message={error} />
  // Render whatever object-valued sections the backend returns (cpu, disk, sys, app, …).
  const sections = Object.entries(info || {}).filter(([, v]) => v && typeof v === 'object' && !Array.isArray(v))
  return (
    <div>
      <h2 style={{ margin: 0 }}>{t('sys.server')}</h2>
      <div className="grid2" style={{ marginTop: 12 }}>
        {sections.map(([k, v]) => <Section key={k} title={k} obj={v as Record<string, unknown>} />)}
      </div>
    </div>
  )
}

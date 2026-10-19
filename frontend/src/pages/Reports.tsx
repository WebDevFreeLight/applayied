import { useEffect, useRef, useState } from 'react'
import { useTranslation } from 'react-i18next'
import * as pmo from '../api/pmo'
import { ErrorBox, Loading } from '../components/ui'

// Loads the Power BI JS client (UMD) from the CDN on demand, so the app bundle stays lean and the
// build never depends on the package being installed. Embedding needs the internet anyway (it calls
// the Power BI service), so a runtime load is appropriate.
function loadPbiSdk(): Promise<any> {
  return new Promise((resolve, reject) => {
    const w = window as any
    if (w['powerbi-client']) return resolve(w['powerbi-client'])
    const s = document.createElement('script')
    s.src = 'https://cdnjs.cloudflare.com/ajax/libs/powerbi-client/2.23.1/powerbi.min.js'
    s.async = true
    s.onload = () => resolve((window as any)['powerbi-client'])
    s.onerror = () => reject(new Error('Failed to load the Power BI SDK'))
    document.head.appendChild(s)
  })
}

// Power BI reporting page. Asks the backend for an "embed for your organization" token
// (pmo:report:view) and renders the report with the JS SDK. Until a service principal is
// configured (appsettings PowerBI), it shows the published report preview + setup steps.
export default function Reports() {
  const { t } = useTranslation()
  const host = useRef<HTMLDivElement>(null)
  const [state, setState] = useState<'loading' | 'embedded' | 'notConfigured' | 'error'>('loading')
  const [msg, setMsg] = useState('')
  const muted: React.CSSProperties = { opacity: 0.68, lineHeight: 1.55 }

  const load = async () => {
    setState('loading'); setMsg('')
    try {
      const embed = await pmo.powerBiEmbed()
      if (!embed.configured || !embed.token) {
        setMsg(embed.error || ''); setState('notConfigured'); return
      }
      const pbi = await loadPbiSdk()
      const models = pbi.models
      const w = window as any
      const svc = w.powerbi || new pbi.service.Service(pbi.factories.hpmFactory, pbi.factories.wpmpFactory, pbi.factories.routerFactory)
      if (host.current) {
        svc.reset(host.current)
        svc.embed(host.current, {
          type: 'report',
          id: embed.reportId,
          embedUrl: embed.embedUrl,
          accessToken: embed.token,
          tokenType: models.TokenType.Embed,
          settings: {
            panes: { filters: { visible: false }, pageNavigation: { visible: true } },
            background: models.BackgroundType.Transparent,
          },
        })
      }
      setState('embedded')
    } catch (e) {
      setMsg((e as Error).message); setState('error')
    }
  }

  useEffect(() => {
    load()
    return () => {
      const w = window as any
      try { if (host.current && w.powerbi) w.powerbi.reset(host.current) } catch { /* noop */ }
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  return (
    <div>
      <div className="row">
        <h2>{t('reports.title')}</h2>
        <div className="spacer" />
        <button className="btn sm" onClick={load}>↻ {t('reports.refresh')}</button>
      </div>
      <p style={{ ...muted, marginTop: -4, marginBottom: 14 }}>{t('reports.subtitle')}</p>

      {state === 'loading' && <Loading />}
      {state === 'error' && <ErrorBox message={`${t('reports.error')}: ${msg}`} />}

      {/* Embed host — kept mounted so the SDK always has a container; shown only once embedded. */}
      <div
        ref={host}
        style={{ height: '72vh', width: '100%', borderRadius: 12, overflow: 'hidden', display: state === 'embedded' ? 'block' : 'none' }}
      />

      {state === 'notConfigured' && (
        <div className="panel">
          <h3>{t('reports.notConfiguredTitle')}</h3>
          <p style={muted}>{t('reports.notConfiguredBody')}</p>
          <p style={{ ...muted, fontSize: 12 }}>{t('reports.steps')}</p>
          {msg && <p style={{ ...muted, fontSize: 12 }}>{msg}</p>}
          <div style={{ marginTop: 12, border: '1px solid rgba(128,128,128,.3)', borderRadius: 12, overflow: 'hidden' }}>
            <img src="/powerbi-sample.png" alt="Power BI report preview" style={{ width: '100%', display: 'block' }} />
          </div>
          <div style={{ ...muted, fontSize: 12, marginTop: 6 }}>{t('reports.previewNote')}</div>
        </div>
      )}
    </div>
  )
}

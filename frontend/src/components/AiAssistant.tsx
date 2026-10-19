import { useEffect, useRef, useState } from 'react'
import { useTranslation } from 'react-i18next'
import * as pmo from '../api/pmo'
import type { Project } from '../api/types'
import Select from './Select'

interface Msg { role: 'user' | 'bot'; text: string }

// Floating AI assistant available on every page. A scope selector grounds each question in a
// specific project, the whole portfolio, or a cross-project comparison — the selected scope's
// facts are sent as context to the multi-provider chat endpoint so answers are data-aware.
export default function AiAssistant() {
  const { t, i18n } = useTranslation()
  const [open, setOpen] = useState(false)
  const [msgs, setMsgs] = useState<Msg[]>([])
  const [input, setInput] = useState('')
  const [busy, setBusy] = useState(false)
  const [configured, setConfigured] = useState<boolean | null>(null)
  const [projects, setProjects] = useState<Project[]>([])
  const [scope, setScope] = useState<string>('all')   // 'all' | 'compare' | <projectId>
  const bodyRef = useRef<HTMLDivElement>(null)

  useEffect(() => {
    if (!open) return
    if (configured === null) pmo.aiStatus().then((s) => setConfigured(s.configured)).catch(() => setConfigured(false))
    if (projects.length === 0) {
      pmo.listProjects({ pageNum: 1, pageSize: 100 }).then((p) => setProjects(p.result || [])).catch(() => { /* lookup optional */ })
    }
  }, [open, configured, projects.length])

  useEffect(() => { bodyRef.current?.scrollTo(0, bodyRef.current.scrollHeight) }, [msgs, busy])

  const scopeOptions = [
    { value: 'all', label: `🗂 ${t('ai.scopeAll')}` },
    { value: 'compare', label: `⚖ ${t('ai.scopeCompare')}` },
    ...projects.map((p) => ({ value: String(p.projectId), label: `${p.code} · ${p.name}` })),
  ]

  // Compact fact row per project (fields all roles can read via the project list).
  const row = (p: Project) => ({
    code: p.code, name: p.name, status: p.status, owner: p.ownerName,
    budget: p.initialApprovedBudget, currency: p.currency,
    progressPct: p.progressPct != null ? Math.round(p.progressPct) : null, tasks: p.taskCount,
  })

  // Build the grounding context + an optional instruction prefix for the current scope.
  const buildScope = (): { context?: string; prefix: string } => {
    if (projects.length === 0) return { prefix: '' }
    if (scope === 'all')
      return { prefix: '', context: `Portfolio of ${projects.length} projects: ${JSON.stringify(projects.map(row))}` }
    if (scope === 'compare')
      return { prefix: 'Compare these projects — contrast budget, progress and status, and call out the outliers. ', context: JSON.stringify(projects.map(row)) }
    const p = projects.find((x) => String(x.projectId) === scope)
    return p ? { prefix: `Answer only about the project "${p.name}". `, context: `Project: ${JSON.stringify(row(p))}` } : { prefix: '' }
  }

  const send = async (text: string) => {
    const q = text.trim()
    if (!q || busy) return
    const { context, prefix } = buildScope()
    setMsgs((m) => [...m, { role: 'user', text: q }])
    setInput('')
    setBusy(true)
    try {
      const r = await pmo.aiChat(prefix + q, context, i18n.language)
      setMsgs((m) => [...m, { role: 'bot', text: r.text }])
    } catch (e) {
      setMsgs((m) => [...m, { role: 'bot', text: (e as Error).message }])
    } finally { setBusy(false) }
  }

  const scopeLabel = scopeOptions.find((o) => o.value === scope)?.label || ''

  return (
    <>
      <button className="ai-fab" title={t('ai.title')} onClick={() => setOpen((o) => !o)}>{open ? '✕' : '✨'}</button>
      {open && (
        <div className="ai-panel">
          <div className="ai-head">
            <strong>{t('ai.title')}</strong>
            <span className="muted" style={{ fontSize: 11 }}>
              {configured === false ? t('ai.offline') : t('ai.online')}
            </span>
          </div>
          <div style={{ padding: '8px 12px 0' }}>
            <label className="muted" style={{ fontSize: 11 }}>{t('ai.scopeLabel')}</label>
            <Select options={scopeOptions} value={scope} isClearable={false}
              onChange={(v) => setScope(v || 'all')} placeholder={t('ai.scopeAll')} />
          </div>
          <div className="ai-quick">
            {[t('ai.q1'), t('ai.q2'), t('ai.q3')].map((q) => (
              <button key={q} className="btn sm" onClick={() => send(q)} disabled={busy}>{q}</button>
            ))}
          </div>
          <div className="ai-body" ref={bodyRef}>
            {msgs.length === 0 && <div className="muted" style={{ fontSize: 13 }}>{t('ai.hint')} <br /><em>{t('ai.scopeHint', { scope: scopeLabel })}</em></div>}
            {msgs.map((m, i) => <div key={i} className={`ai-msg ${m.role}`}>{m.text}</div>)}
            {busy && <div className="ai-msg bot">{t('ai.thinking')}</div>}
          </div>
          <div className="ai-foot">
            <input className="input" value={input} placeholder={t('ai.placeholder')}
              onChange={(e) => setInput(e.target.value)}
              onKeyDown={(e) => { if (e.key === 'Enter') send(input) }} />
            <button className="btn sm primary" onClick={() => send(input)} disabled={busy}>{t('ai.send')}</button>
          </div>
        </div>
      )}
    </>
  )
}

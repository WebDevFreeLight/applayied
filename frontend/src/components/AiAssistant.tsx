import { useEffect, useRef, useState } from 'react'
import { useTranslation } from 'react-i18next'
import * as pmo from '../api/pmo'

interface Msg { role: 'user' | 'bot'; text: string }

// Floating AI assistant available on every page. Chats through the backend multi-provider
// fallback chain (/pmo/ai/chat). Other pages can also call the AI endpoints directly for
// project summaries, risk analysis and dashboard insights.
export default function AiAssistant() {
  const { t, i18n } = useTranslation()
  const [open, setOpen] = useState(false)
  const [msgs, setMsgs] = useState<Msg[]>([])
  const [input, setInput] = useState('')
  const [busy, setBusy] = useState(false)
  const [configured, setConfigured] = useState<boolean | null>(null)
  const bodyRef = useRef<HTMLDivElement>(null)

  useEffect(() => {
    if (open && configured === null) {
      pmo.aiStatus().then((s) => setConfigured(s.configured)).catch(() => setConfigured(false))
    }
  }, [open, configured])

  useEffect(() => { bodyRef.current?.scrollTo(0, bodyRef.current.scrollHeight) }, [msgs, busy])

  const send = async (text: string) => {
    const q = text.trim()
    if (!q || busy) return
    setMsgs((m) => [...m, { role: 'user', text: q }])
    setInput('')
    setBusy(true)
    try {
      const r = await pmo.aiChat(q, undefined, i18n.language)
      setMsgs((m) => [...m, { role: 'bot', text: r.text }])
    } catch (e) {
      setMsgs((m) => [...m, { role: 'bot', text: (e as Error).message }])
    } finally { setBusy(false) }
  }

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
          <div className="ai-quick">
            {[t('ai.q1'), t('ai.q2'), t('ai.q3')].map((q) => (
              <button key={q} className="btn sm" onClick={() => send(q)} disabled={busy}>{q}</button>
            ))}
          </div>
          <div className="ai-body" ref={bodyRef}>
            {msgs.length === 0 && <div className="muted" style={{ fontSize: 13 }}>{t('ai.hint')}</div>}
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

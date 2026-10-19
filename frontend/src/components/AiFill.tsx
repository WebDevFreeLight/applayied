import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import * as pmo from '../api/pmo'

// Reusable "AI autofill" button for any create/edit form. The caller declares the form `kind` and a
// `fields` spec (e.g. "name, description, priority(Low|Medium|High|Critical)"); on click it asks the
// user for a one-line description, sends it to /pmo/ai/autofill, parses the returned JSON and merges
// the values into the form via `onApply`. Fails quietly with an inline message.
export default function AiFill({ kind, fields, onApply }:
  { kind: string; fields: string; onApply: (values: Record<string, unknown>) => void }) {
  const { t, i18n } = useTranslation()
  const [busy, setBusy] = useState(false)
  const [err, setErr] = useState('')

  const run = async () => {
    const prompt = window.prompt(t('ai.fillPrompt', { kind }))
    if (!prompt || !prompt.trim()) return
    setBusy(true); setErr('')
    try {
      const r = await pmo.aiAutofill(kind, fields, prompt.trim(), i18n.language)
      if (!r.ok) { setErr(r.text); return }
      let obj: Record<string, unknown>
      try { obj = JSON.parse(r.text) } catch { setErr(t('ai.fillParseErr')); return }
      // Only apply the fields that were requested + are non-empty.
      const wanted = fields.split(',').map((f) => f.trim().replace(/\(.*\)$/, ''))
      const clean: Record<string, unknown> = {}
      for (const k of wanted) if (obj[k] !== undefined && obj[k] !== null && obj[k] !== '') clean[k] = obj[k]
      onApply(clean)
    } catch (e) { setErr((e as Error).message) } finally { setBusy(false) }
  }

  return (
    <div className="field full" style={{ gap: 4 }}>
      <button type="button" className="btn sm" onClick={run} disabled={busy}
        title={t('ai.fillTitle')}>✨ {busy ? t('ai.thinking') : t('ai.fill')}</button>
      {err && <span className="err" style={{ fontSize: 11 }}>{err}</span>}
    </div>
  )
}

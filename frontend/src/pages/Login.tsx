import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useAuth } from '../auth/auth'
import { changeLanguage } from '../i18n'
import * as pmo from '../api/pmo'

export default function Login() {
  const { t, i18n } = useTranslation()
  const { signIn } = useAuth()
  const [username, setUsername] = useState('pmo.admin')
  const [password, setPassword] = useState('123456')
  const [code, setCode] = useState('')
  const [captcha, setCaptcha] = useState<pmo.Captcha | null>(null)
  const [busy, setBusy] = useState(false)
  const [error, setError] = useState('')

  // Load a captcha challenge. captchaOff === 'off' means the server has captcha disabled.
  const loadCaptcha = () => { pmo.getCaptcha().then(setCaptcha).catch(() => setCaptcha({ captchaOff: 'off', uuid: '', img: '' })) }
  useEffect(loadCaptcha, [])
  const captchaRequired = !!captcha && captcha.captchaOff !== 'off'

  const submit = async (e: React.FormEvent) => {
    e.preventDefault()
    setBusy(true); setError('')
    try {
      await signIn(username.trim(), password, code, captcha?.uuid)
    } catch (err) {
      setError((err as Error).message || t('auth.failed'))
      if (captchaRequired) { setCode(''); loadCaptcha() } // refresh challenge on failure
    } finally {
      setBusy(false)
    }
  }

  return (
    <div className="login-wrap">
      <form className="panel login-card" onSubmit={submit}>
        <div className="row" style={{ justifyContent: 'space-between' }}>
          <h2 style={{ margin: 0 }}>{t('app.title')}</h2>
          <button type="button" className="btn sm" onClick={() => changeLanguage(i18n.language === 'ar' ? 'en' : 'ar')}>
            {t('nav.language')}
          </button>
        </div>
        <p className="muted" style={{ marginTop: 4 }}>{t('app.subtitle')}</p>
        <div style={{ display: 'grid', gap: 12, marginTop: 10 }}>
          <label className="field">{t('auth.username')}
            <input value={username} onChange={(e) => setUsername(e.target.value)} autoFocus required />
          </label>
          <label className="field">{t('auth.password')}
            <input type="password" value={password} onChange={(e) => setPassword(e.target.value)} required />
          </label>
          {captchaRequired && (
            <div className="row" style={{ gap: 8 }}>
              <input placeholder="Captcha" value={code} onChange={(e) => setCode(e.target.value)} style={{ flex: 1 }} required />
              <img src={captcha.img.startsWith('data:') ? captcha.img : `data:image/jpeg;base64,${captcha.img}`}
                alt="captcha" onClick={loadCaptcha} title="click to refresh"
                style={{ height: 38, borderRadius: 6, cursor: 'pointer' }} />
            </div>
          )}
          {error && <div className="err">{error}</div>}
          <button className="btn primary" disabled={busy}>{busy ? t('auth.signingIn') : t('auth.signIn')}</button>
          <div className="muted center" style={{ fontSize: 12 }}>{t('auth.demoHint')}</div>
        </div>
      </form>
    </div>
  )
}

import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import * as sys from '../../api/system'
import { ErrorBox, Loading, Toast } from '../../components/ui'

// Current-user self-service: update own profile + change password.
export default function Profile() {
  const { t } = useTranslation()
  const [form, setForm] = useState<{ nickName: string; email: string; phonenumber: string }>({ nickName: '', email: '', phonenumber: '' })
  const [pwd, setPwd] = useState({ oldPassword: '', newPassword: '' })
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [toast, setToast] = useState('')
  const notify = (m: string) => { setToast(m); setTimeout(() => setToast(''), 2500) }

  useEffect(() => {
    sys.getProfile().then((p) => {
      const u = (p.user || {}) as Record<string, unknown>
      setForm({ nickName: String(u.nickName || ''), email: String(u.email || ''), phonenumber: String(u.phonenumber || '') })
    }).catch((e) => setError((e as Error).message)).finally(() => setLoading(false))
  }, [])

  const saveProfile = async () => {
    try { await sys.updateProfile(form); notify(t('common.saved')) } catch (e) { setError((e as Error).message) }
  }
  const changePwd = async () => {
    try { await sys.updatePwd(pwd.oldPassword, pwd.newPassword); setPwd({ oldPassword: '', newPassword: '' }); notify(t('common.saved')) }
    catch (e) { setError((e as Error).message) }
  }

  if (loading) return <Loading />
  return (
    <div>
      <h2 style={{ margin: 0 }}>{t('sys.profile')}</h2>
      {error && <ErrorBox message={error} />}
      <div className="grid2" style={{ marginTop: 12 }}>
        <div className="panel">
          <h3>{t('sys.updateProfile')}</h3>
          <label className="field">{t('admin.nickName')}<input value={form.nickName} onChange={(e) => setForm({ ...form, nickName: e.target.value })} /></label>
          <label className="field" style={{ marginTop: 8 }}>{t('admin.email')}<input value={form.email} onChange={(e) => setForm({ ...form, email: e.target.value })} /></label>
          <label className="field" style={{ marginTop: 8 }}>{t('admin.phone')}<input value={form.phonenumber} onChange={(e) => setForm({ ...form, phonenumber: e.target.value })} /></label>
          <button className="btn primary" style={{ marginTop: 12 }} onClick={saveProfile}>{t('common.save')}</button>
        </div>
        <div className="panel">
          <h3>{t('sys.changePwd')}</h3>
          <label className="field">{t('sys.oldPwd')}<input type="password" value={pwd.oldPassword} onChange={(e) => setPwd({ ...pwd, oldPassword: e.target.value })} /></label>
          <label className="field" style={{ marginTop: 8 }}>{t('sys.newPwd')}<input type="password" value={pwd.newPassword} onChange={(e) => setPwd({ ...pwd, newPassword: e.target.value })} /></label>
          <button className="btn primary" style={{ marginTop: 12 }} onClick={changePwd}>{t('sys.changePwd')}</button>
        </div>
      </div>
      {toast && <Toast message={toast} />}
    </div>
  )
}

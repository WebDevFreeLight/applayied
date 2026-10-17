import { useState } from 'react'
import { NavLink, Outlet } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { useAuth } from '../auth/auth'
import { changeLanguage } from '../i18n'
import { getTheme, toggleTheme } from '../theme'

const LINKS = [
  { to: '/', key: 'dashboard', end: true },
  { to: '/projects', key: 'projects' },
  { to: '/tasks', key: 'tasks' },
  { to: '/milestones', key: 'milestones' },
  { to: '/risks', key: 'risks' },
  { to: '/change-requests', key: 'changeRequests' },
  { to: '/finance', key: 'finance' },
]

export default function Layout() {
  const { t, i18n } = useTranslation()
  const { session, signOut } = useAuth()
  const [theme, setThemeState] = useState(getTheme())
  const toggleLang = () => changeLanguage(i18n.language === 'ar' ? 'en' : 'ar')
  const onToggleTheme = () => setThemeState(toggleTheme())

  return (
    <div className="shell">
      <aside className="sidebar">
        <div className="brand">{t('app.title')}<small>{t('app.subtitle')}</small></div>
        {LINKS.map((l) => (
          <NavLink key={l.to} to={l.to} end={l.end}
            className={({ isActive }) => `navlink ${isActive ? 'active' : ''}`}>
            {t(`nav.${l.key}`)}
          </NavLink>
        ))}
      </aside>
      <div className="main">
        <header className="topbar">
          <strong>{t('app.title')}</strong>
          <div className="row">
            <button className="btn sm" onClick={onToggleTheme} title="theme">{theme === 'dark' ? '☀️' : '🌙'}</button>
            <button className="btn sm" onClick={toggleLang}>{t('nav.language')}</button>
            <span className="user">{session?.nickName} · {session?.roles.join(', ')}</span>
            <button className="btn sm" onClick={signOut}>{t('nav.logout')}</button>
          </div>
        </header>
        <main className="content"><Outlet /></main>
      </div>
    </div>
  )
}

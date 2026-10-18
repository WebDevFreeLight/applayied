import { useState } from 'react'
import { NavLink, Outlet } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { useAuth } from '../auth/auth'
import { changeLanguage } from '../i18n'
import { getTheme, toggleTheme } from '../theme'
import NotificationBell from './NotificationBell'
import AiAssistant from './AiAssistant'

const LINKS = [
  { to: '/', key: 'dashboard', end: true },
  { to: '/projects', key: 'projects' },
  { to: '/tasks', key: 'tasks' },
  { to: '/milestones', key: 'milestones' },
  { to: '/risks', key: 'risks' },
  { to: '/change-requests', key: 'changeRequests' },
  { to: '/finance', key: 'finance' },
  { to: '/activity', key: 'activity' },
  { to: '/categories', key: 'categories' },
]
// Admin links appear only when the user holds the matching list permission.
// Links without a `perm` (Messages, Profile) are personal and shown to every signed-in user.
const ADMIN_LINKS: { to: string; key: string; perm?: string }[] = [
  { to: '/admin/users', key: 'admin.users', perm: 'system:user:list' },
  { to: '/admin/roles', key: 'admin.roles', perm: 'system:role:list' },
  { to: '/admin/menus', key: 'admin.menus', perm: 'system:menu:list' },
  { to: '/admin/depts', key: 'sys.depts', perm: 'system:dept:list' },
  { to: '/admin/posts', key: 'sys.posts', perm: 'system:post:list' },
  { to: '/admin/dicts', key: 'sys.dicts', perm: 'system:dict:list' },
  { to: '/admin/config', key: 'sys.config', perm: 'system:config:list' },
  { to: '/admin/notices', key: 'sys.notices', perm: 'system:notice:list' },
  { to: '/admin/jobs', key: 'sys.jobs', perm: 'monitor:job:list' },
  { to: '/admin/job-logs', key: 'sys.jobLogs', perm: 'monitor:job:list' },
  { to: '/admin/login-log', key: 'sys.loginLog', perm: 'monitor:logininfor:list' },
  { to: '/admin/sessions', key: 'sys.sessions', perm: 'monitor:online:list' },
  { to: '/admin/oper-log', key: 'sys.operLog', perm: 'monitor:operlog:list' },
  { to: '/admin/online', key: 'sys.online', perm: 'monitor:online:list' },
  { to: '/admin/server', key: 'sys.server', perm: 'monitor:server:list' },
  { to: '/admin/messages', key: 'sys.messages' },
  { to: '/admin/profile', key: 'sys.profile' },
]

export default function Layout() {
  const { t, i18n } = useTranslation()
  const { session, signOut, hasPerm } = useAuth()
  const adminLinks = ADMIN_LINKS.filter((l) => !l.perm || hasPerm(l.perm))
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
        {adminLinks.length > 0 && (
          <>
            <div className="section-title" style={{ padding: '0 12px' }}>{t('admin.section')}</div>
            {adminLinks.map((l) => (
              <NavLink key={l.to} to={l.to}
                className={({ isActive }) => `navlink ${isActive ? 'active' : ''}`}>
                {t(l.key)}
              </NavLink>
            ))}
          </>
        )}
      </aside>
      <div className="main">
        <header className="topbar">
          <strong>{t('app.title')}</strong>
          <div className="row">
            <NotificationBell />
            <button className="btn sm" onClick={onToggleTheme} title="theme">{theme === 'dark' ? '☀️' : '🌙'}</button>
            <button className="btn sm" onClick={toggleLang}>{t('nav.language')}</button>
            <span className="user">{session?.nickName} · {session?.roles.join(', ')}</span>
            <button className="btn sm" onClick={signOut}>{t('nav.logout')}</button>
          </div>
        </header>
        <main className="content"><Outlet /></main>
      </div>
      <AiAssistant />
    </div>
  )
}

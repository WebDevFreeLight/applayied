import { useMutation } from '@tanstack/react-query'
import {
  Languages,
  LogOut,
  Menu,
  Monitor,
  Moon,
  MoreHorizontal,
  PanelLeftClose,
  PanelLeftOpen,
  Search,
  Settings,
  Sun,
  X,
} from 'lucide-react'
import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Link, NavLink, Outlet, useLocation } from 'react-router'
import { authApi } from '@/api/auth'
import type { Language, ThemePreference, UserDto } from '@/api/types'
import { useAuth } from '@/auth/context'
import { useHotkey, useLocalStorageState } from '@/hooks/misc'
import { normalizeLanguage, setLanguage } from '@/i18n'
import { cn } from '@/lib/utils'
import { useTheme } from '@/theme/theme'
import { IconButton } from '../ui/Button'
import { ErrorBoundary } from '../ui/ErrorBoundary'
import { Popover } from '../ui/Popover'
import { NAV_ITEMS, type NavItem } from './nav'
import { QuickSearch } from './QuickSearch'
import { VoiceStatusChip } from './VoiceStatusChip'

function Logo({ collapsed }: { collapsed?: boolean }) {
  const { t } = useTranslation()
  return (
    <Link to="/" className="flex items-center gap-2.5 rounded-xl px-1 py-1" aria-label={t('app.name')}>
      <span className="flex size-9 shrink-0 items-center justify-center rounded-xl bg-gradient-to-br from-[#8b7cff] to-[#4f46e5] shadow-sm">
        <svg viewBox="0 0 24 24" className="size-5 fill-white" aria-hidden>
          <rect x="3" y="9" width="2.4" height="6" rx="1.2" />
          <rect x="7.2" y="5" width="2.4" height="14" rx="1.2" />
          <rect x="11.4" y="7" width="2.4" height="10" rx="1.2" />
          <rect x="15.6" y="3" width="2.4" height="18" rx="1.2" />
          <rect x="19.6" y="9.5" width="2" height="5" rx="1" />
        </svg>
      </span>
      {!collapsed && (
        <span className="flex flex-col leading-tight">
          <span className="text-[15px] font-semibold tracking-tight text-fg">{t('app.name')}</span>
          <span className="text-[11px] text-subtle">{t('app.tagline')}</span>
        </span>
      )}
    </Link>
  )
}

function useNavItems(): NavItem[] {
  const { isAdmin } = useAuth()
  return NAV_ITEMS.filter((i) => !i.adminOnly || isAdmin)
}

function NavLinks({ collapsed, onNavigate }: { collapsed?: boolean; onNavigate?: () => void }) {
  const { t } = useTranslation()
  const items = useNavItems()
  return (
    <ul className="flex flex-col gap-0.5">
      {items.map(({ to, labelKey, icon: Icon, end }) => (
        <li key={to}>
          <NavLink
            to={to}
            end={end}
            onClick={onNavigate}
            title={collapsed ? t(labelKey) : undefined}
            className={({ isActive }) =>
              cn(
                'flex h-10 items-center gap-3 rounded-xl px-3 text-sm font-medium transition-colors',
                collapsed && 'justify-center px-0',
                isActive
                  ? 'bg-accent-soft text-accent-text'
                  : 'text-muted hover:bg-surface-2 hover:text-fg',
              )
            }
          >
            <Icon className="size-[18px] shrink-0" aria-hidden />
            {collapsed ? <span className="sr-only">{t(labelKey)}</span> : <span className="truncate">{t(labelKey)}</span>}
          </NavLink>
        </li>
      ))}
    </ul>
  )
}

/** Persists UI preference changes to the profile (best effort). */
function useSavePrefs() {
  const { user, setUser } = useAuth()
  const m = useMutation({
    mutationFn: (patch: Partial<Pick<UserDto, 'preferredLanguage' | 'theme'>>) => {
      if (!user) return Promise.resolve(null)
      return authApi.updateProfile({
        displayName: user.displayName,
        preferredLanguage: patch.preferredLanguage ?? user.preferredLanguage,
        preferredVoice: user.preferredVoice,
        theme: patch.theme ?? user.theme,
      })
    },
    onSuccess: (u) => {
      if (u) setUser(u)
    },
  })
  return m.mutate
}

function HeaderControls() {
  const { t, i18n } = useTranslation()
  const { theme, setTheme } = useTheme()
  const { user, signOut } = useAuth()
  const savePrefs = useSavePrefs()
  const lang = normalizeLanguage(i18n.language)

  const switchLanguage = () => {
    const next: Language = lang === 'ar' ? 'en' : 'ar'
    void setLanguage(next)
    savePrefs({ preferredLanguage: next })
  }
  const cycleTheme = () => {
    const order: ThemePreference[] = ['light', 'dark', 'system']
    const next = order[(order.indexOf(theme) + 1) % order.length]
    setTheme(next)
    savePrefs({ theme: next })
  }
  const ThemeIcon = theme === 'light' ? Sun : theme === 'dark' ? Moon : Monitor

  return (
    <div className="flex items-center gap-1">
      <VoiceStatusChip className="me-1" />
      <button
        type="button"
        onClick={switchLanguage}
        title={t('header.language')}
        aria-label={`${t('header.language')}: ${lang === 'ar' ? t('header.switchToEnglish') : t('header.switchToArabic')}`}
        className="inline-flex h-9 items-center gap-1.5 rounded-xl px-2.5 text-sm font-medium text-muted hover:bg-surface-2 hover:text-fg"
      >
        <Languages className="size-4" aria-hidden />
        <span className="hidden sm:inline">{lang === 'ar' ? 'EN' : 'ع'}</span>
      </button>
      <IconButton label={`${t('header.toggleTheme')} (${t(`settings.themes.${theme}`)})`} onClick={cycleTheme} size="icon">
        <ThemeIcon className="size-[18px]" aria-hidden />
      </IconButton>
      <Popover
        label={t('header.account')}
        align="end"
        className="w-60"
        trigger={(p) => (
          <button
            type="button"
            {...p}
            aria-label={t('header.account')}
            className="ms-1 flex size-9 items-center justify-center rounded-full bg-accent text-sm font-semibold text-accent-fg uppercase shadow-sm"
          >
            {(user?.displayName || user?.email || '?').trim().charAt(0)}
          </button>
        )}
      >
        {(close) => (
          <div className="flex flex-col gap-1">
            <div className="px-2 pb-2">
              <p className="truncate text-sm font-semibold text-fg" dir="auto">
                {user?.displayName}
              </p>
              <p className="truncate text-xs text-muted">{user?.email}</p>
            </div>
            <Link
              to="/settings"
              onClick={close}
              className="flex h-9 items-center gap-2 rounded-lg px-2 text-sm text-fg hover:bg-surface-2"
            >
              <Settings className="size-4" aria-hidden />
              {t('nav.settings')}
            </Link>
            <button
              type="button"
              onClick={() => {
                close()
                signOut()
              }}
              className="flex h-9 items-center gap-2 rounded-lg px-2 text-start text-sm text-danger hover:bg-danger-soft"
            >
              <LogOut className="size-4 rtl:-scale-x-100" aria-hidden />
              {t('header.logout')}
            </button>
          </div>
        )}
      </Popover>
    </div>
  )
}

export function AppLayout() {
  const { t } = useTranslation()
  const location = useLocation()
  const [collapsed, setCollapsed] = useLocalStorageState('vv.sidebarCollapsed', false)
  const [drawerOpen, setDrawerOpen] = useState(false)
  const [searchOpen, setSearchOpen] = useState(false)
  const items = useNavItems()
  const primary = items.filter((i) => i.primary)

  useHotkey('k', () => setSearchOpen(true), { mod: true })

  useEffect(() => {
    if (!drawerOpen) return
    const onKey = (e: KeyboardEvent) => {
      if (e.key === 'Escape') setDrawerOpen(false)
    }
    document.addEventListener('keydown', onKey)
    return () => document.removeEventListener('keydown', onKey)
  }, [drawerOpen])

  const isMac = typeof navigator !== 'undefined' && /Mac|iPhone|iPad/.test(navigator.platform)

  return (
    <div className="flex min-h-dvh bg-bg">
      <a
        href="#main"
        className="sr-only z-[70] rounded-lg bg-accent px-3 py-2 text-accent-fg focus:not-sr-only focus:fixed focus:top-2 focus:start-2"
      >
        {t('app.skipToContent')}
      </a>

      {/* Desktop sidebar */}
      <aside
        className={cn(
          'sticky top-0 hidden h-dvh shrink-0 flex-col border-e border-border bg-surface/70 backdrop-blur md:flex',
          collapsed ? 'w-[72px] px-3' : 'w-64 px-4',
        )}
      >
        <div className={cn('flex items-center py-4', collapsed ? 'justify-center' : 'justify-between')}>
          <Logo collapsed={collapsed} />
        </div>
        <nav aria-label={t('nav.main')} className="scrollbar-thin -mx-1 flex-1 overflow-y-auto px-1 py-2">
          <NavLinks collapsed={collapsed} />
        </nav>
        <div className={cn('flex py-3', collapsed ? 'justify-center' : 'justify-end')}>
          <IconButton
            label={collapsed ? t('nav.expand') : t('nav.collapse')}
            onClick={() => setCollapsed(!collapsed)}
            size="icon-sm"
          >
            {collapsed ? (
              <PanelLeftOpen className="size-4 rtl:-scale-x-100" aria-hidden />
            ) : (
              <PanelLeftClose className="size-4 rtl:-scale-x-100" aria-hidden />
            )}
          </IconButton>
        </div>
      </aside>

      {/* Mobile drawer */}
      {drawerOpen && (
        <div className="fixed inset-0 z-50 md:hidden" role="dialog" aria-modal="true" aria-label={t('nav.main')}>
          <div className="absolute inset-0 animate-fade-in bg-black/40" onClick={() => setDrawerOpen(false)} aria-hidden />
          <div className="absolute inset-y-0 start-0 flex w-72 max-w-[85vw] animate-fade-in flex-col border-e border-border bg-surface px-4 shadow-pop">
            <div className="flex items-center justify-between py-4">
              <Logo />
              <IconButton label={t('nav.closeMenu')} onClick={() => setDrawerOpen(false)} size="icon-sm">
                <X className="size-4" aria-hidden />
              </IconButton>
            </div>
            <nav aria-label={t('nav.main')} className="flex-1 overflow-y-auto pb-6">
              <NavLinks onNavigate={() => setDrawerOpen(false)} />
            </nav>
          </div>
        </div>
      )}

      <div className="flex min-w-0 flex-1 flex-col">
        <header className="sticky top-0 z-30 flex h-16 items-center gap-2 border-b border-border bg-bg/80 px-3 backdrop-blur-md sm:px-6">
          <IconButton label={t('nav.openMenu')} onClick={() => setDrawerOpen(true)} className="md:hidden">
            <Menu className="size-5" aria-hidden />
          </IconButton>
          <button
            type="button"
            onClick={() => setSearchOpen(true)}
            aria-label={t('header.quickSearchHint')}
            aria-keyshortcuts={isMac ? 'Meta+K' : 'Control+K'}
            className="flex h-10 min-w-0 flex-1 items-center gap-2 rounded-xl border border-border bg-surface px-3 text-sm text-subtle shadow-xs transition-colors hover:border-border-strong sm:max-w-sm"
          >
            <Search className="size-4 shrink-0" aria-hidden />
            <span className="truncate">{t('header.quickSearch')}</span>
            <kbd className="ms-auto hidden rounded-md border border-border bg-surface-2 px-1.5 py-0.5 font-sans text-[11px] text-muted sm:inline" dir="ltr">
              {isMac ? '⌘' : 'Ctrl'} K
            </kbd>
          </button>
          <div className="ms-auto">
            <HeaderControls />
          </div>
        </header>

        <main id="main" tabIndex={-1} className="mx-auto w-full max-w-6xl flex-1 px-4 pt-6 pb-28 focus:outline-none sm:px-6 md:pb-12">
          <ErrorBoundary resetKey={location.pathname}>
            <Outlet />
          </ErrorBoundary>
        </main>
      </div>

      {/* Mobile bottom bar */}
      <nav
        aria-label={t('nav.main')}
        className="fixed inset-x-0 bottom-0 z-40 border-t border-border bg-surface/95 pb-[env(safe-area-inset-bottom)] backdrop-blur md:hidden"
      >
        <ul className="grid grid-cols-5">
          {primary.map(({ to, labelKey, icon: Icon, end }) => (
            <li key={to}>
              <NavLink
                to={to}
                end={end}
                className={({ isActive }) =>
                  cn(
                    'flex h-16 flex-col items-center justify-center gap-1 text-[11px] font-medium',
                    isActive ? 'text-accent-text' : 'text-muted',
                  )
                }
              >
                <Icon className="size-5" aria-hidden />
                <span className="max-w-full truncate px-1">{t(labelKey)}</span>
              </NavLink>
            </li>
          ))}
          <li>
            <button
              type="button"
              onClick={() => setDrawerOpen(true)}
              aria-label={t('nav.more')}
              className="flex h-16 w-full flex-col items-center justify-center gap-1 text-[11px] font-medium text-muted"
            >
              <MoreHorizontal className="size-5" aria-hidden />
              {t('nav.more')}
            </button>
          </li>
        </ul>
      </nav>

      <QuickSearch open={searchOpen} onClose={() => setSearchOpen(false)} />
    </div>
  )
}

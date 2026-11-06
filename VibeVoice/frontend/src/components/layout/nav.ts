import {
  AudioLines,
  BookHeart,
  Images,
  LayoutDashboard,
  NotebookPen,
  Search,
  Settings,
  Sparkles,
  Trash2,
  Users,
  type LucideIcon,
} from 'lucide-react'

export interface NavItem {
  to: string
  labelKey: string
  icon: LucideIcon
  adminOnly?: boolean
  /** Shown in the mobile bottom bar. */
  primary?: boolean
  end?: boolean
}

export const NAV_ITEMS: NavItem[] = [
  { to: '/', labelKey: 'nav.dashboard', icon: LayoutDashboard, primary: true, end: true },
  { to: '/memories', labelKey: 'nav.memories', icon: BookHeart, primary: true },
  { to: '/notes', labelKey: 'nav.notes', icon: NotebookPen, primary: true },
  { to: '/audio', labelKey: 'nav.audio', icon: AudioLines },
  { to: '/studio', labelKey: 'nav.studio', icon: Sparkles, primary: true },
  { to: '/images', labelKey: 'nav.images', icon: Images },
  { to: '/search', labelKey: 'nav.search', icon: Search },
  { to: '/trash', labelKey: 'nav.trash', icon: Trash2 },
  { to: '/settings', labelKey: 'nav.settings', icon: Settings },
  { to: '/users', labelKey: 'nav.users', icon: Users, adminOnly: true },
]

import {
  AudioLines,
  BookHeart,
  Images,
  LayoutDashboard,
  ShieldCheck,
  Gauge,
  NotebookPen,
  Search,
  Settings,
  Sparkles,
  Trash2,
  Users,
  type LucideIcon,
} from 'lucide-react'
import type { Permission } from '@/api/types'
import type { ArtName } from '../art/ArtTile'

export interface NavItem {
  to: string
  labelKey: string
  /** Shorter label for the phone bottom bar, where full names get truncated. */
  shortLabelKey?: string
  icon: LucideIcon
  /** Illustrated tile used in the sidebar / bottom bar. */
  art: ArtName
  /** Shown when the user has ANY of these permissions (none = everyone). */
  permission?: readonly Permission[]
  /** Sidebar section. */
  group?: 'main' | 'admin'
  /** Shown in the mobile bottom bar. */
  primary?: boolean
  end?: boolean
}

export const NAV_ITEMS: NavItem[] = [
  { to: '/', art: 'dashboard', labelKey: 'nav.dashboard', icon: LayoutDashboard, primary: true, end: true },
  { to: '/memories', art: 'memories', labelKey: 'nav.memories', icon: BookHeart, primary: true, permission: ['memories'] },
  { to: '/notes', art: 'notes', labelKey: 'nav.notes', icon: NotebookPen, primary: true, permission: ['notes'] },
  { to: '/audio', art: 'audio', labelKey: 'nav.audio', icon: AudioLines },
  { to: '/studio', art: 'studio', labelKey: 'nav.studio', shortLabelKey: 'nav.studioShort', icon: Sparkles, primary: true },
  { to: '/images', art: 'images', labelKey: 'nav.images', icon: Images },
  { to: '/search', art: 'search', labelKey: 'nav.search', icon: Search },
  { to: '/trash', art: 'trash', labelKey: 'nav.trash', icon: Trash2, permission: ['notes', 'memories'] },
  { to: '/settings', art: 'settings', labelKey: 'nav.settings', icon: Settings },
  { to: '/admin', art: 'overview', labelKey: 'nav.overview', icon: Gauge, permission: ['users.view'], group: 'admin', end: true },
  { to: '/users', art: 'users', labelKey: 'nav.users', icon: Users, permission: ['users.view'], group: 'admin' },
  { to: '/roles', art: 'roles', labelKey: 'nav.roles', icon: ShieldCheck, permission: ['users.view'], group: 'admin' },
]

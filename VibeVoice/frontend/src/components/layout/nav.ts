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
import type { ArtName } from '../art/ArtTile'

export interface NavItem {
  to: string
  labelKey: string
  icon: LucideIcon
  /** Illustrated tile used in the sidebar / bottom bar. */
  art: ArtName
  adminOnly?: boolean
  /** Shown in the mobile bottom bar. */
  primary?: boolean
  end?: boolean
}

export const NAV_ITEMS: NavItem[] = [
  { to: '/', art: 'dashboard', labelKey: 'nav.dashboard', icon: LayoutDashboard, primary: true, end: true },
  { to: '/memories', art: 'memories', labelKey: 'nav.memories', icon: BookHeart, primary: true },
  { to: '/notes', art: 'notes', labelKey: 'nav.notes', icon: NotebookPen, primary: true },
  { to: '/audio', art: 'audio', labelKey: 'nav.audio', icon: AudioLines },
  { to: '/studio', art: 'studio', labelKey: 'nav.studio', icon: Sparkles, primary: true },
  { to: '/images', art: 'images', labelKey: 'nav.images', icon: Images },
  { to: '/search', art: 'search', labelKey: 'nav.search', icon: Search },
  { to: '/trash', art: 'trash', labelKey: 'nav.trash', icon: Trash2 },
  { to: '/settings', art: 'settings', labelKey: 'nav.settings', icon: Settings },
  { to: '/users', art: 'users', labelKey: 'nav.users', icon: Users, adminOnly: true },
]

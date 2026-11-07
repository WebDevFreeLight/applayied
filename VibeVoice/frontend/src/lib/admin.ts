import type { TFunction } from 'i18next'
import { ADMIN_ROLE_ID, type Permission, type PermissionDto, type PermissionGroup, type RoleDto } from '@/api/types'

/** Built-in role names are translated; custom role names are shown as typed. */
export function roleLabel(t: TFunction, name: string): string {
  return name === 'Admin' || name === 'User' ? t(`users.roles.${name}`) : name
}

/** Fallback catalogue (mirrors GET /api/permissions) used until/if the API list loads. */
export const PERMISSION_CATALOG: PermissionDto[] = [
  { key: 'notes', group: 'content' },
  { key: 'memories', group: 'content' },
  { key: 'audio.upload', group: 'media' },
  { key: 'images.upload', group: 'media' },
  { key: 'voice.tts', group: 'ai' },
  { key: 'voice.transcribe', group: 'ai' },
  { key: 'voice.ocr', group: 'ai' },
  { key: 'users.view', group: 'admin' },
  { key: 'users.manage', group: 'admin' },
  { key: 'roles.manage', group: 'admin' },
]

export const PERMISSION_GROUPS: PermissionGroup[] = ['content', 'media', 'ai', 'admin']

export function groupPermissions(list: PermissionDto[]): { group: PermissionGroup; keys: Permission[] }[] {
  return PERMISSION_GROUPS.map((group) => ({ group, keys: list.filter((p) => p.group === group).map((p) => p.key) })).filter(
    (g) => g.keys.length > 0,
  )
}

const LOWER = 'abcdefghijkmnopqrstuvwxyz'
const UPPER = 'ABCDEFGHJKLMNPQRSTUVWXYZ'
const DIGITS = '23456789'
const SYMBOLS = '!@#$%*-_?'

function pick(chars: string, rnd: number): string {
  return chars[rnd % chars.length]
}

/** Random password (14 chars) that always satisfies the API rule: ≥ 8 chars, a letter and a digit. */
export function generatePassword(length = 14): string {
  const all = LOWER + UPPER + DIGITS + SYMBOLS
  const buf = new Uint32Array(length)
  crypto.getRandomValues(buf)
  const chars = Array.from(buf, (n) => pick(all, n))
  chars[0] = pick(UPPER, buf[0])
  chars[1] = pick(LOWER, buf[1])
  chars[2] = pick(DIGITS, buf[2])
  chars[3] = pick(SYMBOLS, buf[3])
  // Shuffle so the guaranteed characters aren't always first.
  for (let i = chars.length - 1; i > 0; i--) {
    const j = buf[i] % (i + 1)
    ;[chars[i], chars[j]] = [chars[j], chars[i]]
  }
  return chars.join('')
}

export type Strength = 'weak' | 'fair' | 'strong'

export function passwordStrength(pw: string): Strength {
  let score = 0
  if (pw.length >= 8) score++
  if (pw.length >= 12) score++
  if (/[a-z]/.test(pw) && /[A-Z]/.test(pw)) score++
  if (/\d/.test(pw)) score++
  if (/[^A-Za-z0-9]/.test(pw)) score++
  if (pw.length < 8 || !/\p{L}/u.test(pw) || !/\d/.test(pw)) return 'weak'
  return score >= 4 ? 'strong' : 'fair'
}

/** The built-in Admin role always has every permission and is read-only. */
export function isAdminRole(role: Pick<RoleDto, 'id' | 'name' | 'isSystem'>): boolean {
  return role.id === ADMIN_ROLE_ID || (role.isSystem && role.name === 'Admin')
}

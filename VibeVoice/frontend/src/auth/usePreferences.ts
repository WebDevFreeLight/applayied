import { useMutation } from '@tanstack/react-query'
import { useCallback } from 'react'
import { authApi } from '@/api/auth'
import type { Language, Palette, ThemePreference } from '@/api/types'
import { useTheme } from '@/theme/theme'
import { useAuth } from './context'

export interface PreferencePatch {
  preferredLanguage?: Language
  theme?: ThemePreference
  palette?: Palette
}

/** Persists UI preferences to the profile (PUT /api/users/me) — best effort, UI is updated first. */
export function useSavePreferences() {
  const { user, setUser } = useAuth()
  const { palette: currentPalette } = useTheme()
  const m = useMutation({
    mutationFn: (patch: PreferencePatch) => {
      if (!user) return Promise.resolve(null)
      return authApi.updateProfile({
        displayName: user.displayName,
        preferredLanguage: patch.preferredLanguage ?? user.preferredLanguage,
        preferredVoice: user.preferredVoice,
        theme: patch.theme ?? user.theme,
        palette: patch.palette ?? user.palette ?? currentPalette,
      })
    },
    onSuccess: (u) => {
      if (u) setUser(u)
    },
  })
  const { mutate } = m
  return useCallback((patch: PreferencePatch) => mutate(patch), [mutate])
}

/** Switches the visual identity instantly (localStorage) and saves it to the profile. */
export function usePaletteSwitch() {
  const { palette, setPalette } = useTheme()
  const save = useSavePreferences()
  const choose = useCallback(
    (p: Palette) => {
      setPalette(p)
      save({ palette: p })
    },
    [setPalette, save],
  )
  return { palette, choose }
}

import { Check } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import type { Palette } from '@/api/types'
import { usePaletteSwitch } from '@/auth/usePreferences'
import { cn } from '@/lib/utils'
import { PALETTES } from '@/theme/theme'
import { LogoMark } from './art/Logo'

/** Mini rendering of the app in one identity + mode (scoped with data-palette / data-mode). */
function MiniPreview({ palette, mode }: { palette: Palette; mode: 'light' | 'dark' }) {
  return (
    <div
      data-palette={palette}
      data-mode={mode}
      className="flex min-w-0 flex-1 flex-col gap-1.5 rounded-xl border border-border bg-bg p-2"
      aria-hidden
    >
      <div className="flex items-center gap-1.5">
        <LogoMark className="size-5" />
        <span className="h-1.5 w-10 rounded-full bg-fg/70" />
      </div>
      <div className="h-3 rounded-md bg-brand" />
      <div className="flex items-center gap-1 rounded-md bg-surface p-1 shadow-card">
        <span className="h-1.5 flex-1 rounded-full bg-muted/50" />
        <span className="rounded bg-accent px-1.5 py-0.5 text-[8px] leading-none font-bold text-accent-fg">Aa</span>
      </div>
    </div>
  )
}

/** Settings → "Visual identity": four swatch cards (radio group). Applies instantly and saves. */
export function IdentityPicker() {
  const { t } = useTranslation()
  const { palette, choose } = usePaletteSwitch()
  return (
    <div role="radiogroup" aria-label={t('identity.title')} className="grid gap-3 sm:grid-cols-2">
      {PALETTES.map((p) => {
        const active = p === palette
        return (
          <button
            key={p}
            type="button"
            role="radio"
            aria-checked={active}
            data-testid={`identity-${p}`}
            onClick={() => choose(p)}
            className={cn(
              'group flex flex-col gap-3 rounded-2xl border bg-surface p-3 text-start transition',
              active
                ? 'border-accent shadow-pop ring-2 ring-ring/40'
                : 'border-border hover:border-border-strong hover:shadow-card',
            )}
          >
            <div className="flex gap-2">
              <MiniPreview palette={p} mode="light" />
              <MiniPreview palette={p} mode="dark" />
            </div>
            <div className="flex items-start gap-2">
              <span data-palette={p} className="mt-0.5 size-5 shrink-0 rounded-full bg-brand shadow-sm" aria-hidden />
              <span className="min-w-0 flex-1">
                <span className="block text-sm font-semibold text-fg">{t(`identity.names.${p}`)}</span>
                <span className="block text-xs text-muted">{t(`identity.descriptions.${p}`)}</span>
              </span>
              {active && (
                <span className="inline-flex items-center gap-1 rounded-full bg-accent px-2 py-0.5 text-[11px] font-semibold text-accent-fg">
                  <Check className="size-3" aria-hidden />
                  {t('identity.active')}
                </span>
              )}
            </div>
          </button>
        )
      })}
    </div>
  )
}

/** Compact identity switch (header account menu): four gradient dots. */
export function IdentityQuickSwitch() {
  const { t } = useTranslation()
  const { palette, choose } = usePaletteSwitch()
  return (
    <div className="flex flex-col gap-1.5 px-2 py-1">
      <span className="text-xs font-medium text-muted">{t('identity.title')}</span>
      <div role="radiogroup" aria-label={t('identity.title')} className="flex gap-2">
        {PALETTES.map((p) => {
          const active = p === palette
          return (
            <button
              key={p}
              type="button"
              role="radio"
              aria-checked={active}
              aria-label={t(`identity.names.${p}`)}
              title={t(`identity.names.${p}`)}
              onClick={() => choose(p)}
              className={cn(
                'flex size-9 items-center justify-center rounded-full transition pointer-coarse:size-11',
                active ? 'ring-2 ring-ring ring-offset-2 ring-offset-surface' : 'hover:scale-105',
              )}
            >
              <span data-palette={p} className="flex size-7 items-center justify-center rounded-full bg-brand shadow-sm" aria-hidden>
                {active && <Check className="size-3.5 text-[color:var(--art-ink)]" />}
              </span>
            </button>
          )
        })}
      </div>
    </div>
  )
}

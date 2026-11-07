import { cn } from '@/lib/utils'
import { useSvgId } from './svgId'

export interface LogoMarkProps {
  className?: string
  /** Accessible name; omit when the mark sits next to visible "VibeVoice" text. */
  title?: string
}

/** VibeVoice mark: a sound wave inside a speech bubble, filled with the identity gradient. */
export function LogoMark({ className, title }: LogoMarkProps) {
  const id = useSvgId('vv-logo')
  return (
    <svg
      viewBox="0 0 64 64"
      className={cn('shrink-0', className)}
      role={title ? 'img' : undefined}
      aria-label={title}
      aria-hidden={title ? undefined : true}
      focusable="false"
    >
      <defs>
        <linearGradient id={`${id}-g`} x1="0" y1="0" x2="1" y2="1">
          <stop offset="0" style={{ stopColor: 'var(--grad-from)' }} />
          <stop offset="0.55" style={{ stopColor: 'var(--grad-via)' }} />
          <stop offset="1" style={{ stopColor: 'var(--grad-to)' }} />
        </linearGradient>
        <linearGradient id={`${id}-shine`} x1="0" y1="0" x2="0" y2="1">
          <stop offset="0" stopColor="#fff" stopOpacity="0.35" />
          <stop offset="0.6" stopColor="#fff" stopOpacity="0" />
        </linearGradient>
      </defs>
      {/* Speech bubble with a tail at the bottom-left */}
      <path
        d="M14 6h36a10 10 0 0 1 10 10v24a10 10 0 0 1-10 10H27l-11.5 9.2c-1.3 1-3.2.1-3.2-1.6V50h0A10 10 0 0 1 4 40V16A10 10 0 0 1 14 6z"
        fill={`url(#${id}-g)`}
      />
      <path
        d="M14 6h36a10 10 0 0 1 10 10v24a10 10 0 0 1-10 10H27l-11.5 9.2c-1.3 1-3.2.1-3.2-1.6V50h0A10 10 0 0 1 4 40V16A10 10 0 0 1 14 6z"
        fill={`url(#${id}-shine)`}
      />
      {/* Sound wave */}
      <g fill="#fff">
        <rect x="14" y="24" width="4.5" height="8" rx="2.25" opacity="0.85" />
        <rect x="21.5" y="17" width="4.5" height="22" rx="2.25" />
        <rect x="29" y="21" width="4.5" height="14" rx="2.25" opacity="0.9" />
        <rect x="36.5" y="13.5" width="4.5" height="29" rx="2.25" />
        <rect x="44" y="22.5" width="4.5" height="11" rx="2.25" opacity="0.85" />
      </g>
    </svg>
  )
}

/** Mark + wordmark. */
export function Logo({ compact, className }: { compact?: boolean; className?: string }) {
  return (
    <span className={cn('flex items-center gap-2.5', className)}>
      <LogoMark className="size-9 drop-shadow-sm" />
      {!compact && (
        <span className="flex flex-col leading-tight">
          <span className="text-[15px] font-bold tracking-tight text-fg">
            Vibe<span className="text-brand">Voice</span>
          </span>
        </span>
      )}
    </span>
  )
}

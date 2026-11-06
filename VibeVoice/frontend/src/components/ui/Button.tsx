import { Loader2 } from 'lucide-react'
import type { ButtonHTMLAttributes, ReactNode, Ref } from 'react'
import { cn } from '@/lib/utils'

export type ButtonVariant = 'primary' | 'secondary' | 'ghost' | 'danger' | 'subtle' | 'outline'
export type ButtonSize = 'sm' | 'md' | 'lg' | 'icon' | 'icon-sm'

const variants: Record<ButtonVariant, string> = {
  primary:
    'bg-accent text-accent-fg hover:bg-accent-hover shadow-sm disabled:hover:bg-accent',
  secondary:
    'bg-surface text-fg border border-border hover:bg-surface-2 hover:border-border-strong shadow-sm',
  outline: 'border border-border text-fg hover:bg-surface-2',
  ghost: 'text-muted hover:text-fg hover:bg-surface-2',
  subtle: 'bg-accent-soft text-accent-text hover:brightness-95 dark:hover:brightness-125',
  danger: 'bg-danger text-white hover:brightness-110 shadow-sm',
}

const sizes: Record<ButtonSize, string> = {
  sm: 'h-8 px-3 text-sm gap-1.5 rounded-lg',
  md: 'h-10 px-4 text-sm gap-2 rounded-xl',
  lg: 'h-12 px-5 text-base gap-2 rounded-xl',
  icon: 'h-10 w-10 rounded-xl',
  'icon-sm': 'h-8 w-8 rounded-lg',
}

export interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: ButtonVariant
  size?: ButtonSize
  loading?: boolean
  icon?: ReactNode
  ref?: Ref<HTMLButtonElement>
}

export function Button({
  variant = 'secondary',
  size = 'md',
  loading = false,
  icon,
  className,
  children,
  disabled,
  type = 'button',
  ...rest
}: ButtonProps) {
  return (
    <button
      type={type}
      disabled={disabled || loading}
      aria-busy={loading || undefined}
      className={cn(
        'inline-flex shrink-0 items-center justify-center font-medium whitespace-nowrap transition-colors duration-150 select-none',
        'disabled:cursor-not-allowed disabled:opacity-50',
        variants[variant],
        sizes[size],
        className,
      )}
      {...rest}
    >
      {loading ? <Loader2 className="size-4 animate-spin" aria-hidden /> : icon}
      {children}
    </button>
  )
}

export interface IconButtonProps extends Omit<ButtonProps, 'children' | 'icon'> {
  /** Accessible label (also shown as tooltip). */
  label: string
  children: ReactNode
}

export function IconButton({
  label,
  children,
  size = 'icon',
  variant = 'ghost',
  ...rest
}: IconButtonProps) {
  return (
    <Button aria-label={label} title={label} size={size} variant={variant} {...rest}>
      {children}
    </Button>
  )
}

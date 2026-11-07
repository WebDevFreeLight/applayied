import { useId, type InputHTMLAttributes, type ReactNode, type Ref, type SelectHTMLAttributes, type TextareaHTMLAttributes } from 'react'
import { cn } from '@/lib/utils'

const control =
  'w-full rounded-xl border border-border bg-surface px-3 text-sm text-fg placeholder:text-subtle shadow-xs transition-colors ' +
  'hover:border-border-strong focus:border-accent focus:ring-2 focus:ring-accent/25 focus:outline-none ' +
  'disabled:opacity-60 aria-[invalid=true]:border-danger'

export interface InputProps extends InputHTMLAttributes<HTMLInputElement> {
  ref?: Ref<HTMLInputElement>
}

export function Input({ className, dir = 'auto', ...rest }: InputProps) {
  return <input dir={dir} className={cn(control, 'h-10 pointer-coarse:h-11', className)} {...rest} />
}

export interface TextareaProps extends TextareaHTMLAttributes<HTMLTextAreaElement> {
  ref?: Ref<HTMLTextAreaElement>
}

export function Textarea({ className, dir = 'auto', ...rest }: TextareaProps) {
  return <textarea dir={dir} className={cn(control, 'min-h-24 py-2.5 leading-relaxed', className)} {...rest} />
}

export interface SelectProps extends SelectHTMLAttributes<HTMLSelectElement> {
  ref?: Ref<HTMLSelectElement>
}

export function Select({ className, children, ...rest }: SelectProps) {
  return (
    <select className={cn(control, 'h-10 pe-8 pointer-coarse:h-11', className)} {...rest}>
      {children}
    </select>
  )
}

export interface FieldProps {
  label: ReactNode
  error?: string
  hint?: ReactNode
  className?: string
  /** Render prop receiving the generated id and aria props for the control. */
  children: (props: { id: string; 'aria-invalid'?: boolean; 'aria-describedby'?: string }) => ReactNode
}

/** Label + control + hint/error with correct aria wiring. */
export function Field({ label, error, hint, className, children }: FieldProps) {
  const id = useId()
  const descId = `${id}-desc`
  return (
    <div className={cn('flex flex-col gap-1.5', className)}>
      <label htmlFor={id} className="text-sm font-medium text-fg">
        {label}
      </label>
      {children({
        id,
        'aria-invalid': error ? true : undefined,
        'aria-describedby': error || hint ? descId : undefined,
      })}
      {error ? (
        <p id={descId} role="alert" className="text-xs text-danger">
          {error}
        </p>
      ) : hint ? (
        <p id={descId} className="text-xs text-muted">
          {hint}
        </p>
      ) : null}
    </div>
  )
}

export function Checkbox({
  label,
  className,
  ...rest
}: InputHTMLAttributes<HTMLInputElement> & { label: ReactNode }) {
  return (
    <label className={cn('inline-flex cursor-pointer items-center gap-2 text-sm text-fg select-none', className)}>
      <input type="checkbox" className="size-4 rounded accent-[var(--accent)]" {...rest} />
      <span>{label}</span>
    </label>
  )
}

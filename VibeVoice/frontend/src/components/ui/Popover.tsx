import { useEffect, useId, useRef, useState, type ReactNode } from 'react'
import { cn } from '@/lib/utils'

export interface PopoverProps {
  /** Renders the trigger; spread `triggerProps` onto a button. */
  trigger: (props: {
    'aria-expanded': boolean
    'aria-controls': string
    'aria-haspopup': 'dialog'
    onClick: () => void
  }) => ReactNode
  children: (close: () => void) => ReactNode
  align?: 'start' | 'end'
  className?: string
  label: string
}

/** Lightweight popover: click-outside and Esc close it; focus moves inside when opened. */
export function Popover({ trigger, children, align = 'start', className, label }: PopoverProps) {
  const [open, setOpen] = useState(false)
  const rootRef = useRef<HTMLDivElement>(null)
  const panelRef = useRef<HTMLDivElement>(null)
  const id = useId()

  useEffect(() => {
    if (!open) return
    const onDown = (e: MouseEvent) => {
      if (!rootRef.current?.contains(e.target as Node)) setOpen(false)
    }
    const onKey = (e: KeyboardEvent) => {
      if (e.key === 'Escape') {
        setOpen(false)
        rootRef.current?.querySelector<HTMLElement>('[aria-haspopup]')?.focus()
      }
    }
    document.addEventListener('mousedown', onDown)
    document.addEventListener('keydown', onKey)
    panelRef.current
      ?.querySelector<HTMLElement>('select, button, input, [tabindex]:not([tabindex="-1"])')
      ?.focus()
    return () => {
      document.removeEventListener('mousedown', onDown)
      document.removeEventListener('keydown', onKey)
    }
  }, [open])

  return (
    <div ref={rootRef} className="relative inline-block">
      {trigger({
        'aria-expanded': open,
        'aria-controls': id,
        'aria-haspopup': 'dialog',
        onClick: () => setOpen((o) => !o),
      })}
      {open && (
        <div
          id={id}
          ref={panelRef}
          role="dialog"
          aria-label={label}
          className={cn(
            'absolute top-full z-40 mt-2 w-72 max-w-[calc(100vw-1.5rem)] animate-pop-in rounded-2xl border border-border bg-surface p-3 shadow-pop',
            align === 'start' ? 'start-0' : 'end-0',
            className,
          )}
        >
          {children(() => setOpen(false))}
        </div>
      )}
    </div>
  )
}

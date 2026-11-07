import { Copy, Eye, EyeOff, Wand2 } from 'lucide-react'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { toast } from 'sonner'
import { copyText } from '@/hooks/misc'
import { generatePassword, passwordStrength } from '@/lib/admin'
import { cn } from '@/lib/utils'
import { Button, IconButton } from '../ui/Button'
import { Input } from '../ui/Field'

export interface PasswordFieldProps {
  id: string
  value: string
  onChange: (value: string) => void
  invalid?: boolean
  describedBy?: string
  autoComplete?: string
}

/** Password input with show/hide, "Generate", copy and a strength hint. */
export function PasswordField({ id, value, onChange, invalid, describedBy, autoComplete = 'new-password' }: PasswordFieldProps) {
  const { t } = useTranslation()
  const [show, setShow] = useState(false)
  const strength = value ? passwordStrength(value) : null
  const bars = strength === 'strong' ? 3 : strength === 'fair' ? 2 : strength ? 1 : 0
  const strengthId = `${id}-strength`

  return (
    <div className="flex flex-col gap-1.5">
      <div className="flex gap-2">
        <div className="relative min-w-0 flex-1">
          <Input
            id={id}
            dir="ltr"
            type={show ? 'text' : 'password'}
            value={value}
            onChange={(e) => onChange(e.target.value)}
            autoComplete={autoComplete}
            aria-invalid={invalid || undefined}
            aria-describedby={[describedBy, strength ? strengthId : undefined].filter(Boolean).join(' ') || undefined}
            className="pe-10 font-mono"
          />
          <button
            type="button"
            onClick={() => setShow((s) => !s)}
            aria-label={show ? t('auth.hidePassword') : t('auth.showPassword')}
            className="absolute end-1 top-1/2 flex size-8 -translate-y-1/2 items-center justify-center rounded-lg text-subtle hover:text-fg pointer-coarse:size-10"
          >
            {show ? <EyeOff className="size-4" aria-hidden /> : <Eye className="size-4" aria-hidden />}
          </button>
        </div>
        <IconButton
          label={t('users.copy')}
          variant="secondary"
          disabled={!value}
          onClick={() => void copyText(value).then((ok) => ok && toast.success(t('users.copied')))}
        >
          <Copy className="size-4" aria-hidden />
        </IconButton>
        <Button
          onClick={() => {
            onChange(generatePassword())
            setShow(true)
          }}
          icon={<Wand2 className="size-4" aria-hidden />}
          aria-label={t('users.generate')}
        >
          <span className="hidden min-[420px]:inline" aria-hidden>
            {t('users.generate')}
          </span>
        </Button>
      </div>
      {strength && (
        <div id={strengthId} className="flex items-center gap-2" aria-live="polite">
          <div className="flex gap-1" aria-hidden>
            {[1, 2, 3].map((i) => (
              <span
                key={i}
                className={cn(
                  'h-1.5 w-8 rounded-full',
                  i <= bars
                    ? strength === 'strong'
                      ? 'bg-success'
                      : strength === 'fair'
                        ? 'bg-warning'
                        : 'bg-danger'
                    : 'bg-surface-3',
                )}
              />
            ))}
          </div>
          <span className="text-xs text-muted">{t('users.strength.label', { level: t(`users.strength.${strength}`) })}</span>
        </div>
      )}
    </div>
  )
}

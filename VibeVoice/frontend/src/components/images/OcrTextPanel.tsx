import { Copy, Download, TextCursorInput } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { toast } from 'sonner'
import { copyText } from '@/hooks/misc'
import { cn, downloadText } from '@/lib/utils'

export interface OcrTextPanelProps {
  text: string
  /** Base name for the downloaded .txt file. */
  filename: string
  /** "Insert into note / story" — inserts into the open editor (autosave persists it). */
  onInsert?: (text: string) => void
  insertLabel?: string
  tone?: 'default' | 'dark'
  className?: string
}

/** Extracted picture text with Copy, Download .txt and (in editors) Insert. */
export function OcrTextPanel({ text, filename, onInsert, insertLabel, tone = 'default', className }: OcrTextPanelProps) {
  const { t } = useTranslation()
  const dark = tone === 'dark'
  const btn = cn(
    'inline-flex h-7 pointer-coarse:h-10 items-center gap-1 rounded-lg border px-2 text-xs font-medium transition-colors',
    dark ? 'border-white/20 text-white/80 hover:bg-white/10 hover:text-white' : 'border-border text-muted hover:bg-surface-2 hover:text-fg',
  )
  return (
    <div className={cn('flex min-h-0 flex-col gap-2', className)}>
      <p
        dir="auto"
        data-testid="ocr-text"
        className={cn(
          'scrollbar-thin min-h-0 flex-1 overflow-y-auto rounded-xl px-3 py-2 text-sm leading-relaxed whitespace-pre-wrap',
          dark ? 'bg-white/10 text-white' : 'bg-surface-2 text-fg',
        )}
      >
        {text}
      </p>
      <div className="flex flex-wrap items-center gap-1.5">
        <button
          type="button"
          className={btn}
          onClick={() => void copyText(text).then((ok) => ok && toast.success(t('common.copied')))}
        >
          <Copy className="size-3.5" aria-hidden />
          {t('ocr.copy')}
        </button>
        <button type="button" className={btn} onClick={() => downloadText(text, filename)}>
          <Download className="size-3.5" aria-hidden />
          {t('ocr.downloadTxt')}
        </button>
        {onInsert && (
          <button
            type="button"
            className={cn(btn, !dark && 'border-accent/40 text-accent-text hover:bg-accent-soft')}
            onClick={() => {
              onInsert(text)
              toast.success(t('ocr.inserted'))
            }}
          >
            <TextCursorInput className="size-3.5" aria-hidden />
            {insertLabel ?? t('ocr.insertNote')}
          </button>
        )}
      </div>
    </div>
  )
}

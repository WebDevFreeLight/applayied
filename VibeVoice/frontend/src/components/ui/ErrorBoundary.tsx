import { AlertOctagon } from 'lucide-react'
import { Component, type ErrorInfo, type ReactNode } from 'react'
import { useTranslation } from 'react-i18next'
import { Link } from 'react-router'
import { Button } from './Button'

interface Props {
  children: ReactNode
  /** Changing this value resets the boundary (e.g. the current pathname). */
  resetKey?: unknown
  fallback?: (reset: () => void, error: Error) => ReactNode
}

interface State {
  error: Error | null
  resetKey: unknown
}

export class ErrorBoundary extends Component<Props, State> {
  override state: State = { error: null, resetKey: this.props.resetKey }

  static getDerivedStateFromError(error: Error): Partial<State> {
    return { error }
  }

  static getDerivedStateFromProps(props: Props, state: State): Partial<State> | null {
    if (props.resetKey !== state.resetKey) return { error: null, resetKey: props.resetKey }
    return null
  }

  override componentDidCatch(error: Error, info: ErrorInfo) {
    console.error('UI error boundary caught:', error, info.componentStack)
  }

  reset = () => this.setState({ error: null })

  override render() {
    const { error } = this.state
    if (!error) return this.props.children
    if (this.props.fallback) return this.props.fallback(this.reset, error)
    return <ErrorFallback error={error} onReset={this.reset} />
  }
}

export function ErrorFallback({ error, onReset }: { error: Error; onReset?: () => void }) {
  const { t } = useTranslation()
  return (
    <div role="alert" className="mx-auto flex max-w-md flex-col items-center gap-4 py-16 text-center">
      <div className="flex size-14 items-center justify-center rounded-2xl bg-danger-soft text-danger">
        <AlertOctagon className="size-7" aria-hidden />
      </div>
      <div>
        <h2 className="text-lg font-semibold text-fg">{t('errors.boundaryTitle')}</h2>
        <p className="mt-1 text-sm text-muted">{t('errors.boundaryBody')}</p>
        <pre className="mt-3 max-h-24 overflow-auto rounded-lg bg-surface-2 p-2 text-start text-xs text-subtle" dir="ltr">
          {error.message}
        </pre>
      </div>
      <div className="flex gap-2">
        {onReset && (
          <Button variant="primary" onClick={onReset}>
            {t('common.retry')}
          </Button>
        )}
        <Link
          to="/"
          className="inline-flex h-10 items-center rounded-xl border border-border px-4 text-sm font-medium hover:bg-surface-2"
        >
          {t('errors.goHome')}
        </Link>
      </div>
    </div>
  )
}

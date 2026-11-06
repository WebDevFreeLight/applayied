import { Compass } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { Link } from 'react-router'
import { EmptyState } from '@/components/ui/Display'

export function NotFoundPage() {
  const { t } = useTranslation()
  return (
    <EmptyState
      icon={Compass}
      title={t('errors.notFoundTitle')}
      body={t('errors.notFoundBody')}
      className="mt-10"
      action={
        <Link
          to="/"
          className="inline-flex h-10 items-center rounded-xl bg-accent px-4 text-sm font-medium text-accent-fg hover:bg-accent-hover"
        >
          {t('errors.goHome')}
        </Link>
      }
    />
  )
}

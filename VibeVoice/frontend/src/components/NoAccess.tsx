import { useTranslation } from 'react-i18next'
import { Link } from 'react-router'
import { EmptyState } from './ui/Display'

/** Shown when the signed-in user's role lacks the permission a page needs. */
export function NoAccess() {
  const { t } = useTranslation()
  return (
    <EmptyState
      art="lock"
      title={t('access.title')}
      body={t('access.body')}
      className="mt-6"
      action={
        <Link
          to="/"
          className="inline-flex h-10 items-center rounded-xl bg-accent px-4 text-sm font-medium text-accent-fg hover:bg-accent-hover pointer-coarse:h-11"
        >
          {t('errors.goHome')}
        </Link>
      }
    />
  )
}

import { QueryClientProvider } from '@tanstack/react-query'
import { useTranslation } from 'react-i18next'
import { RouterProvider } from 'react-router'
import { Toaster } from 'sonner'
import { AuthProvider } from './auth/AuthProvider'
import { isRtl } from './i18n'
import { queryClient } from './queryClient'
import { router } from './router'
import { ThemeProvider } from './theme/ThemeProvider'
import { useTheme } from './theme/theme'

function AppToaster() {
  const { resolved } = useTheme()
  const { i18n } = useTranslation()
  const rtl = isRtl(i18n.language)
  return (
    <Toaster
      theme={resolved}
      dir={rtl ? 'rtl' : 'ltr'}
      position={rtl ? 'bottom-left' : 'bottom-right'}
      richColors
      closeButton
      toastOptions={{ className: 'font-sans' }}
      mobileOffset={{ bottom: 88 }}
    />
  )
}

export function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <ThemeProvider>
        <AuthProvider>
          <RouterProvider router={router} />
          <AppToaster />
        </AuthProvider>
      </ThemeProvider>
    </QueryClientProvider>
  )
}

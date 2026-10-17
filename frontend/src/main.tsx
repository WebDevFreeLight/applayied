import React from 'react'
import ReactDOM from 'react-dom/client'
import './i18n'
import './theme'
import './styles.css'
import App from './App'
import { AuthProvider } from './auth/auth'

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <AuthProvider>
      <App />
    </AuthProvider>
  </React.StrictMode>,
)

import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom'
import { useAuth } from './auth/auth'
import Layout from './components/Layout'
import Login from './pages/Login'
import Dashboard from './pages/Dashboard'
import Projects from './pages/Projects'
import Tasks from './pages/Tasks'
import Milestones from './pages/Milestones'
import Risks from './pages/Risks'
import ChangeRequests from './pages/ChangeRequests'
import Finance from './pages/Finance'

export default function App() {
  const { isAuthed } = useAuth()
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/login" element={isAuthed ? <Navigate to="/" replace /> : <Login />} />
        <Route element={isAuthed ? <Layout /> : <Navigate to="/login" replace />}>
          <Route path="/" element={<Dashboard />} />
          <Route path="/projects" element={<Projects />} />
          <Route path="/tasks" element={<Tasks />} />
          <Route path="/milestones" element={<Milestones />} />
          <Route path="/risks" element={<Risks />} />
          <Route path="/change-requests" element={<ChangeRequests />} />
          <Route path="/finance" element={<Finance />} />
        </Route>
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </BrowserRouter>
  )
}

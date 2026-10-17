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
import ProjectDetail from './pages/ProjectDetail'
import Users from './pages/admin/Users'
import Roles from './pages/admin/Roles'
import Menus from './pages/admin/Menus'
import Config from './pages/admin/Config'
import Notices from './pages/admin/Notices'
import Messages from './pages/admin/Messages'
import Jobs from './pages/admin/Jobs'
import JobLogs from './pages/admin/JobLogs'
import OperLog from './pages/admin/OperLog'
import Online from './pages/admin/Online'
import Server from './pages/admin/Server'
import Profile from './pages/admin/Profile'
import Depts from './pages/admin/Depts'
import Posts from './pages/admin/Posts'
import Dicts from './pages/admin/Dicts'
import LoginLog from './pages/admin/LoginLog'
import Sessions from './pages/admin/Sessions'

export default function App() {
  const { isAuthed } = useAuth()
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/login" element={isAuthed ? <Navigate to="/" replace /> : <Login />} />
        <Route element={isAuthed ? <Layout /> : <Navigate to="/login" replace />}>
          <Route path="/" element={<Dashboard />} />
          <Route path="/projects" element={<Projects />} />
          <Route path="/projects/:projectId" element={<ProjectDetail />} />
          <Route path="/tasks" element={<Tasks />} />
          <Route path="/milestones" element={<Milestones />} />
          <Route path="/risks" element={<Risks />} />
          <Route path="/change-requests" element={<ChangeRequests />} />
          <Route path="/finance" element={<Finance />} />
          <Route path="/admin/users" element={<Users />} />
          <Route path="/admin/roles" element={<Roles />} />
          <Route path="/admin/menus" element={<Menus />} />
          <Route path="/admin/config" element={<Config />} />
          <Route path="/admin/notices" element={<Notices />} />
          <Route path="/admin/messages" element={<Messages />} />
          <Route path="/admin/jobs" element={<Jobs />} />
          <Route path="/admin/job-logs" element={<JobLogs />} />
          <Route path="/admin/oper-log" element={<OperLog />} />
          <Route path="/admin/online" element={<Online />} />
          <Route path="/admin/server" element={<Server />} />
          <Route path="/admin/profile" element={<Profile />} />
          <Route path="/admin/depts" element={<Depts />} />
          <Route path="/admin/posts" element={<Posts />} />
          <Route path="/admin/dicts" element={<Dicts />} />
          <Route path="/admin/login-log" element={<LoginLog />} />
          <Route path="/admin/sessions" element={<Sessions />} />
        </Route>
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </BrowserRouter>
  )
}

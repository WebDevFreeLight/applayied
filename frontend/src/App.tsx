import type { ReactNode } from 'react'
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom'
import { useAuth } from './auth/auth'
import { NoAccess } from './components/ui'
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
import Categories from './pages/Categories'
import Activity from './pages/Activity'
import Employees from './pages/Employees'
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

// Route-level permission gate: renders the page only if the signed-in role holds `perm`,
// otherwise a "no access" panel. Mirrors the backend ActionPermissionFilter so a role can never
// open (even by typing the URL) a page it is not entitled to.
function RequirePerm({ perm, children }: { perm: string; children: ReactNode }) {
  const { hasPerm } = useAuth()
  return hasPerm(perm) ? <>{children}</> : <NoAccess />
}

// The landing page is the executive dashboard (PM+). Contributors, who lack it, land on Projects.
function Home() {
  const { hasPerm } = useAuth()
  return hasPerm('pmo:dashboard:view') ? <Dashboard /> : <Navigate to="/projects" replace />
}

export default function App() {
  const { isAuthed } = useAuth()
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/login" element={isAuthed ? <Navigate to="/" replace /> : <Login />} />
        <Route element={isAuthed ? <Layout /> : <Navigate to="/login" replace />}>
          <Route path="/" element={<Home />} />
          <Route path="/projects" element={<RequirePerm perm="pmo:project:list"><Projects /></RequirePerm>} />
          <Route path="/projects/:projectId" element={<RequirePerm perm="pmo:project:query"><ProjectDetail /></RequirePerm>} />
          <Route path="/tasks" element={<RequirePerm perm="pmo:task:list"><Tasks /></RequirePerm>} />
          <Route path="/milestones" element={<RequirePerm perm="pmo:milestone:list"><Milestones /></RequirePerm>} />
          <Route path="/risks" element={<RequirePerm perm="pmo:risk:list"><Risks /></RequirePerm>} />
          <Route path="/change-requests" element={<RequirePerm perm="pmo:changerequest:list"><ChangeRequests /></RequirePerm>} />
          <Route path="/finance" element={<RequirePerm perm="pmo:finance:list"><Finance /></RequirePerm>} />
          <Route path="/categories" element={<RequirePerm perm="pmo:category:list"><Categories /></RequirePerm>} />
          <Route path="/activity" element={<RequirePerm perm="pmo:automation:list"><Activity /></RequirePerm>} />
          <Route path="/employees" element={<RequirePerm perm="pmo:employee:list"><Employees /></RequirePerm>} />
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

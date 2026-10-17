import api, { unwrap } from './client'
import type {
  Paged, LoginResult, LookupItem, Project, Task, Milestone, Risk, ChangeRequest,
  ChangeHistory, CostTransaction, BudgetRevision, FinanceSummary, CostCategory,
  DashboardKpis, DashboardAnalytics, DashboardFilter,
} from './types'

// ---- auth -------------------------------------------------------------------
// captchaOff holds the raw `sys.account.captchaOnOff` config value: "off" = disabled, anything
// else (e.g. "1") = captcha required.
export interface Captcha { captchaOff: string; uuid: string; img: string }
export const getCaptcha = () => unwrap<Captcha>(api.get('/captchaImage'))
export const login = (username: string, password: string, code?: string, uuid?: string) =>
  unwrap<LoginResult>(api.post('/login', { username, password, code, uuid }))

// ---- lookups ----------------------------------------------------------------
export const getDepartments = () => unwrap<LookupItem[]>(api.get('/pmo/lookup/departments'))
export const getUsers = () => unwrap<LookupItem[]>(api.get('/pmo/lookup/users'))
export const getManagers = () => unwrap<LookupItem[]>(api.get('/pmo/lookup/managers'))
export const getProjectOptions = () => unwrap<LookupItem[]>(api.get('/pmo/lookup/projects'))
export const getCategoryOptions = () => unwrap<LookupItem[]>(api.get('/pmo/lookup/categories'))
export const getCategories = () => unwrap<CostCategory[]>(api.get('/pmo/cost-category/list'))

// ---- projects ---------------------------------------------------------------
export const listProjects = (params: Record<string, unknown>) =>
  unwrap<Paged<Project>>(api.get('/pmo/project/list', { params }))
export const getProject = (id: string) => unwrap<Project>(api.get(`/pmo/project/${id}`))
export const createProject = (dto: Partial<Project>) => unwrap<unknown>(api.post('/pmo/project', dto))
export const updateProject = (dto: Partial<Project>) => unwrap<unknown>(api.put('/pmo/project', dto))

// ---- tasks ------------------------------------------------------------------
export const listTasks = (params: Record<string, unknown>) =>
  unwrap<Paged<Task>>(api.get('/pmo/task/list', { params }))
export const createTask = (dto: Partial<Task>) => unwrap<unknown>(api.post('/pmo/task', dto))
export const updateTask = (dto: Partial<Task>) => unwrap<unknown>(api.put('/pmo/task', dto))
export const updateTaskProgress = (taskId: string, progressPct: number, version?: number) =>
  unwrap<unknown>(api.put(`/pmo/task/${taskId}/progress`, { taskId, progressPct, version }))

// ---- milestones -------------------------------------------------------------
export const listMilestones = (params: Record<string, unknown>) =>
  unwrap<Paged<Milestone>>(api.get('/pmo/milestone/list', { params }))
export const createMilestone = (dto: Partial<Milestone>) => unwrap<unknown>(api.post('/pmo/milestone', dto))
export const updateMilestone = (dto: Partial<Milestone>) => unwrap<unknown>(api.put('/pmo/milestone', dto))

// ---- risks ------------------------------------------------------------------
export const listRisks = (params: Record<string, unknown>) =>
  unwrap<Paged<Risk>>(api.get('/pmo/risk/list', { params }))
export const createRisk = (dto: Partial<Risk>) => unwrap<unknown>(api.post('/pmo/risk', dto))
export const updateRisk = (dto: Partial<Risk>) => unwrap<unknown>(api.put('/pmo/risk', dto))

// ---- change requests --------------------------------------------------------
export const listChangeRequests = (params: Record<string, unknown>) =>
  unwrap<Paged<ChangeRequest>>(api.get('/pmo/change-request/list', { params }))
export const getChangeHistory = (id: string) =>
  unwrap<ChangeHistory[]>(api.get(`/pmo/change-request/${id}/history`))
export const createChangeRequest = (dto: Partial<ChangeRequest>) =>
  unwrap<unknown>(api.post('/pmo/change-request', dto))
export const submitChangeRequest = (id: string) =>
  unwrap<unknown>(api.post(`/pmo/change-request/${id}/submit`))
export const decideChangeRequest = (dto: { changeRequestId: string; action: string; reason?: string; version?: number }) =>
  unwrap<unknown>(api.post('/pmo/change-request/decision', dto))

// ---- finance ----------------------------------------------------------------
export const getFinanceSummary = (projectId: string, cutoff?: string) =>
  unwrap<FinanceSummary>(api.get(`/pmo/finance/summary/${projectId}`, { params: { cutoff } }))
export const listTransactions = (params: Record<string, unknown>) =>
  unwrap<Paged<CostTransaction>>(api.get('/pmo/finance/transactions', { params }))
export const createTransaction = (dto: Partial<CostTransaction>) =>
  unwrap<unknown>(api.post('/pmo/finance/transactions', dto))
export const postTransaction = (transactionId: string) =>
  unwrap<unknown>(api.post(`/pmo/finance/transactions/${transactionId}/post`))
export const listRevisions = (params: Record<string, unknown>) =>
  unwrap<Paged<BudgetRevision>>(api.get('/pmo/finance/revisions', { params }))
export const createRevision = (dto: Partial<BudgetRevision>) =>
  unwrap<unknown>(api.post('/pmo/finance/revisions', dto))
export const decideRevision = (dto: { revisionId: string; approve: boolean; reason?: string }) =>
  unwrap<unknown>(api.post('/pmo/finance/revisions/decision', dto))

// ---- dashboard --------------------------------------------------------------
export const getKpis = (filter: DashboardFilter) =>
  unwrap<DashboardKpis>(api.get('/pmo/dashboard/kpis', { params: filter }))
export const getAnalytics = (filter: DashboardFilter) =>
  unwrap<DashboardAnalytics>(api.get('/pmo/dashboard/analytics', { params: filter }))
export const drilldownTasks = (filter: DashboardFilter, mode: string) =>
  unwrap<Task[]>(api.get('/pmo/dashboard/drilldown/tasks', { params: { ...filter, mode } }))
export const drilldownRisks = (filter: DashboardFilter, highExposureOnly: boolean) =>
  unwrap<Risk[]>(api.get('/pmo/dashboard/drilldown/risks', { params: { ...filter, highExposureOnly } }))

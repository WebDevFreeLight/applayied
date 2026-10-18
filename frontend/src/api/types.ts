// TypeScript models mirroring the backend DTOs. Every 64-bit id is a STRING: the backend
// serializes snowflake ids as strings (ValueToStringConverter) to avoid JS number precision loss.

export interface Paged<T> { pageNum: number; pageSize: number; totalNum: number; result: T[] }

export interface LoginResult {
  token: string
  user: { userId: string; userName: string; nickName: string; deptId?: string }
  roles: string[] // role keys, e.g. ["pmo_admin"]
  permissions: string[]
}

// Unified lookup shape returned by every /pmo/lookup/* endpoint.
export interface LookupItem { id: string; name: string }

export interface Project {
  projectId: string; code: string; name: string; deptId: string; ownerUserId: string
  status: string; priority: string; startDate?: string; plannedEndDate?: string
  actualEndDate?: string; description?: string; initialApprovedBudget: number; currency: string
  ownerName?: string; deptName?: string; taskCount?: number; progressPct?: number
}

export interface Task {
  taskId: string; projectId: string; title: string; assigneeUserId?: string
  status: string; priority: string; startDate?: string; dueDate?: string
  weight: number; progressPct: number; version?: number
  assigneeName?: string; projectName?: string; isOverdue?: boolean
}

export interface Milestone {
  milestoneId: string; projectId: string; name: string; dueDate?: string; status: string
  projectName?: string
}

export interface Risk {
  riskId: string; projectId: string; kind: string; title: string; ownerUserId?: string
  probability: number; impact: number; score: number; mitigation?: string; dueDate?: string
  status: string; ownerName?: string; projectName?: string
}

export interface ChangeRequest {
  changeRequestId: string; projectId: string; title: string; description?: string
  requestedByUserId: string; status: string; decisionByUserId?: string; decisionReason?: string
  decisionAt?: string; estimatedCostImpact?: number; version?: number
  requestedByName?: string; decisionByName?: string; projectName?: string
}

export interface ChangeHistory {
  historyId: string; changeRequestId: string; fromStatus: string; toStatus: string
  actorUserId: string; reason?: string; actedAt: string; actorName?: string
}

export interface CostTransaction {
  transactionId: string; projectId: string; categoryId: string; amount: number
  txnDate: string; description?: string; status: string; postedByUserId?: string; postedAt?: string
  projectName?: string; categoryName?: string; postedByName?: string
}

export interface BudgetRevision {
  revisionId: string; projectId: string; amount: number; reason?: string; status: string
  requestedByUserId: string; approvedByUserId?: string; approvedAt?: string; projectName?: string
}

export interface FinanceSummary {
  projectId: string; currency: string; approvedBudget: number; actualSpend: number
  remainingBudget: number; utilizationPct?: number; eac: number; projectedVariance: number
  hasForecast: boolean
}

export interface CostCategory { categoryId: string; code: string; name: string }

export interface DashboardKpis {
  currency: string; cutoff: string
  totalProjects: number; activeProjects: number; portfolioProgress: number
  overdueTasks: number; milestonesDue14: number
  openHighExposureRisks: number; blockedTasks: number; pendingChangeRequests: number
  approvedBudget: number; actualSpend: number; remainingBudget: number
  utilizationPct?: number; eac: number; projectedOverrunCount: number; projectedOverrunAmount: number
}

export interface DashboardAnalytics {
  projectHealth: { projectId: string; projectName: string; status: string; progress: number; utilizationPct?: number; isProjectedOverrun: boolean }[]
  budgetVsActual: { projectId: string; projectName: string; approvedBudget: number; actualSpend: number; eac: number }[]
  monthlyTrend: { period?: string; month?: string; amount: number }[]
  spendByCategory: { category?: string; categoryName?: string; amount: number }[]
  riskMatrix: { probability: number; impact: number; count: number }[]
  overdueByOwner: { ownerName: string; overdueCount: number }[]
  milestoneRoadmap: { projectId: string; projectName: string; name: string; dueDate?: string; status: string }[]
}

export interface DashboardFilter {
  deptId?: string; projectId?: string; managerId?: string; status?: string
  categoryId?: string; cutoff?: string
}

// ---- design modules: members, snapshots, forecasts, activity/logs -----------
export interface ProjectMember { memberId: string; projectId: string; userId: string; roleInProject?: string; userName?: string }
export interface ProgressSnapshot { snapshotId: string; projectId: string; snapshotDate: string; progressPct: number; tasksTotal: number; tasksDone: number; openRiskCount: number }
export interface FinancialForecast { forecastId: string; projectId: string; periodMonth: string; forecastAmount: number; kind: string }
export interface AutomationRun { runId: string; ruleKey: string; triggeredAt: string; status: string; detail?: string; dedupeKey?: string }
export interface OutboxNotification { notificationId: string; type: string; subject?: string; body?: string; targetUserId?: string; createdAt: string; sentAt?: string; status: string }
export interface AuditEvent { auditId: string; entityType: string; entityId: string; action: string; actorUserId: string; detail?: string; actedAt: string; actorName?: string }

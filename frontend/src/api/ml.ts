import api, { unwrap } from './client'

// Machine-learning insights. The .NET API forwards /pmo/ml/* to the Python PMO ML service
// (ml/ folder) and passes its JSON through unchanged, so field names are snake_case.
// Every response may be { available: false, message } when the ML service or a model is missing.

export interface MlUnavailable { available: false; message: string; detail?: string }

export interface MlModel {
  key: string; type: string; metric: string; name: string; objective: string; unit: string; label: string
  success: string; use: string; trained: boolean
  version?: string; trained_at?: string; rows?: number; best_model?: string; beats_baseline?: boolean
  metrics?: Record<string, number | null>; baseline?: { name: string | null; metrics: Record<string, any> }
  evaluation_strategy?: string; charts?: string[]; versions?: string[]
}

export interface TaskDelayItem {
  task_id: string; project_id: string; title: string; due_date: string | null; status: string; priority: string
  probability: number; risk_band: 'Low' | 'Medium' | 'High'; risk_label: string; verdict: string
}
export interface TaskDelay { available: true; version: string; model: string; as_of: string; drivers: string[]; items: TaskDelayItem[] }

export interface SpendItem {
  project_id: string; code: string; name: string; month: string; forecast: number; low: number; high: number
  planned_estimate: number | null; last_month: number | null; over_plan: boolean
}
export interface SpendForecast {
  available: true; version: string; model: string; mae: number; drivers: string[]
  items: SpendItem[]; history: { month: string; spend: number }[]
}

export interface AnomalyItem {
  transaction_id: string; project_id: string; category: string; amount: number; txn_date: string
  description: string; score: number; flagged: boolean; label: string; reasons: string[]
}
export interface Anomalies { available: true; version: string; model: string; flagged_total: number; items: AnomalyItem[] }

export interface SegmentItem {
  project_id: string; code: string; name: string; status: string; segment: number; segment_key: string
  segment_label: string; utilization: number | null; late_task_share: number | null; avg_progress: number | null
  open_risk_exposure: number | null; task_count: number | null; budget_log: number | null
}
export interface Segments {
  available: true; version: string; silhouette: number
  segments: { segment: number; segment_key: string; segment_label: string; size: number }[]; items: SegmentItem[]
}

export interface RiskSuggestion {
  available: true; version: string; model: string; severity: 'Low' | 'Medium' | 'High'; severity_label: string
  confidence: number; probabilities: Record<string, number>; labels: Record<string, string>; note: string
}

export interface Monitoring {
  checked_at: string; as_of: string; retrain_recommended: boolean
  reasons: { problem: string; reason: string; features?: string[]; new_rows?: number }[]
  live_task_delay: { evaluated: number; pending?: number; accuracy?: number }
  problems: Record<string, { version?: string; trained_at?: string; rows_at_training?: number; rows_now?: number
    new_rows?: number; drift?: boolean; drifted_features?: string[]; psi?: Record<string, number | null>; status?: string }>
}

export interface MlHealth { status: string; version: string; as_of: string; database: string
  models: Record<string, string | null>; scheduler: string; last_check: string | null }

type Maybe<T> = T | MlUnavailable
export const isAvailable = <T,>(r: Maybe<T> | null | undefined): r is T =>
  !!r && (r as any).available !== false

export const mlStatus = () => unwrap<Maybe<MlHealth>>(api.get('/pmo/ml/status'))
export const mlModels = () => unwrap<Maybe<{ models: MlModel[] }>>(api.get('/pmo/ml/models'))
export const mlTaskDelay = (projectId?: string | null) =>
  unwrap<Maybe<TaskDelay>>(api.get('/pmo/ml/task-delay', { params: { projectId: projectId || undefined } }))
export const mlSpendForecast = (projectId?: string | null) =>
  unwrap<Maybe<SpendForecast>>(api.get('/pmo/ml/spend-forecast', { params: { projectId: projectId || undefined } }))
export const mlAnomalies = (projectId?: string | null, top = 10) =>
  unwrap<Maybe<Anomalies>>(api.get('/pmo/ml/cost-anomalies', { params: { projectId: projectId || undefined, top } }))
export const mlSegments = () => unwrap<Maybe<Segments>>(api.get('/pmo/ml/segments'))
export const mlRiskSeverity = (text: string, kind: string, projectId?: string | null) =>
  unwrap<Maybe<RiskSuggestion>>(api.post('/pmo/ml/risk-severity', { text, kind, projectId: projectId || undefined }))
export const mlMonitoring = () => unwrap<Maybe<Monitoring>>(api.get('/pmo/ml/monitoring'))
export const mlHistory = () => unwrap<Maybe<{ runs: any[] }>>(api.get('/pmo/ml/history'))
export const mlTrain = (problems?: string[]) =>
  unwrap<any>(api.post('/pmo/ml/train', { problems, eda: true }, { timeout: 300000 }))

/** Chart PNG from the ML service as an object URL (images need the JWT, so fetch as a blob). */
export async function mlChartUrl(reportPath: string): Promise<string | null> {
  try {
    const path = reportPath.replace(/^\/?reports\//, '')
    const res = await api.get('/pmo/ml/chart', { params: { path }, responseType: 'blob' })
    return URL.createObjectURL(res.data as Blob)
  } catch {
    return null
  }
}

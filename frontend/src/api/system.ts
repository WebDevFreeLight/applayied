import api, { unwrap } from './client'
import type { Paged } from './types'

// ---- types ------------------------------------------------------------------
export interface SysConfig { configId: string; configName: string; configKey: string; configValue: string; configType: string; remark?: string; createTime?: string }
export interface SysNotice { noticeId: string; noticeTitle: string; noticeType: string; noticeContent?: string; status: string; remark?: string; createTime?: string }
export interface SysUserMsg { msgId: string; userId?: string; title?: string; content?: string; isRead?: number; msgType?: string; addTime?: string }
export interface SysJob { id: string; name: string; jobGroup?: string; cron?: string; assemblyName?: string; className?: string; isStart?: boolean; triggerType?: number; apiUrl?: string; taskType?: number; remark?: string }
export interface SysJobLog { jobLogId: string; jobName?: string; jobGroup?: string; status?: string; jobMessage?: string; createTime?: string }
export interface SysOperLog { operId: string; title?: string; businessType?: number; operName?: string; operIp?: string; status?: number; operTime?: string; operUrl?: string; errorMsg?: string }
export interface UserOnlineLog { id: string; userId?: string; userName?: string; userIP?: string; onlineTime?: number; addTime?: string }
export interface ServerInfo { cpu?: Record<string, unknown>; mem?: Record<string, unknown>; sys?: Record<string, unknown>; sysFiles?: unknown[]; [k: string]: unknown }
export interface Profile { user?: Record<string, unknown>; roleGroup?: string; postGroup?: string; dept?: Record<string, unknown> }

// ---- config (system/config) -------------------------------------------------
export const listConfig = (params: Record<string, unknown>) => unwrap<Paged<SysConfig>>(api.get('/system/config/list', { params }))
export const addConfig = (dto: Partial<SysConfig>) => unwrap<unknown>(api.post('/system/config', dto))
export const updateConfig = (dto: Partial<SysConfig>) => unwrap<unknown>(api.put('/system/config', dto))
export const removeConfig = (ids: string) => unwrap<unknown>(api.delete(`/system/config/${ids}`))

// ---- notices (system/notice) ------------------------------------------------
export const listNotice = (params: Record<string, unknown>) => unwrap<Paged<SysNotice>>(api.get('/system/notice/list', { params }))
export const addNotice = (dto: Partial<SysNotice>) => unwrap<unknown>(api.post('/system/notice', dto))
export const updateNotice = (dto: Partial<SysNotice>) => unwrap<unknown>(api.put('/system/notice', dto))
export const sendNotice = (id: string) => unwrap<unknown>(api.put(`/system/notice/send/${id}`))
export const removeNotice = (ids: string) => unwrap<unknown>(api.delete(`/system/notice/${ids}`))

// ---- my messages (SysUserMsg) -----------------------------------------------
export const myMessages = (params: Record<string, unknown>) => unwrap<Paged<SysUserMsg>>(api.get('/SysUserMsg/mylist', { params }))
export const readMessage = (msgId: string, msgType: string) => unwrap<unknown>(api.post(`/SysUserMsg/read/${msgId}/${msgType}`))
export const readAllMessages = () => unwrap<number>(api.post('/SysUserMsg/readAll'))
// Unread count grouped by type: { data: [{msgType, num}], lastSysMsgInfo }
export const myUnread = () => unwrap<{ data: { msgType: number; num: number }[]; lastSysMsgInfo?: SysUserMsg }>(api.get('/SysUserMsg/myMsgNum'))

// ---- scheduled tasks (system/Tasks) -----------------------------------------
export const listJobs = (params: Record<string, unknown>) => unwrap<Paged<SysJob>>(api.get('/system/Tasks/list', { params }))
export const createJob = (dto: Partial<SysJob>) => unwrap<unknown>(api.post('/system/Tasks/create', dto))
export const updateJob = (dto: Partial<SysJob>) => unwrap<unknown>(api.post('/system/Tasks/update', dto))
export const deleteJob = (id: string) => unwrap<unknown>(api.delete('/system/Tasks/delete', { params: { id } }))
export const jobAction = (action: 'start' | 'stop' | 'run', id: string) => unwrap<unknown>(api.get(`/system/Tasks/${action}`, { params: { id } }))

// ---- job logs (monitor/jobLog) ----------------------------------------------
export const listJobLogs = (params: Record<string, unknown>) => unwrap<Paged<SysJobLog>>(api.get('/monitor/jobLog/list', { params }))
export const cleanJobLogs = () => unwrap<unknown>(api.delete('/monitor/jobLog/clean'))

// ---- operation log (monitor/operlog) ----------------------------------------
export const listOperLog = (params: Record<string, unknown>) => unwrap<Paged<SysOperLog>>(api.get('/monitor/operlog/list', { params }))
export const cleanOperLog = () => unwrap<unknown>(api.delete('/monitor/operlog/clean'))

// ---- online users (monitor/UserOnlineLog) -----------------------------------
export const listOnline = (params: Record<string, unknown>) => unwrap<Paged<UserOnlineLog>>(api.get('/monitor/UserOnlineLog/list', { params }))

// ---- server monitor (monitor/server) ----------------------------------------
export const serverInfo = () => unwrap<ServerInfo>(api.get('/monitor/server'))

// ---- departments (system/dept) ----------------------------------------------
export interface SysDept { deptId: string; parentId: string; deptName: string; orderNum: number; leader?: string; phone?: string; email?: string; status: string }
export const listDept = (params: Record<string, unknown>) => unwrap<SysDept[]>(api.get('/system/dept/list', { params }))
export const addDept = (dto: Partial<SysDept>) => unwrap<unknown>(api.post('/system/dept', dto))
export const updateDept = (dto: Partial<SysDept>) => unwrap<unknown>(api.put('/system/dept', dto))
export const removeDept = (deptId: string) => unwrap<unknown>(api.delete(`/system/dept/${deptId}`))

// ---- positions (system/post) ------------------------------------------------
export interface SysPost { postId: string; postCode: string; postName: string; postSort: number; status: string; remark?: string }
export const listPost = (params: Record<string, unknown>) => unwrap<Paged<SysPost>>(api.get('/system/post/list', { params }))
export const addPost = (dto: Partial<SysPost>) => unwrap<unknown>(api.post('/system/post', dto))
export const updatePost = (dto: Partial<SysPost>) => unwrap<unknown>(api.put('/system/post', dto))
export const removePost = (postId: string) => unwrap<unknown>(api.delete(`/system/post/${postId}`))

// ---- dictionaries (system/dict) ---------------------------------------------
export interface DictType { dictId: string; dictName: string; dictType: string; status: string; remark?: string }
export interface DictData { dictCode: string; dictLabel: string; dictValue: string; dictType: string; dictSort: number; status: string }
export const listDictType = (params: Record<string, unknown>) => unwrap<Paged<DictType>>(api.get('/system/dict/type/list', { params }))
export const listDictData = (dictType: string) => unwrap<DictData[]>(api.get(`/system/dict/data/type/${dictType}`))

// ---- login log (monitor/logininfor) -----------------------------------------
export interface SysLogininfor { infoId: string; userName?: string; ipaddr?: string; loginLocation?: string; browser?: string; os?: string; status?: string; msg?: string; loginTime?: string }
export const listLoginLog = (params: Record<string, unknown>) => unwrap<Paged<SysLogininfor>>(api.get('/monitor/logininfor/list', { params }))
export const cleanLoginLog = () => unwrap<unknown>(api.delete('/monitor/logininfor/clean'))

// ---- online sessions (monitor/online) ---------------------------------------
export interface OnlineUser { tokenId: string; userName?: string; ipaddr?: string; loginLocation?: string; browser?: string; os?: string; loginTime?: string }
export const listSessions = (params: Record<string, unknown>) => unwrap<Paged<OnlineUser>>(api.get('/monitor/online/list', { params }))
export const forceLogout = (tokenId: string) => unwrap<unknown>(api.delete('/monitor/online/force', { params: { tokenId } }))

// ---- profile (system/user/profile) ------------------------------------------
export const getProfile = () => unwrap<Profile>(api.get('/system/user/profile'))
export const updateProfile = (dto: Record<string, unknown>) => unwrap<unknown>(api.put('/system/user/profile', dto))
export const updatePwd = (oldPassword: string, newPassword: string) => unwrap<unknown>(api.put('/system/user/profile/updatePwd', { oldPassword, newPassword }))

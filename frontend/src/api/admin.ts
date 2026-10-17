import api, { unwrap } from './client'
import type { Paged } from './types'

// ---- shared shapes ----------------------------------------------------------
export interface SysUser {
  userId: string; userName: string; nickName: string; email?: string; phonenumber?: string
  sex?: number; status: number; deptId?: string; remark?: string; createTime?: string
  roleIds?: string[]
}
export interface SysRole {
  roleId: string; roleName: string; roleKey: string; roleSort: number; status: number
  dataScope?: number; remark?: string; createTime?: string; menuIds?: string[]
}
export interface SysMenu {
  menuId: string; menuName: string; parentId: string; orderNum: number; path?: string
  component?: string; menuType: string; visible?: string; status?: string; perms?: string
  icon?: string; children?: SysMenu[]
}
export interface TreeSelect { id: string; label: string; children?: TreeSelect[] }

// ---- users (system/user) ----------------------------------------------------
export const listUsers = (params: Record<string, unknown>) =>
  unwrap<Paged<SysUser>>(api.get('/system/user/list', { params }))
export const getUser = (userId: string) =>
  unwrap<{ data?: SysUser; roles?: SysRole[]; roleIds?: string[]; posts?: unknown[]; postIds?: string[] }>(
    api.get(`/system/user/${userId}`))
export const getUserFormMeta = () =>
  unwrap<{ roles?: SysRole[]; posts?: unknown[] }>(api.get('/system/user/'))
export const addUser = (dto: Partial<SysUser> & { password?: string; roleIds?: string[] }) =>
  unwrap<unknown>(api.post('/system/user/add', dto))
export const editUser = (dto: Partial<SysUser> & { roleIds?: string[] }) =>
  unwrap<unknown>(api.put('/system/user/edit', dto))
export const changeUserStatus = (userId: string, status: number) =>
  unwrap<unknown>(api.put('/system/user/changeStatus', { userId, status }))
export const removeUser = (userId: string) =>
  unwrap<unknown>(api.delete(`/system/user/${userId}`))
export const resetUserPwd = (userId: string, password: string) =>
  unwrap<unknown>(api.put('/system/user/resetPwd', { userId, password }))

// ---- roles (system/role) ----------------------------------------------------
export const listRoles = (params: Record<string, unknown>) =>
  unwrap<Paged<SysRole>>(api.get('/system/role/list', { params }))
export const allRoles = () => unwrap<Paged<SysRole>>(api.get('/system/role/list', { params: { pageNum: 1, pageSize: 100 } }))
export const addRole = (dto: Partial<SysRole>) => unwrap<unknown>(api.post('/system/role/edit', dto))
export const editRole = (dto: Partial<SysRole>) => unwrap<unknown>(api.put('/system/role/edit', dto))
export const removeRole = (roleId: string) => unwrap<unknown>(api.delete(`/system/role/${roleId}`))
export const changeRoleStatus = (roleId: string, status: number) =>
  unwrap<unknown>(api.put('/system/role/changeStatus', { roleId, status }))
export const authorizeRoleMenus = (roleId: string, menuIds: string[]) =>
  unwrap<unknown>(api.put('/system/role/dataScope', { roleId, menuIds, dataScope: 1 }))
/** Menu tree + the keys already checked for a role (for the permission-assignment dialog). */
export const roleMenuTree = (roleId: string) =>
  unwrap<{ menus: TreeSelect[]; checkedKeys: string[] }>(api.get(`/system/menu/roleMenuTreeselect/${roleId}`))

// ---- menus / permissions (system/menu) --------------------------------------
export const listMenus = (params?: Record<string, unknown>) =>
  unwrap<SysMenu[]>(api.get('/system/menu/list', { params }))
export const addMenu = (dto: Partial<SysMenu>) => unwrap<unknown>(api.put('/system/menu/add', dto))
export const editMenu = (dto: Partial<SysMenu>) => unwrap<unknown>(api.post('/system/menu/edit', dto))
export const removeMenu = (menuId: string) => unwrap<unknown>(api.delete(`/system/menu/${menuId}`))
export const syncPermissions = () => unwrap<unknown>(api.post('/system/menu/SyncPermissions'))
export const setupRbac = () => unwrap<unknown>(api.post('/pmo/admin/setup-rbac'))

// ---- lookups ----------------------------------------------------------------
export const deptTree = () => unwrap<TreeSelect[]>(api.get('/system/dept/treeSelect'))

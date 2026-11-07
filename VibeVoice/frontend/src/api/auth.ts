import { http, type QueryParams } from './client'
import type {
  AdminOverviewDto,
  AdminUserDto,
  AuthResponse,
  CreateUserRequest,
  PagedResult,
  PermissionDto,
  RoleDto,
  SaveRoleRequest,
  UpdateUserRequest,
  UserListQuery,
  ChangePasswordRequest,
  LoginRequest,
  RegisterRequest,
  UpdateProfileRequest,
  UserDto,
} from './types'

export const authApi = {
  register: (body: RegisterRequest) => http.post<AuthResponse>('/api/auth/register', body),
  login: (body: LoginRequest) => http.post<AuthResponse>('/api/auth/login', body),
  me: () => http.get<UserDto>('/api/auth/me'),
  updateProfile: (body: UpdateProfileRequest) => http.put<UserDto>('/api/users/me', body),
  changePassword: (body: ChangePasswordRequest) => http.put<void>('/api/users/me/password', body),
}

export const usersApi = {
  list: (q: UserListQuery = {}, signal?: AbortSignal) =>
    http.get<PagedResult<AdminUserDto>>('/api/users', q as QueryParams, signal),
  get: (id: string) => http.get<UserDto>(`/api/users/${id}`),
  create: (body: CreateUserRequest) => http.post<UserDto>('/api/users', body),
  update: (id: string, body: UpdateUserRequest) => http.put<UserDto>(`/api/users/${id}`, body),
  setRole: (id: string, roleId: string) => http.put<UserDto>(`/api/users/${id}/role`, { roleId }),
  setActive: (id: string, isActive: boolean) => http.put<UserDto>(`/api/users/${id}/active`, { isActive }),
  resetPassword: (id: string, newPassword: string) =>
    http.put<void>(`/api/users/${id}/password`, { newPassword }),
  remove: (id: string) => http.del(`/api/users/${id}`),
}

export const rolesApi = {
  list: () => http.get<RoleDto[]>('/api/roles'),
  create: (body: SaveRoleRequest) => http.post<RoleDto>('/api/roles', body),
  update: (id: string, body: SaveRoleRequest) => http.put<RoleDto>(`/api/roles/${id}`, body),
  remove: (id: string) => http.del(`/api/roles/${id}`),
}

export const permissionsApi = {
  list: () => http.get<PermissionDto[]>('/api/permissions'),
}

export const adminApi = {
  overview: () => http.get<AdminOverviewDto>('/api/admin/overview'),
}

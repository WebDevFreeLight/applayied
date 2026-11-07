import { http } from './client'
import type {
  AuthResponse,
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
  list: () => http.get<UserDto[]>('/api/users'),
  setRole: (id: string, role: UserDto['role']) =>
    http.put<UserDto>(`/api/users/${id}/role`, { role }),
  setActive: (id: string, isActive: boolean) =>
    http.put<UserDto>(`/api/users/${id}/active`, { isActive }),
}

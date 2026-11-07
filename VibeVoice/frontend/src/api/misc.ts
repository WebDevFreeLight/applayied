import { http } from './client'
import type {
  DashboardDto,
  HealthDto,
  NameCount,
  PagedResult,
  SearchQuery,
  SearchResultDto,
} from './types'

export const searchApi = {
  search: ({ q, types, page = 1, pageSize = 20 }: SearchQuery, signal?: AbortSignal) =>
    http.get<PagedResult<SearchResultDto>>('/api/search', { q, types, page, pageSize }, signal),
}

export const tagsApi = {
  tags: () => http.get<NameCount[]>('/api/tags'),
  people: () => http.get<NameCount[]>('/api/people'),
}

export const dashboardApi = {
  get: () => http.get<DashboardDto>('/api/dashboard'),
  health: () => http.get<HealthDto>('/api/health'),
}

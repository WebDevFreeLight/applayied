import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useTranslation } from 'react-i18next'
import { useNavigate } from 'react-router'
import { todayIso } from '@/lib/utils'
import { PERMISSION_CATALOG } from '@/lib/admin'
import { permissionsApi, rolesApi } from './auth'
import { toastError } from '@/lib/notify'
import { qk } from './keys'
import { memoriesApi } from './memories'
import { tagsApi } from './misc'
import { notesApi } from './notes'
import type { SaveNoteRequest } from './types'
import { voiceApi } from './voice'

export function useVoiceStatus() {
  return useQuery({
    queryKey: qk.voice.status(),
    queryFn: voiceApi.status,
    staleTime: 30_000,
    refetchInterval: 60_000,
    retry: false,
  })
}

/** True when the API reports an OCR engine (optimistic while the status is still loading). */
export function useOcrAvailable(): boolean {
  const { data, isError } = useVoiceStatus()
  if (data) return !!data.ocrEngine
  return !isError
}

export function useVoices() {
  return useQuery({
    queryKey: qk.voice.voices(),
    queryFn: voiceApi.voices,
    staleTime: 5 * 60_000,
    retry: false,
  })
}

export function useRoles(enabled = true) {
  return useQuery({ queryKey: qk.roles, queryFn: rolesApi.list, staleTime: 30_000, enabled })
}

/** Permission catalogue (GET /api/permissions) with a built-in fallback. */
export function usePermissionCatalog() {
  const q = useQuery({ queryKey: qk.permissions, queryFn: permissionsApi.list, staleTime: 10 * 60_000 })
  return q.data?.length ? q.data : PERMISSION_CATALOG
}

export function useTags() {
  return useQuery({ queryKey: qk.tags, queryFn: tagsApi.tags, staleTime: 60_000 })
}

export function usePeople() {
  return useQuery({ queryKey: qk.people, queryFn: tagsApi.people, staleTime: 60_000 })
}

/** Creates an "Untitled" note (attachments need an id) and opens the editor. */
export function useCreateNote() {
  const { t } = useTranslation()
  const navigate = useNavigate()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (opts?: { body?: Partial<SaveNoteRequest>; search?: string }) =>
      notesApi.create({ title: t('notes.untitled'), body: '', tags: [], ...opts?.body }),
    onSuccess: (note, opts) => {
      qc.setQueryData(qk.notes.detail(note.id), note)
      void qc.invalidateQueries({ queryKey: qk.notes.lists() })
      void qc.invalidateQueries({ queryKey: qk.dashboard })
      navigate(`/notes/${note.id}${opts?.search ?? ''}`)
    },
    onError: (err) => toastError(err),
  })
}

/** Creates an "Untitled" memory dated today and opens the editor. */
export function useCreateMemory() {
  const { t } = useTranslation()
  const navigate = useNavigate()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: () =>
      memoriesApi.create({
        title: t('memories.untitled'),
        story: '',
        memoryDate: todayIso(),
        tags: [],
        people: [],
      }),
    onSuccess: (memory) => {
      qc.setQueryData(qk.memories.detail(memory.id), memory)
      void qc.invalidateQueries({ queryKey: qk.memories.lists() })
      void qc.invalidateQueries({ queryKey: qk.dashboard })
      navigate(`/memories/${memory.id}`)
    },
    onError: (err) => toastError(err),
  })
}

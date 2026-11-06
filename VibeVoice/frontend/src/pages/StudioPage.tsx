import { zodResolver } from '@hookform/resolvers/zod'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Copy, FileAudio, FilePlus2, Mic, ShieldCheck, Sparkles, Upload, Wand2 } from 'lucide-react'
import { useRef, useState } from 'react'
import { useForm, useWatch } from 'react-hook-form'
import { useTranslation } from 'react-i18next'
import { toast } from 'sonner'
import { z } from 'zod'
import { ApiError, errorMessage } from '@/api/client'
import { useCreateNote, useVoiceStatus, useVoices } from '@/api/hooks'
import { qk } from '@/api/keys'
import type { AudioDto, TranscriptionResult } from '@/api/types'
import { voiceApi } from '@/api/voice'
import { useAuth } from '@/auth/context'
import { AudioPlayer } from '@/components/audio/AudioPlayer'
import { AudioRecorder } from '@/components/audio/AudioRecorder'
import { VoiceSelect } from '@/components/audio/VoiceSelect'
import { VoiceStatusChip } from '@/components/layout/VoiceStatusChip'
import { Button } from '@/components/ui/Button'
import { Card, EmptyState, PageHeader, ProgressBar, Segmented, Skeleton } from '@/components/ui/Display'
import { Checkbox, Field, Select, Textarea } from '@/components/ui/Field'
import { copyText } from '@/hooks/misc'
import { formatDuration, MEDIA_ACCEPT, textToHtml, validateMediaFile } from '@/lib/utils'

type Tab = 'tts' | 'stt'
const LANGS = ['auto', 'en', 'ar'] as const

export function StudioPage() {
  const { t } = useTranslation()
  const [tab, setTab] = useState<Tab>('tts')
  return (
    <div>
      <PageHeader icon={Sparkles} title={t('studio.title')} subtitle={t('studio.subtitle')} actions={<VoiceStatusChip />} />
      <Segmented
        label={t('studio.title')}
        value={tab}
        onChange={setTab}
        options={[
          { value: 'tts', label: t('studio.ttsTab'), icon: Sparkles },
          { value: 'stt', label: t('studio.sttTab'), icon: Mic },
        ]}
        className="mb-6"
      />
      {tab === 'tts' ? <TextToSpeech /> : <SpeechToText />}
    </div>
  )
}

function TextToSpeech() {
  const { t } = useTranslation()
  const { user } = useAuth()
  const qc = useQueryClient()
  const status = useVoiceStatus()
  const voicesQuery = useVoices()
  const voices = voicesQuery.data ?? []
  const [result, setResult] = useState<AudioDto | null>(null)
  const [page, setPage] = useState(1)

  const schema = z.object({
    text: z.string().trim().min(1, t('validation.textRequired')).max(5000),
    language: z.enum(LANGS),
    voice: z.string(),
  })
  type Values = z.infer<typeof schema>
  const form = useForm<Values>({
    resolver: zodResolver(schema),
    defaultValues: { text: '', language: 'auto', voice: user?.preferredVoice ?? '' },
  })
  const language = useWatch({ control: form.control, name: 'language' })
  const text = useWatch({ control: form.control, name: 'text' })
  const voice = useWatch({ control: form.control, name: 'voice' })
  const matching = language === 'auto' ? voices : voices.filter((v) => v.language === language)
  const shownVoices = matching.length ? matching : voices
  // A voice picked for another language is dropped when the language filter changes.
  const effectiveVoice = shownVoices.some((v) => v.id === voice) ? voice : ''

  const tts = useMutation({
    mutationFn: (v: Values) =>
      voiceApi.tts({
        text: v.text.trim(),
        voice: effectiveVoice || undefined,
        language: v.language === 'auto' ? undefined : v.language,
      }),
    onSuccess: (audio) => {
      setResult(audio)
      void qc.invalidateQueries({ queryKey: qk.voice.histories() })
      void qc.invalidateQueries({ queryKey: qk.audio.all })
      void qc.invalidateQueries({ queryKey: qk.dashboard })
    },
    onError: (err) =>
      toast.error(err instanceof ApiError && err.status === 503 ? t('readAloud.unavailable') : errorMessage(err)),
  })

  const history = useQuery({
    queryKey: qk.voice.history(page),
    queryFn: () => voiceApi.history(page, 10),
  })
  const totalPages = history.data ? Math.max(1, Math.ceil(history.data.total / history.data.pageSize)) : 1
  const unavailable = status.data ? !status.data.available : false

  return (
    <div className="grid gap-6 lg:grid-cols-5">
      <Card className="flex flex-col gap-4 p-5 lg:col-span-3">
        <form noValidate onSubmit={form.handleSubmit((v) => tts.mutate(v))} className="flex flex-col gap-4">
          <Field label={t('studio.text')} error={form.formState.errors.text?.message} hint={t('studio.characters', { count: text.length })}>
            {(p) => (
              <Textarea
                {...p}
                {...form.register('text')}
                rows={8}
                maxLength={5000}
                placeholder={t('studio.textPlaceholder')}
                className="min-h-48 text-base"
              />
            )}
          </Field>
          <div className="grid gap-4 sm:grid-cols-2">
            <Field label={t('studio.language')}>
              {(p) => (
                <Select {...p} {...form.register('language')}>
                  {LANGS.map((l) => (
                    <option key={l} value={l}>
                      {t(`studio.languages.${l}`)}
                    </option>
                  ))}
                </Select>
              )}
            </Field>
            <Field label={t('studio.voice')} hint={shownVoices.find((v) => v.id === effectiveVoice)?.language === 'ar' ? t('readAloud.fallbackExplain') : undefined}>
              {(p) =>
                voicesQuery.isLoading ? (
                  <Skeleton className="h-10" />
                ) : (
                  <VoiceSelect
                    {...p}
                    voices={shownVoices}
                    value={effectiveVoice}
                    onChange={(v) => form.setValue('voice', v)}
                    emptyLabel={t('readAloud.defaultVoice')}
                  />
                )
              }
            </Field>
          </div>
          {tts.isPending && <ProgressBar label={t('studio.generating')} />}
          <div className="flex flex-wrap items-center justify-between gap-3">
            <p className="flex items-center gap-1.5 text-xs text-subtle">
              <ShieldCheck className="size-3.5 shrink-0" aria-hidden />
              {t('studio.responsibleUse')}
            </p>
            <span title={unavailable ? t('readAloud.unavailable') : undefined}>
              <Button
                type="submit"
                variant="primary"
                loading={tts.isPending}
                disabled={unavailable}
                icon={<Sparkles className="size-4" aria-hidden />}
              >
                {tts.isPending ? t('studio.generating') : t('studio.generate')}
              </Button>
            </span>
          </div>
        </form>
        {result && (
          <div className="flex flex-col gap-2 border-t border-border pt-4">
            <h2 className="text-sm font-semibold text-fg">{t('studio.result')}</h2>
            <AudioPlayer key={result.id} audio={result} />
          </div>
        )}
      </Card>

      <section className="flex flex-col gap-3 lg:col-span-2" aria-labelledby="studio-history">
        <h2 id="studio-history" className="text-sm font-semibold text-fg">
          {t('studio.history')}
        </h2>
        {history.isLoading ? (
          <Skeleton className="h-32" />
        ) : !history.data?.items.length ? (
          <EmptyState icon={Sparkles} title={t('studio.historyEmpty')} className="py-10" />
        ) : (
          <>
            <ul className="flex flex-col gap-3">
              {history.data.items.map((a) => (
                <li key={a.id}>
                  <AudioPlayer
                    audio={a}
                    onDelete={async () => {
                      await voiceApi.deleteHistory(a.id)
                      if (result?.id === a.id) setResult(null)
                      void qc.invalidateQueries({ queryKey: qk.voice.histories() })
                      void qc.invalidateQueries({ queryKey: qk.audio.all })
                    }}
                  />
                </li>
              ))}
            </ul>
            {totalPages > 1 && (
              <div className="flex items-center justify-between">
                <Button size="sm" disabled={page <= 1} onClick={() => setPage((p) => p - 1)}>
                  {t('common.previous')}
                </Button>
                <span className="text-xs text-muted tabular-nums">
                  {page} / {totalPages}
                </span>
                <Button size="sm" disabled={page >= totalPages} onClick={() => setPage((p) => p + 1)}>
                  {t('common.next')}
                </Button>
              </div>
            )}
          </>
        )}
      </section>
    </div>
  )
}

function SpeechToText() {
  const { t } = useTranslation()
  const qc = useQueryClient()
  const createNote = useCreateNote()
  const [mode, setMode] = useState<'record' | 'upload'>('record')
  const [language, setLanguage] = useState<(typeof LANGS)[number]>('auto')
  const [keep, setKeep] = useState(true)
  const [progress, setProgress] = useState<number | null>(null)
  const [result, setResult] = useState<TranscriptionResult | null>(null)
  const [dragging, setDragging] = useState(false)
  const inputRef = useRef<HTMLInputElement>(null)

  const transcribe = async (file: File, onProgress?: (f: number) => void) => {
    setProgress(0)
    try {
      const res = await voiceApi.transcribe(
        file,
        { language: language === 'auto' ? undefined : language, save: keep },
        (f) => {
          setProgress(f)
          onProgress?.(f)
        },
      )
      setResult(res)
      toast.success(t('audio.transcribed'))
      if (res.audio) {
        void qc.invalidateQueries({ queryKey: qk.voice.histories() })
        void qc.invalidateQueries({ queryKey: qk.audio.all })
      }
    } catch (err) {
      toast.error(err instanceof ApiError && err.status === 503 ? t('errors.voiceUnavailable') : errorMessage(err))
      throw err
    } finally {
      setProgress(null)
    }
  }

  const onFile = (file: File | undefined) => {
    if (!file) return
    const check = validateMediaFile(file)
    if (!check.ok) {
      toast.error(
        check.reason === 'invalidType'
          ? t('audio.invalidType', { name: file.name })
          : t('audio.tooLarge', { name: file.name, kind: t(`audio.kindLower.${check.kind ?? 'Audio'}`), max: check.maxMb }),
      )
      return
    }
    void transcribe(file).catch(() => undefined)
  }

  const saveAsNote = () => {
    if (!result?.text) return
    const title = result.text.replace(/\s+/g, ' ').trim().slice(0, 60)
    createNote.mutate(
      { body: { title: title || t('notes.untitled'), body: textToHtml(result.text) } },
      { onSuccess: () => toast.success(t('studio.savedAsNote')) },
    )
  }

  return (
    <div className="grid gap-6 lg:grid-cols-2">
      <Card className="flex flex-col gap-4 p-5">
        <div className="flex flex-wrap items-end gap-4">
          <Segmented
            label={t('studio.sttTab')}
            value={mode}
            onChange={setMode}
            options={[
              { value: 'record', label: t('studio.record'), icon: Mic },
              { value: 'upload', label: t('studio.uploadFile'), icon: Upload },
            ]}
          />
          <label className="flex flex-col gap-1 text-xs font-medium text-muted">
            {t('studio.language')}
            <Select value={language} onChange={(e) => setLanguage(e.target.value as (typeof LANGS)[number])} className="h-9 w-40">
              {LANGS.map((l) => (
                <option key={l} value={l}>
                  {t(`studio.languages.${l}`)}
                </option>
              ))}
            </Select>
          </label>
        </div>
        <Checkbox label={t('studio.saveClip')} checked={keep} onChange={(e) => setKeep(e.target.checked)} />

        {mode === 'record' ? (
          <AudioRecorder
            saveLabel={t('studio.transcribe')}
            onSave={(file, _duration, onProgress) => transcribe(file, onProgress)}
          />
        ) : (
          <div
            onDragOver={(e) => {
              e.preventDefault()
              setDragging(true)
            }}
            onDragLeave={() => setDragging(false)}
            onDrop={(e) => {
              e.preventDefault()
              setDragging(false)
              onFile(e.dataTransfer.files?.[0])
            }}
            className={
              dragging
                ? 'flex flex-col items-center gap-3 rounded-2xl border-2 border-dashed border-accent bg-accent-soft/60 px-6 py-10 text-center'
                : 'flex flex-col items-center gap-3 rounded-2xl border-2 border-dashed border-border-strong px-6 py-10 text-center'
            }
          >
            <FileAudio className="size-8 text-accent-text" aria-hidden />
            <p className="text-sm text-muted">{t('studio.dropAudio')}</p>
            <input
              ref={inputRef}
              type="file"
              accept={MEDIA_ACCEPT}
              className="sr-only"
              tabIndex={-1}
              aria-hidden
              onChange={(e) => {
                onFile(e.target.files?.[0])
                e.target.value = ''
              }}
            />
            <Button onClick={() => inputRef.current?.click()} loading={progress !== null} icon={<Upload className="size-4" aria-hidden />}>
              {t('studio.chooseFile')}
            </Button>
          </div>
        )}
        {progress !== null && (
          <div className="flex flex-col gap-1.5">
            <span className="text-xs text-muted">{t('studio.transcribing', { percent: Math.round(progress * 100) })}</span>
            <ProgressBar value={progress < 1 ? progress : undefined} label={t('studio.transcribe')} />
          </div>
        )}
      </Card>

      <Card className="flex flex-col gap-4 p-5">
        <div className="flex flex-wrap items-center justify-between gap-2">
          <h2 className="flex items-center gap-2 text-sm font-semibold text-fg">
            <Wand2 className="size-4 text-accent-text" aria-hidden />
            {t('studio.transcript')}
          </h2>
          {result?.text && (
            <div className="flex gap-2">
              <Button
                size="sm"
                icon={<Copy className="size-4" aria-hidden />}
                onClick={() => void copyText(result.text).then((ok) => ok && toast.success(t('common.copied')))}
              >
                {t('common.copy')}
              </Button>
              <Button size="sm" variant="primary" loading={createNote.isPending} onClick={saveAsNote} icon={<FilePlus2 className="size-4" aria-hidden />}>
                {t('studio.saveAsNote')}
              </Button>
            </div>
          )}
        </div>
        {!result ? (
          <p className="rounded-xl bg-surface-2 p-6 text-center text-sm text-muted">{t('studio.noTranscript')}</p>
        ) : (
          <>
            {result.language && (
              <p className="text-xs text-subtle">{t('studio.detectedLanguage', { lang: result.language })}</p>
            )}
            {!result.audio && (
              <p dir="auto" className="rounded-xl bg-surface-2 p-4 text-sm leading-relaxed whitespace-pre-wrap text-fg">
                {result.text}
              </p>
            )}
            {result.audio ? (
              <AudioPlayer
                key={result.audio.id}
                audio={{
                  ...result.audio,
                  transcript: result.audio.transcript ?? result.text,
                  segments: result.audio.segments?.length ? result.audio.segments : result.segments,
                }}
                defaultShowTranscript
              />
            ) : (
              result.segments.length > 0 && (
                <div>
                  <h3 className="mb-2 text-xs font-semibold tracking-wide text-subtle uppercase">{t('studio.segments')}</h3>
                  <ol className="flex max-h-80 flex-col gap-1.5 overflow-y-auto">
                    {result.segments.map((s, i) => (
                      <li key={i} className="flex gap-3 rounded-lg px-2 py-1.5 text-sm hover:bg-surface-2">
                        <span className="shrink-0 font-mono text-xs text-subtle tabular-nums" dir="ltr">
                          {formatDuration(s.start)}–{formatDuration(s.end)}
                        </span>
                        {s.speaker && (
                          <span className="shrink-0 text-xs font-medium text-accent-text" title={t('studio.speaker')}>
                            {s.speaker}
                          </span>
                        )}
                        <span dir="auto" className="text-fg">
                          {s.text}
                        </span>
                      </li>
                    ))}
                  </ol>
                </div>
              )
            )}
          </>
        )}
      </Card>
    </div>
  )
}

// Mirrors docs/API_CONTRACT.md exactly. JSON is camelCase, enums are strings,
// dates are ISO-8601 UTC strings and `memoryDate` is a date-only string (yyyy-MM-dd).

export type Role = 'Admin' | 'User'
export type Language = 'en' | 'ar'
export type ThemePreference = 'light' | 'dark' | 'system'
/** Visual identity (v4). */
export type Palette = 'sunset' | 'oasis' | 'ocean' | 'neon'

export interface UserDto {
  id: string
  email: string
  displayName: string
  role: Role
  preferredLanguage: Language
  preferredVoice: string | null
  theme: ThemePreference
  /** v4 visual identity (may be missing from older API builds → default "sunset"). */
  palette: Palette
  isActive: boolean
  createdAt: string
}

export interface AuthResponse {
  token: string
  expiresAt: string
  user: UserDto
}

export interface RegisterRequest {
  email: string
  password: string
  displayName: string
  preferredLanguage?: Language
}

export interface LoginRequest {
  email: string
  password: string
}

export interface UpdateProfileRequest {
  displayName: string
  preferredLanguage: Language
  preferredVoice?: string | null
  theme: ThemePreference
  palette?: Palette
}

export interface ChangePasswordRequest {
  currentPassword: string
  newPassword: string
}

export interface PagedResult<T> {
  items: T[]
  page: number
  pageSize: number
  total: number
}

export type ImageOwnerType = 'Note' | 'Memory' | 'Library'
export type AudioOwnerType = 'Note' | 'Memory' | 'Studio'
export type AudioSource = 'Recorded' | 'Uploaded' | 'Tts'
export type MediaKind = 'Audio' | 'Video'

export interface ImageDto {
  id: string
  ownerType: ImageOwnerType
  ownerId: string
  filePath: string
  thumbnailPath: string
  mimeType: string
  width: number
  height: number
  sizeBytes: number
  caption: string | null
  sortOrder: number
  /** v3: text extracted from the picture (null until extracted). */
  ocrText: string | null
  ocrLanguage: string | null
  createdAt: string
}

export type ImageWithOwnerDto = ImageDto & { ownerTitle: string | null }

export interface AudioDto {
  id: string
  ownerType: AudioOwnerType
  ownerId: string
  source: AudioSource
  filePath: string
  mimeType: string
  durationSeconds: number | null
  sizeBytes: number
  transcript: string | null
  transcriptLanguage: string | null
  voiceName: string | null
  ttsEngine: string | null
  /** The text a TTS clip was generated from. */
  text: string | null
  /** True when source == "Tts" → the UI must show an "AI-generated" label. */
  isAiGenerated: boolean
  /** v2: render <video> for Video, waveform/audio player for Audio. */
  mediaKind: MediaKind
  /** v2: timestamped transcript ([] until transcribed). */
  segments: Segment[]
  createdAt: string
}

export type AudioWithOwnerDto = AudioDto & { ownerTitle: string | null }

// ---------- Notes ----------

export interface NoteDto {
  id: string
  title: string
  /** HTML from TipTap */
  body: string
  color: string | null
  isPinned: boolean
  isArchived: boolean
  isDeleted: boolean
  tags: string[]
  images: ImageDto[]
  audio: AudioDto[]
  createdAt: string
  updatedAt: string
  deletedAt: string | null
}

export interface NoteListItemDto {
  id: string
  title: string
  /** plain text, ≤ 200 chars */
  excerpt: string
  color: string | null
  isPinned: boolean
  isArchived: boolean
  isDeleted: boolean
  tags: string[]
  imageCount: number
  audioCount: number
  /** first image thumb */
  thumbnailPath: string | null
  createdAt: string
  updatedAt: string
}

export interface SaveNoteRequest {
  /** ≤ 200 */
  title: string
  body: string
  color?: string | null
  tags: string[]
}

export type NoteSort = 'updated_desc' | 'updated_asc' | 'created_desc' | 'created_asc' | 'title_asc'

export interface NoteListQuery {
  page?: number
  pageSize?: number
  search?: string
  tag?: string
  from?: string
  to?: string
  sort?: NoteSort
  includeArchived?: boolean
  archivedOnly?: boolean
  deleted?: boolean
  hasAudio?: boolean
  hasImages?: boolean
}

// ---------- Memories ----------

export const MOODS = ['Happy', 'Grateful', 'Calm', 'Excited', 'Nostalgic', 'Proud', 'Sad', 'Loved'] as const
export type Mood = (typeof MOODS)[number]

export interface MemoryDto {
  id: string
  title: string
  /** HTML */
  story: string
  /** yyyy-MM-dd */
  memoryDate: string
  location: string | null
  mood: Mood | null
  coverImageId: string | null
  isPinned: boolean
  isArchived: boolean
  isDeleted: boolean
  tags: string[]
  people: string[]
  images: ImageDto[]
  audio: AudioDto[]
  createdAt: string
  updatedAt: string
  deletedAt: string | null
}

export interface MemoryListItemDto {
  id: string
  title: string
  excerpt: string
  memoryDate: string
  location: string | null
  mood: Mood | null
  isPinned: boolean
  isArchived: boolean
  isDeleted: boolean
  tags: string[]
  people: string[]
  imageCount: number
  audioCount: number
  coverThumbnailPath: string | null
  createdAt: string
  updatedAt: string
}

export interface SaveMemoryRequest {
  title: string
  story: string
  memoryDate: string
  location?: string | null
  mood?: Mood | null
  tags: string[]
  people: string[]
}

export type MemorySort = 'date_desc' | 'date_asc' | 'updated_desc' | 'title_asc'

export interface MemoryListQuery {
  page?: number
  pageSize?: number
  search?: string
  tag?: string
  person?: string
  mood?: Mood
  from?: string
  to?: string
  sort?: MemorySort
  includeArchived?: boolean
  archivedOnly?: boolean
  deleted?: boolean
  hasAudio?: boolean
  hasImages?: boolean
}

// ---------- Shared attachment requests ----------

export interface TtsRequest {
  voice?: string
  text?: string
  language?: string
}

export type TranscriptFormat = 'txt' | 'srt' | 'vtt'

export interface TranscribeRequest {
  appendToBody?: boolean
  language?: string
}

// ---------- Library / audio / voice ----------

export interface ImageListQuery {
  page?: number
  pageSize?: number
  ownerType?: ImageOwnerType
}

export interface AudioListQuery {
  page?: number
  pageSize?: number
  kind?: MediaKind
  source?: AudioSource
  ownerType?: AudioOwnerType
  search?: string
}

export type VoiceEngine = 'vibevoice' | 'edge' | 'dev'

export interface VoiceDto {
  id: string
  name: string
  /** "en", "ar", ... */
  language: string
  gender: 'male' | 'female' | null
  engine: VoiceEngine
  isFallback: boolean
}

export interface VoiceStatus {
  available: boolean
  ttsEngine: string | null
  asrEngine: string | null
  /** v3: OCR engine (e.g. "rapidocr"), null when text extraction is unavailable. */
  ocrEngine: string | null
  device: string | null
  message: string | null
}

export interface StudioTtsRequest {
  text: string
  voice?: string
  language?: string
}

export interface Segment {
  start: number
  end: number
  text: string
  speaker: string | null
}

export interface OcrLine {
  text: string
  confidence: number
}

export interface OcrResultDto {
  text: string
  language: string | null
  /** top-to-bottom reading order */
  lines: OcrLine[]
  /** set when the picture was kept in the Images library (save=true) */
  image: ImageDto | null
}

export interface OcrRequest {
  appendToBody?: boolean
  language?: string
}

export interface TranscriptionResult {
  text: string
  language: string
  segments: Segment[]
  audio: AudioDto | null
}

// ---------- Search / tags / dashboard / health ----------

export type SearchKind = 'note' | 'memory' | 'audio' | 'image'
export type SearchType = 'notes' | 'memories' | 'audio' | 'images'
export type MatchedIn = 'title' | 'body' | 'tag' | 'transcript' | 'person' | 'location' | 'imageText'

export interface SearchResultDto {
  kind: SearchKind
  id: string
  ownerId: string | null
  ownerKind: 'note' | 'memory' | 'studio' | 'library' | null
  title: string
  snippet: string
  matchedIn: MatchedIn
  date: string
  /** Optional thumbnail for image results (rendered when the API provides it). */
  thumbnailPath?: string | null
}

export interface SearchQuery {
  q: string
  types?: SearchType[]
  page?: number
  pageSize?: number
}

export interface NameCount {
  name: string
  count: number
}

export interface DashboardStats {
  notes: number
  memories: number
  audioClips: number
  images: number
  aiVoiceClips: number
  transcripts: number
  pinned: number
  trash: number
}

export interface ActivityItem {
  kind: 'note' | 'memory' | 'audio' | 'image'
  action: 'created' | 'updated'
  id: string
  ownerId: string | null
  title: string
  at: string
}

export interface DashboardDto {
  stats: DashboardStats
  recentNotes: NoteListItemDto[]
  recentMemories: MemoryListItemDto[]
  /** same month/day, earlier years */
  onThisDay: MemoryListItemDto[]
  activity: ActivityItem[]
}

export interface HealthDto {
  status: 'Healthy' | 'Degraded'
  database: 'up' | 'down'
  voiceService: 'up' | 'down'
}

export interface ProblemDetails {
  type?: string
  title?: string
  status?: number
  detail?: string
  errors?: Record<string, string[]>
  [key: string]: unknown
}

// SignalR realtime client. One shared connection to the backend MessageHub (/msgHub).
// Connecting makes the user appear in the server's online-clients registry (so they show up on
// the Sessions page) and lets the server push notifications/notices in real time.
import * as signalR from '@microsoft/signalr'

export type HubEvent =
  | 'notification' // in-app notification (automation / direct) -> {msgId,title,body,type,time}
  | 'notice'       // published announcement -> {title, body}
  | 'force'        // admin forced this session to log out -> {reason}
  | 'online'       // online count changed -> {num}

type Handler = (payload: unknown) => void

const handlers: Record<HubEvent, Set<Handler>> = {
  notification: new Set(), notice: new Set(), force: new Set(), online: new Set(),
}

let connection: signalR.HubConnection | null = null

/** Subscribe to a realtime event. Returns an unsubscribe function. */
export function onHub(event: HubEvent, handler: Handler): () => void {
  handlers[event].add(handler)
  return () => handlers[event].delete(handler)
}

function emit(event: HubEvent, payload: unknown) {
  handlers[event].forEach((h) => { try { h(payload) } catch { /* ignore handler errors */ } })
}

/** Open the realtime connection (idempotent). Call once the user is authenticated. */
export function connectRealtime(): void {
  if (connection) return
  const token = localStorage.getItem('pmo.token')
  if (!token) return

  connection = new signalR.HubConnectionBuilder()
    .withUrl('/msgHub', {
      accessTokenFactory: () => localStorage.getItem('pmo.token') || '',
    })
    .withAutomaticReconnect([0, 2000, 5000, 10000, 20000])
    .configureLogging(signalR.LogLevel.Warning)
    .build()

  // Server -> client events (names match ZR.ServiceCore.Signalr.HubsConstant).
  connection.on('receiveNotification', (p) => emit('notification', p))
  connection.on('receiveNotice', (title: string, body: string) => emit('notice', { title, body }))
  connection.on('onlineNum', (p: { num: number }) => emit('online', p))
  connection.on('forceUser', (p: { Reason?: string; reason?: string }) =>
    emit('force', { reason: p?.Reason || p?.reason || 'Your session was ended.' }))

  connection.start().catch(() => { /* reconnect policy handles retries */ })
}

/** Close the realtime connection (call on sign-out). */
export function disconnectRealtime(): void {
  const c = connection
  connection = null
  if (c) c.stop().catch(() => { /* ignore */ })
}

export function isRealtimeConnected(): boolean {
  return connection?.state === signalR.HubConnectionState.Connected
}

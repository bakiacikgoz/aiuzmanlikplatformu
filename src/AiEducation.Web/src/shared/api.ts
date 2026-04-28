const API_BASE = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5076/api/v1'

export type AuthSession = {
  accessToken: string
  expiresAtUtc: string
  userId: string
  displayName: string
  email: string
}

export function getToken() {
  return localStorage.getItem('ai_education_token')
}

export function saveSession(session: AuthSession) {
  localStorage.setItem('ai_education_token', session.accessToken)
  localStorage.setItem('ai_education_user', JSON.stringify(session))
}

export function clearSession() {
  localStorage.removeItem('ai_education_token')
  localStorage.removeItem('ai_education_user')
}

export async function api<T>(path: string, options: RequestInit = {}): Promise<T> {
  const token = getToken()
  const headers = new Headers(options.headers)
  headers.set('Content-Type', 'application/json')
  if (token) {
    headers.set('Authorization', `Bearer ${token}`)
  }

  const response = await fetch(`${API_BASE}${path}`, {
    ...options,
    headers,
    credentials: 'include',
  })

  if (!response.ok) {
    const text = await response.text()
    throw new Error(text || `Request failed: ${response.status}`)
  }

  if (response.status === 204) {
    return undefined as T
  }

  return (await response.json()) as T
}

export async function post<T>(path: string, body: unknown): Promise<T> {
  return api<T>(path, { method: 'POST', body: JSON.stringify(body) })
}

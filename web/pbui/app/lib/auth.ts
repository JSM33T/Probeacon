const KEYS = {
  accessToken: "pb_access_token",
  refreshToken: "pb_refresh_token",
  sessionId: "pb_session_id",
} as const

export interface AuthUser {
  userId: string
  email: string
  displayName: string
  role: string
  sessionId: string
  emailVerified: boolean
}

function parseJwt(token: string): AuthUser | null {
  try {
    const payload = JSON.parse(atob(token.split(".")[1]))
    return {
      userId: payload.sub,
      email: payload.email,
      displayName: payload.name,
      role: payload.role,
      sessionId: payload.session_id,
      emailVerified: payload.email_verified === "true",
    }
  } catch {
    return null
  }
}

function getJwtExpiry(token: string): number | null {
  try {
    const payload = JSON.parse(atob(token.split(".")[1]))
    return typeof payload.exp === "number" ? payload.exp * 1000 : null
  } catch {
    return null
  }
}

export function getToken(): string | null {
  if (typeof window === "undefined") return null
  return localStorage.getItem(KEYS.accessToken)
}

export function getRefreshToken(): string | null {
  if (typeof window === "undefined") return null
  return localStorage.getItem(KEYS.refreshToken)
}

export function getSessionId(): string | null {
  if (typeof window === "undefined") return null
  return localStorage.getItem(KEYS.sessionId)
}

export function getUser(): AuthUser | null {
  const token = getToken()
  return token ? parseJwt(token) : null
}

export function isTokenExpired(): boolean {
  const token = getToken()
  if (!token) return true
  const expiry = getJwtExpiry(token)
  if (expiry === null) return true
  return Date.now() >= expiry - 30_000 // 30s buffer
}

export function setSession(accessToken: string, refreshToken: string, sessionId: string): void {
  localStorage.setItem(KEYS.accessToken, accessToken)
  localStorage.setItem(KEYS.refreshToken, refreshToken)
  localStorage.setItem(KEYS.sessionId, sessionId)
}

export function setToken(accessToken: string): void {
  localStorage.setItem(KEYS.accessToken, accessToken)
}

export function clearSession(): void {
  localStorage.removeItem(KEYS.accessToken)
  localStorage.removeItem(KEYS.refreshToken)
  localStorage.removeItem(KEYS.sessionId)
}

/** @deprecated use clearSession */
export const clearToken = clearSession

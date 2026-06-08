// Legacy localStorage keys — only referenced to scrub any tokens left over from the
// previous (pre-HttpOnly-cookie) implementation. Nothing is written here anymore.
const LEGACY_KEYS = ["pb_access_token", "pb_refresh_token", "pb_session_id"] as const

export interface AuthUser {
  userId: string
  email: string
  displayName: string
  role: string
  sessionId: string
  emailVerified: boolean
  tenantId: string
  tenantSlug: string
  tenantKind: string
  tenantExpiresAt: string | null
}

// The access token lives in memory only — never in localStorage — so an XSS payload can't
// read it from storage. The long-lived refresh token is an HttpOnly cookie the JS never sees.
// A full page reload clears this; `ensureSession()` re-mints it from the refresh cookie.
let accessToken: string | null = null

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
      tenantId: payload.tenant_id,
      tenantSlug: payload.tenant_slug,
      tenantKind: payload.tenant_kind,
      tenantExpiresAt: payload.tenant_expires_at ?? null,
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
  return accessToken
}

export function getUser(): AuthUser | null {
  return accessToken ? parseJwt(accessToken) : null
}

export function isTokenExpired(): boolean {
  if (!accessToken) return true
  const expiry = getJwtExpiry(accessToken)
  if (expiry === null) return true
  return Date.now() >= expiry - 30_000 // 30s buffer
}

export function setToken(token: string): void {
  accessToken = token
}

export function clearSession(): void {
  accessToken = null
  if (typeof window !== "undefined") {
    for (const key of LEGACY_KEYS) localStorage.removeItem(key)
  }
}

// Dedupe concurrent refreshes within this tab so a burst of 401s triggers a single rotation.
let refreshPromise: Promise<boolean> | null = null

/**
 * Mints a fresh access token from the HttpOnly refresh cookie. Returns false when no valid
 * session exists. On an expired (410) workspace it clears state and redirects to /expired.
 */
export function refreshSession(): Promise<boolean> {
  if (refreshPromise) return refreshPromise

  refreshPromise = (async () => {
    try {
      const res = await fetch("/api/auth/refresh", {
        method: "POST",
        credentials: "include",
      })

      if (res.status === 410) {
        clearSession()
        if (typeof window !== "undefined") window.location.href = "/expired"
        return false
      }

      if (!res.ok) return false

      const data = (await res.json()) as { accessToken: string }
      setToken(data.accessToken)
      return true
    } catch {
      return false
    } finally {
      refreshPromise = null
    }
  })()

  return refreshPromise
}

/**
 * Guarantees a usable access token for route loaders: returns true if one is already in
 * memory and unexpired, otherwise attempts a cookie-backed refresh.
 */
export async function ensureSession(): Promise<boolean> {
  if (accessToken && !isTokenExpired()) return true
  return refreshSession()
}

/** @deprecated use clearSession */
export const clearToken = clearSession

import { clearSession, getRefreshToken, getSessionId, getToken, setSession, setToken } from "./auth"

export class ApiError extends Error {
  constructor(public status: number, message: string) {
    super(message)
  }
}

let refreshPromise: Promise<boolean> | null = null

async function tryRefresh(): Promise<boolean> {
  if (refreshPromise) return refreshPromise

  refreshPromise = (async () => {
    const refreshToken = getRefreshToken()
    const sessionId = getSessionId()
    if (!refreshToken || !sessionId) return false

    try {
      const res = await fetch("/api/auth/refresh", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ sessionId, refreshToken }),
      })

      if (!res.ok) return false

      const data = await res.json()
      setSession(data.accessToken, data.refreshToken, sessionId)
      return true
    } catch {
      return false
    } finally {
      refreshPromise = null
    }
  })()

  return refreshPromise
}

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const token = getToken()
  const headers: Record<string, string> = { "Content-Type": "application/json" }
  if (token) headers["Authorization"] = `Bearer ${token}`

  const res = await fetch(path, { ...init, headers: { ...headers, ...init?.headers } })

  if (res.status === 401) {
    const refreshed = await tryRefresh()
    if (refreshed) {
      const newToken = getToken()
      const retryHeaders: Record<string, string> = { "Content-Type": "application/json" }
      if (newToken) retryHeaders["Authorization"] = `Bearer ${newToken}`

      const retry = await fetch(path, { ...init, headers: { ...retryHeaders, ...init?.headers } })
      if (retry.ok) {
        if (retry.status === 204) return undefined as T
        return retry.json()
      }
    }

    clearSession()
    window.location.href = "/login"
    throw new ApiError(401, "Unauthorized")
  }

  if (!res.ok) {
    const body = await res.text().catch(() => res.statusText)
    throw new ApiError(res.status, body)
  }

  if (res.status === 204) return undefined as T
  return res.json()
}

export const api = {
  get: <T>(path: string) => request<T>(path),
  post: <T>(path: string, body: unknown) =>
    request<T>(path, { method: "POST", body: JSON.stringify(body) }),
  put: <T>(path: string, body: unknown) =>
    request<T>(path, { method: "PUT", body: JSON.stringify(body) }),
  patch: <T>(path: string, body: unknown) =>
    request<T>(path, { method: "PATCH", body: JSON.stringify(body) }),
  delete: <T>(path: string) => request<T>(path, { method: "DELETE" }),
}

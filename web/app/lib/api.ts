import { clearSession, getToken, refreshSession } from "./auth"

export class ApiError extends Error {
  constructor(
    public status: number,
    message: string,
    public code?: string
  ) {
    super(message)
  }
}

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const token = getToken()
  const headers: Record<string, string> = { "Content-Type": "application/json" }
  if (token) headers["Authorization"] = `Bearer ${token}`

  const res = await fetch(path, { ...init, headers: { ...headers, ...init?.headers } })

  if (res.status === 401) {
    const refreshed = await refreshSession()
    if (refreshed) {
      const newToken = getToken()
      const retryHeaders: Record<string, string> = { "Content-Type": "application/json" }
      if (newToken) retryHeaders["Authorization"] = `Bearer ${newToken}`

      const retry = await fetch(path, { ...init, headers: { ...retryHeaders, ...init?.headers } })
      if (retry.ok) {
        if (retry.status === 204) return undefined as T
        return retry.json()
      }

      if (retry.status === 410) {
        const body = await readError(retry)
        clearSession()
        window.location.href = "/expired"
        throw new ApiError(410, body.message, body.code)
      }
    }

    clearSession()
    window.location.href = "/login"
    throw new ApiError(401, "Unauthorized")
  }

  if (!res.ok) {
    const body = await readError(res)
    if (res.status === 503 && body.configured === false) {
      clearSession()
      window.location.href = "/setup"
    }
    if (res.status === 410 && body.code === "workspace_expired") {
      clearSession()
      window.location.href = "/expired"
    }
    throw new ApiError(res.status, body.message, body.code)
  }

  if (res.status === 204) return undefined as T
  return res.json()
}

async function readError(res: Response): Promise<{
  message: string
  code?: string
  configured?: boolean
}> {
  const text = await res.text().catch(() => "")
  if (!text) return { message: res.statusText }

  try {
    const parsed = JSON.parse(text)
    return {
      message: parsed.detail ?? parsed.title ?? text,
      code: parsed.code,
      configured: parsed.configured,
    }
  } catch {
    return { message: text }
  }
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
  download: async (path: string, fallbackFilename: string) => {
    const token = getToken()
    const res = await fetch(path, {
      headers: token ? { Authorization: `Bearer ${token}` } : {},
    })
    if (!res.ok) {
      const body = await res.text().catch(() => res.statusText)
      throw new ApiError(res.status, body)
    }

    const disposition = res.headers.get("Content-Disposition")
    const match = disposition?.match(/filename="?([^";]+)"?/i)
    const filename = match?.[1] ?? fallbackFilename

    const blob = await res.blob()
    const url = URL.createObjectURL(blob)
    const anchor = document.createElement("a")
    anchor.href = url
    anchor.download = filename
    document.body.appendChild(anchor)
    anchor.click()
    anchor.remove()
    URL.revokeObjectURL(url)
  },
}

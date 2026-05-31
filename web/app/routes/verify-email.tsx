import { useEffect, useRef, useState } from "react"
import { Link, useSearchParams } from "react-router"
import { CheckCircle, XCircle } from "lucide-react"
import { api } from "~/lib/api"
import { getRefreshToken, getSessionId, setSession } from "~/lib/auth"
import { Button } from "~/components/ui/button"

type Status = "verifying" | "success" | "error"

export default function VerifyEmailPage() {
  const [searchParams] = useSearchParams()
  const token = searchParams.get("token")
  const [status, setStatus] = useState<Status>("verifying")
  const [errorMessage, setErrorMessage] = useState<string>("")
  const attempted = useRef(false)

  useEffect(() => {
    if (!token || attempted.current) return
    attempted.current = true

    const verify = async () => {
      try {
        await api.post("/api/auth/verify-email", { token })

        // refresh the JWT so email_verified claim updates immediately
        const refreshToken = getRefreshToken()
        const sessionId = getSessionId()
        if (refreshToken && sessionId) {
          try {
            const res = await api.post<{
              accessToken: string
              refreshToken: string
            }>("/api/auth/refresh", { sessionId, refreshToken })
            setSession(res.accessToken, res.refreshToken, sessionId)
          } catch {
            // non-fatal — banner will disappear on next login
          }
        }

        setStatus("success")
      } catch (err: unknown) {
        setErrorMessage(err instanceof Error ? err.message : "Verification failed.")
        setStatus("error")
      }
    }

    verify()
  }, [token])

  if (!token) {
    return (
      <div className="flex min-h-screen items-center justify-center px-4">
        <div className="text-center">
          <XCircle className="mx-auto mb-4 size-12 text-destructive" />
          <h1 className="text-xl font-semibold">Invalid link</h1>
          <p className="mt-2 text-sm text-muted-foreground">No verification token found in this URL.</p>
          <Button asChild className="mt-6">
            <Link to="/dashboard">Go to dashboard</Link>
          </Button>
        </div>
      </div>
    )
  }

  if (status === "verifying") {
    return (
      <div className="flex min-h-screen items-center justify-center">
        <span className="text-sm text-muted-foreground">Verifying your email…</span>
      </div>
    )
  }

  if (status === "success") {
    return (
      <div className="flex min-h-screen items-center justify-center px-4">
        <div className="text-center">
          <CheckCircle className="mx-auto mb-4 size-12 text-green-600" />
          <h1 className="text-xl font-semibold">Email verified</h1>
          <p className="mt-2 text-sm text-muted-foreground">Your email address has been confirmed.</p>
          <Button asChild className="mt-6">
            <Link to="/dashboard">Go to dashboard</Link>
          </Button>
        </div>
      </div>
    )
  }

  return (
    <div className="flex min-h-screen items-center justify-center px-4">
      <div className="text-center">
        <XCircle className="mx-auto mb-4 size-12 text-destructive" />
        <h1 className="text-xl font-semibold">Verification failed</h1>
        <p className="mt-2 text-sm text-muted-foreground">
          {errorMessage || "This link may have expired. Request a new one from the dashboard."}
        </p>
        <Button asChild className="mt-6">
          <Link to="/dashboard">Go to dashboard</Link>
        </Button>
      </div>
    </div>
  )
}

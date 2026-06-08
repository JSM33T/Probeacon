import { type FormEvent, useState } from "react"
import { Link, useNavigate, useSearchParams } from "react-router"
import { XCircle } from "lucide-react"
import { api } from "~/lib/api"
import { setSession } from "~/lib/auth"
import { Button } from "~/components/ui/button"
import { Input } from "~/components/ui/input"
import { Label } from "~/components/ui/label"
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "~/components/ui/card"

export default function SetPasswordPage() {
  const [searchParams] = useSearchParams()
  const token = searchParams.get("token")
  const navigate = useNavigate()
  const [form, setForm] = useState({ password: "", confirm: "" })
  const [error, setError] = useState<string | null>(null)
  const [loading, setLoading] = useState(false)

  const set =
    (key: keyof typeof form) => (e: React.ChangeEvent<HTMLInputElement>) =>
      setForm((f) => ({ ...f, [key]: e.target.value }))

  const submit = async (e: FormEvent) => {
    e.preventDefault()
    setError(null)

    if (form.password.length < 8) {
      setError("Password must be at least 8 characters.")
      return
    }
    if (form.password !== form.confirm) {
      setError("Passwords do not match.")
      return
    }

    setLoading(true)
    try {
      const res = await api.post<{
        accessToken: string
        refreshToken: string
        sessionId: string
      }>("/api/auth/set-password", { token, password: form.password })
      setSession(res.accessToken, res.refreshToken, res.sessionId)
      navigate("/dashboard", { replace: true })
    } catch (err) {
      setError(
        err instanceof Error ? err.message : "This link is invalid or has expired."
      )
    } finally {
      setLoading(false)
    }
  }

  if (!token) {
    return (
      <div className="flex min-h-screen items-center justify-center px-4">
        <div className="text-center">
          <XCircle className="mx-auto mb-4 size-12 text-destructive" />
          <h1 className="text-xl font-semibold">Invalid link</h1>
          <p className="mt-2 text-sm text-muted-foreground">
            No token found in this URL. Ask your admin to send a new invite.
          </p>
          <Button asChild className="mt-6">
            <Link to="/login">Go to sign in</Link>
          </Button>
        </div>
      </div>
    )
  }

  return (
    <div className="flex min-h-screen items-center justify-center bg-background p-4">
      <div className="w-full max-w-sm">
        <div className="mb-8 text-center">
          <h1 className="text-2xl font-semibold">ProBeacon</h1>
          <p className="mt-1 text-sm text-muted-foreground">
            Set a password to access your account
          </p>
        </div>
        <Card>
          <CardHeader>
            <CardTitle>Set your password</CardTitle>
            <CardDescription>
              Choose a password — you'll be signed in straight away.
            </CardDescription>
          </CardHeader>
          <CardContent>
            <form onSubmit={submit} className="flex flex-col gap-4">
              <div className="flex flex-col gap-1.5">
                <Label>New password</Label>
                <Input
                  type="password"
                  value={form.password}
                  onChange={set("password")}
                  placeholder="At least 8 characters"
                  autoComplete="new-password"
                  required
                />
              </div>
              <div className="flex flex-col gap-1.5">
                <Label>Confirm password</Label>
                <Input
                  type="password"
                  value={form.confirm}
                  onChange={set("confirm")}
                  placeholder="Re-enter password"
                  autoComplete="new-password"
                  required
                />
              </div>
              {error && <p className="text-sm text-destructive">{error}</p>}
              <Button type="submit" disabled={loading} className="mt-1 w-full">
                {loading ? "Setting password..." : "Set password & continue"}
              </Button>
            </form>
          </CardContent>
        </Card>
      </div>
    </div>
  )
}

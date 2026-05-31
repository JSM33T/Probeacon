import { type FormEvent, useState } from "react"
import { redirect, useNavigate } from "react-router"
import { api } from "~/lib/api"
import { getToken, setSession } from "~/lib/auth"
import { Button } from "~/components/ui/button"
import { Input } from "~/components/ui/input"
import { Label } from "~/components/ui/label"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "~/components/ui/card"

export async function clientLoader() {
  if (getToken()) return redirect("/dashboard")
  return null
}

export function HydrateFallback() {
  return (
    <div className="min-h-screen flex items-center justify-center">
      <span className="text-sm text-muted-foreground">Loading…</span>
    </div>
  )
}

export default function LoginPage() {
  const navigate = useNavigate()
  const [form, setForm] = useState({ email: "", password: "" })
  const [error, setError] = useState<string | null>(null)
  const [loading, setLoading] = useState(false)

  const set = (key: keyof typeof form) => (e: React.ChangeEvent<HTMLInputElement>) =>
    setForm((f) => ({ ...f, [key]: e.target.value }))

  const submit = async (e: FormEvent) => {
    e.preventDefault()
    setError(null)
    setLoading(true)
    try {
      const res = await api.post<{ accessToken: string; refreshToken: string; sessionId: string }>("/api/auth/login", form)
      setSession(res.accessToken, res.refreshToken, res.sessionId)
      navigate("/dashboard", { replace: true })
    } catch {
      setError("Invalid email or password.")
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="min-h-screen flex items-center justify-center bg-background p-4">
      <div className="w-full max-w-sm">
        <div className="text-center mb-8">
          <h1 className="text-2xl font-semibold">ProBeacon</h1>
          <p className="text-sm text-muted-foreground mt-1">Sign in to your account</p>
        </div>
        <Card>
          <CardHeader>
            <CardTitle>Sign in</CardTitle>
            <CardDescription>Enter your credentials to continue</CardDescription>
          </CardHeader>
          <CardContent>
            <form onSubmit={submit} className="flex flex-col gap-4">
              <div className="flex flex-col gap-1.5">
                <Label>Email</Label>
                <Input type="email" value={form.email} onChange={set("email")} placeholder="jane@acme.com" required />
              </div>
              <div className="flex flex-col gap-1.5">
                <Label>Password</Label>
                <Input type="password" value={form.password} onChange={set("password")} placeholder="••••••••" required />
              </div>
              {error && <p className="text-sm text-destructive">{error}</p>}
              <Button type="submit" disabled={loading} className="w-full mt-1">
                {loading ? "Signing in…" : "Sign in"}
              </Button>
            </form>
          </CardContent>
        </Card>
      </div>
    </div>
  )
}

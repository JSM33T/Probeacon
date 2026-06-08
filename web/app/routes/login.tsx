import { type FormEvent, useState } from "react"
import { Link, redirect, useLoaderData, useNavigate } from "react-router"
import { api } from "~/lib/api"
import { ensureSession, setToken } from "~/lib/auth"
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

interface SetupStatus {
  configured: boolean
  deploymentMode: "SelfHosted" | "OnlineDemo"
}

export async function clientLoader() {
  if (await ensureSession()) return redirect("/dashboard")
  const status = await api.get<SetupStatus>("/api/setup/status")
  if (status.deploymentMode === "SelfHosted" && !status.configured) {
    return redirect("/setup")
  }
  return { deploymentMode: status.deploymentMode }
}

export function HydrateFallback() {
  return (
    <div className="flex min-h-screen items-center justify-center">
      <span className="text-sm text-muted-foreground">Loading...</span>
    </div>
  )
}

export default function LoginPage() {
  const { deploymentMode } = useLoaderData<typeof clientLoader>()
  const navigate = useNavigate()
  const [form, setForm] = useState({ email: "", password: "" })
  const [error, setError] = useState<string | null>(null)
  const [loading, setLoading] = useState(false)
  const isOnlineDemo = deploymentMode === "OnlineDemo"

  const set =
    (key: keyof typeof form) => (e: React.ChangeEvent<HTMLInputElement>) =>
      setForm((f) => ({ ...f, [key]: e.target.value }))

  const submit = async (e: FormEvent) => {
    e.preventDefault()
    setError(null)
    setLoading(true)
    try {
      const res = await api.post<{ accessToken: string }>("/api/auth/login", form)
      setToken(res.accessToken)
      navigate("/dashboard", { replace: true })
    } catch (err) {
      setError(err instanceof Error ? err.message : "Invalid email or password.")
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="flex min-h-screen items-center justify-center bg-background p-4">
      <div className="w-full max-w-sm">
        <div className="mb-8 text-center">
          <h1 className="text-2xl font-semibold">ProBeacon</h1>
          <p className="mt-1 text-sm text-muted-foreground">
            {isOnlineDemo
              ? "Sign in to your temporary workspace"
              : "Sign in to your account"}
          </p>
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
                <Input
                  type="email"
                  value={form.email}
                  onChange={set("email")}
                  placeholder="jane@company.com"
                  required
                />
              </div>
              <div className="flex flex-col gap-1.5">
                <div className="flex items-center justify-between">
                  <Label>Password</Label>
                  <Link
                    to="/forgot-password"
                    className="text-xs text-muted-foreground underline"
                  >
                    Forgot password?
                  </Link>
                </div>
                <Input
                  type="password"
                  value={form.password}
                  onChange={set("password")}
                  placeholder="Password"
                  required
                />
              </div>
              {error && <p className="text-sm text-destructive">{error}</p>}
              <Button type="submit" disabled={loading} className="mt-1 w-full">
                {loading ? "Signing in..." : "Sign in"}
              </Button>
            </form>
            {isOnlineDemo && (
              <p className="mt-4 text-center text-sm text-muted-foreground">
                Need a demo workspace?{" "}
                <Link to="/signup" className="font-medium text-foreground underline">
                  Create one
                </Link>
              </p>
            )}
          </CardContent>
        </Card>
      </div>
    </div>
  )
}

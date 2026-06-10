import { type FormEvent, useState } from "react"
import { redirect, useNavigate } from "react-router"
import { api } from "~/lib/api"
import { ensureSession, setToken } from "~/lib/auth"
import { passwordError } from "~/lib/password"
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

export async function clientLoader() {
  if (await ensureSession()) return redirect("/dashboard")
  const status = await api.get<{ configured: boolean; deploymentMode: string }>(
    "/api/setup/status"
  )
  if (status.deploymentMode === "OnlineDemo") return redirect("/signup")
  if (status.configured) return redirect("/login")
  return null
}

export default function SetupPage() {
  const navigate = useNavigate()
  const [form, setForm] = useState({
    orgName: "",
    adminName: "",
    email: "",
    password: "",
  })
  const [error, setError] = useState<string | null>(null)
  const [loading, setLoading] = useState(false)

  const set =
    (key: keyof typeof form) => (e: React.ChangeEvent<HTMLInputElement>) =>
      setForm((f) => ({ ...f, [key]: e.target.value }))

  const submit = async (e: FormEvent) => {
    e.preventDefault()
    setError(null)
    const pwError = passwordError(form.password)
    if (pwError) {
      setError(pwError)
      return
    }
    setLoading(true)
    try {
      const res = await api.post<{ accessToken: string }>("/api/setup", form)
      setToken(res.accessToken)
      navigate("/dashboard", { replace: true })
    } catch (err) {
      setError(err instanceof Error ? err.message : "Setup failed.")
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="flex min-h-screen items-center justify-center bg-background p-4">
      <div className="w-full max-w-md">
        <div className="mb-8 text-center">
          <h1 className="text-2xl font-semibold">Welcome to ProBeacon</h1>
          <p className="mt-1 text-sm text-muted-foreground">
            Set up your organization to get started
          </p>
        </div>
        <Card>
          <CardHeader>
            <CardTitle>Create organization</CardTitle>
            <CardDescription>This only runs once on first launch</CardDescription>
          </CardHeader>
          <CardContent>
            <form onSubmit={submit} className="flex flex-col gap-4">
              <Field
                label="Organization name"
                value={form.orgName}
                onChange={set("orgName")}
                placeholder="Acme Inc."
              />
              <Field
                label="Your name"
                value={form.adminName}
                onChange={set("adminName")}
                placeholder="Jane Smith"
              />
              <Field
                label="Email"
                type="email"
                value={form.email}
                onChange={set("email")}
                placeholder="admin@company.com"
              />
              <Field
                label="Password"
                type="password"
                value={form.password}
                onChange={set("password")}
                placeholder="Password"
              />
              {error && <p className="text-sm text-destructive">{error}</p>}
              <Button type="submit" disabled={loading} className="mt-1 w-full">
                {loading ? "Setting up..." : "Create organization"}
              </Button>
            </form>
          </CardContent>
        </Card>
      </div>
    </div>
  )
}

function Field({
  label,
  type = "text",
  value,
  onChange,
  placeholder,
}: {
  label: string
  type?: string
  value: string
  onChange: (e: React.ChangeEvent<HTMLInputElement>) => void
  placeholder?: string
}) {
  return (
    <div className="flex flex-col gap-1.5">
      <Label>{label}</Label>
      <Input
        type={type}
        value={value}
        onChange={onChange}
        placeholder={placeholder}
        required
      />
    </div>
  )
}

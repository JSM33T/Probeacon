import { type FormEvent, useState } from "react"
import { Link, redirect, useLoaderData, useNavigate } from "react-router"
import { api } from "~/lib/api"
import { getToken, setSession } from "~/lib/auth"
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
  demoWorkspaceLifetimeHours: number
}

export async function clientLoader() {
  if (getToken()) return redirect("/dashboard")
  const status = await api.get<SetupStatus>("/api/setup/status")
  if (status.deploymentMode !== "OnlineDemo") {
    return redirect(status.configured ? "/login" : "/setup")
  }
  return { lifetimeHours: status.demoWorkspaceLifetimeHours }
}

export function HydrateFallback() {
  return (
    <div className="flex min-h-screen items-center justify-center">
      <span className="text-sm text-muted-foreground">Loading...</span>
    </div>
  )
}

export default function SignupPage() {
  const { lifetimeHours } = useLoaderData<typeof clientLoader>()
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
    setLoading(true)
    try {
      const res = await api.post<{
        accessToken: string
        refreshToken: string
        sessionId: string
      }>("/api/auth/signup", form)
      setSession(res.accessToken, res.refreshToken, res.sessionId)
      navigate("/dashboard", { replace: true })
    } catch (err) {
      setError(err instanceof Error ? err.message : "Signup failed.")
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="flex min-h-screen items-center justify-center bg-background p-4">
      <div className="w-full max-w-md">
        <div className="mb-8 text-center">
          <h1 className="text-2xl font-semibold">Try ProBeacon</h1>
          <p className="mt-1 text-sm text-muted-foreground">
            Create a temporary workspace that expires in {lifetimeHours} hours.
          </p>
        </div>
        <Card>
          <CardHeader>
            <CardTitle>Create demo workspace</CardTitle>
            <CardDescription>
              Use a real email so you can sign back in before it expires.
            </CardDescription>
          </CardHeader>
          <CardContent>
            <form onSubmit={submit} className="flex flex-col gap-4">
              <Field
                label="Workspace name"
                value={form.orgName}
                onChange={set("orgName")}
                placeholder="Acme Observability"
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
                placeholder="jane@company.com"
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
                {loading ? "Creating..." : "Create workspace"}
              </Button>
            </form>
            <p className="mt-4 text-center text-sm text-muted-foreground">
              Already created one?{" "}
              <Link to="/login" className="font-medium text-foreground underline">
                Sign in
              </Link>
            </p>
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

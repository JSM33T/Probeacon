import { type FormEvent, useState } from "react"
import { useLoaderData } from "react-router"
import { KeyRound, Lock } from "lucide-react"
import { toast } from "sonner"
import { api } from "~/lib/api"
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
import { cn } from "~/lib/utils"

interface SmtpSettings {
  host: string
  port: number
  username: string
  hasPassword: boolean
  fromAddress: string
  fromName: string
  enableSsl: boolean
  isConfigured: boolean
}

interface LockoutSettings {
  enabled: boolean
  maxAttempts: number
  baseMinutes: number
  maxMinutes: number
}

export async function clientLoader() {
  const [smtp, lockout] = await Promise.all([
    api.get<SmtpSettings>("/api/settings/smtp"),
    api.get<LockoutSettings>("/api/settings/lockout"),
  ])
  return { smtp, lockout }
}

// ── method registry ──────────────────────────────────────────────────────────

type MethodKey = "email"

const METHODS: {
  key: MethodKey
  label: string
  description: string
  pivot: boolean
  available: boolean
}[] = [
  {
    key: "email",
    label: "Email / Password",
    description: "Local accounts with email verification via SMTP.",
    pivot: true,
    available: true,
  },
]

// ── SMTP config panel ────────────────────────────────────────────────────────

type TestState =
  | { status: "idle" }
  | { status: "testing" }
  | { status: "ok"; message: string }
  | { status: "fail"; message: string }

function SmtpPanel({ initial }: { initial: SmtpSettings }) {
  const [smtp, setSmtp] = useState(initial)
  const [form, setForm] = useState({
    host: initial.host,
    port: initial.port,
    username: initial.username,
    password: "",
    fromAddress: initial.fromAddress,
    fromName: initial.fromName,
    enableSsl: initial.enableSsl,
  })
  const [saving, setSaving] = useState(false)
  const [test, setTest] = useState<TestState>({ status: "idle" })

  const field =
    (key: keyof typeof form) => (e: React.ChangeEvent<HTMLInputElement>) =>
      setForm((f) => ({ ...f, [key]: e.target.value }))

  const save = async (e: FormEvent) => {
    e.preventDefault()
    setSaving(true)
    try {
      const updated = await api.put<SmtpSettings>("/api/settings/smtp", {
        ...form,
        port: Number(form.port),
        password: form.password || undefined,
      })
      setSmtp(updated)
      setForm((f) => ({ ...f, password: "" }))
      toast.success("SMTP settings saved")
    } catch (err: unknown) {
      toast.error(
        err instanceof Error ? err.message : "Failed to save SMTP settings."
      )
    } finally {
      setSaving(false)
    }
  }

  const runTest = async () => {
    setTest({ status: "testing" })
    try {
      const res = await api.post<{ success: boolean; message: string }>(
        "/api/settings/smtp/test",
        {}
      )
      setTest(
        res.success
          ? { status: "ok", message: res.message }
          : { status: "fail", message: res.message }
      )
      if (res.success)
        toast.success("Test email sent", { description: res.message })
      else toast.error("Test email failed", { description: res.message })
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : "Test failed."
      setTest({
        status: "fail",
        message,
      })
      toast.error("Test email failed", { description: message })
    }
  }

  return (
    <form onSubmit={save} className="space-y-4">
      <div className="grid gap-4 sm:grid-cols-2">
        <div className="space-y-1.5">
          <Label htmlFor="smtp-host">SMTP host</Label>
          <Input
            id="smtp-host"
            value={form.host}
            onChange={field("host")}
            placeholder="smtp.example.com"
            required
          />
        </div>
        <div className="space-y-1.5">
          <Label htmlFor="smtp-port">Port</Label>
          <Input
            id="smtp-port"
            type="number"
            value={form.port}
            onChange={field("port")}
            placeholder="587"
            required
          />
        </div>
      </div>

      <div className="grid gap-4 sm:grid-cols-2">
        <div className="space-y-1.5">
          <Label htmlFor="smtp-username">Username</Label>
          <Input
            id="smtp-username"
            value={form.username}
            onChange={field("username")}
            placeholder="user@example.com"
            autoComplete="off"
          />
        </div>
        <div className="space-y-1.5">
          <Label htmlFor="smtp-password">
            Password
            {smtp.hasPassword && (
              <span className="ml-1.5 text-xs font-normal text-muted-foreground">
                (set — leave blank to keep)
              </span>
            )}
          </Label>
          <Input
            id="smtp-password"
            type="password"
            value={form.password}
            onChange={field("password")}
            placeholder={smtp.hasPassword ? "••••••••" : ""}
            autoComplete="new-password"
          />
        </div>
      </div>

      <div className="grid gap-4 sm:grid-cols-2">
        <div className="space-y-1.5">
          <Label htmlFor="smtp-from">From address</Label>
          <Input
            id="smtp-from"
            type="email"
            value={form.fromAddress}
            onChange={field("fromAddress")}
            placeholder="noreply@example.com"
            required
          />
        </div>
        <div className="space-y-1.5">
          <Label htmlFor="smtp-name">From name</Label>
          <Input
            id="smtp-name"
            value={form.fromName}
            onChange={field("fromName")}
            placeholder="ProBeacon"
          />
        </div>
      </div>

      <div className="flex items-center justify-between rounded-lg border px-4 py-3">
        <div>
          <p className="text-sm font-medium">Enable SSL / STARTTLS</p>
          <p className="text-xs text-muted-foreground">
            Recommended for port 587. Disable only for local dev servers.
          </p>
        </div>
        <button
          type="button"
          role="switch"
          aria-checked={form.enableSsl}
          onClick={() => setForm((f) => ({ ...f, enableSsl: !f.enableSsl }))}
          className={cn(
            "relative inline-flex h-6 w-11 shrink-0 cursor-pointer rounded-full border-2 border-transparent transition-colors focus-visible:ring-2 focus-visible:ring-ring focus-visible:outline-none",
            form.enableSsl ? "bg-primary" : "bg-input"
          )}
        >
          <span
            className={cn(
              "pointer-events-none inline-block size-5 rounded-full bg-background shadow-lg ring-0 transition-transform",
              form.enableSsl ? "translate-x-5" : "translate-x-0"
            )}
          />
        </button>
      </div>

      <div className="flex items-center gap-3 pt-1">
        <Button type="submit" disabled={saving}>
          {saving ? "Saving…" : "Save"}
        </Button>
        <Button
          type="button"
          variant="outline"
          disabled={test.status === "testing" || !smtp.isConfigured}
          onClick={runTest}
          title={!smtp.isConfigured ? "Save settings first" : undefined}
        >
          {test.status === "testing" ? "Sending…" : "Send test email"}
        </Button>
      </div>
    </form>
  )
}

// ── account-lockout panel ────────────────────────────────────────────────────

function LockoutPanel({ initial }: { initial: LockoutSettings }) {
  const [form, setForm] = useState(initial)
  const [saving, setSaving] = useState(false)

  const num =
    (key: "maxAttempts" | "baseMinutes" | "maxMinutes") =>
    (e: React.ChangeEvent<HTMLInputElement>) =>
      setForm((f) => ({ ...f, [key]: Number(e.target.value) }))

  const save = async (e: FormEvent) => {
    e.preventDefault()
    setSaving(true)
    try {
      const updated = await api.put<LockoutSettings>(
        "/api/settings/lockout",
        form
      )
      setForm(updated)
      toast.success("Lockout settings saved")
    } catch (err: unknown) {
      toast.error(
        err instanceof Error ? err.message : "Failed to save lockout settings."
      )
    } finally {
      setSaving(false)
    }
  }

  return (
    <form onSubmit={save} className="space-y-4">
      <div className="flex items-center justify-between rounded-lg border px-4 py-3">
        <div>
          <p className="text-sm font-medium">Lock accounts after failed sign-ins</p>
          <p className="text-xs text-muted-foreground">
            When on, repeated wrong passwords temporarily lock the account.
          </p>
        </div>
        <button
          type="button"
          role="switch"
          aria-checked={form.enabled}
          onClick={() => setForm((f) => ({ ...f, enabled: !f.enabled }))}
          className={cn(
            "relative inline-flex h-6 w-11 shrink-0 cursor-pointer rounded-full border-2 border-transparent transition-colors focus-visible:ring-2 focus-visible:ring-ring focus-visible:outline-none",
            form.enabled ? "bg-primary" : "bg-input"
          )}
        >
          <span
            className={cn(
              "pointer-events-none inline-block size-5 rounded-full bg-background shadow-lg ring-0 transition-transform",
              form.enabled ? "translate-x-5" : "translate-x-0"
            )}
          />
        </button>
      </div>

      <div className="grid gap-4 sm:grid-cols-3">
        <div className="space-y-1.5">
          <Label htmlFor="lockout-attempts">Max failed attempts</Label>
          <Input
            id="lockout-attempts"
            type="number"
            min={1}
            max={100}
            value={form.maxAttempts}
            onChange={num("maxAttempts")}
            disabled={!form.enabled}
            required
          />
        </div>
        <div className="space-y-1.5">
          <Label htmlFor="lockout-base">Base lockout (min)</Label>
          <Input
            id="lockout-base"
            type="number"
            min={1}
            max={1440}
            value={form.baseMinutes}
            onChange={num("baseMinutes")}
            disabled={!form.enabled}
            required
          />
        </div>
        <div className="space-y-1.5">
          <Label htmlFor="lockout-max">Max lockout (min)</Label>
          <Input
            id="lockout-max"
            type="number"
            min={1}
            max={1440}
            value={form.maxMinutes}
            onChange={num("maxMinutes")}
            disabled={!form.enabled}
            required
          />
        </div>
      </div>

      <p className="text-xs text-muted-foreground">
        After {form.maxAttempts} failed attempts the account locks for{" "}
        {form.baseMinutes} min, doubling on each further failure up to{" "}
        {form.maxMinutes} min. A successful sign-in clears it.
      </p>

      <Button type="submit" disabled={saving}>
        {saving ? "Saving…" : "Save"}
      </Button>
    </form>
  )
}

// ── page ─────────────────────────────────────────────────────────────────────

export default function AuthConfigPage() {
  const { smtp, lockout } = useLoaderData<typeof clientLoader>()
  const [selected, setSelected] = useState<MethodKey>("email")

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold">Authentication</h1>
        <p className="mt-1 text-sm text-muted-foreground">
          Choose and configure how users sign in to ProBeacon.
        </p>
      </div>

      {/* method selector */}
      <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
        {METHODS.map((m) => {
          const isSelected = selected === m.key
          const isSmtpOk = m.key === "email" && smtp.isConfigured

          return (
            <button
              key={m.key}
              type="button"
              disabled={!m.available}
              onClick={() => setSelected(m.key)}
              className={cn(
                "relative rounded-xl border p-4 text-left transition-all focus-visible:ring-2 focus-visible:ring-ring focus-visible:outline-none",
                m.available
                  ? isSelected
                    ? "border-primary bg-primary/5 shadow-sm"
                    : "border-border hover:border-primary/50 hover:bg-muted/40"
                  : "cursor-not-allowed opacity-40"
              )}
            >
              {/* pivot lock */}
              {m.pivot && (
                <span className="absolute top-3 right-3 flex items-center gap-1 text-[10px] font-medium text-muted-foreground">
                  <Lock className="size-3" />
                  Required
                </span>
              )}

              <div className="mb-3 flex size-9 items-center justify-center rounded-lg border bg-background">
                <KeyRound className="size-4 text-muted-foreground" />
              </div>

              <p className="text-sm font-medium">{m.label}</p>
              <p className="mt-0.5 text-xs text-muted-foreground">
                {m.description}
              </p>

              <div className="mt-3">
                <span
                  className={cn(
                    "rounded-full px-2 py-0.5 text-[11px] font-medium",
                    isSmtpOk
                      ? "bg-green-100 text-green-700 dark:bg-green-900/30 dark:text-green-400"
                      : "bg-muted text-muted-foreground"
                  )}
                >
                  {isSmtpOk ? "SMTP configured" : "Needs SMTP config"}
                </span>
              </div>
            </button>
          )
        })}
      </div>

      {/* config panel for selected method */}
      {selected === "email" && (
        <Card>
          <CardHeader>
            <CardTitle>SMTP settings</CardTitle>
            <CardDescription>
              ProBeacon uses SMTP to send verification and notification emails.
            </CardDescription>
          </CardHeader>
          <CardContent>
            <SmtpPanel initial={smtp} />
          </CardContent>
        </Card>
      )}

      <Card>
        <CardHeader>
          <CardTitle>Account lockout</CardTitle>
          <CardDescription>
            Throttle brute-force sign-in attempts per account. Changes apply
            immediately — no restart needed.
          </CardDescription>
        </CardHeader>
        <CardContent>
          <LockoutPanel initial={lockout} />
        </CardContent>
      </Card>
    </div>
  )
}

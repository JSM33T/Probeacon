import { type FormEvent, useState } from "react"
import { useLoaderData } from "react-router"
import { api } from "~/lib/api"
import { Button } from "~/components/ui/button"
import { Input } from "~/components/ui/input"
import { Label } from "~/components/ui/label"
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "~/components/ui/card"

interface Setting {
  key: string
  value: string
}

export async function clientLoader() {
  const settings = await api.get<Setting[]>("/api/settings")
  return { settings }
}

export default function SettingsPage() {
  const { settings: initial } = useLoaderData<typeof clientLoader>()
  const [settings, setSettings] = useState<Setting[]>(initial)
  const [form, setForm] = useState({ key: "", value: "" })
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [success, setSuccess] = useState(false)

  const submit = async (e: FormEvent) => {
    e.preventDefault()
    setError(null)
    setSuccess(false)
    setSaving(true)
    try {
      await api.put("/api/settings", form)
      setSettings((prev) => {
        const idx = prev.findIndex((s) => s.key === form.key)
        if (idx >= 0) {
          const updated = [...prev]
          updated[idx] = { ...form }
          return updated
        }
        return [...prev, { ...form }]
      })
      setForm({ key: "", value: "" })
      setSuccess(true)
    } catch {
      setError("Failed to save setting.")
    } finally {
      setSaving(false)
    }
  }

  return (
    <div>
      <div className="mb-8">
        <h1 className="text-xl font-semibold">Settings</h1>
        <p className="text-sm text-muted-foreground mt-1">Organization-level configuration</p>
      </div>

      <Card className="mb-6">
        <CardHeader>
          <CardTitle className="text-sm">Current settings</CardTitle>
        </CardHeader>
        <CardContent>
          {settings.length === 0 ? (
            <p className="text-sm text-muted-foreground">No settings yet.</p>
          ) : (
            <div className="divide-y">
              {settings.map((s) => (
                <div key={s.key} className="py-2 flex items-center justify-between">
                  <span className="text-sm font-mono">{s.key}</span>
                  <span className="text-sm text-muted-foreground">{s.value}</span>
                </div>
              ))}
            </div>
          )}
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle className="text-sm">Add / update setting</CardTitle>
          <CardDescription>Upserts by key</CardDescription>
        </CardHeader>
        <CardContent>
          <form onSubmit={submit} className="flex gap-3 items-end">
            <div className="flex-1 flex flex-col gap-1.5">
              <Label>Key</Label>
              <Input
                value={form.key}
                onChange={(e) => setForm((f) => ({ ...f, key: e.target.value }))}
                placeholder="smtp.host"
                required
              />
            </div>
            <div className="flex-1 flex flex-col gap-1.5">
              <Label>Value</Label>
              <Input
                value={form.value}
                onChange={(e) => setForm((f) => ({ ...f, value: e.target.value }))}
                placeholder="mail.acme.com"
                required
              />
            </div>
            <Button type="submit" disabled={saving}>
              {saving ? "Saving…" : "Save"}
            </Button>
          </form>
          {error && <p className="text-sm text-destructive mt-3">{error}</p>}
          {success && <p className="text-sm text-green-600 mt-3">Saved.</p>}
        </CardContent>
      </Card>
    </div>
  )
}

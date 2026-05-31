import { type FormEvent, useMemo, useRef, useState } from "react"
import {
  Download,
  Eye,
  EyeOff,
  Pencil,
  Plus,
  Save,
  Trash2,
  TriangleAlert,
  Upload,
  X,
} from "lucide-react"
import { useLoaderData } from "react-router"
import { toast } from "sonner"

import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogMedia,
  AlertDialogTitle,
} from "~/components/ui/alert-dialog"
import { Badge } from "~/components/ui/badge"
import { Button } from "~/components/ui/button"
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "~/components/ui/card"
import {
  Dialog,
  DialogClose,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "~/components/ui/dialog"
import { Input } from "~/components/ui/input"
import { Label } from "~/components/ui/label"
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "~/components/ui/table"
import { api } from "~/lib/api"
import { cn } from "~/lib/utils"

interface Setting {
  key: string
  value: string
}

interface ImportReport {
  created: number
  updated: number
  secretsPreserved: number
  deleted: number
  skipped: { key: string; reason: string }[]
}

type ImportMode = "merge" | "replace"

type FormErrors = Partial<Record<keyof Setting, string>>

const propertyKeyPattern = /^[a-z][a-z0-9]*(\.[a-z0-9][a-z0-9_-]*)*$/
const secretPattern = /(password|secret|token|credential|private|api[_-]?key)/i

export async function clientLoader() {
  const settings = await api.get<Setting[]>("/api/settings")
  return { settings }
}

function getCategory(key: string) {
  return key.includes(".") ? key.split(".")[0] : "general"
}

function getValueKind(setting: Setting) {
  if (secretPattern.test(setting.key)) return "Secret"
  if (/^(true|false)$/i.test(setting.value)) return "Boolean"
  if (/^-?\d+(\.\d+)?$/.test(setting.value)) return "Number"
  return "Text"
}

function isSecret(setting: Setting) {
  return getValueKind(setting) === "Secret"
}

function maskValue(value: string) {
  if (!value) return "Not set"
  return "************"
}

function validate(form: Setting): FormErrors {
  const errors: FormErrors = {}
  const key = form.key.trim()
  const value = form.value.trim()

  if (!key) {
    errors.key = "Property key is required."
  } else if (!propertyKeyPattern.test(key)) {
    errors.key = "Use lowercase dotted keys, for example smtp.host."
  }

  if (!value) {
    errors.value = "Value is required."
  } else if (value.length > 1000) {
    errors.value = "Value must be 1000 characters or fewer."
  }

  return errors
}

export default function SettingsPage() {
  const { settings: initial } = useLoaderData<typeof clientLoader>()
  const [settings, setSettings] = useState<Setting[]>(initial)
  const [form, setForm] = useState<Setting>({ key: "", value: "" })
  const [visibleSecrets, setVisibleSecrets] = useState<Set<string>>(new Set())
  const [propertyDialogOpen, setPropertyDialogOpen] = useState(false)
  const [saving, setSaving] = useState(false)
  const [deletingKey, setDeletingKey] = useState<string | null>(null)
  const [pendingDelete, setPendingDelete] = useState<Setting | null>(null)
  const [errors, setErrors] = useState<FormErrors>({})

  const [exportDialogOpen, setExportDialogOpen] = useState(false)
  const [exportSecrets, setExportSecrets] = useState(false)
  const [exporting, setExporting] = useState(false)

  const [importDialogOpen, setImportDialogOpen] = useState(false)
  const [importFile, setImportFile] = useState<File | null>(null)
  const [importMode, setImportMode] = useState<ImportMode>("merge")
  const [importing, setImporting] = useState(false)
  const fileInputRef = useRef<HTMLInputElement>(null)

  const sortedSettings = useMemo(
    () => [...settings].sort((a, b) => a.key.localeCompare(b.key)),
    [settings]
  )

  const editingExisting = settings.some((setting) => setting.key === form.key)
  const formIsSecret = secretPattern.test(form.key)

  const updateField =
    (field: keyof Setting) => (event: React.ChangeEvent<HTMLInputElement>) => {
      setForm((current) => ({ ...current, [field]: event.target.value }))
      setErrors((current) => ({ ...current, [field]: undefined }))
    }

  const edit = (setting: Setting) => {
    setForm({ ...setting })
    setErrors({})
    setPropertyDialogOpen(true)
  }

  const resetForm = () => {
    setForm({ key: "", value: "" })
    setErrors({})
  }

  const addProperty = () => {
    resetForm()
    setPropertyDialogOpen(true)
  }

  const confirmDelete = async () => {
    if (!pendingDelete) return
    const setting = pendingDelete

    setDeletingKey(setting.key)
    try {
      await api.delete(`/api/settings/${encodeURIComponent(setting.key)}`)
      setSettings((prev) => prev.filter((s) => s.key !== setting.key))
      if (form.key === setting.key) resetForm()
      toast.success("Property deleted", {
        description: `${setting.key} has been removed.`,
      })
      setPendingDelete(null)
    } catch {
      toast.error("Failed to delete property")
    } finally {
      setDeletingKey(null)
    }
  }

  const runExport = async () => {
    setExporting(true)
    try {
      await api.download(
        `/api/settings/export?includeSecrets=${exportSecrets}`,
        "probeacon-settings.yaml"
      )
      setExportDialogOpen(false)
      toast.success("Settings exported", {
        description: exportSecrets
          ? "Secret values were included — store the file securely."
          : "Secret values were redacted.",
      })
      setExportSecrets(false)
    } catch {
      toast.error("Export failed")
    } finally {
      setExporting(false)
    }
  }

  const runImport = async () => {
    if (!importFile) return
    setImporting(true)
    try {
      const content = await importFile.text()
      const report = await api.post<ImportReport>("/api/settings/import", {
        content,
        replace: importMode === "replace",
      })

      const fresh = await api.get<Setting[]>("/api/settings")
      setSettings(fresh)

      const parts = [`${report.created} added`, `${report.updated} updated`]
      if (report.secretsPreserved)
        parts.push(`${report.secretsPreserved} secret${report.secretsPreserved === 1 ? "" : "s"} kept`)
      if (report.deleted) parts.push(`${report.deleted} removed`)
      if (report.skipped.length) parts.push(`${report.skipped.length} skipped`)

      toast.success("Settings imported", { description: parts.join(" · ") })
      report.skipped.forEach((s) =>
        toast.warning(`Skipped ${s.key}`, { description: s.reason })
      )

      setImportDialogOpen(false)
      setImportFile(null)
      setImportMode("merge")
      if (fileInputRef.current) fileInputRef.current.value = ""
    } catch (err: unknown) {
      toast.error("Import failed", {
        description: err instanceof Error ? err.message : undefined,
      })
    } finally {
      setImporting(false)
    }
  }

  const toggleSecret = (key: string) => {
    setVisibleSecrets((current) => {
      const next = new Set(current)
      if (next.has(key)) next.delete(key)
      else next.add(key)
      return next
    })
  }

  const submit = async (e: FormEvent) => {
    e.preventDefault()

    const trimmed = {
      key: form.key.trim(),
      value: form.value.trim(),
    }
    const nextErrors = validate(trimmed)

    setErrors(nextErrors)

    if (Object.keys(nextErrors).length > 0) return

    setSaving(true)
    try {
      await api.put("/api/settings", trimmed)
      setSettings((prev) => {
        const idx = prev.findIndex((s) => s.key === trimmed.key)
        if (idx >= 0) {
          const updated = [...prev]
          updated[idx] = { ...trimmed }
          return updated
        }
        return [...prev, { ...trimmed }]
      })
      setForm({ key: "", value: "" })
      setPropertyDialogOpen(false)
      toast.success("Property saved", {
        description: `${trimmed.key} has been updated.`,
      })
    } catch {
      toast.error("Failed to save property")
    } finally {
      setSaving(false)
    }
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-xl font-semibold">Settings</h1>
        <p className="mt-1 text-sm text-muted-foreground">
          Manage organization-level configuration properties used by ProBeacon.
        </p>
      </div>

      <Card>
        <CardHeader className="gap-2">
          <div className="flex flex-col gap-2 sm:flex-row sm:items-start sm:justify-between">
            <div>
              <CardTitle className="text-sm">
                Configuration properties
              </CardTitle>
              <CardDescription>
                Key/value settings grouped by namespace. Secret-looking values
                are masked by default.
              </CardDescription>
            </div>
            <div className="flex flex-wrap items-center gap-2">
              <Badge variant="outline">{settings.length} total</Badge>
              <Button
                type="button"
                size="sm"
                variant="outline"
                onClick={() => setImportDialogOpen(true)}
              >
                <Upload />
                Import
              </Button>
              <Button
                type="button"
                size="sm"
                variant="outline"
                disabled={settings.length === 0}
                onClick={() => setExportDialogOpen(true)}
              >
                <Download />
                Export
              </Button>
              <Button type="button" size="sm" onClick={addProperty}>
                <Plus />
                Add property
              </Button>
            </div>
          </div>
        </CardHeader>
        <CardContent>
          {sortedSettings.length === 0 ? (
            <div className="rounded-md border border-dashed px-4 py-10 text-center">
              <p className="text-sm font-medium">No properties yet</p>
              <p className="mt-1 text-sm text-muted-foreground">
                Add your first property to configure this workspace.
              </p>
              <Button type="button" className="mt-4" onClick={addProperty}>
                <Plus />
                Add property
              </Button>
            </div>
          ) : (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Property</TableHead>
                  <TableHead>Value</TableHead>
                  <TableHead>Type</TableHead>
                  <TableHead className="w-24 text-right">Actions</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {sortedSettings.map((setting) => {
                  const secret = isSecret(setting)
                  const revealed = visibleSecrets.has(setting.key)
                  const isGeneral = getCategory(setting.key) === "general"

                  return (
                    <TableRow key={setting.key}>
                      <TableCell>
                        <div className="flex min-w-56 flex-col gap-1">
                          <span className="font-mono text-sm">
                            {setting.key}
                          </span>
                          <span className="text-xs text-muted-foreground">
                            {getCategory(setting.key)}
                          </span>
                        </div>
                      </TableCell>
                      <TableCell className="max-w-[28rem]">
                        <div className="flex items-center gap-2">
                          <span className="min-w-0 truncate text-muted-foreground">
                            {secret && !revealed
                              ? maskValue(setting.value)
                              : setting.value}
                          </span>
                          {secret && (
                            <Button
                              type="button"
                              variant="ghost"
                              size="icon-xs"
                              aria-label={
                                revealed
                                  ? "Hide secret value"
                                  : "Show secret value"
                              }
                              onClick={() => toggleSecret(setting.key)}
                            >
                              {revealed ? <EyeOff /> : <Eye />}
                            </Button>
                          )}
                        </div>
                      </TableCell>
                      <TableCell>
                        <Badge variant={secret ? "destructive" : "secondary"}>
                          {getValueKind(setting)}
                        </Badge>
                      </TableCell>
                      <TableCell className="text-right">
                        <div className="flex items-center justify-end gap-1">
                          <Button
                            type="button"
                            variant="ghost"
                            size="icon-sm"
                            aria-label={`Edit ${setting.key}`}
                            onClick={() => edit(setting)}
                          >
                            <Pencil />
                          </Button>
                          {isGeneral && (
                            <Button
                              type="button"
                              variant="ghost"
                              size="icon-sm"
                              className="text-muted-foreground hover:text-destructive"
                              aria-label={`Delete ${setting.key}`}
                              disabled={deletingKey === setting.key}
                              onClick={() => setPendingDelete(setting)}
                            >
                              <Trash2 />
                            </Button>
                          )}
                        </div>
                      </TableCell>
                    </TableRow>
                  )
                })}
              </TableBody>
            </Table>
          )}
        </CardContent>
      </Card>

      <Dialog open={propertyDialogOpen} onOpenChange={setPropertyDialogOpen}>
        <DialogContent className="sm:max-w-2xl">
          <DialogHeader>
            <div className="flex items-center gap-2">
              <DialogTitle>
                {editingExisting ? "Update property" : "Add property"}
              </DialogTitle>
              {editingExisting && <Badge variant="secondary">Editing</Badge>}
            </div>
            <DialogDescription>
              Use dotted keys such as smtp.host or auth.session_timeout.
            </DialogDescription>
          </DialogHeader>

          <form onSubmit={submit} className="space-y-4">
            <div className="grid gap-4 sm:grid-cols-2">
              <div className="flex flex-col gap-1.5">
                <Label htmlFor="property-key">Key</Label>
                <Input
                  id="property-key"
                  value={form.key}
                  onChange={updateField("key")}
                  placeholder="smtp.host"
                  aria-invalid={Boolean(errors.key)}
                />
                {errors.key ? (
                  <p className="text-xs text-destructive">{errors.key}</p>
                ) : (
                  <p className="text-xs text-muted-foreground">
                    Lowercase dotted namespace, no spaces.
                  </p>
                )}
              </div>

              <div className="flex flex-col gap-1.5 sm:col-span-2">
                <Label htmlFor="property-value">Value</Label>
                <Input
                  id="property-value"
                  type={formIsSecret ? "password" : "text"}
                  value={form.value}
                  onChange={updateField("value")}
                  placeholder={
                    formIsSecret ? "Enter secret value" : "mail.acme.com"
                  }
                  aria-invalid={Boolean(errors.value)}
                />
                {errors.value ? (
                  <p className="text-xs text-destructive">{errors.value}</p>
                ) : (
                  <p className="text-xs text-muted-foreground">
                    Secret keys are masked in the table.
                  </p>
                )}
              </div>
            </div>

            <DialogFooter>
              <DialogClose asChild>
                <Button type="button" variant="outline">
                  Cancel
                </Button>
              </DialogClose>
              {(form.key || form.value) && (
                <Button type="button" variant="outline" onClick={resetForm}>
                  <X />
                  Clear
                </Button>
              )}
              <Button type="submit" disabled={saving}>
                <Save />
                {saving ? "Saving..." : editingExisting ? "Update" : "Save"}
              </Button>
            </DialogFooter>
          </form>
        </DialogContent>
      </Dialog>

      <AlertDialog
        open={pendingDelete !== null}
        onOpenChange={(open) => {
          if (!open) setPendingDelete(null)
        }}
      >
        <AlertDialogContent className="rounded-lg shadow-2xl">
          <AlertDialogHeader>
            <AlertDialogMedia className="bg-destructive/10 text-destructive">
              <Trash2 className="size-5" />
            </AlertDialogMedia>
            <AlertDialogTitle>Delete this property?</AlertDialogTitle>
            <AlertDialogDescription>
              <span className="font-mono">{pendingDelete?.key}</span> will be
              permanently removed. This cannot be undone.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Cancel</AlertDialogCancel>
            <AlertDialogAction
              variant="destructive"
              disabled={deletingKey !== null}
              onClick={(event) => {
                event.preventDefault()
                void confirmDelete()
              }}
            >
              Delete
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>

      <Dialog
        open={exportDialogOpen}
        onOpenChange={(open) => {
          setExportDialogOpen(open)
          if (!open) setExportSecrets(false)
        }}
      >
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Export settings</DialogTitle>
            <DialogDescription>
              Downloads all properties as a flat-key YAML file you can version,
              back up, or import into another workspace.
            </DialogDescription>
          </DialogHeader>

          <div className="space-y-4">
            <div className="flex items-start justify-between gap-4 rounded-lg border px-4 py-3">
              <div>
                <p className="text-sm font-medium">Include secret values</p>
                <p className="text-xs text-muted-foreground">
                  When off, secrets (passwords, tokens, keys) are redacted and
                  re-importing keeps the existing stored values.
                </p>
              </div>
              <button
                type="button"
                role="switch"
                aria-checked={exportSecrets}
                onClick={() => setExportSecrets((v) => !v)}
                className={cn(
                  "relative inline-flex h-6 w-11 shrink-0 cursor-pointer rounded-full border-2 border-transparent transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring",
                  exportSecrets ? "bg-destructive" : "bg-input"
                )}
              >
                <span
                  className={cn(
                    "pointer-events-none inline-block size-5 rounded-full bg-background shadow-lg ring-0 transition-transform",
                    exportSecrets ? "translate-x-5" : "translate-x-0"
                  )}
                />
              </button>
            </div>

            {exportSecrets && (
              <div className="flex items-start gap-2 rounded-md bg-destructive/10 px-3 py-2 text-sm text-destructive">
                <TriangleAlert className="mt-0.5 size-4 shrink-0" />
                <span>
                  The file will contain secrets in plaintext. Store and transfer
                  it securely, and delete it when no longer needed.
                </span>
              </div>
            )}
          </div>

          <DialogFooter>
            <DialogClose asChild>
              <Button type="button" variant="outline">
                Cancel
              </Button>
            </DialogClose>
            <Button
              type="button"
              variant={exportSecrets ? "destructive" : "default"}
              disabled={exporting}
              onClick={runExport}
            >
              <Download />
              {exporting ? "Exporting..." : "Download YAML"}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      <Dialog
        open={importDialogOpen}
        onOpenChange={(open) => {
          setImportDialogOpen(open)
          if (!open) {
            setImportFile(null)
            setImportMode("merge")
            if (fileInputRef.current) fileInputRef.current.value = ""
          }
        }}
      >
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Import settings</DialogTitle>
            <DialogDescription>
              Upload a YAML export. Each property is validated before it is
              saved; invalid entries are skipped and reported.
            </DialogDescription>
          </DialogHeader>

          <div className="space-y-4">
            <div className="space-y-1.5">
              <Label htmlFor="import-file">YAML file</Label>
              <Input
                id="import-file"
                ref={fileInputRef}
                type="file"
                accept=".yaml,.yml,text/yaml,application/x-yaml"
                onChange={(e) => setImportFile(e.target.files?.[0] ?? null)}
              />
            </div>

            <div className="space-y-1.5">
              <Label>Mode</Label>
              <div className="grid grid-cols-2 gap-2">
                {(["merge", "replace"] as ImportMode[]).map((mode) => (
                  <button
                    key={mode}
                    type="button"
                    onClick={() => setImportMode(mode)}
                    className={cn(
                      "rounded-lg border px-3 py-2 text-left transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring",
                      importMode === mode
                        ? "border-primary bg-primary/5"
                        : "border-border hover:bg-muted/40"
                    )}
                  >
                    <p className="text-sm font-medium capitalize">{mode}</p>
                    <p className="text-xs text-muted-foreground">
                      {mode === "merge"
                        ? "Add and update keys from the file."
                        : "Also delete keys not in the file."}
                    </p>
                  </button>
                ))}
              </div>
            </div>

            {importMode === "replace" && (
              <div className="flex items-start gap-2 rounded-md bg-destructive/10 px-3 py-2 text-sm text-destructive">
                <TriangleAlert className="mt-0.5 size-4 shrink-0" />
                <span>
                  Replace mode deletes any property not present in the file.
                  Redacted secrets in the file are preserved.
                </span>
              </div>
            )}
          </div>

          <DialogFooter>
            <DialogClose asChild>
              <Button type="button" variant="outline">
                Cancel
              </Button>
            </DialogClose>
            <Button
              type="button"
              variant={importMode === "replace" ? "destructive" : "default"}
              disabled={importing || !importFile}
              onClick={runImport}
            >
              <Upload />
              {importing ? "Importing..." : "Import"}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  )
}

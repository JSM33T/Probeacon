import { type FormEvent, useState } from "react"
import { useLoaderData, useRevalidator } from "react-router"
import {
  Copy,
  KeyRound,
  Plus,
  RotateCcw,
  ShieldCheck,
  UserMinus,
  Users,
} from "lucide-react"
import { toast } from "sonner"
import { api } from "~/lib/api"
import { getUser } from "~/lib/auth"
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

interface TeamUser {
  id: string
  email: string
  displayName: string
  role: "Admin" | "User"
  isActive: boolean
  isEmailVerified: boolean
  createdAt: string
}

interface CreateUserResult {
  user: TeamUser
  temporaryPassword: string
}

interface PasswordReveal {
  email: string
  temporaryPassword: string
}

export async function clientLoader() {
  const users = await api.get<TeamUser[]>("/api/users")
  return { users, currentUser: getUser() }
}

export function HydrateFallback() {
  return (
    <div className="flex min-h-[200px] items-center justify-center">
      <span className="text-sm text-muted-foreground">Loading...</span>
    </div>
  )
}

export default function TeamPage() {
  const { users, currentUser } = useLoaderData<typeof clientLoader>()
  const { revalidate } = useRevalidator()
  const [createOpen, setCreateOpen] = useState(false)
  const [created, setCreated] = useState<CreateUserResult | null>(null)
  const [passwordReveal, setPasswordReveal] = useState<PasswordReveal | null>(null)
  const [form, setForm] = useState({
    displayName: "",
    email: "",
    role: "User" as "Admin" | "User",
  })
  const [saving, setSaving] = useState(false)
  const [busyUserId, setBusyUserId] = useState<string | null>(null)

  const createUser = async (e: FormEvent) => {
    e.preventDefault()
    setSaving(true)
    try {
      const result = await api.post<CreateUserResult>("/api/users", form)
      setCreated(result)
      setForm({ displayName: "", email: "", role: "User" })
      toast.success("User created")
      await revalidate()
    } catch (err) {
      toast.error(err instanceof Error ? err.message : "Failed to create user.")
    } finally {
      setSaving(false)
    }
  }

  const deactivate = async (user: TeamUser) => {
    setBusyUserId(user.id)
    try {
      await api.delete(`/api/users/${user.id}`)
      toast.success(`${user.displayName} deactivated`)
      await revalidate()
    } catch (err) {
      toast.error(
        err instanceof Error ? err.message : "Failed to deactivate user."
      )
    } finally {
      setBusyUserId(null)
    }
  }

  const promote = async (user: TeamUser) => {
    setBusyUserId(user.id)
    try {
      await api.post(`/api/users/${user.id}/promote`, {})
      toast.success(`${user.displayName} is now an admin`)
      await revalidate()
    } catch (err) {
      toast.error(err instanceof Error ? err.message : "Failed to promote user.")
    } finally {
      setBusyUserId(null)
    }
  }

  const resetPassword = async (user: TeamUser) => {
    setBusyUserId(user.id)
    try {
      const result = await api.post<{ temporaryPassword: string }>(
        `/api/users/${user.id}/reset-password`,
        {}
      )
      setPasswordReveal({
        email: user.email,
        temporaryPassword: result.temporaryPassword,
      })
      toast.success("Temporary password generated")
      await revalidate()
    } catch (err) {
      toast.error(
        err instanceof Error ? err.message : "Failed to reset password."
      )
    } finally {
      setBusyUserId(null)
    }
  }

  const reactivate = async (user: TeamUser) => {
    setBusyUserId(user.id)
    try {
      await api.post(`/api/users/${user.id}/reactivate`, {})
      toast.success(`${user.displayName} reactivated`)
      await revalidate()
    } catch (err) {
      toast.error(
        err instanceof Error ? err.message : "Failed to reactivate user."
      )
    } finally {
      setBusyUserId(null)
    }
  }

  const copyPassword = async () => {
    const password = created?.temporaryPassword ?? passwordReveal?.temporaryPassword
    if (!password) return
    await navigator.clipboard.writeText(password)
    toast.success("Temporary password copied")
  }

  return (
    <div className="space-y-6">
      <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h1 className="text-2xl font-semibold">Users</h1>
          <p className="mt-1 text-sm text-muted-foreground">
            Create accounts and manage access to this workspace.
          </p>
        </div>
        <Button onClick={() => setCreateOpen(true)}>
          <Plus className="size-4" />
          Add user
        </Button>
      </div>

      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2 text-base">
            <Users className="size-4" />
            Team members
          </CardTitle>
          <CardDescription>
            New users receive a temporary password that they can change from
            their profile after signing in.
          </CardDescription>
        </CardHeader>
        <CardContent>
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Name</TableHead>
                <TableHead>Role</TableHead>
                <TableHead>Status</TableHead>
                <TableHead>Created</TableHead>
                <TableHead className="w-0 text-right">Actions</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {users.map((user) => {
                const isCurrentUser = user.id === currentUser?.userId
                return (
                  <TableRow key={user.id}>
                    <TableCell>
                      <div>
                        <p className="font-medium">{user.displayName}</p>
                        <p className="text-xs text-muted-foreground">
                          {user.email}
                        </p>
                      </div>
                    </TableCell>
                    <TableCell>
                      <Badge variant={user.role === "Admin" ? "default" : "secondary"}>
                        {user.role}
                      </Badge>
                    </TableCell>
                    <TableCell>
                      <Badge variant={user.isActive ? "secondary" : "outline"}>
                        {user.isActive ? "Active" : "Inactive"}
                      </Badge>
                    </TableCell>
                    <TableCell>
                      {new Date(user.createdAt).toLocaleDateString()}
                    </TableCell>
                    <TableCell>
                      <div className="flex justify-end gap-2">
                        {user.role !== "Admin" && user.isActive && (
                          <Button
                            variant="outline"
                            size="sm"
                            disabled={busyUserId === user.id}
                            onClick={() => promote(user)}
                          >
                            <ShieldCheck className="size-4" />
                            Admin
                          </Button>
                        )}
                        {user.isActive && (
                          <Button
                            variant="outline"
                            size="sm"
                            disabled={busyUserId === user.id}
                            onClick={() => resetPassword(user)}
                          >
                            <KeyRound className="size-4" />
                            Reset
                          </Button>
                        )}
                        {user.isActive && !isCurrentUser && (
                          <Button
                            variant="outline"
                            size="sm"
                            disabled={busyUserId === user.id}
                            onClick={() => deactivate(user)}
                          >
                            <UserMinus className="size-4" />
                            Deactivate
                          </Button>
                        )}
                        {!user.isActive && (
                          <Button
                            variant="outline"
                            size="sm"
                            disabled={busyUserId === user.id}
                            onClick={() => reactivate(user)}
                          >
                            <RotateCcw className="size-4" />
                            Reactivate
                          </Button>
                        )}
                      </div>
                    </TableCell>
                  </TableRow>
                )
              })}
            </TableBody>
          </Table>
        </CardContent>
      </Card>

      <Dialog
        open={createOpen}
        onOpenChange={(open) => {
          setCreateOpen(open)
          if (!open) setCreated(null)
        }}
      >
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Add user</DialogTitle>
            <DialogDescription>
              Create an account and share the temporary password securely.
            </DialogDescription>
          </DialogHeader>

          {created ? (
            <div className="space-y-4">
              <div className="rounded-lg border bg-muted/40 p-4">
                <p className="text-sm font-medium">{created.user.email}</p>
                <p className="mt-1 text-xs text-muted-foreground">
                  Temporary password
                </p>
                <div className="mt-2 flex items-center gap-2">
                  <code className="min-w-0 flex-1 rounded-md bg-background px-3 py-2 text-sm">
                    {created.temporaryPassword}
                  </code>
                  <Button size="icon" variant="outline" onClick={copyPassword}>
                    <Copy className="size-4" />
                  </Button>
                </div>
              </div>
              <DialogFooter>
                <Button onClick={() => setCreateOpen(false)}>Done</Button>
              </DialogFooter>
            </div>
          ) : (
            <form onSubmit={createUser} className="space-y-4">
              <div className="space-y-1.5">
                <Label htmlFor="displayName">Display name</Label>
                <Input
                  id="displayName"
                  value={form.displayName}
                  onChange={(e) =>
                    setForm((f) => ({ ...f, displayName: e.target.value }))
                  }
                  required
                />
              </div>
              <div className="space-y-1.5">
                <Label htmlFor="email">Email</Label>
                <Input
                  id="email"
                  type="email"
                  value={form.email}
                  onChange={(e) =>
                    setForm((f) => ({ ...f, email: e.target.value }))
                  }
                  required
                />
              </div>
              <div className="space-y-1.5">
                <Label htmlFor="role">Role</Label>
                <select
                  id="role"
                  value={form.role}
                  onChange={(e) =>
                    setForm((f) => ({
                      ...f,
                      role: e.target.value as "Admin" | "User",
                    }))
                  }
                  className="h-9 w-full rounded-md border bg-background px-3 text-sm"
                >
                  <option value="User">User</option>
                  <option value="Admin">Admin</option>
                </select>
              </div>
              <DialogFooter>
                <Button type="submit" disabled={saving}>
                  {saving ? "Creating..." : "Create user"}
                </Button>
              </DialogFooter>
            </form>
          )}
        </DialogContent>
      </Dialog>

      <Dialog
        open={Boolean(passwordReveal)}
        onOpenChange={(open) => {
          if (!open) setPasswordReveal(null)
        }}
      >
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Temporary password</DialogTitle>
            <DialogDescription>
              Copy this password now. It will not be shown again.
            </DialogDescription>
          </DialogHeader>
          {passwordReveal && (
            <div className="space-y-4">
              <div className="rounded-lg border bg-muted/40 p-4">
                <p className="text-sm font-medium">{passwordReveal.email}</p>
                <p className="mt-1 text-xs text-muted-foreground">
                  Temporary password
                </p>
                <div className="mt-2 flex items-center gap-2">
                  <code className="min-w-0 flex-1 rounded-md bg-background px-3 py-2 text-sm">
                    {passwordReveal.temporaryPassword}
                  </code>
                  <Button size="icon" variant="outline" onClick={copyPassword}>
                    <Copy className="size-4" />
                  </Button>
                </div>
              </div>
              <DialogFooter>
                <Button onClick={() => setPasswordReveal(null)}>Done</Button>
              </DialogFooter>
            </div>
          )}
        </DialogContent>
      </Dialog>
    </div>
  )
}

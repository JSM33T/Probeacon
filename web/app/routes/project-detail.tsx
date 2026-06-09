import { type FormEvent, useMemo, useState } from "react"
import { Link, useLoaderData, useRevalidator } from "react-router"
import { ArrowLeft, Plus, Save, Trash2 } from "lucide-react"
import { toast } from "sonner"
import { api } from "~/lib/api"
import { getUser } from "~/lib/auth"
import { Badge } from "~/components/ui/badge"
import { Button } from "~/components/ui/button"
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
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "~/components/ui/card"
import { Input } from "~/components/ui/input"
import { Label } from "~/components/ui/label"
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "~/components/ui/select"
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "~/components/ui/dialog"
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "~/components/ui/table"

type ProjectAccessRole = "Full access" | "Manager" | "Editor" | "Viewer"
type AssignableRole = "Viewer" | "Editor" | "Manager"

interface Project {
  id: string
  name: string
  description: string | null
  createdAt: string
  createdByUserId: string
  accessRole: ProjectAccessRole
  memberCount: number
}

interface ProjectMember {
  userId: string
  email: string
  displayName: string
  isActive: boolean
  role: AssignableRole
  assignedAt: string
  assignedByUserId: string
}

interface AssignableUser {
  id: string
  email: string
  displayName: string
  isActive: boolean
}

export async function clientLoader({ params }: { params: { projectId: string } }) {
  const user = getUser()
  const project = await api.get<Project>(`/api/projects/${params.projectId}`)

  // Managers (and global admins) can manage members; everyone else just views.
  const canManage = user?.role === "Admin" || project.accessRole === "Manager"
  if (!canManage) return { project, members: [], users: [], user }

  const [members, users] = await Promise.all([
    api.get<ProjectMember[]>(`/api/projects/${params.projectId}/members`),
    api.get<AssignableUser[]>(`/api/projects/${params.projectId}/assignable-users`),
  ])

  return { project, members, users, user }
}

export default function ProjectDetailPage() {
  const { project, members, users, user } = useLoaderData<typeof clientLoader>()
  const { revalidate } = useRevalidator()
  const isAdmin = user?.role === "Admin"
  // Editors & Managers (and admins) can edit name/description; only Managers/admins manage members.
  const canEdit =
    isAdmin || project.accessRole === "Editor" || project.accessRole === "Manager"
  const canManage = isAdmin || project.accessRole === "Manager"
  const [form, setForm] = useState({
    name: project.name,
    description: project.description ?? "",
  })
  const [savingProject, setSavingProject] = useState(false)
  const [assignment, setAssignment] = useState({
    userId: "",
    role: "Viewer" as AssignableRole,
  })
  const [assignOpen, setAssignOpen] = useState(false)
  const [removeMemberTarget, setRemoveMemberTarget] =
    useState<ProjectMember | null>(null)
  const [savingMember, setSavingMember] = useState(false)
  const [busyUserId, setBusyUserId] = useState<string | null>(null)

  const assignableUsers = useMemo(() => {
    const memberIds = new Set(members.map((member) => member.userId))
    return users.filter((teamUser) => teamUser.isActive && !memberIds.has(teamUser.id))
  }, [members, users])

  const saveProject = async (e: FormEvent) => {
    e.preventDefault()
    setSavingProject(true)
    try {
      await api.patch<Project>(`/api/projects/${project.id}`, {
        name: form.name,
        description: form.description || null,
      })
      toast.success("Project updated")
      await revalidate()
    } catch (err) {
      toast.error(err instanceof Error ? err.message : "Failed to update project.")
    } finally {
      setSavingProject(false)
    }
  }

  const assignMember = async (e: FormEvent) => {
    e.preventDefault()
    if (!assignment.userId) return

    setSavingMember(true)
    try {
      await api.put(`/api/projects/${project.id}/members/${assignment.userId}`, {
        role: assignment.role,
      })
      setAssignment({ userId: "", role: "Viewer" })
      setAssignOpen(false)
      toast.success("Project access updated")
      await revalidate()
    } catch (err) {
      toast.error(err instanceof Error ? err.message : "Failed to assign user.")
    } finally {
      setSavingMember(false)
    }
  }

  const updateMemberRole = async (member: ProjectMember, role: AssignableRole) => {
    setBusyUserId(member.userId)
    try {
      await api.put(`/api/projects/${project.id}/members/${member.userId}`, { role })
      toast.success("Project access updated")
      await revalidate()
    } catch (err) {
      toast.error(err instanceof Error ? err.message : "Failed to update access.")
    } finally {
      setBusyUserId(null)
      setRemoveMemberTarget(null)
    }
  }

  const removeMember = async (member: ProjectMember) => {
    setBusyUserId(member.userId)
    try {
      await api.delete(`/api/projects/${project.id}/members/${member.userId}`)
      toast.success(`${member.displayName} removed`)
      await revalidate()
    } catch (err) {
      toast.error(err instanceof Error ? err.message : "Failed to remove user.")
    } finally {
      setBusyUserId(null)
    }
  }

  return (
    <div className="space-y-6">
      <div>
        <Button asChild variant="ghost" size="sm" className="-ml-2 mb-2">
          <Link to="/projects">
            <ArrowLeft className="size-4" />
            Projects
          </Link>
        </Button>
        <div className="flex flex-col gap-3 sm:flex-row sm:items-start sm:justify-between">
          <div>
            <h1 className="text-2xl font-semibold">{project.name}</h1>
            <p className="mt-1 text-sm text-muted-foreground">
              {project.description || "No description"}
            </p>
          </div>
          <Badge
            variant={
              project.accessRole === "Full access" || project.accessRole === "Manager"
                ? "default"
                : "secondary"
            }
          >
            {project.accessRole}
          </Badge>
        </div>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Project details</CardTitle>
          <CardDescription>
            Created {new Date(project.createdAt).toLocaleDateString()}.
          </CardDescription>
        </CardHeader>
        <CardContent>
          {canEdit ? (
            <form onSubmit={saveProject} className="space-y-4">
              <div className="space-y-1.5">
                <Label htmlFor="name">Name</Label>
                <Input
                  id="name"
                  value={form.name}
                  onChange={(e) => setForm((f) => ({ ...f, name: e.target.value }))}
                  required
                />
              </div>
              <div className="space-y-1.5">
                <Label htmlFor="description">Description</Label>
                <Input
                  id="description"
                  value={form.description}
                  onChange={(e) =>
                    setForm((f) => ({ ...f, description: e.target.value }))
                  }
                />
              </div>
              <Button type="submit" disabled={savingProject}>
                <Save className="size-4" />
                {savingProject ? "Saving..." : "Save changes"}
              </Button>
            </form>
          ) : (
            <div className="grid gap-4 sm:grid-cols-3">
              <Info label="Access" value={project.accessRole} />
              <Info label="Members" value={String(project.memberCount)} />
              <Info
                label="Created"
                value={new Date(project.createdAt).toLocaleDateString()}
              />
            </div>
          )}
        </CardContent>
      </Card>

      {canManage && (
        <Card>
          <CardHeader>
            <div className="flex flex-col gap-3 sm:flex-row sm:items-start sm:justify-between">
              <div>
                <CardTitle>Members</CardTitle>
                <CardDescription>
                  Assign users as viewers, editors, or managers for this project.
                </CardDescription>
              </div>
              <Button onClick={() => setAssignOpen(true)}>
                <Plus className="size-4" />
                Add member
              </Button>
            </div>
          </CardHeader>
          <CardContent>
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>User</TableHead>
                  <TableHead>Role</TableHead>
                  <TableHead>Status</TableHead>
                  <TableHead>Assigned</TableHead>
                  <TableHead className="w-0 text-right">Actions</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {members.map((member) => (
                  <TableRow key={member.userId}>
                    <TableCell>
                      <p className="font-medium">{member.displayName}</p>
                      <p className="text-xs text-muted-foreground">{member.email}</p>
                    </TableCell>
                    <TableCell>
                      <Select
                        value={member.role}
                        disabled={busyUserId === member.userId || !member.isActive}
                        onValueChange={(value) =>
                          updateMemberRole(member, value as AssignableRole)
                        }
                      >
                        <SelectTrigger className="h-8 w-[7.5rem]">
                          <SelectValue />
                        </SelectTrigger>
                        <SelectContent>
                          <SelectItem value="Viewer">Viewer</SelectItem>
                          <SelectItem value="Editor">Editor</SelectItem>
                          <SelectItem value="Manager">Manager</SelectItem>
                        </SelectContent>
                      </Select>
                    </TableCell>
                    <TableCell>
                      <Badge variant={member.isActive ? "secondary" : "outline"}>
                        {member.isActive ? "Active" : "Inactive"}
                      </Badge>
                    </TableCell>
                    <TableCell>
                      {new Date(member.assignedAt).toLocaleDateString()}
                    </TableCell>
                    <TableCell>
                      <div className="flex justify-end">
                        <Button
                          variant="outline"
                          size="sm"
                          disabled={busyUserId === member.userId}
                          onClick={() => setRemoveMemberTarget(member)}
                        >
                          <Trash2 className="size-4" />
                          Remove
                        </Button>
                      </div>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </CardContent>
        </Card>
      )}

      <Dialog open={assignOpen} onOpenChange={setAssignOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Add member</DialogTitle>
            <DialogDescription>
              Choose an active user and set project access.
            </DialogDescription>
          </DialogHeader>
          <form onSubmit={assignMember} className="space-y-4">
            <div className="space-y-1.5">
              <Label htmlFor="member">User</Label>
              <Select
                value={assignment.userId}
                onValueChange={(value) =>
                  setAssignment((f) => ({ ...f, userId: value }))
                }
                required
              >
                <SelectTrigger id="member">
                  <SelectValue placeholder="Select user" />
                </SelectTrigger>
                <SelectContent>
                  {assignableUsers.map((teamUser) => (
                    <SelectItem key={teamUser.id} value={teamUser.id}>
                      {teamUser.displayName} ({teamUser.email})
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
            <div className="space-y-1.5">
              <Label htmlFor="role">Role</Label>
              <Select
                value={assignment.role}
                onValueChange={(value) =>
                  setAssignment((f) => ({
                    ...f,
                    role: value as AssignableRole,
                  }))
                }
              >
                <SelectTrigger id="role">
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="Viewer">Viewer</SelectItem>
                  <SelectItem value="Editor">Editor</SelectItem>
                  <SelectItem value="Manager">Manager</SelectItem>
                </SelectContent>
              </Select>
            </div>
            <DialogFooter>
              <Button type="submit" disabled={savingMember || !assignment.userId}>
                {savingMember ? "Adding..." : "Add member"}
              </Button>
            </DialogFooter>
          </form>
        </DialogContent>
      </Dialog>

      <AlertDialog
        open={Boolean(removeMemberTarget)}
        onOpenChange={(open) => {
          if (!open) setRemoveMemberTarget(null)
        }}
      >
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogMedia className="bg-destructive/10 text-destructive">
              <Trash2 className="size-5" />
            </AlertDialogMedia>
            <AlertDialogTitle>Remove project member?</AlertDialogTitle>
            <AlertDialogDescription>
              {removeMemberTarget
                ? `${removeMemberTarget.displayName} will lose access to ${project.name}.`
                : "This user will lose project access."}
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Cancel</AlertDialogCancel>
            <AlertDialogAction
              variant="destructive"
              disabled={
                Boolean(removeMemberTarget) &&
                busyUserId === removeMemberTarget?.userId
              }
              onClick={() => {
                if (removeMemberTarget) void removeMember(removeMemberTarget)
              }}
            >
              Remove
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  )
}

function Info({ label, value }: { label: string; value: string }) {
  return (
    <div className="rounded-lg border p-4">
      <p className="text-xs text-muted-foreground">{label}</p>
      <p className="mt-1 text-sm font-medium">{value}</p>
    </div>
  )
}

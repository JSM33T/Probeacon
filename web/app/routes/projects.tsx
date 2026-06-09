import { type FormEvent, useState } from "react"
import { Link, useLoaderData, useRevalidator } from "react-router"
import { FolderKanban, Pencil, Plus, Trash2, Users } from "lucide-react"
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

interface Project {
  id: string
  name: string
  description: string | null
  createdAt: string
  createdByUserId: string
  accessRole: "Full access" | "Manager" | "Editor" | "Viewer"
  memberCount: number
}

export async function clientLoader() {
  const projects = await api.get<Project[]>("/api/projects")
  return { projects, user: getUser() }
}

export default function ProjectsPage() {
  const { projects, user } = useLoaderData<typeof clientLoader>()
  const { revalidate } = useRevalidator()
  const isAdmin = user?.role === "Admin"
  const [createOpen, setCreateOpen] = useState(false)
  const [editingProject, setEditingProject] = useState<Project | null>(null)
  const [deleteProjectTarget, setDeleteProjectTarget] =
    useState<Project | null>(null)
  const [form, setForm] = useState({ name: "", description: "" })
  const [editForm, setEditForm] = useState({ name: "", description: "" })
  const [saving, setSaving] = useState(false)
  const [editSaving, setEditSaving] = useState(false)
  const [busyProjectId, setBusyProjectId] = useState<string | null>(null)

  const createProject = async (e: FormEvent) => {
    e.preventDefault()
    setSaving(true)
    try {
      await api.post<Project>("/api/projects", {
        name: form.name,
        description: form.description || null,
      })
      setForm({ name: "", description: "" })
      setCreateOpen(false)
      toast.success("Project created")
      await revalidate()
    } catch (err) {
      toast.error(err instanceof Error ? err.message : "Failed to create project.")
    } finally {
      setSaving(false)
    }
  }

  const deleteProject = async (project: Project) => {
    setBusyProjectId(project.id)
    try {
      await api.delete(`/api/projects/${project.id}`)
      toast.success(`${project.name} deleted`)
      await revalidate()
    } catch (err) {
      toast.error(err instanceof Error ? err.message : "Failed to delete project.")
    } finally {
      setBusyProjectId(null)
      setDeleteProjectTarget(null)
    }
  }

  const openEdit = (project: Project) => {
    setEditingProject(project)
    setEditForm({
      name: project.name,
      description: project.description ?? "",
    })
  }

  const updateProject = async (e: FormEvent) => {
    e.preventDefault()
    if (!editingProject) return

    setEditSaving(true)
    try {
      await api.patch<Project>(`/api/projects/${editingProject.id}`, {
        name: editForm.name,
        description: editForm.description || null,
      })
      setEditingProject(null)
      toast.success("Project updated")
      await revalidate()
    } catch (err) {
      toast.error(err instanceof Error ? err.message : "Failed to update project.")
    } finally {
      setEditSaving(false)
    }
  }

  return (
    <div className="space-y-6">
      <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h1 className="text-2xl font-semibold">Projects</h1>
          <p className="mt-1 text-sm text-muted-foreground">
            Organize probes and access by project.
          </p>
        </div>
        {isAdmin && (
          <Button onClick={() => setCreateOpen(true)}>
            <Plus className="size-4" />
            Create project
          </Button>
        )}
      </div>

      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2 text-base">
            <FolderKanban className="size-4" />
            All projects
          </CardTitle>
          <CardDescription>
            {isAdmin
              ? "Admins can see every project in this workspace."
              : "You can see projects assigned to you."}
          </CardDescription>
        </CardHeader>
        <CardContent>
          {projects.length === 0 ? (
            <div className="rounded-lg border border-dashed p-8 text-center">
              <p className="text-sm font-medium">No projects yet</p>
              <p className="mt-1 text-sm text-muted-foreground">
                {isAdmin
                  ? "Create the first project to start organizing monitors."
                  : "Ask an admin to assign you to a project."}
              </p>
            </div>
          ) : (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Name</TableHead>
                  <TableHead>Access</TableHead>
                  <TableHead>Members</TableHead>
                  <TableHead>Created</TableHead>
                  <TableHead className="w-0 text-right">Actions</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {projects.map((project) => (
                  <TableRow key={project.id}>
                    <TableCell>
                      <Link to={`/projects/${project.id}`} className="block">
                        <p className="font-medium underline-offset-4 hover:underline">
                          {project.name}
                        </p>
                        {project.description && (
                          <p className="text-xs text-muted-foreground">
                            {project.description}
                          </p>
                        )}
                      </Link>
                    </TableCell>
                    <TableCell>
                      <Badge
                        variant={
                          project.accessRole === "Full access" ||
                          project.accessRole === "Manager"
                            ? "default"
                            : "secondary"
                        }
                      >
                        {project.accessRole}
                      </Badge>
                    </TableCell>
                    <TableCell>{project.memberCount}</TableCell>
                    <TableCell>
                      {new Date(project.createdAt).toLocaleDateString()}
                    </TableCell>
                    <TableCell>
                      <div className="flex justify-end gap-2">
                        <Button asChild variant="outline" size="sm">
                          <Link to={`/projects/${project.id}`}>
                            <Users className="size-4" />
                            Manage
                          </Link>
                        </Button>
                        {isAdmin && (
                          <>
                            <Button
                              variant="outline"
                              size="sm"
                              onClick={() => openEdit(project)}
                            >
                              <Pencil className="size-4" />
                              Edit
                            </Button>
                          <Button
                            variant="outline"
                            size="sm"
                            disabled={busyProjectId === project.id}
                            onClick={() => setDeleteProjectTarget(project)}
                          >
                            <Trash2 className="size-4" />
                            Delete
                          </Button>
                          </>
                        )}
                      </div>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          )}
        </CardContent>
      </Card>

      <Dialog open={createOpen} onOpenChange={setCreateOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Create project</DialogTitle>
            <DialogDescription>
              Create a project workspace for monitors, users, and future reports.
            </DialogDescription>
          </DialogHeader>
          <form onSubmit={createProject} className="space-y-4">
            <div className="space-y-1.5">
              <Label htmlFor="project-name">Name</Label>
              <Input
                id="project-name"
                value={form.name}
                onChange={(e) => setForm((f) => ({ ...f, name: e.target.value }))}
                required
              />
            </div>
            <div className="space-y-1.5">
              <Label htmlFor="project-description">Description</Label>
              <Input
                id="project-description"
                value={form.description}
                onChange={(e) =>
                  setForm((f) => ({ ...f, description: e.target.value }))
                }
              />
            </div>
            <DialogFooter>
              <Button type="submit" disabled={saving}>
                {saving ? "Creating..." : "Create project"}
              </Button>
            </DialogFooter>
          </form>
        </DialogContent>
      </Dialog>

      <Dialog
        open={Boolean(editingProject)}
        onOpenChange={(open) => {
          if (!open) setEditingProject(null)
        }}
      >
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Edit project</DialogTitle>
            <DialogDescription>
              Update the project name and description.
            </DialogDescription>
          </DialogHeader>
          <form onSubmit={updateProject} className="space-y-4">
            <div className="space-y-1.5">
              <Label htmlFor="edit-project-name">Name</Label>
              <Input
                id="edit-project-name"
                value={editForm.name}
                onChange={(e) =>
                  setEditForm((f) => ({ ...f, name: e.target.value }))
                }
                required
              />
            </div>
            <div className="space-y-1.5">
              <Label htmlFor="edit-project-description">Description</Label>
              <Input
                id="edit-project-description"
                value={editForm.description}
                onChange={(e) =>
                  setEditForm((f) => ({ ...f, description: e.target.value }))
                }
              />
            </div>
            <DialogFooter>
              <Button type="submit" disabled={editSaving}>
                {editSaving ? "Saving..." : "Save changes"}
              </Button>
            </DialogFooter>
          </form>
        </DialogContent>
      </Dialog>

      <AlertDialog
        open={Boolean(deleteProjectTarget)}
        onOpenChange={(open) => {
          if (!open) setDeleteProjectTarget(null)
        }}
      >
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogMedia className="bg-destructive/10 text-destructive">
              <Trash2 className="size-5" />
            </AlertDialogMedia>
            <AlertDialogTitle>Delete project?</AlertDialogTitle>
            <AlertDialogDescription>
              {deleteProjectTarget
                ? `${deleteProjectTarget.name} and its project membership assignments will be removed.`
                : "This project will be removed."}
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Cancel</AlertDialogCancel>
            <AlertDialogAction
              variant="destructive"
              disabled={
                Boolean(deleteProjectTarget) &&
                busyProjectId === deleteProjectTarget?.id
              }
              onClick={() => {
                if (deleteProjectTarget) void deleteProject(deleteProjectTarget)
              }}
            >
              Delete
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  )
}

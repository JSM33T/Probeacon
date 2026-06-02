import { type FormEvent, useState } from "react"
import { Link, useLoaderData, useRevalidator } from "react-router"
import { FolderKanban, Plus, Trash2 } from "lucide-react"
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

interface Project {
  id: string
  name: string
  description: string | null
  createdAt: string
  createdByUserId: string
  accessRole: "Admin" | "Editor" | "Viewer"
  memberCount: number
}

export async function clientLoader() {
  const projects = await api.get<Project[]>("/api/projects")
  return { projects, user: getUser() }
}

export function HydrateFallback() {
  return (
    <div className="flex min-h-[200px] items-center justify-center">
      <span className="text-sm text-muted-foreground">Loading...</span>
    </div>
  )
}

export default function ProjectsPage() {
  const { projects, user } = useLoaderData<typeof clientLoader>()
  const { revalidate } = useRevalidator()
  const isAdmin = user?.role === "Admin"
  const [createOpen, setCreateOpen] = useState(false)
  const [form, setForm] = useState({ name: "", description: "" })
  const [saving, setSaving] = useState(false)
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
                  {isAdmin && <TableHead className="w-0 text-right">Actions</TableHead>}
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
                      <Badge variant={project.accessRole === "Admin" ? "default" : "secondary"}>
                        {project.accessRole}
                      </Badge>
                    </TableCell>
                    <TableCell>{project.memberCount}</TableCell>
                    <TableCell>
                      {new Date(project.createdAt).toLocaleDateString()}
                    </TableCell>
                    {isAdmin && (
                      <TableCell>
                        <div className="flex justify-end">
                          <Button
                            variant="outline"
                            size="sm"
                            disabled={busyProjectId === project.id}
                            onClick={() => deleteProject(project)}
                          >
                            <Trash2 className="size-4" />
                            Delete
                          </Button>
                        </div>
                      </TableCell>
                    )}
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
    </div>
  )
}

import { useState } from "react"
import { useLoaderData } from "react-router"
import { Monitor, Smartphone, Trash2 } from "lucide-react"
import { api } from "~/lib/api"
import { getUser } from "~/lib/auth"
import { Button } from "~/components/ui/button"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "~/components/ui/card"
import { cn } from "~/lib/utils"

interface Session {
  id: string
  userAgent: string
  ipAddress: string
  createdAt: string
  lastActiveAt: string
  isCurrentSession: boolean
}

export async function clientLoader() {
  const sessions = await api.get<Session[]>("/api/auth/sessions")
  return { sessions, user: getUser() }
}

function parseDevice(userAgent: string): { label: string; isMobile: boolean } {
  const ua = userAgent.toLowerCase()
  const isMobile = /mobile|android|iphone|ipad/.test(ua)

  if (ua.includes("edg")) return { label: "Edge", isMobile }
  if (ua.includes("chrome")) return { label: "Chrome", isMobile }
  if (ua.includes("firefox")) return { label: "Firefox", isMobile }
  if (ua.includes("safari")) return { label: "Safari", isMobile }
  return { label: "Unknown browser", isMobile }
}

function formatRelative(dateStr: string): string {
  const diff = Date.now() - new Date(dateStr).getTime()
  const mins = Math.floor(diff / 60_000)
  if (mins < 1) return "Just now"
  if (mins < 60) return `${mins}m ago`
  const hrs = Math.floor(mins / 60)
  if (hrs < 24) return `${hrs}h ago`
  const days = Math.floor(hrs / 24)
  return `${days}d ago`
}

export default function SessionsPage() {
  const { sessions: initial } = useLoaderData<typeof clientLoader>()
  const [sessions, setSessions] = useState(initial)
  const [revoking, setRevoking] = useState<string | null>(null)

  const revoke = async (id: string) => {
    setRevoking(id)
    try {
      await api.delete(`/api/auth/sessions/${id}`)
      setSessions((prev) => prev.filter((s) => s.id !== id))
    } catch {
      // silently ignore — UI stays unchanged if it fails
    } finally {
      setRevoking(null)
    }
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold">Active Sessions</h1>
        <p className="mt-1 text-sm text-muted-foreground">
          All devices currently signed in to your account. Revoke any session you don't recognise.
        </p>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Signed-in devices</CardTitle>
          <CardDescription>{sessions.length} active session{sessions.length !== 1 ? "s" : ""}</CardDescription>
        </CardHeader>
        <CardContent className="p-0">
          {sessions.length === 0 ? (
            <p className="px-6 py-8 text-center text-sm text-muted-foreground">No active sessions.</p>
          ) : (
            <ul className="divide-y divide-border">
              {sessions.map((session) => {
                const { label, isMobile } = parseDevice(session.userAgent)
                const DeviceIcon = isMobile ? Smartphone : Monitor

                return (
                  <li
                    key={session.id}
                    className={cn(
                      "flex items-center gap-4 px-6 py-4",
                      session.isCurrentSession && "bg-muted/40"
                    )}
                  >
                    <div className="flex size-10 shrink-0 items-center justify-center rounded-lg border bg-background">
                      <DeviceIcon className="size-5 text-muted-foreground" />
                    </div>

                    <div className="min-w-0 flex-1">
                      <div className="flex items-center gap-2">
                        <span className="text-sm font-medium">{label}</span>
                        {session.isCurrentSession && (
                          <span className="rounded-full bg-primary/10 px-2 py-0.5 text-[11px] font-medium text-primary">
                            This device
                          </span>
                        )}
                      </div>
                      <p className="mt-0.5 truncate text-xs text-muted-foreground">
                        {session.ipAddress} · Last active {formatRelative(session.lastActiveAt)}
                      </p>
                      <p className="text-xs text-muted-foreground/60">
                        Signed in {new Date(session.createdAt).toLocaleDateString()}
                      </p>
                    </div>

                    {!session.isCurrentSession && (
                      <Button
                        type="button"
                        variant="ghost"
                        size="icon-sm"
                        disabled={revoking === session.id}
                        onClick={() => revoke(session.id)}
                        className="shrink-0 text-muted-foreground hover:text-destructive"
                        aria-label="Revoke session"
                      >
                        <Trash2 className="size-4" />
                      </Button>
                    )}
                  </li>
                )
              })}
            </ul>
          )}
        </CardContent>
      </Card>
    </div>
  )
}

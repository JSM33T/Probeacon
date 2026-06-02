import { useState } from "react"
import { Link, useLoaderData } from "react-router"
import { CheckCircle2, Circle, KeyRound, Mail, UserCircle } from "lucide-react"
import { toast } from "sonner"
import { api } from "~/lib/api"
import { getUser } from "~/lib/auth"
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
  CardDescription,
} from "~/components/ui/card"
import { Button } from "~/components/ui/button"
import { cn } from "~/lib/utils"

interface SmtpStatus {
  isConfigured: boolean
}

export async function clientLoader() {
  const user = getUser()
  const smtp = await api.get<SmtpStatus>("/api/settings/smtp")
  return { user, smtpConfigured: smtp.isConfigured }
}

const quickLinks = [
  {
    title: "Settings",
    description: "Manage organization settings",
    to: "/settings",
    enabled: true,
  },
  {
    title: "Projects",
    description: "Manage project workspaces",
    to: "/projects",
    enabled: true,
  },
  { title: "Probes", description: "Coming soon", to: null, enabled: false },
  {
    title: "Team",
    description: "Invite and manage users",
    to: "/users",
    enabled: true,
  },
]

export default function DashboardPage() {
  const { user, smtpConfigured } = useLoaderData<typeof clientLoader>()
  const hasRealEmail = user?.email.includes("@") ?? false
  const emailVerified = user?.emailVerified ?? false

  type ChecklistItem = {
    key: string
    icon: React.ComponentType<{ className?: string }>
    title: string
    description: string
    action: React.ReactNode
  }

  const checklist: ChecklistItem[] = []

  if (!smtpConfigured)
    checklist.push({
      key: "smtp",
      icon: KeyRound,
      title: "Configure SMTP",
      description: "Required for email verification and notifications.",
      action: (
        <Link to="/auth-config">
          <Button size="sm" variant="outline">
            Configure
          </Button>
        </Link>
      ),
    })

  if (!hasRealEmail)
    checklist.push({
      key: "email",
      icon: UserCircle,
      title: "Set a real email address",
      description:
        "You're using a temporary identifier. Add a real email in your profile.",
      action: (
        <Link to="/profile">
          <Button size="sm" variant="outline">
            Go to profile
          </Button>
        </Link>
      ),
    })

  if (hasRealEmail && !emailVerified)
    checklist.push({
      key: "verify",
      icon: Mail,
      title: "Verify your email",
      description: `A verification link was sent to ${user?.email}.`,
      action: <ResendButton />,
    })

  return (
    <div className="space-y-8">
      <div>
        <h1 className="text-xl font-semibold">Dashboard</h1>
        <p className="mt-1 text-sm text-muted-foreground">
          Welcome back, {user?.displayName}
        </p>
      </div>

      {checklist.length > 0 && (
        <Card>
          <CardHeader>
            <CardTitle className="text-base">Getting started</CardTitle>
            <CardDescription>
              Complete these steps to finish setting up ProBeacon.
            </CardDescription>
          </CardHeader>
          <CardContent className="p-0">
            <ul className="divide-y divide-border">
              {checklist.map((item) => {
                const Icon = item.icon
                return (
                  <li
                    key={item.key}
                    className="flex items-center gap-4 px-6 py-4"
                  >
                    <Circle className="size-5 shrink-0 text-muted-foreground/40" />
                    <div className="flex size-9 shrink-0 items-center justify-center rounded-lg border bg-muted">
                      <Icon className="size-4 text-muted-foreground" />
                    </div>
                    <div className="min-w-0 flex-1">
                      <p className="text-sm font-medium">{item.title}</p>
                      <p className="text-xs text-muted-foreground">
                        {item.description}
                      </p>
                    </div>
                    <div className="shrink-0">{item.action}</div>
                  </li>
                )
              })}
              {checklist.length === 0 && (
                <li className="flex items-center gap-3 px-6 py-4 text-sm text-muted-foreground">
                  <CheckCircle2 className="size-5 text-green-600" />
                  All set up!
                </li>
              )}
            </ul>
          </CardContent>
        </Card>
      )}

      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        {quickLinks.map((card) =>
          card.enabled && card.to ? (
            <Link key={card.title} to={card.to}>
              <Card
                className={cn(
                  "cursor-pointer transition-shadow hover:shadow-md"
                )}
              >
                <CardHeader>
                  <CardTitle className="text-sm">{card.title}</CardTitle>
                  <CardDescription>{card.description}</CardDescription>
                </CardHeader>
              </Card>
            </Link>
          ) : (
            <Card key={card.title} className="opacity-50">
              <CardHeader>
                <CardTitle className="text-sm">{card.title}</CardTitle>
                <CardDescription>{card.description}</CardDescription>
              </CardHeader>
            </Card>
          )
        )}
      </div>
    </div>
  )
}

function ResendButton() {
  const [state, setState] = useState<"idle" | "sending" | "sent">("idle")

  const resend = async () => {
    setState("sending")
    try {
      await api.post("/api/auth/send-verification", {})
      setState("sent")
      toast.success("Verification email sent", {
        description: "Check your inbox for the verification link.",
      })
    } catch {
      setState("idle")
      toast.error("Could not send verification email")
    }
  }

  return (
    <Button
      size="sm"
      variant="outline"
      disabled={state === "sending"}
      onClick={resend}
    >
      {state === "sending" ? "Sending…" : "Resend email"}
    </Button>
  )
}

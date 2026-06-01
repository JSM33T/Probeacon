import { useEffect, useMemo, useState, type ComponentType } from "react"
import {
  Link,
  Outlet,
  redirect,
  useLoaderData,
  useLocation,
} from "react-router"
import { toast } from "sonner"
import {
  ChevronDown,
  Clock3,
  CircleGauge,
  Database,
  FileText,
  FolderKanban,
  KeyRound,
  LogOut,
  Mail,
  Moon,
  MonitorDot,
  Settings,
  ShieldCheck,
  Sun,
  Users,
} from "lucide-react"

import { Avatar, AvatarFallback } from "~/components/ui/avatar"
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
  Collapsible,
  CollapsibleContent,
  CollapsibleTrigger,
} from "~/components/ui/collapsible"
import {
  Sidebar,
  SidebarContent,
  SidebarFooter,
  SidebarGroup,
  SidebarGroupContent,
  SidebarGroupLabel,
  SidebarHeader,
  SidebarInset,
  SidebarMenu,
  SidebarMenuBadge,
  SidebarMenuButton,
  SidebarMenuItem,
  SidebarMenuSub,
  SidebarMenuSubButton,
  SidebarMenuSubItem,
  SidebarProvider,
  SidebarSeparator,
  SidebarTrigger,
  useSidebar,
} from "~/components/ui/sidebar"
import { api } from "~/lib/api"
import { clearSession, getToken, getUser, type AuthUser } from "~/lib/auth"
import { cn } from "~/lib/utils"

export async function clientLoader() {
  const status = await api.get<{ configured: boolean; deploymentMode: string }>(
    "/api/setup/status"
  )
  if (status.deploymentMode === "SelfHosted" && !status.configured) {
    clearSession()
    return redirect("/setup")
  }

  if (!getToken()) return redirect("/login")
  return { user: getUser() }
}

export function HydrateFallback() {
  return (
    <div className="flex min-h-screen items-center justify-center">
      <span className="text-sm text-muted-foreground">Loading...</span>
    </div>
  )
}

type NavItem = {
  title: string
  href?: string
  icon: ComponentType<{ className?: string }>
  badge?: string
  disabled?: boolean
}

type NavGroup = {
  title: string
  icon: ComponentType<{ className?: string }>
  items: NavItem[]
}

const primaryNav: NavItem[] = [
  { title: "Dashboard", href: "/dashboard", icon: CircleGauge },
]

const navGroups: NavGroup[] = [
  {
    title: "Probes",
    icon: MonitorDot,
    items: [
      { title: "All Probes", icon: MonitorDot, badge: "Soon", disabled: true },
      { title: "Probe Groups", icon: Database, badge: "Soon", disabled: true },
      { title: "Probe Results", icon: FileText, badge: "Soon", disabled: true },
    ],
  },
  {
    title: "Projects",
    icon: FolderKanban,
    items: [
      {
        title: "All Projects",
        icon: FolderKanban,
        badge: "Soon",
        disabled: true,
      },
      { title: "Members", icon: Users, badge: "Soon", disabled: true },
      { title: "Reports", icon: FileText, badge: "Soon", disabled: true },
    ],
  },
  {
    title: "Configuration",
    icon: Settings,
    items: [
      { title: "Settings", href: "/settings", icon: Settings },
      { title: "Authentication", href: "/auth-config", icon: KeyRound },
      { title: "Data Sources", icon: Database, badge: "Soon", disabled: true },
      { title: "Sessions", href: "/sessions", icon: ShieldCheck },
    ],
  },
]

function isActivePath(currentPath: string, href?: string) {
  return Boolean(
    href && (currentPath === href || currentPath.startsWith(`${href}/`))
  )
}

function AppSidebarLink({
  item,
  currentPath,
}: {
  item: NavItem
  currentPath: string
}) {
  const { isMobile, setOpenMobile } = useSidebar()
  const Icon = item.icon
  const active = isActivePath(currentPath, item.href)
  const closeMobile = () => {
    if (isMobile) setOpenMobile(false)
  }

  return (
    <SidebarMenuItem>
      {item.disabled || !item.href ? (
        <SidebarMenuButton disabled tooltip={item.title}>
          <Icon />
          <span>{item.title}</span>
        </SidebarMenuButton>
      ) : (
        <SidebarMenuButton asChild isActive={active} tooltip={item.title}>
          <Link to={item.href} onClick={closeMobile}>
            <Icon />
            <span>{item.title}</span>
          </Link>
        </SidebarMenuButton>
      )}
      {item.badge && (
        <SidebarMenuBadge className="text-[10px]">
          {item.badge}
        </SidebarMenuBadge>
      )}
    </SidebarMenuItem>
  )
}

function AppSidebarGroup({
  group,
  currentPath,
}: {
  group: NavGroup
  currentPath: string
}) {
  const { isMobile, setOpenMobile } = useSidebar()
  const Icon = group.icon
  const hasActiveChild = group.items.some((item) =>
    isActivePath(currentPath, item.href)
  )
  const closeMobile = () => {
    if (isMobile) setOpenMobile(false)
  }

  return (
    <Collapsible defaultOpen={hasActiveChild} className="group/collapsible">
      <SidebarMenuItem>
        <CollapsibleTrigger asChild>
          <SidebarMenuButton tooltip={group.title}>
            <Icon />
            <span>{group.title}</span>
            <ChevronDown className="ml-auto transition-transform group-data-[state=open]/collapsible:rotate-180" />
          </SidebarMenuButton>
        </CollapsibleTrigger>
        <CollapsibleContent>
          <SidebarMenuSub>
            {group.items.map((item) => {
              const ItemIcon = item.icon
              const active = isActivePath(currentPath, item.href)

              return (
                <SidebarMenuSubItem key={item.title}>
                  {item.disabled || !item.href ? (
                    <SidebarMenuSubButton
                      aria-disabled="true"
                      className="pointer-events-none opacity-50"
                    >
                      <ItemIcon />
                      <span>{item.title}</span>
                      {item.badge && (
                        <span className="ml-auto rounded-md bg-muted px-1.5 py-0.5 text-[10px] text-muted-foreground">
                          {item.badge}
                        </span>
                      )}
                    </SidebarMenuSubButton>
                  ) : (
                    <SidebarMenuSubButton asChild isActive={active}>
                      <Link to={item.href} onClick={closeMobile}>
                        <ItemIcon />
                        <span>{item.title}</span>
                      </Link>
                    </SidebarMenuSubButton>
                  )}
                </SidebarMenuSubItem>
              )
            })}
          </SidebarMenuSub>
        </CollapsibleContent>
      </SidebarMenuItem>
    </Collapsible>
  )
}

function EmailVerificationBanner() {
  const [sending, setSending] = useState(false)
  const [sent, setSent] = useState(false)

  const resend = async () => {
    setSending(true)
    try {
      await api.post("/api/auth/send-verification", {})
      setSent(true)
      toast.success("Verification email sent", {
        description: "Check your inbox for the verification link.",
      })
    } catch {
      toast.error("Could not send verification email")
    } finally {
      setSending(false)
    }
  }

  return (
    <div className="mb-6 flex items-center gap-3 rounded-lg border border-amber-200 bg-amber-50 px-4 py-3 text-sm text-amber-800 dark:border-amber-900 dark:bg-amber-950/40 dark:text-amber-300">
      <Mail className="size-4 shrink-0" />
      <span className="flex-1">
        {sent
          ? "Verification email sent - check your inbox."
          : "Please verify your email address to unlock all features."}
      </span>
      {!sent && (
        <button
          type="button"
          className="shrink-0 font-medium underline underline-offset-2 hover:opacity-75 disabled:opacity-50"
          disabled={sending}
          onClick={resend}
        >
          {sending ? "Sending..." : "Resend email"}
        </button>
      )}
    </div>
  )
}

function DemoExpiryBanner({ expiresAt }: { expiresAt: string }) {
  const [now, setNow] = useState(() => Date.now())
  const expiry = new Date(expiresAt).getTime()
  const remainingMs = Math.max(0, expiry - now)
  const hours = Math.floor(remainingMs / 3_600_000)
  const minutes = Math.floor((remainingMs % 3_600_000) / 60_000)
  const urgent = remainingMs <= 3_600_000

  useEffect(() => {
    const id = window.setInterval(() => setNow(Date.now()), 60_000)
    return () => window.clearInterval(id)
  }, [])

  useEffect(() => {
    if (remainingMs === 0) {
      clearSession()
      window.location.href = "/expired"
    }
  }, [remainingMs])

  return (
    <div
      className={cn(
        "mb-6 flex items-center gap-3 rounded-lg border px-4 py-3 text-sm",
        urgent
          ? "border-red-200 bg-red-50 text-red-800 dark:border-red-900 dark:bg-red-950/40 dark:text-red-300"
          : "border-amber-200 bg-amber-50 text-amber-800 dark:border-amber-900 dark:bg-amber-950/40 dark:text-amber-300"
      )}
    >
      <Clock3 className="size-4 shrink-0" />
      <span className="flex-1">
        Demo workspace expires in {hours}h {minutes}m.
      </span>
    </div>
  )
}

function AppSidebar({
  user,
  initials,
  theme,
  onToggleTheme,
  onLogoutClick,
}: {
  user: AuthUser | null
  initials: string
  theme: "light" | "dark"
  onToggleTheme: () => void
  onLogoutClick: () => void
}) {
  const location = useLocation()
  const { isMobile, setOpenMobile } = useSidebar()
  const closeMobile = () => {
    if (isMobile) setOpenMobile(false)
  }

  return (
    <Sidebar collapsible="offcanvas">
      <SidebarHeader className="gap-0 p-0">
        <SidebarMenu className="p-2">
          <SidebarMenuItem>
            <SidebarMenuButton asChild size="lg" tooltip="ProBeacon">
              <Link to="/dashboard" onClick={closeMobile}>
                <div className="flex size-9 items-center justify-center rounded-lg bg-sidebar-primary text-sm font-semibold text-sidebar-primary-foreground">
                  PB
                </div>
                <div className="min-w-0 flex-1">
                  <span className="block truncate text-sm font-semibold">
                    ProBeacon
                  </span>
                  <span className="block truncate text-xs text-sidebar-foreground/60">
                    Monitoring workspace
                  </span>
                </div>
              </Link>
            </SidebarMenuButton>
          </SidebarMenuItem>
        </SidebarMenu>
      </SidebarHeader>

      <SidebarSeparator />

      <SidebarContent>
        <SidebarGroup>
          <SidebarGroupLabel>Menu</SidebarGroupLabel>
          <SidebarGroupContent>
            <SidebarMenu>
              {primaryNav.map((item) => (
                <AppSidebarLink
                  key={item.title}
                  item={item}
                  currentPath={location.pathname}
                />
              ))}
            </SidebarMenu>
          </SidebarGroupContent>
        </SidebarGroup>

        <SidebarGroup>
          <SidebarGroupLabel>Workspace</SidebarGroupLabel>
          <SidebarGroupContent>
            <SidebarMenu>
              {navGroups.map((group) => (
                <AppSidebarGroup
                  key={group.title}
                  group={group}
                  currentPath={location.pathname}
                />
              ))}
            </SidebarMenu>
          </SidebarGroupContent>
        </SidebarGroup>
      </SidebarContent>

      <SidebarFooter>
        <SidebarSeparator className="mb-1" />
        <div className="flex items-center gap-2 px-2 py-1">
          <Avatar className="rounded-md" size="lg">
            <AvatarFallback className="rounded-md bg-background font-semibold text-foreground">
              {initials}
            </AvatarFallback>
          </Avatar>
          <div className="min-w-0 flex-1 group-data-[collapsible=icon]:hidden">
            <p className="truncate text-sm font-medium">{user?.displayName}</p>
            <p className="truncate text-xs text-sidebar-foreground/60">
              {user?.role}
            </p>
          </div>
          <Button
            type="button"
            variant="ghost"
            size="icon-sm"
            className="shrink-0 text-sidebar-foreground/60 group-data-[collapsible=icon]:hidden hover:bg-sidebar-accent hover:text-sidebar-accent-foreground"
            aria-label={
              theme === "dark" ? "Switch to light mode" : "Switch to dark mode"
            }
            onClick={onToggleTheme}
          >
            {theme === "dark" ? (
              <Sun className="size-4" />
            ) : (
              <Moon className="size-4" />
            )}
          </Button>
          <Button
            asChild
            variant="ghost"
            size="icon-sm"
            className="shrink-0 text-sidebar-foreground/60 group-data-[collapsible=icon]:hidden hover:bg-sidebar-accent hover:text-sidebar-accent-foreground"
          >
            <Link
              to="/profile"
              onClick={closeMobile}
              aria-label="Open profile settings"
            >
              <Settings className="size-4" />
            </Link>
          </Button>
        </div>
        <SidebarMenu>
          <SidebarMenuItem>
            <SidebarMenuButton tooltip="Logout" onClick={onLogoutClick}>
              <LogOut />
              <span>Logout</span>
            </SidebarMenuButton>
          </SidebarMenuItem>
        </SidebarMenu>
      </SidebarFooter>
    </Sidebar>
  )
}

export default function AppLayout() {
  const { user } = useLoaderData<typeof clientLoader>()
  const [logoutDialogOpen, setLogoutDialogOpen] = useState(false)
  const [theme, setTheme] = useState<"light" | "dark">("light")
  const initials = useMemo(() => {
    const name = user?.displayName?.trim()
    if (!name) return "PB"
    return name
      .split(/\s+/)
      .slice(0, 2)
      .map((part) => part[0]?.toUpperCase())
      .join("")
  }, [user?.displayName])

  useEffect(() => {
    const savedTheme = localStorage.getItem("pb_theme")
    const prefersDark = window.matchMedia(
      "(prefers-color-scheme: dark)"
    ).matches
    const nextTheme =
      savedTheme === "dark" || (!savedTheme && prefersDark) ? "dark" : "light"

    setTheme(nextTheme)
    document.documentElement.classList.toggle("dark", nextTheme === "dark")
  }, [])

  const toggleTheme = () => {
    const nextTheme = theme === "dark" ? "light" : "dark"
    setTheme(nextTheme)
    localStorage.setItem("pb_theme", nextTheme)
    document.documentElement.classList.toggle("dark", nextTheme === "dark")
  }

  const logout = async () => {
    const toastId = toast.loading("Logging out...")
    try {
      await api.post("/api/auth/logout", {})
    } catch {
      /* ignore */
    }
    toast.dismiss(toastId)
    clearSession()
    window.location.href = "/login"
  }

  return (
    <SidebarProvider
      style={
        {
          "--sidebar-width": "17.5rem",
        } as React.CSSProperties
      }
    >
      <AppSidebar
        user={user}
        initials={initials}
        theme={theme}
        onToggleTheme={toggleTheme}
        onLogoutClick={() => setLogoutDialogOpen(true)}
      />

      <SidebarInset>
        <header className="sticky top-0 z-30 flex h-14 items-center gap-3 border-b bg-background/95 px-4 backdrop-blur md:hidden">
          <SidebarTrigger aria-label="Open menu" />
          <Link to="/dashboard" className="flex items-center gap-2">
            <span className="flex size-8 items-center justify-center rounded-md bg-primary text-xs font-semibold text-primary-foreground">
              PB
            </span>
            <span className="text-sm font-semibold">ProBeacon</span>
          </Link>
        </header>

        <main className="min-w-0 px-4 py-6 sm:px-6 lg:px-8">
          <div className="mx-auto w-full max-w-6xl">
            {user?.tenantKind === "OnlineDemo" && user.tenantExpiresAt && (
              <DemoExpiryBanner expiresAt={user.tenantExpiresAt} />
            )}
            {!user?.emailVerified && <EmailVerificationBanner />}
            <Outlet />
          </div>
        </main>
      </SidebarInset>

      <AlertDialog open={logoutDialogOpen} onOpenChange={setLogoutDialogOpen}>
        <AlertDialogContent className="rounded-lg shadow-2xl">
          <AlertDialogHeader>
            <AlertDialogMedia className="bg-destructive/10 text-destructive">
              <LogOut className="size-5" />
            </AlertDialogMedia>
            <AlertDialogTitle>Log out of ProBeacon?</AlertDialogTitle>
            <AlertDialogDescription>
              Your current session will end and you will need to sign in again
              to continue.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Cancel</AlertDialogCancel>
            <AlertDialogAction variant="destructive" onClick={logout}>
              Logout
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </SidebarProvider>
  )
}

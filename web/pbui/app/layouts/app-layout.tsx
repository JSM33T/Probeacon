import { useMemo, useState } from "react"
import {
  Link,
  Outlet,
  redirect,
  useLoaderData,
  useLocation,
} from "react-router"
import {
  ChevronDown,
  CircleGauge,
  Database,
  FileText,
  FolderKanban,
  LogOut,
  Menu,
  MonitorDot,
  Settings,
  ShieldCheck,
  Users,
  X,
} from "lucide-react"

import { Button } from "~/components/ui/button"
import { clearSession, getToken, getUser } from "~/lib/auth"
import { api } from "~/lib/api"
import { cn } from "~/lib/utils"

export async function clientLoader() {
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
  icon: React.ComponentType<{ className?: string }>
  badge?: string
  disabled?: boolean
}

type NavGroup = {
  title: string
  icon: React.ComponentType<{ className?: string }>
  items: NavItem[]
}

const primaryNav: NavItem[] = [
  { title: "Dashboard", href: "/dashboard", icon: CircleGauge },
  { title: "Settings", href: "/settings", icon: Settings },
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
      { title: "Data Sources", icon: Database, badge: "Soon", disabled: true },
      { title: "Sessions", href: "/sessions", icon: ShieldCheck },
    ],
  },
]

function SidebarLink({
  item,
  active,
  onNavigate,
}: {
  item: NavItem
  active?: boolean
  onNavigate?: () => void
}) {
  const Icon = item.icon
  const className = cn(
    "flex h-9 items-center gap-2 rounded-md px-3 text-sm font-medium transition-colors",
    active
      ? "bg-sidebar-accent text-sidebar-accent-foreground shadow-sm"
      : "text-sidebar-foreground/72 hover:bg-sidebar-accent hover:text-sidebar-accent-foreground",
    item.disabled && "pointer-events-none opacity-50"
  )

  const content = (
    <>
      <Icon className="size-4" />
      <span className="min-w-0 flex-1 truncate">{item.title}</span>
      {item.badge && (
        <span className="rounded-md bg-muted px-1.5 py-0.5 text-[10px] font-medium text-muted-foreground">
          {item.badge}
        </span>
      )}
    </>
  )

  if (!item.href || item.disabled) {
    return (
      <div className={className} aria-disabled="true">
        {content}
      </div>
    )
  }

  return (
    <Link to={item.href} className={className} onClick={onNavigate}>
      {content}
    </Link>
  )
}

function SidebarGroup({
  group,
  currentPath,
  onNavigate,
}: {
  group: NavGroup
  currentPath: string
  onNavigate?: () => void
}) {
  const hasActiveChild = group.items.some(
    (item) => item.href && currentPath.startsWith(item.href)
  )
  const [open, setOpen] = useState(hasActiveChild)
  const Icon = group.icon

  return (
    <div>
      <Button
        type="button"
        variant="ghost"
        className="h-9 w-full justify-start gap-2 rounded-md px-3 text-sidebar-foreground/82 hover:bg-sidebar-accent hover:text-sidebar-accent-foreground"
        aria-expanded={open}
        onClick={() => setOpen((value) => !value)}
      >
        <Icon className="size-4" />
        <span className="flex-1 text-left text-sm">{group.title}</span>
        <ChevronDown
          className={cn("size-4 transition-transform", open && "rotate-180")}
        />
      </Button>
      {open && (
        <div className="mt-1 ml-4 space-y-1 border-l border-sidebar-border pl-3">
          {group.items.map((item) => (
            <SidebarLink
              key={item.title}
              item={item}
              active={Boolean(item.href && currentPath.startsWith(item.href))}
              onNavigate={onNavigate}
            />
          ))}
        </div>
      )}
    </div>
  )
}

export default function AppLayout() {
  const { user } = useLoaderData<typeof clientLoader>()
  const location = useLocation()
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false)
  const initials = useMemo(() => {
    const name = user?.displayName?.trim()
    if (!name) return "PB"
    return name
      .split(/\s+/)
      .slice(0, 2)
      .map((part) => part[0]?.toUpperCase())
      .join("")
  }, [user?.displayName])

  const logout = async () => {
    try { await api.post("/api/auth/logout", {}) } catch { /* ignore */ }
    clearSession()
    window.location.href = "/login"
  }

  const closeMobileMenu = () => setMobileMenuOpen(false)

  const sidebar = (
    <>
      <div className="flex h-16 items-center gap-3 border-b border-sidebar-border px-5">
        <div className="flex size-9 items-center justify-center rounded-lg bg-sidebar-primary text-sm font-semibold text-sidebar-primary-foreground">
          PB
        </div>
        <div className="min-w-0 flex-1">
          <Link
            to="/dashboard"
            className="block truncate text-sm font-semibold"
            onClick={closeMobileMenu}
          >
            ProBeacon
          </Link>
          <p className="truncate text-xs text-sidebar-foreground/60">
            Monitoring workspace
          </p>
        </div>
        <Button
          type="button"
          variant="ghost"
          size="icon-sm"
          className="md:hidden"
          aria-label="Close menu"
          onClick={closeMobileMenu}
        >
          <X className="size-4" />
        </Button>
      </div>

      <nav className="flex-1 space-y-5 overflow-y-auto px-3 py-4">
        <div className="space-y-1">
          <p className="px-3 text-xs font-medium tracking-wide text-sidebar-foreground/50 uppercase">
            Menu
          </p>
          {primaryNav.map((item) => (
            <SidebarLink
              key={item.title}
              item={item}
              active={
                location.pathname === item.href ||
                location.pathname.startsWith(`${item.href}/`)
              }
              onNavigate={closeMobileMenu}
            />
          ))}
        </div>

        <div className="space-y-2">
          <p className="px-3 text-xs font-medium tracking-wide text-sidebar-foreground/50 uppercase">
            Workspace
          </p>
          {navGroups.map((group) => (
            <SidebarGroup
              key={group.title}
              group={group}
              currentPath={location.pathname}
              onNavigate={closeMobileMenu}
            />
          ))}
        </div>
      </nav>

      <div className="border-t border-sidebar-border p-3">
        <div className="mb-3 flex items-center gap-3 rounded-md bg-sidebar-accent px-3 py-2">
          <div className="flex size-9 shrink-0 items-center justify-center rounded-md bg-background text-xs font-semibold text-foreground">
            {initials}
          </div>
          <div className="min-w-0 flex-1">
            <p className="truncate text-sm font-medium">{user?.displayName}</p>
            <p className="truncate text-xs text-sidebar-foreground/60">
              {user?.role}
            </p>
          </div>
        </div>
        <Button
          type="button"
          variant="ghost"
          className="h-9 w-full justify-start gap-2 rounded-md px-3 text-sidebar-foreground/72 hover:bg-sidebar-accent hover:text-sidebar-accent-foreground"
          onClick={logout}
        >
          <LogOut className="size-4" />
          <span>Logout</span>
        </Button>
      </div>
    </>
  )

  return (
    <div className="min-h-screen bg-background text-foreground md:grid md:grid-cols-[280px_1fr]">
      <header className="sticky top-0 z-30 flex h-14 items-center justify-between border-b bg-background/95 px-4 backdrop-blur md:hidden">
        <Link to="/dashboard" className="flex items-center gap-2">
          <span className="flex size-8 items-center justify-center rounded-md bg-primary text-xs font-semibold text-primary-foreground">
            PB
          </span>
          <span className="text-sm font-semibold">ProBeacon</span>
        </Link>
        <Button
          type="button"
          variant="outline"
          size="icon-sm"
          aria-label="Open menu"
          aria-expanded={mobileMenuOpen}
          onClick={() => setMobileMenuOpen(true)}
        >
          <Menu className="size-4" />
        </Button>
      </header>

      {mobileMenuOpen && (
        <button
          type="button"
          aria-label="Close menu overlay"
          className="fixed inset-0 z-40 bg-background/80 backdrop-blur-sm md:hidden"
          onClick={closeMobileMenu}
        />
      )}

      <aside
        className={cn(
          "fixed inset-y-0 left-0 z-50 flex w-[min(20rem,calc(100vw-2rem))] flex-col border-r border-sidebar-border bg-sidebar text-sidebar-foreground shadow-xl transition-transform duration-200 md:sticky md:top-0 md:z-auto md:h-screen md:w-auto md:translate-x-0 md:shadow-none",
          mobileMenuOpen ? "translate-x-0" : "-translate-x-full"
        )}
      >
        {sidebar}
      </aside>

      <main className="min-w-0 px-4 py-6 sm:px-6 lg:px-8">
        <div className="mx-auto w-full max-w-6xl">
          <Outlet />
        </div>
      </main>
    </div>
  )
}

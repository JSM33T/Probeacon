import { redirect, useLoaderData } from "react-router"
import { api } from "~/lib/api"
import { getToken } from "~/lib/auth"
import { Button } from "~/components/ui/button"

export async function clientLoader() {
  if (getToken()) return redirect("/dashboard")

  try {
    const { configured } = await api.get<{ configured: boolean }>("/api/setup/status")
    return redirect(configured ? "/login" : "/setup")
  } catch {
    return { error: true }
  }
}

export function HydrateFallback() {
  return (
    <div className="min-h-screen flex items-center justify-center">
      <span className="text-sm text-muted-foreground">Loading…</span>
    </div>
  )
}

export default function Home() {
  const data = useLoaderData<typeof clientLoader>()

  if (data && "error" in data) {
    return (
      <div className="min-h-screen flex flex-col items-center justify-center gap-3">
        <p className="text-sm font-medium">Could not reach the API.</p>
        <p className="text-sm text-muted-foreground">Make sure the server is running at the configured URL.</p>
        <Button variant="outline" onClick={() => window.location.reload()}>
          Retry
        </Button>
      </div>
    )
  }

  return null
}

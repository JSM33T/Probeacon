import { Link } from "react-router"
import { Clock3 } from "lucide-react"
import { Button } from "~/components/ui/button"
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "~/components/ui/card"
import { clearSession } from "~/lib/auth"

export function clientLoader() {
  clearSession()
  return null
}

export default function ExpiredWorkspacePage() {
  return (
    <div className="flex min-h-screen items-center justify-center bg-background p-4">
      <Card className="w-full max-w-md">
        <CardHeader className="text-center">
          <div className="mx-auto mb-2 flex size-11 items-center justify-center rounded-lg border bg-muted">
            <Clock3 className="size-5 text-muted-foreground" />
          </div>
          <CardTitle>Demo workspace expired</CardTitle>
          <CardDescription>
            Temporary ProBeacon workspaces are cleared after their demo window.
            Create a new workspace to keep exploring.
          </CardDescription>
        </CardHeader>
        <CardContent className="flex flex-col gap-3">
          <Button asChild>
            <Link to="/signup">Create new workspace</Link>
          </Button>
          <Button asChild variant="outline">
            <Link to="/login">Back to sign in</Link>
          </Button>
        </CardContent>
      </Card>
    </div>
  )
}

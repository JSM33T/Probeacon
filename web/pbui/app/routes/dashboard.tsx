import { Link, useLoaderData } from "react-router"
import { getUser } from "~/lib/auth"
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "~/components/ui/card"

export async function clientLoader() {
  return { user: getUser() }
}

const cards = [
  { title: "Settings", description: "Manage organization settings", to: "/settings", enabled: true },
  { title: "Projects", description: "Coming soon", to: null, enabled: false },
  { title: "Probes", description: "Coming soon", to: null, enabled: false },
  { title: "Team", description: "Coming soon", to: null, enabled: false },
]

export default function DashboardPage() {
  const { user } = useLoaderData<typeof clientLoader>()

  return (
    <div>
      <div className="mb-8">
        <h1 className="text-xl font-semibold">Dashboard</h1>
        <p className="text-sm text-muted-foreground mt-1">Welcome back, {user?.displayName}</p>
      </div>
      <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
        {cards.map((card) =>
          card.enabled && card.to ? (
            <Link key={card.title} to={card.to}>
              <Card className="hover:shadow-md transition-shadow cursor-pointer">
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
          ),
        )}
      </div>
    </div>
  )
}

import { type FormEvent, useState } from "react"
import { Link } from "react-router"
import { CheckCircle } from "lucide-react"
import { api } from "~/lib/api"
import { Button } from "~/components/ui/button"
import { Input } from "~/components/ui/input"
import { Label } from "~/components/ui/label"
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "~/components/ui/card"

export default function ForgotPasswordPage() {
  const [email, setEmail] = useState("")
  const [sent, setSent] = useState(false)
  const [loading, setLoading] = useState(false)

  const submit = async (e: FormEvent) => {
    e.preventDefault()
    setLoading(true)
    try {
      await api.post("/api/auth/forgot-password", { email })
      setSent(true)
    } catch {
      // Always show the same confirmation — never reveal whether the email exists.
      setSent(true)
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="flex min-h-screen items-center justify-center bg-background p-4">
      <div className="w-full max-w-sm">
        <div className="mb-8 text-center">
          <h1 className="text-2xl font-semibold">ProBeacon</h1>
          <p className="mt-1 text-sm text-muted-foreground">Reset your password</p>
        </div>
        <Card>
          {sent ? (
            <CardContent className="py-8 text-center">
              <CheckCircle className="mx-auto mb-4 size-12 text-green-600" />
              <h2 className="text-lg font-semibold">Check your email</h2>
              <p className="mt-2 text-sm text-muted-foreground">
                If an account exists for <span className="font-medium">{email}</span>,
                we've sent a link to reset your password. The link expires in 1 hour.
              </p>
              <Button asChild variant="outline" className="mt-6 w-full">
                <Link to="/login">Back to sign in</Link>
              </Button>
            </CardContent>
          ) : (
            <>
              <CardHeader>
                <CardTitle>Forgot password</CardTitle>
                <CardDescription>
                  Enter your email and we'll send you a reset link.
                </CardDescription>
              </CardHeader>
              <CardContent>
                <form onSubmit={submit} className="flex flex-col gap-4">
                  <div className="flex flex-col gap-1.5">
                    <Label>Email</Label>
                    <Input
                      type="email"
                      value={email}
                      onChange={(e) => setEmail(e.target.value)}
                      placeholder="jane@company.com"
                      required
                    />
                  </div>
                  <Button type="submit" disabled={loading} className="mt-1 w-full">
                    {loading ? "Sending..." : "Send reset link"}
                  </Button>
                </form>
                <p className="mt-4 text-center text-sm text-muted-foreground">
                  Remembered it?{" "}
                  <Link to="/login" className="font-medium text-foreground underline">
                    Sign in
                  </Link>
                </p>
              </CardContent>
            </>
          )}
        </Card>
      </div>
    </div>
  )
}

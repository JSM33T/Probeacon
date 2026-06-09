import { type FormEvent, useState } from "react"
import { useLoaderData, useRevalidator } from "react-router"
import { TriangleAlert } from "lucide-react"
import { toast } from "sonner"
import { api } from "~/lib/api"
import { refreshSession } from "~/lib/auth"
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

interface Profile {
  id: string
  email: string
  displayName: string
  role: string
}

const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/

export async function clientLoader() {
  const profile = await api.get<Profile>("/api/users/me")
  return { profile }
}

export default function ProfilePage() {
  const { profile: initial } = useLoaderData<typeof clientLoader>()
  const { revalidate } = useRevalidator()

  const [info, setInfo] = useState({
    displayName: initial.displayName,
    email: initial.email,
  })
  const [infoSaving, setInfoSaving] = useState(false)
  const trimmedEmail = info.email.trim()
  const emailChanged = trimmedEmail !== initial.email
  const emailValid = emailPattern.test(trimmedEmail)
  const canSaveInfo = emailChanged && emailValid

  const [pw, setPw] = useState({
    currentPassword: "",
    newPassword: "",
    confirmPassword: "",
  })
  const [pwSaving, setPwSaving] = useState(false)

  async function refreshJwt() {
    // Re-mint the access token (via the refresh cookie) so updated claims show immediately.
    // Non-fatal — correct values otherwise appear on the next refresh/login.
    if (await refreshSession()) revalidate()
  }

  const saveInfo = async (e: FormEvent) => {
    e.preventDefault()
    if (!canSaveInfo) return

    setInfoSaving(true)
    try {
      await api.patch("/api/users/me", {
        displayName: info.displayName,
        email: trimmedEmail,
      })
      toast.success("Profile updated")
      await refreshJwt()
    } catch (err: unknown) {
      toast.error(
        err instanceof Error ? err.message : "Failed to save profile."
      )
    } finally {
      setInfoSaving(false)
    }
  }

  const savePassword = async (e: FormEvent) => {
    e.preventDefault()

    if (pw.newPassword !== pw.confirmPassword) {
      toast.error("New passwords do not match")
      return
    }
    if (pw.newPassword.length < 8) {
      toast.error("Password must be at least 8 characters")
      return
    }

    setPwSaving(true)
    try {
      await api.patch("/api/users/me", {
        currentPassword: pw.currentPassword,
        newPassword: pw.newPassword,
      })
      setPw({ currentPassword: "", newPassword: "", confirmPassword: "" })
      toast.success("Password updated")
    } catch (err: unknown) {
      toast.error(
        err instanceof Error ? err.message : "Failed to update password."
      )
    } finally {
      setPwSaving(false)
    }
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold">Profile</h1>
        <p className="mt-1 text-sm text-muted-foreground">
          Manage your personal information and password.
        </p>
      </div>

      {!initial.email.includes("@") && (
        <div className="flex items-start gap-3 rounded-lg border border-orange-200 bg-orange-50 px-4 py-3 text-sm text-orange-800 dark:border-orange-900 dark:bg-orange-950/40 dark:text-orange-300">
          <TriangleAlert className="mt-0.5 size-4 shrink-0" />
          <div>
            <p className="font-medium">Set a real email address</p>
            <p className="mt-0.5 text-orange-700 dark:text-orange-400">
              You're using a temporary login identifier. Configure SMTP under
              Settings, then update your email here to enable notifications and
              account recovery.
            </p>
          </div>
        </div>
      )}

      <Card>
        <CardHeader>
          <CardTitle>Personal info</CardTitle>
          <CardDescription>
            Update your display name and email address.
          </CardDescription>
        </CardHeader>
        <CardContent>
          <form onSubmit={saveInfo} className="space-y-4">
            <div className="space-y-1.5">
              <Label htmlFor="displayName">Display name</Label>
              <Input
                id="displayName"
                value={info.displayName}
                onChange={(e) =>
                  setInfo((f) => ({ ...f, displayName: e.target.value }))
                }
                required
              />
            </div>
            <div className="space-y-1.5">
              <Label htmlFor="email">Email</Label>
              <Input
                id="email"
                type="email"
                value={info.email}
                onChange={(e) =>
                  setInfo((f) => ({ ...f, email: e.target.value }))
                }
                required
              />
              {emailChanged && !emailValid && (
                <p className="text-xs text-destructive">
                  Enter a valid email address.
                </p>
              )}
              {emailChanged && emailValid && (
                <p className="text-xs text-muted-foreground">
                  Changing your email will require re-verification.
                </p>
              )}
            </div>
            {canSaveInfo && (
              <Button type="submit" disabled={infoSaving}>
                {infoSaving ? "Saving..." : "Save changes"}
              </Button>
            )}
          </form>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Change password</CardTitle>
          <CardDescription>
            You must provide your current password to set a new one.
          </CardDescription>
        </CardHeader>
        <CardContent>
          <form onSubmit={savePassword} className="space-y-4">
            <div className="space-y-1.5">
              <Label htmlFor="currentPassword">Current password</Label>
              <Input
                id="currentPassword"
                type="password"
                value={pw.currentPassword}
                onChange={(e) =>
                  setPw((f) => ({ ...f, currentPassword: e.target.value }))
                }
                required
                autoComplete="current-password"
              />
            </div>
            <div className="space-y-1.5">
              <Label htmlFor="newPassword">New password</Label>
              <Input
                id="newPassword"
                type="password"
                value={pw.newPassword}
                onChange={(e) =>
                  setPw((f) => ({ ...f, newPassword: e.target.value }))
                }
                required
                autoComplete="new-password"
              />
            </div>
            <div className="space-y-1.5">
              <Label htmlFor="confirmPassword">Confirm new password</Label>
              <Input
                id="confirmPassword"
                type="password"
                value={pw.confirmPassword}
                onChange={(e) =>
                  setPw((f) => ({ ...f, confirmPassword: e.target.value }))
                }
                required
                autoComplete="new-password"
              />
            </div>
            <Button type="submit" disabled={pwSaving}>
              {pwSaving ? "Updating…" : "Update password"}
            </Button>
          </form>
        </CardContent>
      </Card>
    </div>
  )
}

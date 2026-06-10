// Mirrors the server password policy (ProBeacon.Application Common/Validation/PasswordRules).
// Keep the two in sync.
export const PASSWORD_MIN_LENGTH = 10
export const PASSWORD_MAX_LENGTH = 72 // bcrypt truncates beyond 72 bytes

/** Returns a human-readable error for an invalid password, or null when it passes. */
export function passwordError(password: string): string | null {
  if (password.length < PASSWORD_MIN_LENGTH)
    return `Password must be at least ${PASSWORD_MIN_LENGTH} characters.`
  if (password.length > PASSWORD_MAX_LENGTH)
    return `Password must be at most ${PASSWORD_MAX_LENGTH} characters.`
  if (!/[a-z]/.test(password)) return "Password must contain a lowercase letter."
  if (!/[A-Z]/.test(password)) return "Password must contain an uppercase letter."
  if (!/[0-9]/.test(password)) return "Password must contain a number."
  return null
}

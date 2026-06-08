# ProBeacon — Auth Hardening TODO

Goal: get authentication to "proper / production-grade" for both the **online-demo (web)** and
**self-hosted** deployment modes. This file tracks the gaps found in a review of the current
auth implementation (2026-06-08) and what "done" looks like for each.

**Status legend:** `[ ]` not started · `[~]` in progress · `[x]` done · `[-]` deferred by design

Current state is solid for Phase 1: deployment-mode split (`IsOnlineDemo`), 15-min JWT + opaque
refresh token (SHA-256 hashed at rest, rotated per session), per-device revocable sessions,
tenant-expiry enforced on login + refresh, hashed single-use invite/reset tokens, no email
enumeration on forgot-password. The items below are what stands between that and "proper."

---

## P0 — Blockers for "proper"

### [x] 1. Move refresh token to an HttpOnly cookie (out of localStorage)  — DONE 2026-06-08
**Why:** The 30-day refresh token used to live in `localStorage`, so any XSS = full account
takeover with a long-lived credential. `docs/auth.md` mandates HttpOnly Secure cookies.
**What shipped:**
- Refresh token is now an `HttpOnly; SameSite=Strict` cookie (`pb_refresh`), path-scoped to
  `/api/auth`, set on login / signup / setup / set-password / refresh-rotation and cleared on
  logout. `Secure` follows request scheme (off on plain-HTTP dev, on in prod). Helpers:
  `SetRefreshCookie`/`ClearRefreshCookie` on
  [ApiControllerBase](src/ProBeacon.Api/Controllers/ApiControllerBase.cs).
- `POST /api/auth/refresh` takes **no body** — reads the cookie; 401 if absent. Session is
  looked up by refresh-token **hash alone** (the token is cryptographically unique), so
  `RefreshTokenCommand` no longer carries `SessionId`. The raw token is stripped from all
  response bodies (`RefreshToken` made nullable, nulled in the controllers).
- Web: access token held **in memory only** ([auth.ts](web/app/lib/auth.ts)); `localStorage`
  no longer stores any tokens (legacy keys are scrubbed on `clearSession`). New
  `refreshSession()` / `ensureSession()`; route loaders bootstrap via the cookie on hard reload.
- **CSRF:** handled by `SameSite=Strict` — the cookie is only sent on same-site requests and
  is the sole cookie-authenticated endpoint; every other endpoint uses the Bearer header, so
  there's no cookie-based CSRF surface. (No separate anti-forgery token needed while API + SPA
  are same-origin. If the API is ever split to a different origin, revisit: SameSite=None +
  CSRF token.)
**Verified:** `dotnet build` green; web `typecheck` green.
**Follow-ups / watch-outs:**
- Multi-tab concurrent refresh can race on rotation (narrow window). Will interact with item 4
  (reuse detection) — a naive reuse check could false-positive across tabs. Handle together.
- Not yet manually exercised end-to-end in a browser (login → reload → refresh → logout). Worth
  a quick `/verify` pass.

### [ ] 2. Rate limiting + account lockout
**Why:** [src/ProBeacon.Api/Program.cs](src/ProBeacon.Api/Program.cs) never calls
`AddRateLimiter`. Login, refresh, forgot-password, verify-email, and public demo signup are all
unthrottled → credential stuffing / email-bombing.
**Done when:**
- `.NET` rate limiter wired in `Program.cs` (per-IP fixed/sliding window) and applied to all
  auth endpoints via `[EnableRateLimiting]`.
- Per-account failed-login backoff / temporary lockout after N failures.
- forgot-password + send-verification throttled per email/IP.
**Touches:** `Program.cs`, `LoginCommandHandler`, auth endpoints.

### [ ] 3. Self-hosted: invite/reset without SMTP
**Why:** [CreateUserCommandHandler.cs:29](src/ProBeacon.Application/Users/Commands/CreateUser/CreateUserCommandHandler.cs)
hard-throws `EmailNotConfiguredException` when SMTP is unset, so a fresh self-hosted instance
**cannot add users at all**.
**Done when:**
- When SMTP is unconfigured, create-user (and admin reset) instead **return a copy-paste
  set-password link** (or one-time temp password) for the admin to hand over.
- Email path still used automatically when SMTP is configured.
- UI surfaces the link to the admin.
**Touches:** `CreateUserCommandHandler`, `SendPasswordSetupEmail`/`PasswordSetupMailer`,
`ResetUserPasswordCommand`, `web/app/routes/users.tsx`.

### [ ] 4. Refresh-token reuse detection
**Why:** [RefreshTokenCommandHandler.cs](src/ProBeacon.Application/Auth/Commands/RefreshToken/RefreshTokenCommandHandler.cs)
overwrites the hash in place — no rotation chain, no reuse detection. `docs/auth.md` calls for
`ReplacedByTokenId` + reuse detection.
**Done when:**
- Replay of a superseded refresh token is detected and **revokes the whole session** (and
  optionally all sessions for the user) instead of failing silently.
- Rotation chain tracked (`ReplacedByTokenId` or a small token-history table).
**Touches:** `UserSession` (or new `RefreshToken` entity), `RefreshTokenCommandHandler`,
migration.

---

## P1 — Should-fix

### [ ] 5. Honor session revocation within the access-token window
**Why:** The JWT pipeline validates signature only; nothing checks the `session_id` claim
against the sessions table, so a revoked session's JWT still works for up to 15 min.
**Done when:** `OnTokenValidated` (or middleware) checks session is not revoked, with a short
cache to avoid a DB hit per request — OR we consciously document the 15-min window as accepted.

### [ ] 6. Decide + enforce email verification
**Why:** `email_verified` is a JWT claim but login never enforces it — purely informational
today.
**Done when:** explicit decision recorded (gate vs cosmetic); if gating, login/sensitive actions
check verification, with a clear "verify your email" path.

### [ ] 7. Add `logout-all` endpoint
**Why:** `docs/auth.md` lists `POST /api/auth/logout-all`; only single-session logout and
revoke-by-id exist today.
**Done when:** endpoint revokes all of the current user's sessions.

### [ ] 8. Set JWT `ClockSkew` to ~30s
**Why:** Default skew is 5 min, so a 15-min token really lives ~20 min.
**Done when:** `TokenValidationParameters.ClockSkew` set explicitly in `Program.cs`.

### [ ] 9. Strengthen password policy  — REVIEWED 2026-06-08
**Finding:** all four validators (`SetupCommandValidator`, `SignupCommandValidator`,
`SetPasswordCommandValidator`, and the profile change-password path) enforce only
`MinimumLength(8)` + not-empty. **No complexity, no max length, no breach/zxcvbn check.** The
client mirrors just the 8-char rule. So today the policy is weak-but-consistent.
**Done when:** decide the bar (recommend: ≥10–12 chars, a max length ~128 to bound bcrypt/argon
input, optional zxcvbn strength meter) and apply it consistently server + client.

### [ ] 10. Build demo "claim" (demo → permanent) flow  — CONFIRMED MISSING 2026-06-08
**Finding:** no claim command/endpoint exists — every `claim` hit in `src` is JWT
`ClaimsPrincipal`, unrelated. [docs/onboarding.md](docs/onboarding.md) describes it but it was
never built. The `Tenant` aggregate already supports the mechanics (`Kind`, `ExpiresAt`).
**Done when:** built (authenticated demo user sets `Kind=SelfHosted`, clears `ExpiresAt`,
optionally re-keys email/password) **or** explicitly cut from scope. Decide before launch.

### [ ] 11. Ensure JWT secret is per-install, never a shipped default
**Why:** HS256 secret comes from config; a default secret baked into docker-compose would be a
critical risk.
**Done when:** install/bootstrap generates a unique `Jwt:Secret` (and refuses to boot with a
known placeholder).

---

## P2 — Later (by design / out of Phase 1 scope)

### [-] 12. Enterprise SSO — OIDC/OAuth2, SAML 2.0, LDAP/AD
Phase 2-4 per `docs/auth.md`. Deferred.

### [-] 13. MFA / TOTP two-factor
Not yet scoped. Deferred, but required before "enterprise-grade."

---

## Notes
- Reference design: [docs/auth.md](docs/auth.md), [docs/onboarding.md](docs/onboarding.md).
- Recommended order: ~~1~~ done. Next: **2 → 3 → 4** (rate-limit and SMTP-fallback are small &
  high-value; reuse detection is larger and should account for the multi-tab refresh race noted
  under item 1).

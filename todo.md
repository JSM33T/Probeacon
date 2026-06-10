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

### [x] 3. Self-hosted: invite without SMTP  — DONE 2026-06-08
**What shipped:** create-user no longer throws when SMTP is unset. `IPasswordSetupMailer` split
into `IssueLinkAsync` (issue token + return `/set-password` link, no email) and `SendAsync`.
`CreateUserCommandHandler` always issues the link; emails it when SMTP is configured, else
returns it in `CreateUserResult.InviteLink`. `web/app/routes/users.tsx` shows a copy-paste link
in the "Add user" result dialog when present. (Admin password **reset** already had an SMTP-less
path — `ResetUserPasswordCommand` returns a temp password — so it was left as-is.)
**Verified:** build + web typecheck green.

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

### [x] 7. Add `logout-all` endpoint  — DONE 2026-06-08
`POST /api/auth/logout-all` ([LogoutAllCommand](src/ProBeacon.Application/Auth/Commands/LogoutAll/))
revokes every active session for the current user and clears the refresh cookie. UI: "Sign out
all devices" button on `web/app/routes/sessions.tsx` (clears client state → redirects to /login).
Note: revoked sessions' access tokens still work until expiry — see item #5 (accepted window).

### [x] 8. Set JWT `ClockSkew` to ~30s  — DONE 2026-06-08
`TokenValidationParameters.ClockSkew = TimeSpan.FromSeconds(30)` in `Program.cs`.

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
- Done so far: **1** (HttpOnly cookie), **3** (SMTP-less invite), **7** (logout-all), **8**
  (clock skew). Foundation (user/project/auth) considered finalized for probe work to start.
- Remaining hardening (can run alongside probes, no rework): **2** rate-limit · **4** reuse
  detection (account for multi-tab refresh race) · **5** revocation window · **6** email-verify
  decision · **9** password policy · **10** demo claim · **11** per-install JWT secret.

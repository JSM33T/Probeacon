# ProBeacon — Auth Hardening TODO

Goal: get authentication to "proper / production-grade" for both the **online-demo (web)** and
**self-hosted** deployment modes. This file tracks the remaining gaps and what "done" looks like
for each.

**Status legend:** `[ ]` not started · `[~]` in progress · `[-]` deferred by design

**Baseline already in place:** deployment-mode split (`IsOnlineDemo`), 15-min JWT (30s clock skew)
+ opaque refresh token (SHA-256 hashed at rest, rotated per session) in an `HttpOnly; SameSite=Strict`
cookie, per-device revocable sessions + `logout-all`, tenant-expiry enforced on login/refresh,
hashed single-use invite/reset tokens (SMTP-less link fallback), no email enumeration on
forgot-password, strengthened password policy, and per-IP rate limiting + per-account lockout. The
items below are what's left between that and "proper." All can run alongside probe work — none
require reworking the user/project/auth foundation.

---

## P0 — Blockers for "proper"

### [ ] 1. Refresh-token reuse detection
**Why:** [RefreshTokenCommandHandler.cs](src/ProBeacon.Application/Auth/Commands/RefreshToken/RefreshTokenCommandHandler.cs)
overwrites the hash in place — no rotation chain, no reuse detection. `docs/auth.md` calls for
`ReplacedByTokenId` + reuse detection.
**Done when:**
- Replay of a superseded refresh token is detected and **revokes the whole session** (and
  optionally all sessions for the user) instead of failing silently.
- Rotation chain tracked (`ReplacedByTokenId` or a small token-history table).
**Touches:** `UserSession` (or new `RefreshToken` entity), `RefreshTokenCommandHandler`, migration.
**Watch-out:** multi-tab concurrent refresh can race on rotation (narrow window) — a naive reuse
check could false-positive across tabs. Handle the race here (e.g. small grace window or
single-flight) rather than treating every overlap as a replay.

---

## P1 — Should-fix

### [ ] 2. Honor session revocation within the access-token window
**Why:** The JWT pipeline validates signature only; nothing checks the `session_id` claim
against the sessions table, so a revoked session's JWT still works for up to 15 min.
**Done when:** `OnTokenValidated` (or middleware) checks session is not revoked, with a short
cache to avoid a DB hit per request — OR we consciously document the 15-min window as accepted.

### [ ] 3. Decide + enforce email verification
**Why:** `email_verified` is a JWT claim but login never enforces it — purely informational
today.
**Done when:** explicit decision recorded (gate vs cosmetic); if gating, login/sensitive actions
check verification, with a clear "verify your email" path.

### [ ] 4. Build demo "claim" (demo → permanent) flow  — CONFIRMED MISSING
**Finding:** no claim command/endpoint exists — every `claim` hit in `src` is JWT
`ClaimsPrincipal`, unrelated. [docs/onboarding.md](docs/onboarding.md) describes it but it was
never built. The `Tenant` aggregate already supports the mechanics (`Kind`, `ExpiresAt`).
**Done when:** built (authenticated demo user sets `Kind=SelfHosted`, clears `ExpiresAt`,
optionally re-keys email/password) **or** explicitly cut from scope. Decide before launch.

### [ ] 5. Ensure JWT secret is per-install, never a shipped default
**Why:** HS256 secret comes from config; a default secret baked into docker-compose would be a
critical risk.
**Done when:** install/bootstrap generates a unique `Jwt:Secret` (and refuses to boot with a
known placeholder).

---

## P2 — Later (deferred by design / out of Phase 1 scope)

### [-] 6. Enterprise SSO — OIDC/OAuth2, SAML 2.0, LDAP/AD
Phase 2-4 per `docs/auth.md`. Deferred.

### [-] 7. MFA / TOTP two-factor
Not yet scoped. Deferred, but required before "enterprise-grade."

---

## Notes
- Reference design: [docs/auth.md](docs/auth.md), [docs/onboarding.md](docs/onboarding.md).
- Open hardening to do alongside probes: **1** reuse detection · **2** revocation window ·
  **3** email-verify decision · **4** demo claim · **5** per-install JWT secret.

# Enterprise Authentication & Session Management

## Supported Authentication Modes

| Auth Mode | Purpose | Priority |
|---|---|---|
| Local Login | Default username/password login for self-hosted setup | Phase 1 |
| OIDC / OAuth2 | Enterprise SSO using Microsoft Entra, Okta, Auth0, Keycloak, Google Workspace | Phase 2 |
| SAML 2.0 | Enterprise SSO for older corporate identity providers | Phase 3 |
| LDAP / Active Directory | Internal company directory login | Phase 4 |

## Recommended Login Flow

User enters email → System detects tenant/domain → Chooses auth provider → User logs in via Local / OIDC / SAML / LDAP → ProBeacon creates internal user session → Access token is issued → Refresh token is stored against device/session → User permissions are resolved from ProBeacon RBAC

## Core Auth Tables

### Users

| Column | Purpose |
|---|---|
| Id | User identifier |
| TenantId | Tenant/company this user belongs to |
| Email | User email |
| DisplayName | User name |
| PasswordHash | Only used for local login |
| AuthType | Local, OIDC, SAML, LDAP |
| ExternalProviderId | User ID from external auth provider |
| IsActive | Disable/enable user |
| CreatedAt | User creation time |

### AuthProviders

| Column | Purpose |
|---|---|
| Id | Provider identifier |
| TenantId | Tenant/company using this provider |
| Type | Local, OIDC, SAML, LDAP |
| Name | Provider name |
| AuthorityUrl | OIDC authority URL |
| ClientId | OAuth/OIDC client ID |
| ClientSecretEncrypted | Encrypted client secret |
| CallbackUrl | Login callback URL |
| AllowedDomains | Email domains allowed |
| ClaimMappingsJson | Maps provider claims |
| IsEnabled | Enable/disable provider |

## Session & Refresh Token Tables

### UserSessions

| Column | Purpose |
|---|---|
| Id | Session identifier |
| UserId | Logged-in user |
| TenantId | Tenant scope |
| DeviceName | Browser/device name |
| UserAgent | Browser user agent |
| IpAddress | Login IP |
| Location | Approximate location |
| CreatedAt | Login time |
| LastSeenAt | Last activity |
| ExpiresAt | Session expiry |
| RevokedAt | Null if active |
| RevokeReason | Logout/admin revoke |

### RefreshTokens

| Column | Purpose |
|---|---|
| Id | Token identifier |
| SessionId | Linked session/device |
| UserId | Token owner |
| TokenHash | Hashed refresh token |
| CreatedAt | Token creation time |
| ExpiresAt | Token expiry |
| UsedAt | Last successful use |
| RevokedAt | Token revoked time |
| ReplacedByTokenId | New token after rotation |
| RevocationReason | Logout/rotation/reuse detected |

## Auth APIs

| API | Purpose |
|---|---|
| POST /api/auth/login | Local login |
| POST /api/auth/refresh | Rotate refresh token |
| POST /api/auth/logout | Logout current device |
| POST /api/auth/logout-all | Logout all devices |
| GET /api/auth/sessions | List active devices |
| DELETE /api/auth/sessions/{sessionId} | Logout a specific device |
| GET /api/auth/providers | Get tenant auth providers |

## Security Rules

- Store refresh tokens as hashes
- Use refresh token rotation
- Detect refresh token reuse
- Use HttpOnly Secure cookies
- Keep access tokens short-lived
- Allow device/session revocation
- Keep RBAC inside ProBeacon

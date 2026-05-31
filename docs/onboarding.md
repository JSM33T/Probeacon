# Onboarding & Demo Provisioning

## Overview

ProBeacon supports two entry points: a **live demo** (no signup required) and **self-registration** for permanent accounts. Both result in an isolated tenant with full RBAC and data separation.

---

## 1. Demo Flow

### How it works

1. Visitor clicks **"Try Demo"** on `probeacon-site`
2. API auto-provisions a demo tenant in the background
3. A temporary `demo-[uuid]@probeacon.com` account is created
4. Tenant is seeded with sample projects, probes, and check history
5. Visitor is auto-logged in and lands directly on the dashboard
6. Credentials are displayed once — visitor can note them to return later
7. Tenant and all its data are deleted after **24 hours**

### Demo credentials

Each demo user receives a generated identity scoped to ProBeacon's domain:

```
Email:    demo-a3f9x2@probeacon.com
Password: Xk9#mP2qL
```

These are identifiers only — no real inbox exists at this address. No email is ever sent to them. If the visitor wants to receive their credentials or expiry reminders, they can optionally enter their real email on the demo start screen.

### Pre-seeded sample data

Every demo tenant is provisioned with:

| Entity | Sample data |
|---|---|
| Projects | `My Website`, `API Service`, `Internal Tools` |
| Probes | 2–3 probes per project (HTTP, TCP, ping mix) |
| Check history | 7 days of simulated probe results with realistic uptime/downtime patterns |
| Users | One admin user (the demo account itself) |

This gives the visitor a realistic dashboard to explore without having to set anything up themselves.

---

## 2. Demo Expiry

### Tenant lifecycle

The `Tenant` table carries two extra columns for demo management:

| Column | Type | Purpose |
|---|---|---|
| `IsDemo` | bool | Marks this as a temporary demo tenant |
| `ExpiresAt` | timestamp | When the tenant and all its data will be deleted |

Demo tenants expire **24 hours** after provisioning. The `.NET Worker` service runs a cleanup job hourly:

- Queries all tenants where `IsDemo = true AND ExpiresAt < now`
- Deletes each expired tenant — cascade removes all projects, probes, check results, users, and sessions

### Dashboard expiry banner

While in a demo tenant, the dashboard shows a persistent banner:

```
⚡ Demo — expires in 18h 42m   [Claim your account →]
```

The banner updates in real time and becomes more prominent as expiry approaches (e.g. turns amber at 6h, red at 1h).

---

## 3. Claiming a Demo Account

Before expiry, a visitor can convert their demo into a permanent account.

### Claim flow

1. Visitor clicks **"Claim your account"** in the expiry banner
2. They enter their real email address and choose a password
3. The demo tenant is re-associated with their real email — `IsDemo` is set to `false`, `ExpiresAt` is cleared
4. Their projects, probes, and check history are preserved exactly as they set them up
5. The `demo-[uuid]@probeacon.com` identity is replaced with their real email

This means a visitor who configured monitors during the demo does not lose that work when they convert.

---

## 4. Self-Registration (Non-Demo)

For users who want a permanent account without going through the demo:

1. Visitor clicks **"Get Started"** or **"Sign Up"** on `probeacon-site`
2. They enter name, email, and password
3. API creates a permanent tenant scoped to their account
4. They are redirected to the dashboard with an empty workspace — no sample data

Registration triggers the standard auth flow (see [auth.md](auth.md)).

---

## 5. API Endpoints

| Endpoint | Purpose |
|---|---|
| `POST /api/demo/provision` | Provision a new demo tenant, returns credentials + redirect |
| `POST /api/demo/claim` | Convert demo tenant to permanent with real email |
| `POST /api/auth/register` | Standard self-registration (permanent tenant) |

---

## 6. Worker Jobs

| Job | Schedule | Purpose |
|---|---|---|
| `DemoTenantCleanupJob` | Every hour | Deletes expired demo tenants and all associated data |
| `DemoExpiryReminderJob` | Every hour | Sends reminder email to claimed real emails approaching expiry |

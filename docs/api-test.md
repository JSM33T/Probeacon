# API Test Sequence

Base URLs:
- **Local dev:** `http://localhost:5214`
- **Docker:** `http://localhost:8080`

Replace `{{base}}` with whichever you're using.

> **Windows PowerShell:** use `curl.exe` not `curl` (which aliases to Invoke-WebRequest).

---

## 1. Setup Status Check

Check whether ProBeacon has been configured. Call this first on every app load.

```bash
curl http://localhost:5214/api/setup/status
```

**Expected — not yet configured:**
```json
{ "configured": false }
```

**Expected — already configured:**
```json
{ "configured": true }
```

---

## 2. Setup

One-time only. Creates the tenant, the first admin user, and returns a JWT.
Returns `409 Conflict` if called again after setup is complete.

```bash
curl -X POST http://localhost:5214/api/setup \
  -H "Content-Type: application/json" \
  -d "{\"orgName\": \"Acme Corp\", \"adminName\": \"Jasmeet\", \"email\": \"admin@acme.com\", \"password\": \"Secret123!\"}"
```

**PowerShell:**
```powershell
curl.exe -X POST http://localhost:5214/api/setup `
  -H "Content-Type: application/json" `
  -d "{\"orgName\": \"Acme Corp\", \"adminName\": \"Jasmeet\", \"email\": \"admin@acme.com\", \"password\": \"Secret123!\"}"
```

**Expected `200 OK`:**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2026-06-01T09:00:00Z",
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "email": "admin@acme.com",
  "displayName": "Jasmeet"
}
```

> **Save the `accessToken`** — you need it for all authenticated requests below.

---

## 3. Login

> ⚠️ Not yet built — planned for next phase. Will follow this contract:

```bash
curl -X POST http://localhost:5214/api/auth/login \
  -H "Content-Type: application/json" \
  -d "{\"email\": \"admin@acme.com\", \"password\": \"Secret123!\"}"
```

**Expected `200 OK`:**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2026-06-01T09:00:00Z",
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "email": "admin@acme.com",
  "displayName": "Jasmeet"
}
```

---

## 4. Add / Update a Setting

Replace `<token>` with the `accessToken` from setup or login.

```bash
curl -X PUT http://localhost:5214/api/settings \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <token>" \
  -d "{\"key\": \"site.name\", \"value\": \"ProBeacon\"}"
```

**PowerShell:**
```powershell
curl.exe -X PUT http://localhost:5214/api/settings `
  -H "Content-Type: application/json" `
  -H "Authorization: Bearer <token>" `
  -d "{\"key\": \"site.name\", \"value\": \"ProBeacon\"}"
```

**Expected `200 OK`:**
```json
{
  "key": "site.name",
  "value": "ProBeacon",
  "updatedAt": "2026-05-31T08:30:00Z"
}
```

Add a second setting:

```bash
curl -X PUT http://localhost:5214/api/settings \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <token>" \
  -d "{\"key\": \"probe.default_interval\", \"value\": \"60\"}"
```

---

## 5. Get All Settings

```bash
curl http://localhost:5214/api/settings \
  -H "Authorization: Bearer <token>"
```

**PowerShell:**
```powershell
curl.exe http://localhost:5214/api/settings `
  -H "Authorization: Bearer <token>"
```

**Expected `200 OK`:**
```json
[
  { "key": "site.name", "value": "ProBeacon", "updatedAt": "2026-05-31T08:30:00Z" },
  { "key": "probe.default_interval", "value": "60", "updatedAt": "2026-05-31T08:31:00Z" }
]
```

---

## Quick Re-test Sequence

```bash
# 1. check status
curl http://localhost:5214/api/setup/status

# 2. setup
curl -X POST http://localhost:5214/api/setup \
  -H "Content-Type: application/json" \
  -d "{\"orgName\":\"Acme\",\"adminName\":\"Jasmeet\",\"email\":\"admin@acme.com\",\"password\":\"Secret123!\"}"

# 3. add setting (paste token from step 2)
curl -X PUT http://localhost:5214/api/settings \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <token>" \
  -d "{\"key\":\"site.name\",\"value\":\"ProBeacon\"}"

# 4. list settings
curl http://localhost:5214/api/settings \
  -H "Authorization: Bearer <token>"
```

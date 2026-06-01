# Probeacon

A self-hosted-style uptime and service monitoring platform with multi-tenancy, project-based organization, and role-based access control.

> Think "Uptime Kuma, but multi-tenant, project-aware, and access-controlled."

## What it does

- **Probe** HTTP endpoints, TCP ports, and hosts on a schedule
- **Organize** monitors into projects per team or service
- **Control access** with fine-grained RBAC per project
- **Scale** check history with ClickHouse time-series storage

## Tech Stack

| Layer | Technology |
|---|---|
| Dashboard | React + Vite (SPA) |
| Marketing site | Next.js |
| API | .NET 10 (CQRS) |
| Worker / Checker | .NET Worker Service |
| Messaging | RabbitMQ + MassTransit |
| Relational DB | PostgreSQL 16 |
| Time-series DB | ClickHouse 24 |

## Local Development

**Prerequisites:** Docker, .NET 10 SDK, Node.js 20+

Start the infrastructure:

```bash
cp env.example .env
docker compose up -d
```

This starts:
- PostgreSQL on `localhost:5432`
- ClickHouse on `localhost:8123` (HTTP) / `9000` (native)
- RabbitMQ on `localhost:5672` — management UI at `http://localhost:15672`

Default credentials for all services: `probeacon / probeacon_dev`

## Project Structure

```
src/
├── probeacon-web/            # React SPA (Vite) — authenticated dashboard
├── probeacon-site/           # Next.js — public marketing/landing site
├── ProBeacon.Api/            # .NET API — CQRS commands & queries
├── ProBeacon.Worker/         # .NET Worker Service — probe scheduler
├── ProBeacon.Ingest/         # Consumer: RabbitMQ → ClickHouse
├── ProBeacon.Domain/         # Shared domain models & contracts
└── ProBeacon.Infrastructure/ # EF Core, ClickHouse client, MassTransit
```

## Documentation

- [Architecture & Build Plan](docs/outline.md)
- [Authentication Design](docs/auth.md)

## License

See [LICENSE](LICENSE).

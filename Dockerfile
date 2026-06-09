# ── Web (React SPA) build ─────────────────────────────────────────────────────
# Builds the client-only React app. Output (build/client) is copied into the API's
# wwwroot below so a single image serves both the SPA and the API.
FROM node:22-alpine AS web-build
WORKDIR /web
RUN corepack enable

# Install deps with the lockfile first for layer caching. pnpm-workspace.yaml carries the
# build-script approvals (allowBuilds), so it must be present before install.
COPY web/package.json web/pnpm-lock.yaml web/pnpm-workspace.yaml ./
RUN pnpm install --frozen-lockfile

COPY web/ ./
RUN pnpm run build          # SPA → /web/build/client

# ── API build ─────────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Copy csproj files first for layer caching on restore
COPY src/ProBeacon.Domain/ProBeacon.Domain.csproj           src/ProBeacon.Domain/
COPY src/ProBeacon.Application/ProBeacon.Application.csproj src/ProBeacon.Application/
COPY src/ProBeacon.Infrastructure/ProBeacon.Infrastructure.csproj src/ProBeacon.Infrastructure/
COPY src/ProBeacon.Api/ProBeacon.Api.csproj                 src/ProBeacon.Api/

RUN dotnet restore src/ProBeacon.Api/ProBeacon.Api.csproj

# Copy everything else and publish
COPY src/ src/
RUN dotnet publish src/ProBeacon.Api/ProBeacon.Api.csproj \
    -c Release -o /publish --no-restore

# Drop the built SPA into wwwroot — the API serves it as static files with an
# index.html fallback for non-/api routes.
COPY --from=web-build /web/build/client /publish/wwwroot

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /publish .

ENTRYPOINT ["dotnet", "ProBeacon.Api.dll"]

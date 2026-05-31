import { type RouteConfig, index, layout, route } from "@react-router/dev/routes"

export default [
  index("routes/home.tsx"),
  route("setup", "routes/setup.tsx"),
  route("login", "routes/login.tsx"),
  route("verify-email", "routes/verify-email.tsx"),
  layout("layouts/app-layout.tsx", [
    route("dashboard", "routes/dashboard.tsx"),
    route("settings", "routes/settings.tsx"),
    route("sessions", "routes/sessions.tsx"),
    route("profile", "routes/profile.tsx"),
    route("auth-config", "routes/auth-config.tsx"),
  ]),
] satisfies RouteConfig

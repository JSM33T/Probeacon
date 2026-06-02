import { type RouteConfig, index, layout, route } from "@react-router/dev/routes"

export default [
  index("routes/home.tsx"),
  route("setup", "routes/setup.tsx"),
  route("signup", "routes/signup.tsx"),
  route("login", "routes/login.tsx"),
  route("expired", "routes/expired.tsx"),
  route("verify-email", "routes/verify-email.tsx"),
  layout("layouts/app-layout.tsx", [
    route("dashboard", "routes/dashboard.tsx"),
    route("settings", "routes/settings.tsx"),
    route("sessions", "routes/sessions.tsx"),
    route("team", "routes/team.tsx"),
    route("profile", "routes/profile.tsx"),
    route("auth-config", "routes/auth-config.tsx"),
  ]),
] satisfies RouteConfig

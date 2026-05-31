import { reactRouter } from "@react-router/dev/vite"
import tailwindcss from "@tailwindcss/vite"
import { defineConfig, loadEnv } from "vite"

export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd())

  return {
    resolve: { tsconfigPaths: true },
    plugins: [tailwindcss(), reactRouter()],
    server: {
      proxy: {
        "/api": {
          target: env.VITE_API_URL ?? "http://localhost:8080",
          changeOrigin: true,
        },
      },
    },
  }
})

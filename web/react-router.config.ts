import type { Config } from "@react-router/dev/config"

export default {
  // SPA mode — fully client-side. `react-router build` emits static client assets +
  // index.html (no server bundle); all data loading is via clientLoader.
  ssr: false,
} satisfies Config

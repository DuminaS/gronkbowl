import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    // Bind to every network interface (not just localhost) and keep a fixed port, so the dev
    // server is reachable from a phone on the same Wi-Fi via this machine's LAN IP.
    host: true,
    port: 5173,
    strictPort: true,
  },
})

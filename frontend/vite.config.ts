import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    // Bind to every network interface (not just localhost) and keep a fixed port, so the dev
    // server is reachable from a phone on the same Wi-Fi via this machine's LAN IP, or through
    // a tunnel (cloudflared etc.) pointed at this one port.
    host: true,
    port: 5173,
    strictPort: true,
    // A quick cloudflared tunnel gets a random *.trycloudflare.com hostname every run, so it
    // can't be allowlisted by name - dev-only, not exposed to real traffic, so trusting any host
    // header is an acceptable trade for not having to edit this file every time the tunnel URL
    // changes.
    allowedHosts: true,
    // Proxy API calls server-side instead of the browser hitting the API's own host:port
    // directly - this is what lets a single tunneled URL (just this port) reach the whole app,
    // and it means the browser never makes a cross-origin request at all, LAN or tunneled.
    proxy: {
      '/api': {
        target: 'http://localhost:5091',
        changeOrigin: true,
      },
    },
  },
})

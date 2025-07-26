import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
      '/quality': {
        target: 'http://localhost:5274',
        changeOrigin: true,
      },
    },
  },
})

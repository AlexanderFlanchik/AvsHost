import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'

const apiUrl = 'http://localhost:5000';

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [vue()],
  server: {
    proxy: {
      '/auth/token': { target: apiUrl },
      '/api': { target: apiUrl }
    }
  }
})

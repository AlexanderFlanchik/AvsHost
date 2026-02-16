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
  },
  optimizeDeps: {
    include: [
      `monaco-editor/esm/vs/language/json/json.worker`,
      `monaco-editor/esm/vs/language/css/css.worker`,
      `monaco-editor/esm/vs/language/html/html.worker`,
      `monaco-editor/esm/vs/language/typescript/ts.worker`,
      `monaco-editor/esm/vs/editor/editor.worker`,
    ],
  }
})

import { defineConfig } from 'vitest/config';
import react from '@vitejs/plugin-react';
export default defineConfig({
  plugins: [react()],
  server: { port: 5173, proxy: { '/api': 'http://localhost:5098', '/health': 'http://localhost:5098' } },
  test: { environment: 'node', include: ['src/**/*.test.ts'] }
});

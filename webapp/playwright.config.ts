import { defineConfig } from '@playwright/test';

export default defineConfig({
  webServer: [
    {
      command: '$HOME/dotnet/dotnet run --project ../DocQualityChecker.Api/DocQualityChecker.Api.csproj --urls http://localhost:5274',
      port: 5274,
      reuseExistingServer: true,
      timeout: 120000,
    },
    {
      command: 'npm run dev -- --port 5173',
      port: 5173,
      reuseExistingServer: true,
      timeout: 120000,
      cwd: new URL('.', import.meta.url).pathname,
    },
  ],
  testDir: './tests',
  use: {
    baseURL: 'http://localhost:5173',
    headless: true,
    video: 'on',
    screenshot: 'on',
  },
});

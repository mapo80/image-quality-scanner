# Instructions

## Integration Tests

To execute the Playwright integration tests located under `webapp/tests` you must:

1. Install the local .NET runtime (used by the API) using the bundled script:
   ```bash
   bash dotnet-install.sh -InstallDir "$HOME/dotnet" -Version latest
   ```
2. From the `webapp` folder install npm dependencies and Playwright browsers:
   ```bash
   cd webapp
   npm install
   npx playwright install --with-deps
   ```
3. Execute the tests:
   ```bash
   npm test --silent
   ```

The configuration defined in `webapp/playwright.config.ts` automatically starts the .NET API and the Vite dev server.

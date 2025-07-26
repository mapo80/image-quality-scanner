# Instructions

## Integration Tests

To execute the Playwright integration tests located under `webapp/tests` you must:
These tests are not part of the standard test run and should be executed only on demand or when strictly necessary.

1. Install the local .NET runtime (used by the API) using the bundled script:
   ```bash
   bash dotnet-install.sh -InstallDir "$HOME/dotnet" -Version 9.0.303
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

## .NET version

All projects target **.NET 9** (stable release **9.0.303**). Preview
versions are not permitted. Upgrading or downgrading the SDK or other
libraries must be explicitly requested and must not be performed autonomously.

## Performance report

Running `dotnet test` from the repository root generates the file
`docs/dataset_samples/performance_report.json` with the execution times of each
check for the sample images. When this file changes remember to update the
tables in the README accordingly.

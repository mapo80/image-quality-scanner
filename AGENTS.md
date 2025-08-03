# Instructions

## .NET version

All projects target **.NET 9** (stable release **9.0.303**). Preview
versions are not permitted. Upgrading or downgrading the SDK or other
libraries must be explicitly requested and must not be performed autonomously.

## Performance report

Running `dotnet test` from the repository root generates the file
`docs/dataset_samples/performance_report.json` with the execution times of each
check for the sample images. When this file changes remember to update the
tables in the README accordingly.

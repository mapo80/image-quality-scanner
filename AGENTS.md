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

## MIDV-500 dataset

To download the MIDV-500 sample used in tests:

1. Install dependencies: `pip install -r requirements.txt`.
2. Set the Hugging Face token in the environment (do **not** commit it):
   `export HF_TOKEN=<your token>`.
3. Run the helper script: `python tools/download_midv500.py`.
   - Optional: set `MIDV500_DIR` to choose the output directory.
   - The script saves a `sample_50.txt` file with frame paths.

## Python.NET for tests

Some tests load the .NET assemblies via Python.NET. To configure:

1. Ensure Python 3 is installed and install deps: `pip install -r requirements.txt`.
2. Set environment variables:
   - `DOTNET_ROOT` pointing to the .NET SDK (e.g. `~/.dotnet`).
   - `PYTHONNET_RUNTIME=coreclr`.
   - If needed, `PYTHONNET_PYDLL` to the Python library path.
3. Use `pythonnet.load` with the project's `runtimeconfig.json` and `deps.json` as shown in `run_smoke_test.py`.

## Required metrics for tests

Boolean:
- `IsBlurry`
- `HasGlare`
- `HasNoise`
- `HasLowContrast`
- `HasColorDominance`
- `IsWellExposed` (fail if false)

Numeric:
- `BlurScore`
- `MotionBlurScore`
- `GlareArea`
- `Exposure`
- `Contrast`
- `Noise`
- `ColorDominance`
- `BandingScore`
- `BrisqueScore`
- `ElapsedMs`


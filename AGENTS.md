# AGENT Instructions

To run the tests locally you must install .NET and OpenCV dependencies:

1. Install the .NET SDK using the provided `dotnet-install.sh` script.
   ```bash
   bash dotnet-install.sh --channel 9.0 --install-dir /usr/share/dotnet --skip-non-versioned-files
   export PATH=$PATH:/usr/share/dotnet
   ```
2. Install the native libraries required by OpenCvSharp. The project uses the
   `OpenCvSharp4.runtime.linux-x64` package, so on Ubuntu 24.04 you will
   need the following libraries:
   ```bash
   sudo apt-get update
   sudo apt-get install libtesseract5 libgtk2.0-0 libdc1394-25 libavcodec60 libavformat60 libavutil58 libswscale7 libtiff6 libopenexr-3-1-30
   ```
3. Restore packages and run tests:
   ```bash
   dotnet test DocQualityChecker.Tests/DocQualityChecker.Tests.csproj
   ```

The tests will automatically skip dataset analysis when the optional `dataset/` directory is missing.

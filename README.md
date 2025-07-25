# Image Quality Scanner

This repository contains a small library for evaluating the quality of document images using OpenCV and BRISQUE.

## Building and Testing

1. Install the .NET SDK (version 9.0 or later). You can use the provided `dotnet-install.sh` script:

```bash
bash dotnet-install.sh --channel 9.0 --install-dir /usr/share/dotnet --skip-non-versioned-files
export PATH=$PATH:/usr/share/dotnet
```

2. Restore packages and run tests:

```bash
dotnet test DocQualityChecker.Tests/DocQualityChecker.Tests.csproj
```

The tests include synthetic images and optional dataset images located in the `dataset/` directory. If the dataset folder is missing, the dataset tests will be skipped.

### Native Dependencies

The project relies on the `OpenCvSharp4.runtime.linux-x64` package which
bundles the native `OpenCvSharpExtern` library for any Linux distribution.
To satisfy its dependencies on Ubuntu 24.04 install the following packages
(see the [Docker setup](https://gist.github.com/mapo80/32a68cc1b797e9115ad73b6de8056394)
for a full example):

```bash
sudo apt-get update
sudo apt-get install libtesseract5 libgtk2.0-0 libdc1394-25 libavcodec60 libavformat60 libavutil58 libswscale7 libtiff6 libopenexr-3-1-30
```

## Using the Library

```csharp
var checker = new DocumentQualityChecker();
var settings = new QualitySettings();
using var image = Cv2.ImRead("path/to/image.jpg");
var result = checker.CheckQuality(image, settings);
```

`DocumentQualityResult` will contain BRISQUE score, blur and glare measurements.

## Dataset

Place your test images inside the `dataset/` folder at the repository root. JPEG and PNG files will be processed by the dataset tests.

## Analysis Report

No dataset files were found in the repository, so no image quality analysis was performed.

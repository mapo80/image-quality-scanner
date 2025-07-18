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

# Image Quality Scanner

Questa libreria fornisce semplici controlli di qualità su immagini di documenti tramite API gestite.
L'implementazione utilizza [SkiaSharp](https://github.com/mono/SkiaSharp) per la manipolazione delle immagini.

## Requisiti

- .NET 8 SDK

Le dipendenze NuGet vengono ripristinate automaticamente durante la fase di build/test.


## Controlli di qualità
La classe `DocumentQualityChecker` include i seguenti controlli:

- **BrisqueScore**: misura la varianza dell'intensità dei pixel in scala di grigi. Valori alti indicano immagini di bassa qualità.
- **IsBlurry**: rileva la sfocatura calcolando la varianza del filtro Laplaciano; restituisce il relativo punteggio.
- **HasGlare**: conta i pixel molto luminosi per stimare le zone di riflesso.
- **CheckQuality**: combina i controlli precedenti usando le soglie definite in `QualitySettings` e restituisce un `DocumentQualityResult` con i dettagli.

Le soglie possono essere personalizzate tramite l'oggetto `QualitySettings`.

## Esecuzione dei test

1. Installare lo SDK .NET 8 (se non presente).
2. Dalla cartella del progetto eseguire:

```bash
dotnet test DocQualityChecker.Tests/DocQualityChecker.Tests.csproj -c Release
```

I test creeranno alcune immagini di prova e verificheranno le funzioni di blur, glare e calcolo del punteggio.


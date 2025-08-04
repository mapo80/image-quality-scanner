# Image Quality Scanner

> **Libreria cross‑platform per l’analisi della qualità di immagini di documenti**\
> Implementazioni in **.NET 9 (SkiaSharp)** e **Python 3 (pythonnet)** con supporto PDF via [PDFtoImage](https://www.nuget.org/packages/PDFtoImage).

---

## 1. Introduzione

Image Quality Scanner fornisce API gestite per misurare **sfocatura, riflessi, esposizione, contrasto, rumore** e altre anomalie che possono compromettere OCR, KYC e pipeline di verifica documentale.

```csharp
QualityResult CheckQuality(Stream imageOrPdf,
                          QualitySettings settings,
                          int? pageIndex = null);
```

- Se `pageIndex` è `null` vengono processate **tutte** le pagine.
- L’oggetto `QualityResult` espone punteggi numerici, flag booleani, heat‑map opzionali e statistiche di tempo.

---

## 2. Requisiti e dipendenze

| Ambiente   | Requisiti minimi                              | Note                                           |
| ---------- | --------------------------------------------- | ---------------------------------------------- |
| **.NET**   | .NET 9 SDK                                    | Ripristino automatico pacchetti NuGet          |
| **Python** | ≥ 3.10  \|  `pip install -r requirements.txt` | Usa **pythonnet** per caricare l’assembly .NET |

---

## 3. Installazione

### 3.1 Clonazione

```bash
git clone https://github.com/mapo80/image-quality-scanner.git
cd image-quality-scanner
```

### 3.2 Build .NET

```bash
dotnet restore
dotnet build -c Release
```

### 3.3 Ambiente Python

```bash
pip install -r requirements.txt
```

Se si vuole riprodurre il benchmark **MIDV‑500** esportare:

```bash
export HF_TOKEN="<hugging‑face‑token>"
```

---

## 4. Smoke‑test rapidi

### 4.1 Python

```bash
dotnet publish DocQualityChecker -c Release -o bin/DocQualityChecker
python tools/download_midv500.py
python run_smoke_test.py
```

### 4.2 .NET

```bash
dotnet run --project DocQualitySmoke -- --sample docs/dataset_samples/sample_paths.txt --outDir docs/dataset_samples
```

*Output sintetico su 150 frame MIDV‑500*

```text
| Metric          | MeanRelError | Status |
|-----------------|--------------|--------|
| BlurScore       | 0.8152       | FAIL   |
| Noise           | 0.5079       | FAIL   |
| IsBlurry        | 0.6133       | FAIL   |
| HasNoise        | 0.2933       | FAIL   |
| MotionBlurScore | 0.0381       | OK     |
| GlareArea       | 0.1016       | FAIL   |
| ElapsedMs       | 0.2058       | FAIL   |
```

Tempo medio: **288 ms (Python)** vs **363 ms (.NET)**.

---

## 5. Check di qualità & soglie di default

| Metrica / Flag        | Descrizione                                                     | Soglia di default\*    |
| --------------------- | --------------------------------------------------------------- | ---------------------- |
| **BlurScore**         | Varianza del gradiente (kernel Laplaciano)                      | `< 100` ⇒ sfocata      |
| **IsBlurry**          | Flag qualità nitidezza                                          | Derivato da BlurScore  |
| **MotionBlurScore**   | Convoluzione direzionale per valutare la sfocatura da movimento | `> 4` ⇒ motion‑blur    |
| **GlareArea**         | Pixel saturi raggruppati in cluster                             | `> 5 000 px²`          |
| **HasGlare**          | Flag riflessi                                                   | Derivato da GlareArea  |
| **Exposure**          | Media dei valori d’intensità (8‑bit)                            | `80 – 240` ottimale    |
| **IsWellExposed**     | Flag esposizione                                                | Fuori range            |
| **Contrast**          | RMS contrast                                                    | `< 50` ⇒ basso         |
| **HasLowContrast**    | Flag contrasto basso                                            | Derivato da Contrast   |
| **Noise**             | Deviazione standard su patch casuali                            | `> 0.20` ⇒ noisy       |
| **HasNoise**          | Flag rumore                                                     | Derivato da Noise      |
| **ColorDominance**    | Rapporto tra canali (max/min)                                   | `> 1.30 / < 0.77`      |
| **HasColorDominance** | Flag dominante colore                                           | Derivato               |
| **BandingScore**      | Energia spettrale da FFT su bande orizzontali                   | `> 0.03` ⇒ banding     |
| **HasBanding**        | Flag banding                                                    | Derivato               |
| **BrisqueScore**      | Modello BRISQUE no‑reference                                    | `> 40` ⇒ qualità bassa |

*Le soglie sono interamente configurabili tramite **`QualitySettings`**.*

### 5.1 Metriche essenziali & risultati acquisiti (MIDV‑500, 20 immagini)

| Metrica          | Media | Dev.Std. | Pass‑rate                     |
| ---------------- | ----- | -------- | ----------------------------- |
| **BlurScore**    | 475.8 | 250.47   | 95 % (*IsBlurry* TRUE)        |
| **GlareArea**    | 6 977 | 6 635    | 65 % (*HasGlare* FALSE)       |
| **Exposure**     | 176   | 41       | 87 % (*IsWellExposed* TRUE)   |
| **Contrast**     | 65    | 15       | 88 % (*HasLowContrast* FALSE) |
| **Noise**        | 0.14  | 0.07     | 92 % (*HasNoise* FALSE)       |
| **BrisqueScore** | 37.5  | 6.9      | 72 % (≤ 40)                   |

> I valori sono calcolati sul subset di 20 immagini del dataset \*\*

---

## 6. Differenze tra implementazioni Python e .NET

| Aspetto                     | Python       | .NET     | Note                           |
| --------------------------- | ------------ | -------- | ------------------------------ |
| **Tempo medio (150 frame)** | **≈ 288 ms** | ≈ 363 ms | Python più rapido (NumPy)      |
| BlurScore – errore relativo | 81 %         | —        | Kernel Gaussiano differente    |
| `IsBlurry` – disaccordo     | 61 %         | —        | Soglia divergente              |
| Noise – errore relativo     | 51 %         | —        | Campionamento pixel differente |
| `HasNoise` – disaccordo     | 29 %         | —        | Soglie non allineate           |
| MotionBlurScore             | 3,8 %        | —        | Allineate entro ±5 %           |
| GlareArea                   | 10 %         | —        | Metodo clustering identico     |

> **Da allineare**\
> 1. Kernel e sigma del filtro Gaussiano per `BlurScore`.\
> 2. Soglie `IsBlurry` e `HasNoise`.\
> 3. Strategia di campionamento in `ComputeNoise` (full‑frame vs patch‑sampling).

---

## 7. Dataset & Benchmark

*(Sezione invariata – vedi dettagli nel canvas originale)*

---

## 8. Galleria di esempi per metrica

Ogni blocco è comprimibile; all’interno le immagini sono disposte su **due colonne** così da confrontare rapidamente un esempio "buono" e uno "problematico" per ciascun check.

**Alta qualità**  BlurScore 661.16 IsBlurry False

**Sfocata**  BlurScore 1.22 IsBlurry True

**Glare evidente**  GlareArea 8 421 HasGlare True

**Assenza di glare**  GlareArea 0 HasGlare False

**Sotto‑esposta**  Exposure 62 IsWellExposed False

**Sovra‑esposta**  Exposure 255 IsWellExposed False

**Basso contrasto**  Contrast 31 HasLowContrast True

**Rumore elevato**  Noise 0.34 HasNoise True

**Banding**  BandingScore 0.041 HasBanding True

**BRISQUE elevato**  BrisqueScore 6.50 IsValidDocument False

**Dominante di colore**  ColorDominance 3.00 HasColorDominance True

### 8.1 Guida alla lettura dei punteggi Guida alla lettura dei punteggi

I valori mostrati sotto ogni immagine sono calcolati con le **soglie di default** (§5).\
*Verde* = rientra nei limiti consigliati, *rosso* = richiede intervento (nuovo scatto o preprocessing).\
Personalizza le soglie tramite `QualitySettings` per adattare la sensibilità alle tue esigenze.

---

## 9. Ottimizzazioni performance (lug 2025). Ottimizzazioni performance (lug 2025)

- **Intensity buffer down‑sampling** per immagini > 512 px.
- Campionamento adattivo in `ComputeNoise`.
- Memoizzazione di `MotionBlurScore`, `Noise`, `BandingScore`.
- `Parallel.For` sostituisce doppi cicli annidati.

Con tali ottimizzazioni il tempo per immagine nel dataset *glare* è **23 – 71 ms**.

---

## 10. Web App & API

```bash
dotnet run --project DocQualityChecker.Api/DocQualityChecker.Api.csproj
```

---

## 11. Docker

```bash
docker build -t image-quality-scanner .
docker run -p 8080:8080 image-quality-scanner
```

Applicazione disponibile su [**http://localhost:8080**](http://localhost:8080).

---

## 12. Struttura del repository (principale)

```
├── DocQualityChecker           # Libreria .NET
├── DocQualitySmoke             # Smoke‑test CLI .NET
├── DocsGenerator               # Generazione immagini campione
├── DatasetEvaluator            # Benchmark CLI
├── tools/                      # Script utility Python
└── docs/                       # Dataset & immagini di esempio
```

---

## 13. Contribuire

1. Eseguire `dotnet test` e `pytest`.\
2. Allineare soglie Python/.NET se necessario.\
3. Aggiornare documentazione.

---

## 14. Licenza

Da specificare.


# .NET vs Python alignment

Modifiche applicate alla libreria `DocQualityChecker` per allineare le metriche con l'implementazione Python:

- **Motion blur**: il punteggio ora usa il rapporto tra i gradienti orizzontali e verticali senza ordinamento; il flag è positivo se il rapporto supera la soglia o è inferiore all'inverso.
- **Glare**: l'area viene calcolata contando i canali RGB sopra soglia, replicando il comportamento di NumPy.
- **Noise**: il rumore medio è calcolato sulla differenza assoluta tra l'immagine e la sua versione sfocata con kernel gaussiano 3×3, mantenendo l'aritmetica su byte per emulare la pipeline Python.
- **Banding**: il punteggio corrisponde alla somma delle varianze delle medie di righe e colonne; la luminanza utilizza la conversione ponderata `0.299·R + 0.587·G + 0.114·B`.
- **Soglie**: `NoiseThreshold` predefinita ridotta a 20 per un confronto coerente.

Tutti i controlli ora concordano con la versione Python entro il 10 % di errore relativo.
